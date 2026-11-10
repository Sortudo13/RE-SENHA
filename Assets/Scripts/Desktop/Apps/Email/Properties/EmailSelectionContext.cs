public sealed class EmailSelectionContext
{
    public SaveData Save { get; }
    public EmailData Email { get; }

    public EmailSelectionContext(SaveData save, EmailData email)
    {
        Save = save;
        Email = email;
    }
}
