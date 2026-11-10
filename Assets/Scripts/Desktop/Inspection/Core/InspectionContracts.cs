using System.Collections.Generic;

public interface IInspectionSurface
{
    string SourceType { get; }
    IReadOnlyCollection<string> Capabilities { get; }
    IInspectionFieldSource FieldSource { get; }
    IInspectionFindingStore FindingStore { get; }
}

public interface IInspectionFieldSource
{
    bool TryGetText(string fieldId, out string value);
    IReadOnlyList<InspectionAttachmentDescriptor> GetAttachments();
}

public sealed class InspectionAttachmentDescriptor
{
    public string Extension { get; }

    public InspectionAttachmentDescriptor(string extension)
    {
        Extension = extension;
    }
}

public sealed class InspectionSelection
{
    public string FieldId { get; }
    public string SelectionId { get; }

    public InspectionSelection(string fieldId, string selectionId)
    {
        FieldId = fieldId;
        SelectionId = selectionId;
    }
}

public sealed class InspectionEvaluationResult
{
    public string FindingId { get; }
    public string EvaluatorId { get; }
    public string FieldId { get; }
    public string RuleId { get; }
    public string Title { get; }
    public string Evidence { get; }
    public string Explanation { get; }
    public string Reason { get; }

    public InspectionEvaluationResult(
        string findingId,
        string evaluatorId,
        string fieldId,
        string ruleId,
        string title,
        string evidence,
        string explanation,
        string reason
    )
    {
        FindingId = findingId;
        EvaluatorId = evaluatorId;
        FieldId = fieldId;
        RuleId = ruleId;
        Title = title;
        Evidence = evidence;
        Explanation = explanation;
        Reason = reason;
    }
}

public interface IInspectionFindingStore
{
    bool HasFinding(string findingId);
    bool TryRegisterFinding(InspectionEvaluationResult result);
}
