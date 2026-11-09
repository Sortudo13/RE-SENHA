using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailController : MonoBehaviour
{
    private EmailData currentEmail = null;
    private SaveData currentSave = null;
    private List<EmailData> inbox = null;
    private Transform listBar;

    [Header("List Config")]
    [SerializeField] private GameObject listButton;

    private GameObject emailInterface;
    private GameObject emailSender;
    private GameObject emailSubject;
    private GameObject emailBody;
    private GameObject emailAttachments;

    private void Awake()
    {
        ActionController.SyncSignal<string>("OpenAttachment", OpenAttachment);
        ActionController.SyncSignal<EmailData>("AddEmail", AddEmail);
        ActionController.SyncSignal<EmailData>("RemoveEmail", RemoveEmail);
        ActionController.SyncSignal("ReloadInbox", ReloadInbox);
        ActionController.SyncSignal("CloseEmailWindow", CloseEmailWindow);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<string>("OpenAttachment", OpenAttachment);
        ActionController.UnsyncSignal<EmailData>("AddEmail", AddEmail);
        ActionController.UnsyncSignal<EmailData>("RemoveEmail", RemoveEmail);
        ActionController.UnsyncSignal("ReloadInbox", ReloadInbox);
        ActionController.UnsyncSignal("CloseEmailWindow", CloseEmailWindow);
        ActionController.SendAction("CurrentEmailChanged", new EmailSelectionContext(null, null));
    }

    void Start()
    {
        emailInterface = transform.GetChild(0).gameObject;
        listBar = transform.GetChild(1).GetChild(0);

        emailSender = emailInterface.transform.GetChild(1).gameObject;
        emailSubject = emailInterface.transform.GetChild(3).gameObject;
        emailAttachments = emailInterface.transform.GetChild(5).gameObject;
        emailBody = emailInterface.transform.GetChild(6).gameObject;

        foreach (Transform child in emailInterface.transform)
        {
            child.gameObject.SetActive(false);
        }

        currentSave = DataController.LoadEmails();
        inbox = currentSave.emails;
        UpdateEmailSidebar();
    }

    private void AddEmail(EmailData newEmail)
    {
        if (newEmail == null)
            return;

        inbox.Add(newEmail);
        DataController.SaveEmails(currentSave);
        UpdateEmailSidebar();
    }

    private void RemoveEmail(EmailData email)
    {
        if (email == null)
            return;

        if (inbox.Contains(email))
        {
            inbox.Remove(email);
            DataController.SaveEmails(currentSave);
            UpdateEmailSidebar();
            if (email == currentEmail)
                LoadEmail(currentEmail);
        }
    }

    public void UpdateEmailSidebar()
    {
        if (listBar.childCount > 0)
        {
            foreach (Transform child in listBar)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (EmailData email in inbox)
        {
            GameObject newButton = Instantiate(listButton, listBar);

            if (newButton.TryGetComponent(out Button botao))
            {
                botao.onClick.RemoveAllListeners();
                botao.onClick.AddListener(() => LoadEmail(email));
            }

            newButton.GetComponentInChildren<TextMeshProUGUI>().text = email.subject;

        }
    }

    public void LoadEmail(EmailData email)
    {
        if (currentEmail != email)
            currentEmail = email;
        else
            currentEmail = null;

        ActionController.SendAction("CurrentEmailChanged", new EmailSelectionContext(currentSave, currentEmail));

        if (currentEmail != null)
        {
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
                    {
                        attach.text += attachment.linkedName + ", ";
                    }
                    attach.text = attach.text.TrimEnd(',', ' ');
                }
                else
                {
                    attach.text = "Sem anexos";
                }
            }
            foreach (Transform child in emailInterface.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (Transform child in emailInterface.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private void OpenAttachment(string position)
    {
        if (int.TryParse(position, out int numero))
        {
            string command = $"Init{currentEmail.attachments[numero - 1].documentType}";
            ActionController.SendAction(command, currentEmail.attachments[numero - 1].data);
        }
        else
        {
            Debug.Log("Linked attachment's position not a number.");
        }
    }

    private void ReloadInbox()
    {
        currentSave = DataController.LoadEmails();
        inbox = currentSave.emails;
        UpdateEmailSidebar();
    }

    private void CloseEmailWindow()
    {
        transform.parent.GetComponentInParent<WindowController>().CloseWindow();
    }
}
