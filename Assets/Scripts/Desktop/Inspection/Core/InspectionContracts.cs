using System;
using System.Collections.Generic;

public interface IInspectionSurface
{
    string SourceType { get; }
    string SourceItemId { get; }
    IReadOnlyList<InspectionFieldDescriptor> Fields { get; }
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

public sealed class InspectionFieldDescriptor
{
    public string FieldId { get; }
    public string DisplayName { get; }
    public string SelectionId { get; }
    public bool IsSelectable { get; }

    public InspectionFieldDescriptor(
        string fieldId,
        string displayName,
        string selectionId,
        bool isSelectable = true
    )
    {
        FieldId = fieldId;
        DisplayName = displayName;
        SelectionId = selectionId;
        IsSelectable = isSelectable;
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

public class InspectionEvaluationResult
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

public interface IInspectionEvaluator
{
    string EvaluatorId { get; }
    IReadOnlyCollection<string> RequiredCapabilities { get; }

    IReadOnlyList<InspectionEvaluationResult> Evaluate(IInspectionSurface surface);
}

public interface IInspectionFindingStore
{
    bool HasFinding(string findingId);
    bool TryRegisterFinding(InspectionEvaluationResult result);
}
