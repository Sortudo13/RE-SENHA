using System.Collections.Generic;

public sealed class EmailInspectionSurface : IInspectionSurface, IInspectionFieldSource
{
    private static readonly IReadOnlyList<InspectionFieldDescriptor> FieldDescriptors =
        new List<InspectionFieldDescriptor>
        {
            new("Sender", "Remetente", "Sender"),
            new("Subject", "Assunto", "Subject"),
            new("Body", "Corpo", "Body"),
            new("Attachments", "Anexos", "Attachments"),
        };

    private readonly EmailData email;
    private readonly IInspectionFindingStore findingStore;

    public string SourceType => "email";
    public string SourceItemId => string.Empty;
    public IReadOnlyList<InspectionFieldDescriptor> Fields => FieldDescriptors;
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

        switch (fieldId)
        {
            case "sender":
            case "email.sender":
                value = email.sender;
                return true;
            case "subject":
            case "email.subject":
                value = email.subject;
                return true;
            case "body":
            case "email.body":
                value = email.body;
                return true;
            default:
                return false;
        }
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
