using System;
using System.Collections.Generic;

// Instancia unica do Manual, compartilhada pela janela do Manual e pelo avaliador do Inspector.
public static class ManualRepository
{
    private static ManualRuleBookData manual;

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
