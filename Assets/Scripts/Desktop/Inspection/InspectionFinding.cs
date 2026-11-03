using System;

[Serializable]
public sealed class InspectionFinding
{
    public InspectionRuleType RuleType { get; }
    public InspectionField Field { get; }
    public string Rule { get; }
    public string Evidence { get; }
    public string Explanation { get; }
    public string Reason { get; }

    public InspectionFinding(
        InspectionRuleType ruleType,
        InspectionField field,
        string rule,
        string evidence,
        string explanation,
        string reason = ""
    )
    {
        RuleType = ruleType;
        Field = field;
        Rule = rule;
        Evidence = evidence;
        Explanation = explanation;
        Reason = reason;
    }
}
