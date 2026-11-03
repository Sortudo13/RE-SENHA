using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EmailController : MonoBehaviour, IInspectionMatchTarget
{
    private EmailData currentEmail;
    private SaveData currentSave;
    private List<EmailData> inbox;

    [Header("List Config")]
    private Transform listBar;

    [SerializeField]
    private GameObject listButton;

    [Header("Alert Window")]
    [SerializeField]
    private GameObject alertWindow;

    [SerializeField]
    private GameObject inspectionAlert;

    [SerializeField]
    private AudioClip approveClickSound;

    [Header("Email Actions")]
    [SerializeField]
    private Button approveButton;

    [SerializeField]
    private Button blockButton;

    [SerializeField]
    private Button inspectRulesButton;

    private readonly EmailInspector emailInspector = new();
    private static readonly Dictionary<string, List<InspectionFinding>> findingsByEmail = new();
    private IReadOnlyList<InspectionFinding> currentFindings = new List<InspectionFinding>();
    private GameObject emailInterface;
    private GameObject emailSender;
    private GameObject emailSubject;
    private GameObject emailBody;
    private GameObject emailAttachments;
    private GameObject emailReport;

    private void Awake()
    {
        ActionController.SyncSignal<string>("OpenAttachment", OpenAttachment);
        InspectionModeController.ActiveStateChanged += HandleInspectionStateChanged;
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<string>("OpenAttachment", OpenAttachment);
        InspectionModeController.ActiveStateChanged -= HandleInspectionStateChanged;
        approveButton?.onClick.RemoveListener(ApproveEmail);
        blockButton?.onClick.RemoveListener(BlockEmail);
        inspectRulesButton?.onClick.RemoveListener(ShowInspectionRules);
    }

    private void Start()
    {
        emailInterface = transform.GetChild(0).gameObject;
        listBar = transform.GetChild(1).GetChild(0);

        emailSender = emailInterface.transform.GetChild(1).gameObject;
        emailSubject = emailInterface.transform.GetChild(3).gameObject;
        emailAttachments = emailInterface.transform.GetChild(5).gameObject;
        emailBody = emailInterface.transform.GetChild(6).gameObject;
        emailReport = emailInterface.transform.GetChild(7).gameObject;

        ConfigureInspectionFields();

        foreach (Transform child in emailInterface.transform)
            child.gameObject.SetActive(false);

        ConfigureActionButtons();
        currentSave = DataController.LoadEmails();
        inbox = currentSave.emails;
        UpdateEmailSidebar();
    }

    public void AddEmail(EmailData newEmail)
    {
        if (newEmail == null)
            return;

        inbox.Add(newEmail);
        DataController.SaveEmails(currentSave);
        UpdateEmailSidebar();
    }

    public void RemoveEmail(EmailData email)
    {
        if (email == null || !inbox.Contains(email))
            return;

        inbox.Remove(email);
        findingsByEmail.Remove(GetEmailKey(email));
        DataController.SaveEmails(currentSave);
        UpdateEmailSidebar();
        if (email == currentEmail)
            LoadEmail(currentEmail);
    }

    public void UpdateEmailSidebar()
    {
        foreach (Transform child in listBar)
            Destroy(child.gameObject);

        foreach (EmailData email in inbox)
        {
            GameObject newButton = Instantiate(listButton, listBar);
            GameObject newText = new("Texto");
            newText.transform.SetParent(newButton.transform, false);

            TextMeshProUGUI text = newText.AddComponent<TextMeshProUGUI>();
            text.text = email.subject;
            text.enableAutoSizing = true;
            text.fontSizeMin = 30;
            text.fontSizeMax = 90;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;

            RectTransform rect = newText.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            if (newButton.TryGetComponent(out Button button))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => LoadEmail(email));
            }
        }
    }

    public void LoadEmail(EmailData email)
    {
        if (email == null)
            return;

        if (currentEmail == email)
        {
            currentEmail = null;
            currentFindings = new List<InspectionFinding>();
            UpdateInspectionButton();
            foreach (Transform child in emailInterface.transform)
                child.gameObject.SetActive(false);
            return;
        }

        currentEmail = email;
        string emailKey = GetEmailKey(currentEmail);
        currentFindings = findingsByEmail.TryGetValue(emailKey, out List<InspectionFinding> findings)
            ? findings
            : new List<InspectionFinding>();
        UpdateInspectionButton();

        emailSender.GetComponentInChildren<TextMeshProUGUI>().text = currentEmail.sender;
        emailSubject.GetComponentInChildren<TextMeshProUGUI>().text = currentEmail.subject;
        emailBody.GetComponentInChildren<TextMeshProUGUI>().text = currentEmail.body;

        TextMeshProUGUI attachmentText =
            emailAttachments.GetComponentInChildren<TextMeshProUGUI>();
        attachmentText.text = currentEmail.attachments != null && currentEmail.attachments.Count > 0
            ? string.Join(", ", currentEmail.attachments.ConvertAll(attachment => attachment.linkedName))
            : "Sem anexos";

        foreach (Transform child in emailInterface.transform)
            child.gameObject.SetActive(true);
    }

    private void OpenAttachment(string position)
    {
        if (!int.TryParse(position, out int number))
        {
            Debug.LogError("Linked attachment's position is not a number.");
            return;
        }

        if (
            currentEmail?.attachments == null
            || number < 1
            || number > currentEmail.attachments.Count
        )
        {
            Debug.LogError("Linked attachment's position is outside the current email.");
            return;
        }

        AttachmentData attachment = currentEmail.attachments[number - 1];
        ActionController.SendAction($"Init{attachment.documentType}", attachment.data);
    }

    private void ConfigureActionButtons()
    {
        approveButton ??= FindActionButton("ApproveButton", "SubmitButton");
        blockButton ??= FindActionButton("BlockButton");
        inspectRulesButton ??= FindActionButton("InspectRulesButton");

        if (approveButton != null)
            approveButton.onClick.AddListener(ApproveEmail);
        if (blockButton != null)
            blockButton.onClick.AddListener(BlockEmail);
        if (inspectRulesButton != null)
            inspectRulesButton.onClick.AddListener(ShowInspectionRules);

        if (approveButton == null || blockButton == null || inspectRulesButton == null)
        {
            Debug.LogError(
                "Email prefab must contain ApproveButton, BlockButton and InspectRulesButton."
            );
        }

        UpdateInspectionButton();
    }

    private Button FindActionButton(params string[] names)
    {
        foreach (string name in names)
        {
            Transform buttonTransform = emailReport.transform.Find(name);
            if (buttonTransform != null && buttonTransform.TryGetComponent(out Button button))
                return button;
        }

        return null;
    }

    public void UpdateInspectionButton()
    {
        if (inspectRulesButton != null)
            inspectRulesButton.interactable = currentFindings.Count > 0;
    }

    private void HandleInspectionStateChanged(bool isActive)
    {
        if (!isActive)
            UpdateInspectionButton();
    }

    private void ConfigureInspectionFields()
    {
        AddInspectionField(emailSender, InspectionField.Sender);
        AddInspectionField(emailSubject, InspectionField.Subject);
        AddInspectionField(emailBody, InspectionField.Body);
        AddInspectionField(emailAttachments, InspectionField.Attachments);
    }

    private static void AddInspectionField(GameObject fieldObject, InspectionField field)
    {
        InspectionFieldSelectable selectable = fieldObject.GetComponent<InspectionFieldSelectable>();
        if (selectable == null)
            selectable = fieldObject.AddComponent<InspectionFieldSelectable>();
        selectable.Initialize(field);
    }

    public bool TryRegisterInspectionMatch(
        InspectionField field,
        RegraData rule,
        SubregraData subrule
    )
    {
        if (currentEmail == null || rule == null || subrule == null)
            return false;

        if (field != rule.campo)
        {
            Debug.Log("O campo selecionado não corresponde à regra selecionada.");
            return false;
        }

        EmailError expectedError = field switch
        {
            InspectionField.Sender => EmailError.SenderMismatch,
            InspectionField.Subject => EmailError.SubjectMismatch,
            InspectionField.Body or InspectionField.Links => EmailError.BodyMismatch,
            InspectionField.Attachments => EmailError.AttachmentMismatch,
            _ => EmailError.None
        };
        if (currentEmail.error != expectedError || currentEmail.subregra != subrule.idSubregra)
        {
            Debug.Log(
                $"A regra selecionada não foi quebrada por este email. " +
                $"Campo: {field}; erro do email: {currentEmail.error}; " +
                $"subregra do email: {currentEmail.subregra}; " +
                $"regra selecionada: {rule.titulo} / {subrule.idSubregra}."
            );
            return false;
        }

        InspectionFinding finding = emailInspector.Inspect(currentEmail)
            .FirstOrDefault(item => item.Field == field);
        if (finding == null)
        {
            Debug.Log(
                $"A regra selecionada não foi quebrada por este email. " +
                $"Campo: {field}; erro do email: {currentEmail.error}; regra: {rule.titulo}."
            );
            return false;
        }

        string emailKey = GetEmailKey(currentEmail);
        if (!findingsByEmail.TryGetValue(emailKey, out List<InspectionFinding> findings))
        {
            findings = new List<InspectionFinding>();
            findingsByEmail[emailKey] = findings;
        }

        if (!findings.Exists(item => item.Field == finding.Field))
            findings.Add(finding);

        currentFindings = findings;
        UpdateInspectionButton();
        return true;
    }

    private static string GetEmailKey(EmailData email)
    {
        StringBuilder key = new();
        key.Append(email.sender).Append('\u001F');
        key.Append(email.subject).Append('\u001F');
        key.Append(email.body).Append('\u001F');
        key.Append((int)email.error).Append('\u001F');
        key.Append(email.reason).Append('\u001F');
        key.Append(email.subregra).Append('\u001F');

        if (email.attachments != null)
        {
            foreach (AttachmentData attachment in email.attachments)
            {
                key.Append((int)attachment.documentType)
                    .Append(':')
                    .Append(attachment.name)
                    .Append(':')
                    .Append(attachment.extension)
                    .Append(':')
                    .Append(attachment.linkedName)
                    .Append('\u001E');
            }
        }

        return key.ToString();
    }

    public void ApproveEmail()
    {
        if (approveClickSound != null && AudioManager.Instance != null)
            AudioManager.Instance.TocarSFX(approveClickSound);
        ValidateDecision(false);
    }

    public void BlockEmail()
    {
        ValidateDecision(true);
    }

    public void ShowInspectionRules()
    {
        if (currentFindings.Count == 0 || inspectionAlert == null)
            return;

        GameObject alert = AlertManagerController.ShowAlert(inspectionAlert);
        if (alert != null && alert.TryGetComponent(out EmailInspectionAlertController controller))
            controller.Initialize(currentFindings);
    }

    private void ValidateDecision(bool playerBlockedEmail)
    {
        if (currentEmail == null)
            return;

        bool emailIsUnsafe = currentEmail.error != EmailError.None;
        bool correctDecision = playerBlockedEmail == emailIsUnsafe;
        string feedbackMessage = correctDecision
            ? emailIsUnsafe
                ? "Decisão correta! O email suspeito foi bloqueado."
                : "Decisão correta! O email seguro foi aprovado."
            : emailIsUnsafe
                ? "Erro de análise! Este email suspeito deveria ter sido bloqueado."
                : "Erro de análise! Este email era seguro e deveria ter sido aprovado.";

        if (!correctDecision)
            DataController.CurrentErrorCount++;

        ShowFeedback(correctDecision, feedbackMessage);
        RemoveEmail(currentEmail);

        if (
            (inbox.Count == 0 && DataController.CurrentDay >= DataController.MaxDay)
            || DataController.CurrentErrorCount >= DataController.MaxErrors
        )
        {
            transform.parent.GetComponentInParent<WindowController>().CloseWindow();
            string endingScene = inbox.Count == 0 ? "GameWin" : "Gameover";
            DataController.ResetDays();
            SceneManager.LoadScene(endingScene);
        }

        if (inbox.Count == 0)
        {
            DataController.AdvanceDay();
            currentSave = DataController.LoadEmails();
            inbox = currentSave.emails;
            UpdateEmailSidebar();
        }
    }

    public void ValidateAnswer()
    {
        ApproveEmail();
    }

    public void ShowFeedback(bool correct, string message)
    {
        GameObject connectedAlert = AlertManagerController.ShowAlert(alertWindow);
        if (connectedAlert == null)
            return;

        GameObject content = connectedAlert.transform.GetChild(0).gameObject;
        TextMeshProUGUI title = content.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI feedback = content.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        connectedAlert.SetActive(false);

        if (content.transform.GetChild(2).TryGetComponent(out Button button))
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => AlertManagerController.CloseAlert(connectedAlert));
        }

        feedback.text = message;
        if (!correct)
        {
            title.text = "[ALERTA DE SEGURANÇA]";
            title.color = new Color(150 / 255f, 58 / 255f, 27 / 255f);
            feedback.text += DataController.CurrentErrorCount < DataController.MaxErrors
                ? $"\n\nREGISTRO: Você possui {DataController.CurrentErrorCount}/{DataController.MaxErrors} advertências."
                : "\n\nCRÍTICO: Você acumulou todas as advertências. VOCÊ FOI DEMITIDO!";
        }
        else
        {
            title.text = "[ANÁLISE CORRETA]";
            title.color = new Color(53 / 255f, 128 / 255f, 19 / 255f);
        }

        connectedAlert.SetActive(true);
    }
}
