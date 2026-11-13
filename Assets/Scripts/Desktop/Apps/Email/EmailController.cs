using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailController : MonoBehaviour
{
    [Header("List Config")]
    [SerializeField] private GameObject listItem;

    private EmailData currentEmail = null;
    private MailBox currentBox = null;
    private List<MailBox> mailBoxes = null;
    private GameObject listScreen;
    private GameObject mailScreen;

    // Bolinha de nao lido: filho "UnreadBadge" do item da lista e dos botoes User/Company.
    private const string UnreadBadgeName = "UnreadBadge";

    private void Awake()
    {
        ActionController.SyncSignal<string>("OpenAttachment", OpenAttachment);
        ActionController.SyncSignal("ShowList", ShowList);
        ActionController.SyncSignal<string>("MailChanged", MailChanged);
        ActionController.SyncSignal<EmailData>("RemoveEmail", RemoveEmail);
    }

    private void OnEnable()
    {
        ActionController.SyncSignal<string>("ShowNotificationBadge", OcultBadge);
    }

    private void OnDisable()
    {
        ActionController.UnsyncSignal<string>("ShowNotificationBadge", OcultBadge);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<string>("OpenAttachment", OpenAttachment);
        ActionController.UnsyncSignal("ShowList", ShowList);
        ActionController.UnsyncSignal<string>("MailChanged", MailChanged);
        ActionController.UnsyncSignal<EmailData>("RemoveEmail", RemoveEmail);

        if (currentEmail != null)
            PublishCurrentEmail(null);
    }

    void Start()
    {
        mailScreen = transform.Find("MailScreen").gameObject;
        listScreen = transform.Find("ListScreen").gameObject;

        ShowList();
        LoadEmails();
    }

    private void LoadEmails()
    {
        mailBoxes = DataController.CurrentSave.currentMailBoxes;
        currentBox = mailBoxes.Find(box => box.mailBox == "User");

        Transform userTarget = transform.Find("User");
        if (userTarget != null && userTarget.TryGetComponent(out Button userButton))
        {
            userButton.onClick.RemoveAllListeners();
            userButton.onClick.AddListener(() => UpdateEmailListScreen(mailBoxes.Find(box => box.mailBox == "User")));
            userButton.onClick.AddListener(() => ShowList());
        }

        Transform companyTarget = transform.Find("Company");
        if (companyTarget != null && companyTarget.TryGetComponent(out Button companyButton))
        {
            companyButton.onClick.RemoveAllListeners();
            companyButton.onClick.AddListener(() => UpdateEmailListScreen(mailBoxes.Find(box => box.mailBox == "Company")));
            companyButton.onClick.AddListener(() => ShowList());
        }

        UpdateEmailListScreen(currentBox);
    }

    private void OcultBadge (string appName)
    {
        if (appName == "Email")
            ActionController.SendAction("HideNotificationBadge", "Email");
    }

    private void MailChanged(string mailBox)
    {
        if (currentBox != null && currentBox.mailBox == mailBox)
            UpdateEmailListScreen(currentBox);
        else
            UpdateBoxBadges();
    }

    private static void SetUnreadBadge(Transform target, bool visible)
    {
        Transform badge = target != null ? target.Find(UnreadBadgeName) : null;
        if (badge != null)
            badge.gameObject.SetActive(visible);
    }

    private void UpdateBoxBadges()
    {
        if (mailBoxes == null)
            return;

        foreach (MailBox box in mailBoxes)
            SetUnreadBadge(transform.Find(box.mailBox), box.emails.Exists(email => !email.isRead));
    }

    private void RemoveEmail(EmailData email)
    {
        if (email == null)
            return;

        foreach (MailBox box in mailBoxes)
            if (box.mailBox == email.mailBox && box.emails.Contains(email))
            {
                if (!email.isRead)
                    box.newEmails = Math.Max(0, box.newEmails - 1);
                box.emails.Remove(email);
                DataController.SavingGame();
                ActionController.SendAction("MailChanged", email.mailBox);
                if (email == currentEmail)
                    ShowList();
                break;
            }
    }

    private void UpdateEmailListScreen(MailBox box)
    {
        currentBox = box;

        Transform listContainer = listScreen.transform.Find("ListContainer");
        if (listContainer.childCount > 0)
            foreach (Transform child in listContainer)
                Destroy(child.gameObject);

        foreach (EmailData email in box.emails)
        {
            GameObject newButton = Instantiate(listItem, listContainer);

            if (newButton.TryGetComponent(out Button botao))
            {
                botao.onClick.RemoveAllListeners();
                botao.onClick.AddListener(() => OpenEmail(email));
            }
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = email.subject;
            SetUnreadBadge(newButton.transform, !email.isRead);
        }

        UpdateBoxBadges();
    }

    public void OpenEmail(EmailData email)
    {
        if (currentEmail != email)
            ShowMail(email);
        else
            ShowList();
    }

    private static void PublishCurrentEmail(EmailData email)
    {
        ActionController.SendAction("CurrentEmailChanged", email);
    }

    private void ShowMail(EmailData email)
    {
        currentEmail = email;
        PublishCurrentEmail(email);

        if (!currentEmail.isRead)
        {
            currentBox.newEmails = Math.Max(0, currentBox.newEmails - 1);
            currentEmail.isRead = true;
            DataController.SavingGame();
            // Apaga a bolinha do item e, se for o ultimo nao lido, a da caixa.
            UpdateEmailListScreen(currentBox);
        }


        Transform emailSender = mailScreen.transform.Find("SenderField");
        Transform emailSubject = mailScreen.transform.Find("SubjectField");
        Transform emailBody = mailScreen.transform.Find("BodyField");
        Transform emailAttachments = mailScreen.transform.Find("AttachmentField");
        Transform emailButtons = mailScreen.transform.Find("ButtonField");

        if (emailSender != null)
            emailSender.GetComponentInChildren<TextMeshProUGUI>().text = email.sender;
        if (emailSubject != null)
            emailSubject.GetComponentInChildren<TextMeshProUGUI>().text = email.subject;
        if (emailBody != null)
            emailBody.GetComponentInChildren<TextMeshProUGUI>().text = email.body;
        if (emailAttachments != null)
        {
            TextMeshProUGUI attach = emailAttachments.GetComponentInChildren<TextMeshProUGUI>();
            attach.text = "";
            if (email.attachments.Count > 0)
            {
                foreach (AttachmentData attachment in email.attachments)
                    attach.text += attachment.linkedName + ";\n";
                attach.text = attach.text.TrimEnd(';', '\n');
            }
            else
                attach.text = "Sem anexos";
        }
        if (emailButtons != null)
            emailButtons.gameObject.SetActive(currentBox.mailBox == "Company");

        transform.Find("Back").gameObject.SetActive(true);
        listScreen.SetActive(false);
        mailScreen.SetActive(true);
    }

    private void ShowList()
    {
        if (currentEmail != null)
            PublishCurrentEmail(null);
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
