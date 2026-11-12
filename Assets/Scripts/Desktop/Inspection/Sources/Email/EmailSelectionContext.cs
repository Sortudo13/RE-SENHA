public sealed class EmailSelectionContext
{
    public EmailData Email { get; }

    public EmailSelectionContext(EmailData email)
    {
        Email = email;
    }
}
