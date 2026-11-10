using System.Collections.Generic;

public sealed class EmailInspectionSurface : IInspectionSurface, IInspectionFieldSource
{
    private readonly EmailData email;
    private readonly IInspectionFindingStore findingStore;

    public string SourceType => "email";
    public IReadOnlyCollection<string> Capabilities { get; } =
        new HashSet<string>
        {
            "sender",
            "subject",
            "body",
            "attachments",
        };
    public IInspectionFieldSource FieldSource => this;
    public IInspectionFindingStore FindingStore => findingStore;

    public EmailInspectionSurface(
        EmailData email,
        IInspectionFindingStore findingStore = null
    )
    {
        this.email = email;
        this.findingStore = findingStore;
    }

    public bool TryGetText(string fieldId, out string value)
    {
        value = null;
        if (email == null)
            return false;

        value = fieldId switch
        {
            "sender" => email.sender,
            "subject" => email.subject,
            "body" => email.body,
            _ => null,
        };
        return fieldId is "sender" or "subject" or "body";
    }

    public IReadOnlyList<InspectionAttachmentDescriptor> GetAttachments()
    {
        List<InspectionAttachmentDescriptor> attachments = new();
        if (email?.attachments == null)
            return attachments;

        foreach (AttachmentData attachment in email.attachments)
        {
            if (attachment != null)
                attachments.Add(new InspectionAttachmentDescriptor(attachment.extension));
        }

        return attachments;
    }
}
