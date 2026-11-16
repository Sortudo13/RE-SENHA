using System;
using System.Collections.Generic;

// Instancia unica do Manual, compartilhada pela janela do Manual e pelo avaliador do Inspector.
public static class ManualRepository
{
    private static ManualRuleBookData manual;

    // sobrevivem entre Plays. Limpar aqui faz cada Play reler o ManualRules.json atual.
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache()
    {
        manual = null;
    }

    public static ManualRuleBookData Load()
    {
        return manual ??= ManualRulesLoader.Load();
    }

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
}
