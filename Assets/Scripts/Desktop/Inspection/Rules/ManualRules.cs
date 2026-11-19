using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

// Regras do Manual (Resources/Manual/ManualRules.json): carrega uma vez, valida e filtra.
// Instancia unica, compartilhada pela janela do Manual e pelo avaliador do Inspector.
public static class ManualRules
{
    public const string ResourcePath = "Manual/ManualRules";
    public const int SupportedVersion = 1;

    private static ManualRuleBookData cached;

    // Estaticos sobrevivem entre Plays. Limpar aqui faz cada Play reler o ManualRules.json atual.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache()
    {
        cached = null;
    }

    // Chave desconhecida (ex.: "allowedDomain" sem "s") falha no carregamento, em vez de virar null em silencio.
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        MissingMemberHandling = MissingMemberHandling.Error,
    };

    private static readonly HashSet<string> SupportedApps = new()
    {
        "email",
        "ticket",
        "chat",
    };

    public static bool IsSupportedApp(string app)
    {
        return !string.IsNullOrWhiteSpace(app) && SupportedApps.Contains(app.Trim());
    }

    // Lanca InvalidOperationException se o JSON nao existe ou e invalido.
    public static ManualRuleBookData Load()
    {
        return cached ??= LoadFromResources(ResourcePath);
    }

    // Regras do app que ja valem no dia.
    public static IReadOnlyList<ManualRuleData> GetActiveRules(
        ManualRuleBookData manual,
        int currentDay,
        string app
    )
    {
        if (manual?.rules == null)
            throw new ArgumentNullException(nameof(manual));

        List<ManualRuleData> activeRules = new();
        foreach (ManualRuleData rule in manual.rules)
        {
            if (rule.introductionDay <= currentDay
                && string.Equals(rule.app, app, StringComparison.OrdinalIgnoreCase))
                activeRules.Add(rule);
        }

        return activeRules;
    }

    private static ManualRuleBookData LoadFromResources(string resourcePath)
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
        if (jsonAsset == null)
            throw new InvalidOperationException(
                $"Manual rules resource was not found at Resources/{resourcePath}.json."
            );

        // Mesmo leitor do DataController (Emails/Tickets/Save): Newtonsoft.
        ManualRuleBookData manual;
        try
        {
            manual = JsonConvert.DeserializeObject<ManualRuleBookData>(jsonAsset.text, JsonSettings);
        }
        catch (JsonException exception)
        {
            throw InvalidManual(resourcePath, $"Malformed JSON: {exception.Message}");
        }

        Validate(manual, resourcePath);
        return manual;
    }

    private static void Validate(ManualRuleBookData manual, string resourcePath)
    {
        if (manual == null)
            throw InvalidManual(resourcePath, "The JSON root is missing.");

        if (manual.version != SupportedVersion)
        {
            throw InvalidManual(
                resourcePath,
                $"Unsupported version '{manual.version}'. Expected '{SupportedVersion}'."
            );
        }

        if (manual.rules == null)
            throw InvalidManual(resourcePath, "The rules collection is missing.");

        HashSet<string> ruleIds = new();
        HashSet<string> subruleIds = new();
        for (int ruleIndex = 0; ruleIndex < manual.rules.Count; ruleIndex++)
        {
            ManualRuleData rule = manual.rules[ruleIndex];
            string ruleContext = $"rule at index {ruleIndex}";

            if (rule == null)
                throw InvalidManual(resourcePath, $"{ruleContext} is null.");
            if (string.IsNullOrWhiteSpace(rule.id))
                throw InvalidManual(resourcePath, $"{ruleContext} has no id.");
            if (!ruleIds.Add(rule.id))
                throw InvalidManual(resourcePath, $"Duplicate rule id '{rule.id}'.");
            if (string.IsNullOrWhiteSpace(rule.app) || !SupportedApps.Contains(rule.app))
                throw InvalidManual(
                    resourcePath,
                    $"Rule '{rule.id}' has an unsupported app '{rule.app}'."
                );
            if (rule.introductionDay < 1)
                throw InvalidManual(resourcePath, $"Rule '{rule.id}' has an invalid introductionDay.");
            if (string.IsNullOrWhiteSpace(rule.field))
                throw InvalidManual(resourcePath, $"Rule '{rule.id}' has no field.");
            if (rule.subrules == null || rule.subrules.Count == 0)
                throw InvalidManual(resourcePath, $"Rule '{rule.id}' has no subrules.");

            for (int subruleIndex = 0; subruleIndex < rule.subrules.Count; subruleIndex++)
            {
                ManualSubruleData subrule = rule.subrules[subruleIndex];
                string subruleContext = $"rule '{rule.id}', subrule at index {subruleIndex}";

                if (subrule == null)
                    throw InvalidManual(resourcePath, $"{subruleContext} is null.");
                if (string.IsNullOrWhiteSpace(subrule.id))
                    throw InvalidManual(resourcePath, $"{subruleContext} has no id.");
                if (!subruleIds.Add(subrule.id))
                    throw InvalidManual(resourcePath, $"Duplicate subrule id '{subrule.id}'.");
                if (subrule.check == null)
                    throw InvalidManual(resourcePath, $"{subruleContext} has no check.");
                if (!ManualRuleEvaluator.SupportsCheck(subrule.check.type))
                    throw InvalidManual(
                        resourcePath,
                        $"{subruleContext} uses unsupported check type '{subrule.check.type}'."
                    );
                if (subrule.finding == null)
                    throw InvalidManual(resourcePath, $"{subruleContext} has no finding.");
            }
        }
    }

    private static InvalidOperationException InvalidManual(string resourcePath, string message)
    {
        return new InvalidOperationException(
            $"Invalid manual rules JSON at Resources/{resourcePath}.json: {message}"
        );
    }
}
