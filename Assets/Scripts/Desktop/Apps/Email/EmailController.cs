using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailController : MonoBehaviour
{
    [Header("List Config")]
    [SerializeField] private GameObject listButton;

    private EmailData currentEmail = null;
    private SaveData currentSave = null;
    private List<EmailData> inbox = null;
    private GameObject listScreen;
    private GameObject mailScreen;

    private void Awake()
    {
        ActionController.SyncSignal<string>("OpenAttachment", OpenAttachment);
        ActionController.SyncSignal<EmailData>("AddEmail", AddEmail);
        ActionController.SyncSignal<EmailData>("RemoveEmail", RemoveEmail);
        ActionController.SyncSignal("ShowList", ShowList);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<string>("OpenAttachment", OpenAttachment);
        ActionController.UnsyncSignal<EmailData>("AddEmail", AddEmail);
        ActionController.UnsyncSignal<EmailData>("RemoveEmail", RemoveEmail);
        ActionController.UnsyncSignal("ShowList", ShowList);
    }

    void Start()
    {
        mailScreen = transform.Find("MailScreen").gameObject;
        listScreen = transform.Find("ListScreen").gameObject;

        ShowList();

        currentSave = DataController.LoadEmails();
        inbox = currentSave.emails;

        UpdateEmailListScreen();
    }

    private void AddEmail(EmailData newEmail)
    {
        if (newEmail == null)
            return;

        inbox.Add(newEmail);
        DataController.SaveEmails(currentSave);
        UpdateEmailListScreen();
    }

    private void RemoveEmail(EmailData email)
    {
        if (email == null)
            return;

        if (inbox.Contains(email))
        {
            inbox.Remove(email);
            DataController.SaveEmails(currentSave);
            UpdateEmailListScreen();
            if (email == currentEmail)
                OpenEmail(currentEmail);
        }
    }

    public void UpdateEmailListScreen()
    {
        Transform listContainer = listScreen.transform.Find("ListContainer");
        if (listContainer.childCount > 0)
            foreach (Transform child in listContainer)
                Destroy(child.gameObject);

        foreach (EmailData email in inbox)
        {
            GameObject newButton = Instantiate(listButton, listContainer);

            if (newButton.TryGetComponent(out Button botao))
            {
                botao.onClick.RemoveAllListeners();
                botao.onClick.AddListener(() => OpenEmail(email));
            }

            newButton.GetComponentInChildren<TextMeshProUGUI>().text = email.subject;

        }
    }

    public void OpenEmail(EmailData email)
    {
        if (currentEmail != email)
        {
            currentEmail = email;
            ShowMail(currentEmail);
        }
        else
            ShowList();

        ActionController.SendAction("CurrentEmailChanged", new EmailSelectionContext(currentSave, currentEmail));
    }

    private void ShowMail(EmailData currentEmail)
    {
        Transform emailSender = mailScreen.transform.Find("SenderField");
        Transform emailSubject = mailScreen.transform.Find("SubjectField");
        Transform emailBody = mailScreen.transform.Find("BodyField");
        Transform emailAttachments = mailScreen.transform.Find("AttachmentField");

        if (emailSender != null)
            emailSender.GetComponentInChildren<TextMeshProUGUI>().text = currentEmail.sender;
        if (emailSubject != null)
            emailSubject.GetComponentInChildren<TextMeshProUGUI>().text = currentEmail.subject;
        if (emailBody != null)
            emailBody.GetComponentInChildren<TextMeshProUGUI>().text = currentEmail.body;
        if (emailAttachments != null)
        {
            TextMeshProUGUI attach = emailAttachments.GetComponentInChildren<TextMeshProUGUI>();
            attach.text = "";
            if (currentEmail.attachments.Count > 0)
            {
                foreach (AttachmentData attachment in currentEmail.attachments)
                    attach.text += attachment.linkedName + ";\n";
                attach.text = attach.text.TrimEnd(';', '\n');
            }
            else
                attach.text = "Sem anexos";
        }

        transform.Find("Back").gameObject.SetActive(true);
        listScreen.SetActive(false);
        mailScreen.SetActive(true);
    }

    private void ShowList()
    {
        currentEmail = null;
        transform.Find("Back").gameObject.SetActive(false);
        mailScreen.SetActive(false);
        listScreen.SetActive(true);
    }

    private void OpenAttachment(string position)
    {
        if (int.TryParse(position, out int numero))
        {
            string command = $"Init{currentEmail.attachments[numero - 1].documentType}";
            ActionController.SendAction(command, currentEmail.attachments[numero - 1].data);
        }
    }
}
