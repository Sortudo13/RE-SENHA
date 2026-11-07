using System.Collections.Generic;

public sealed class EmailInspectionFindingStore : IInspectionFindingStore
{
    private readonly SaveData save;
    private readonly EmailData email;

    public EmailInspectionFindingStore(SaveData save, EmailData email)
    {
        this.save = save;
        this.email = email;
    }

    public bool HasFinding(string findingId)
    {
        if (email?.inspectionFindings == null)
            return false;

        return email.inspectionFindings.Exists(
            finding =>
                finding != null
                && (finding.findingId == findingId || finding.subruleId == findingId)
        );
    }

    public bool TryRegisterFinding(InspectionEvaluationResult result)
    {
        if (result == null || save == null || email == null)
            return false;

        if (HasFinding(result.FindingId))
            return false;

        email.inspectionFindings ??= new List<InspectionFindingData>();
        email.inspectionFindings.Add(
            new InspectionFindingData
            {
                findingId = result.FindingId,
                sourceType = "email",
                sourceItemId = string.Empty,
                fieldId = result.FieldId,
                evaluatorId = result.EvaluatorId,
                ruleId = result.RuleId,
                rule = result.Title,
                field = result.FieldId,
                evidence = result.Evidence,
                explanation = result.Explanation,
                reason = result.Reason,
            }
        );
        DataController.SaveEmails(save);
        return true;
    }

}
