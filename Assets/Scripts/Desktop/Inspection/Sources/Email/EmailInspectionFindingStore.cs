using System.Collections.Generic;

public sealed class EmailInspectionFindingStore : IInspectionFindingStore
{
    private readonly EmailData email;

    public EmailInspectionFindingStore(EmailData email)
    {
        this.email = email;
    }

    public IReadOnlyList<EvidenceData> Findings =>
        email?.evidences ?? (IReadOnlyList<EvidenceData>)System.Array.Empty<EvidenceData>();

    public bool HasFinding(string findingId)
    {
        if (email?.evidences == null)
            return false;

        return email.evidences.Exists(
            finding =>
                finding != null
                && (finding.findingId == findingId || finding.subruleId == findingId)
        );
    }

    public bool TryRegisterFinding(InspectionEvaluationResult result)
    {
        if (result == null|| email == null)
            return false;

        if (HasFinding(result.FindingId))
            return false;

        email.evidences ??= new List<EvidenceData>();
        email.evidences.Add(
            new EvidenceData
            {
                findingId = result.FindingId,
                sourceType = EmailInspectionSurface.Source,
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
        DataController.SavingGame();
        return true;
    }

}
