using System;

[Serializable]
public sealed class InspectionRuleSelection
{
    public string subruleId;

    public InspectionRuleSelection(string id)
    {
        subruleId = id;
    }
}
