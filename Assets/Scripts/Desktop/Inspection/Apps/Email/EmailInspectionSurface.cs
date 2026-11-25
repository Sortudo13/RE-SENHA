using System.Collections.Generic;

// Superficie de inspecao do Email: expoe os campos do email aberto para as regras do Manual.
// Serve de modelo para os outros apps (ticket, chat, database).
public sealed class EmailInspectionSurface
    : IInspectionSurface,
        IInspectionFieldSource,
        IInspectionThreadSource
{
    public const string Source = "email";
    private const string UserMailBox = "User";

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

    // Assuntos das conversas que o jogador ja tem: emails que ja chegaram na caixa User e
    // emails anexados a este. Lido na hora da checagem (algo pode chegar com o email aberto).
    public IReadOnlyCollection<string> PreviousSubjects
    {
        get
        {
            List<string> subjects = new();

            MailBox userBox = DataController.CurrentSave?.currentMailBoxes?.Find(
                box => box.mailBox == UserMailBox
            );
            if (userBox?.emails != null)
                foreach (EmailData email in userBox.emails)
                    if (email != null && email != Email)
                        subjects.Add(email.subject);

            if (Email?.attachments != null)
                foreach (AttachmentData attachment in Email.attachments)
                    if (attachment?.data is EmailData attached)
                        subjects.Add(attached.subject);

            return subjects;
        }
    }
}
