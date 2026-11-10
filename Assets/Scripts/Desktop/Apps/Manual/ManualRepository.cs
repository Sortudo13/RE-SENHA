using System;
using System.Collections.Generic;

public static class ManualRepository
{
    private static readonly Dictionary<string, ManualRuleBookData> cache = new();

    public static ManualRuleBookData Load(
        string resourcePath = ManualRulesLoader.DefaultResourcePath
    )
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
            throw new ArgumentException("Manual rules resource path is required.", nameof(resourcePath));

        if (cache.TryGetValue(resourcePath, out ManualRuleBookData manual))
            return manual;

        manual = ManualRulesLoader.Load(resourcePath);
        cache.Add(resourcePath, manual);
        return manual;
    }

    public static IReadOnlyList<ManualRuleData> GetActiveRules(
        string resourcePath,
        int currentDay
    )
    {
        return GetActiveRules(Load(resourcePath), currentDay);
    }

    public static IReadOnlyList<ManualRuleData> GetActiveRules(
        ManualRuleBookData manual,
        int currentDay
    )
    {
        if (manual == null)
            throw new ArgumentNullException(nameof(manual));
        if (currentDay < 1)
            throw new ArgumentOutOfRangeException(nameof(currentDay));
        if (manual.rules == null)
            throw new InvalidOperationException("Manual rules collection is missing.");

        List<ManualRuleData> activeRules = new();
        foreach (ManualRuleData rule in manual.rules)
        {
            if (rule.introductionDay <= currentDay)
                activeRules.Add(rule);
        }

        return activeRules;
    }

    public static IReadOnlyList<ManualRuleData> GetActiveRules(
        ManualRuleBookData manual,
        int currentDay,
        string app
    )
    {
        List<ManualRuleData> appRules = new();
        foreach (ManualRuleData rule in GetActiveRules(manual, currentDay))
        {
            if (string.Equals(rule.app, app, StringComparison.OrdinalIgnoreCase))
                appRules.Add(rule);
        }

        return appRules;
    }

    public static void Clear()
    {
        cache.Clear();
    }
}
