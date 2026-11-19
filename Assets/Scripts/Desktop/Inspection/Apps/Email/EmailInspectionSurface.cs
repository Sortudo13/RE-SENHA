using System.Collections.Generic;

// Superficie de inspecao do Email: expoe os campos do email aberto para as regras do Manual.
// Serve de modelo para os outros apps (ticket, chat, database).
public sealed class EmailInspectionSurface : IInspectionSurface, IInspectionFieldSource
{
    public const string Source = "email";

    private readonly IInspectionFindingStore findingStore;

    public EmailData Email { get; }
    public string SourceType => Source;
    public IReadOnlyCollection<string> Capabilities { get; } =
        new[]
        {
            InspectionFields.Sender,
            InspectionFields.Subject,
            InspectionFields.Body,
            InspectionFields.Attachments,
        };
    public IInspectionFieldSource FieldSource => this;
    public IInspectionFindingStore FindingStore => findingStore;

    public EmailInspectionSurface(
        EmailData email,
        IInspectionFindingStore findingStore = null
    )
    {
        Email = email;
        this.findingStore = findingStore;
    }

    public bool TryGetText(string fieldId, out string value)
    {
        value = null;
        if (Email == null)
            return false;

        if (InspectionFields.Matches(fieldId, InspectionFields.Sender))
            value = Email.sender;
        else if (InspectionFields.Matches(fieldId, InspectionFields.Subject))
            value = Email.subject;
        else if (InspectionFields.Matches(fieldId, InspectionFields.Body))
            value = Email.body;
        else
            return false;

        return true;
    }

    public IReadOnlyList<InspectionAttachmentDescriptor> GetAttachments()
    {
        List<InspectionAttachmentDescriptor> attachments = new();
        if (Email?.attachments == null)
            return attachments;

        foreach (AttachmentData attachment in Email.attachments)
        {
            if (attachment != null)
                attachments.Add(new InspectionAttachmentDescriptor(attachment.extension));
        }

        return attachments;
    }
}
