using System;
using System.Collections.Generic;

public sealed class EmailInspector
{
    public IReadOnlyList<InspectionFinding> Inspect(EmailData email)
    {
        var findings = new List<InspectionFinding>();
        if (email == null)
            return findings;

        AddErrorFindings(email.error, email.reason, findings);

        return findings;
    }

    private static void AddErrorFindings(
        EmailError error,
        string reason,
        List<InspectionFinding> findings
    )
    {
        if (error == EmailError.None)
            return;

        InspectionField field;
        string rule;
        string explanation;
        switch (error)
        {
            case EmailError.SenderMismatch:
                field = InspectionField.Sender;
                rule = "Verificação do remetente";
                explanation = "O remetente foi identificado como divergente.";
                break;
            case EmailError.SubjectMismatch:
                field = InspectionField.Subject;
                rule = "Verificação do assunto";
                explanation = "O assunto foi identificado como divergente.";
                break;
            case EmailError.BodyMismatch:
                field = InspectionField.Body;
                rule = "Verificação do corpo";
                explanation = "O corpo foi identificado como divergente.";
                break;
            case EmailError.AttachmentMismatch:
                field = InspectionField.Attachments;
                rule = "Verificação dos anexos";
                explanation = "Um anexo foi identificado como divergente.";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(error), error, null);
        }

        findings.Add(
            new InspectionFinding(
                ToRuleType(error),
                field,
                rule,
                error.ToString(),
                explanation,
                reason
            )
        );
    }

    private static InspectionRuleType ToRuleType(EmailError error)
    {
        return error switch
        {
            EmailError.SenderMismatch => InspectionRuleType.SenderDomain,
            EmailError.SubjectMismatch => InspectionRuleType.SuspiciousWords,
            EmailError.BodyMismatch => InspectionRuleType.SuspiciousWords,
            EmailError.AttachmentMismatch => InspectionRuleType.Attachments,
            EmailError.None => throw new ArgumentException("None is not an inspection finding.", nameof(error)),
            _ => throw new ArgumentOutOfRangeException(nameof(error), error, null),
        };
    }
}
