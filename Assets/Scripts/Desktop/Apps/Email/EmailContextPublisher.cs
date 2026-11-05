public static class EmailContextPublisher
{
    public const string EmailContextChangedAction = "InspectionEmailContextChanged";

    public static void Publish(SaveData save, EmailData email)
    {
        ActionController.SendAction(
            EmailContextChangedAction,
            new InspectionEmailContext(save, email)
        );
    }
}
