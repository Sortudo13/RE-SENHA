using System;
using System.Collections.Generic;
using UnityEngine;

public static class ManualRulesLoader
{
    public const string DefaultResourcePath = "Manual/ManualRules";
    public const int SupportedVersion = 1;

    private static readonly HashSet<string> SupportedCheckTypes = new()
    {
        "SenderDomain",
        "SubjectTerms",
        "BodyTerms",
        "AttachmentExtension",
    };

    private static readonly HashSet<string> SupportedApps = new()
    {
        "email",
        "ticket",
        "chat",
    };

    public static ManualRuleBookData Load(
        string resourcePath = DefaultResourcePath
    )
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
            throw new ArgumentException("Manual rules resource path is required.", nameof(resourcePath));

        TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
        if (jsonAsset == null)
            throw new InvalidOperationException(
                $"Manual rules resource was not found at Resources/{resourcePath}.json."
            );

        ManualRuleBookData manual = JsonUtility.FromJson<ManualRuleBookData>(jsonAsset.text);
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
                if (!SupportedCheckTypes.Contains(subrule.check.type))
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
