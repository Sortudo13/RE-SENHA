using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SenderController : MonoBehaviour
{
    void Awake()
    {
        ActionController.SyncSignal("DayChanged", CreateRoutines);
    }

    void OnDestroy()
    {
        ActionController.UnsyncSignal("DayChanged", CreateRoutines);
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
            if (availableMails.Count <= 0)
                yield break;

            EmailData email = availableMails[0];

            yield return new WaitForSeconds(availableMails[0].timeToReceive);

            availableMails = DataController.CurrentSave.availableMails;
            if (availableMails.Count <= 0)
                yield break;

            if (availableMails[0] != email)
                continue;

            availableMails.RemoveAt(0);

            List<MailBox> mailBoxes = DataController.CurrentSave.currentMailBoxes;
            foreach (MailBox box in mailBoxes)
                if (box.mailBox == email.mailBox)
                {
                    box.emails.Insert(0, email);
                    box.newEmails++;
                    ActionController.SendAction("MailChanged", email.mailBox);
                    break;
                }
            DataController.SavingGame();
        }
    }
}
