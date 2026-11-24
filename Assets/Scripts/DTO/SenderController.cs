using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SenderController : MonoBehaviour
{
    void Awake()
    {
        ActionController.SyncSignal("WorkStarted", CreateRoutines);
        ActionController.SyncSignal<string>("TriggerEmail", SendTriggeredEmail);
    }

    void OnDestroy()
    {
        ActionController.UnsyncSignal("WorkStarted", CreateRoutines);
        ActionController.UnsyncSignal<string>("TriggerEmail", SendTriggeredEmail);
    }

    private void CreateRoutines()
    {
        StartCoroutine(GenerateEmailsRoutine());
    }

    private IEnumerator GenerateEmailsRoutine()
    {

        while (true)
        {
            List<EmailData> availableMails = DataController.CurrentSave.availableMails;
            // Emails com triggerId esperam o chat (SendTriggeredEmail).
            EmailData email = availableMails.Find(m => string.IsNullOrEmpty(m.triggerId));
            if (email == null)
                yield break;

            yield return new WaitUntil(() =>
            {
                int currentTotalMinutes = ClockController.CurrentHour * 60 + ClockController.CurrentMinute;
                int targetTotalMinutes = email.hourToReceive * 60 + email.minuteToReceive;

                return currentTotalMinutes >= targetTotalMinutes || !ClockController.IsStillWorking;
            });
            yield return new WaitForSeconds(email.timeToReceive * ClockController.RealSecondsPerGameMinute);

            availableMails = DataController.CurrentSave.availableMails;

            if (!ClockController.IsStillWorking)
                availableMails.RemoveAll(m => m.mailBox == "Company");

            if (!availableMails.Contains(email))
                continue;

            Deliver(email);
        }
    }

    private void SendTriggeredEmail(string triggerId)
    {
        EmailData email = DataController.CurrentSave.availableMails.Find(m => m.triggerId == triggerId);
        if (email != null) // null: ja entregue
            Deliver(email);
    }

    private static void Deliver(EmailData email)
    {
        DataController.CurrentSave.availableMails.Remove(email);

        List<MailBox> mailBoxes = DataController.CurrentSave.currentMailBoxes;
        foreach (MailBox box in mailBoxes)
            if (box.mailBox == email.mailBox)
            {
                box.emails.Insert(0, email);
                box.newEmails++;
                ActionController.SendAction("MailChanged", email.mailBox);
                ActionController.SendAction("ShowNotificationBadge", "Email");
                break;
            }
        DataController.SavingGame();
    }

    //Caso precise ordenar algum dia
    private List<EmailData> OrderList(List<EmailData> emails)
    {
        return emails.OrderBy(email => GetSortTimeInMinutes(email)).ToList();
    }

    private int GetSortTimeInMinutes(EmailData email)
    {
        // E-mails sem horário agendado (-1) ganham prioridade e ficam no topo da fila
        if (email.hourToReceive < 0)
            return -1;

        // Normaliza horários após a meia-noite (ex: 01:00 passa a valer 25h, vindo depois das 23:00)
        int startH = ClockController.StartHour;
        int normalizedHour = email.hourToReceive < startH
            ? email.hourToReceive + 24
            : email.hourToReceive;

        return (normalizedHour * 60) + email.minuteToReceive;
    }
}
