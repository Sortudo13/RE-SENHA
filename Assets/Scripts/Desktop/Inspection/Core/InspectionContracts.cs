using System;
using System.Collections.Generic;

// Nomes dos campos inspecionaveis. O mesmo nome aparece em tres lugares e precisa bater:
// "field" da regra no ManualRules.json, "field" do InspectionFieldSelectable no prefab
// e Capabilities/TryGetText da superficie. A comparacao ignora maiusculas ("Sender" == "sender").
public static class InspectionFields
{
    public const string Sender = "sender";
    public const string Subject = "subject";
    public const string Body = "body";
    public const string Attachments = "attachments";

    public static bool Matches(string a, string b)
    {
        return !string.IsNullOrWhiteSpace(a)
            && !string.IsNullOrWhiteSpace(b)
            && string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    // Campo com o app dono: Qualify("database", "nome") == "database/nome".
    // Sem fonte devolve so o campo (campo do item em julgamento).
    public static string Qualify(string source, string field)
    {
        return string.IsNullOrWhiteSpace(source) ? field : $"{source.Trim()}/{field}";
    }

    // Inverso de Qualify: "database/nome" -> ("database", "nome"); "Sender" -> (null, "Sender").
    public static void Split(string qualifiedField, out string source, out string field)
    {
        int separator = qualifiedField?.IndexOf('/') ?? -1;
        source = separator > 0 ? qualifiedField[..separator] : null;
        field = separator > 0 ? qualifiedField[(separator + 1)..] : qualifiedField;
    }

    public static bool Contains(IEnumerable<string> fields, string field)
    {
        if (fields == null)
            return false;

        foreach (string candidate in fields)
        {
            if (Matches(candidate, field))
                return true;
        }

        return false;
    }
}

// Um item inspecionavel de um app (o email aberto, o ticket aberto...). O app cria a sua
// superficie e avisa o Inspector com InspectionEvaluationController.PublishSurface.
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
    // Achados ja registrados no item inspecionado (o alerta de achados le daqui).
    IReadOnlyList<EvidenceData> Findings { get; }
    bool HasFinding(string findingId);
    bool TryRegisterFinding(InspectionEvaluationResult result);
}
