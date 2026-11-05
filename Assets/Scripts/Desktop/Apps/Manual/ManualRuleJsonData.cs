using System;
using System.Collections.Generic;

[Serializable]
public sealed class ManualRuleBookData
{
    public int version;
    public List<ManualRuleData> rules;
}

[Serializable]
public sealed class ManualRuleData
{
    public string id;
    public string title;
    public string description;
    public int introductionDay;
    public string field;
    public List<ManualSubruleData> subrules;
}

[Serializable]
public sealed class ManualSubruleData
{
    public string id;
    public string description;
    public ManualRuleCheckData check;
    public ManualFindingData finding;
}

[Serializable]
public sealed class ManualRuleCheckData
{
    public string type;
    public string[] allowedDomains;
    public string[] terms;
    public string[] forbiddenExtensions;
    public string[] suspiciousDomains;
    public bool caseSensitive;
}

[Serializable]
public sealed class ManualFindingData
{
    public string rule;
    public string evidence;
    public string explanation;
    public string reason;
}
