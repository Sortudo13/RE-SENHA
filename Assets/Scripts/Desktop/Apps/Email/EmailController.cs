using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EmailController : MonoBehaviour
{
    private EmailData currentEmail = null;
    private SaveData currentSave = null;
    private List<EmailData> inbox = null;

    [Header("List Config")]
    private Transform listBar;
    [SerializeField] private GameObject listButton;

    [Header("Alert Window")]
    [SerializeField] private GameObject alertWindow;
    private GameObject emailInterface;
    private GameObject emailSender;
    private GameObject emailSubject;
    private GameObject emailBody;
    private GameObject emailAttachments;
    private GameObject emailReport;

    void Awake()
    {
        ActionController.SyncSignal<string>("OpenAttachment", OpenAttachment);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<string>("OpenAttachment", OpenAttachment);
    }

    void Start()
    {
        emailInterface = transform.GetChild(0).gameObject;
        listBar = transform.GetChild(1).GetChild(0);

        emailSender = emailInterface.transform.GetChild(1).gameObject;
        emailSubject = emailInterface.transform.GetChild(3).gameObject;
        emailAttachments = emailInterface.transform.GetChild(5).gameObject;
        emailBody = emailInterface.transform.GetChild(6).gameObject;
        emailReport = emailInterface.transform.GetChild(7).gameObject;

        foreach (Transform child in emailInterface.transform)
        {
            child.gameObject.SetActive(false);
        }

        currentSave = DataController.LoadEmails();
        inbox = currentSave.emails;
        UpdateEmailSidebar();
        StartDropdown();
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

            GameObject newText = new("Texto");
            newText.transform.SetParent(newButton.transform, false);

            TextMeshProUGUI text = newText.AddComponent<TextMeshProUGUI>();
            text.text = email.subject;
            text.enableAutoSizing = true;
            text.fontSizeMin = 30;
            text.fontSizeMax = 50;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;

            RectTransform rect = newText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            if (newButton.TryGetComponent(out Button botao))        
            {
                botao.onClick.RemoveAllListeners();
                botao.onClick.AddListener(() => LoadEmail(email));
            }
        }
    }

    public void LoadEmail(EmailData email)
    {
        if (currentEmail != email)
            currentEmail = email;
        else
            currentEmail = null;

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

    public void StartDropdown()
    {
        TMP_Dropdown dropdown = emailReport.transform.GetComponentInChildren<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(delegate { SwapReportButton(); });
        SwapReportButton();
    }

    public void SwapReportButton()
    {
        TMP_Dropdown dropdown = emailReport.transform.GetComponentInChildren<TMP_Dropdown>();
        Button submitButton = emailReport.transform.GetComponentInChildren<Button>();

        if (dropdown.value == 0)
        {
            if (submitButton.gameObject.TryGetComponent(out Image image))
                image.color = new Color(13/255f, 190/255f, 0/255f);

            TextMeshProUGUI text = submitButton.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = new Color(255/255f, 255/255f, 255/255f);
                text.text = "Aprovar";
            }
        }
        else
        {
            if (submitButton.gameObject.TryGetComponent(out Image image))
                image.color = new Color(124/255f, 41/255f, 41/255f);

            TextMeshProUGUI text = submitButton.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = new Color(255/255f, 3/255f, 0/255f);
                text.text = "Reportar";
            }
        }
    }

    public void ValidateAnswer()
    {
        if (currentEmail == null)
            return;

        TMP_Dropdown dropdown = emailReport.transform.GetComponentInChildren<TMP_Dropdown>();
        var playerIsPhishing = dropdown.value switch
        {
            0 => EmailError.None,
            1 => EmailError.SenderMismatch,
            2 => EmailError.SubjectMismatch,
            3 => EmailError.BodyMismatch,
            4 => EmailError.AttachmentMismatch,
            _ => EmailError.None,
        };

        string feedbackMessage = "";
        switch (currentEmail.error)
        {
            case EmailError.None:
                if (playerIsPhishing == EmailError.None)
                {
                    feedbackMessage = "Decisão Correta! O e-mail é realmente seguro.";
                }
                else
                {
                    feedbackMessage = "Erro de Análise! Este e-mail, na verdade, é seguro. Pelo menos manteve a vigilância.";
                }
                break;
            case EmailError.SenderMismatch:
                if (playerIsPhishing == EmailError.SenderMismatch)
                {
                    feedbackMessage = "Decisão Correta! O e-mail realmente tinha o remetente duvidoso. A ameaça foi neutralizada.";
                }
                else
                {
                    feedbackMessage = "Erro de Análise! Este e-mail, na verdade, tinha um remetente duvidoso.";
                }
                break;
            case EmailError.SubjectMismatch:
                if (playerIsPhishing == EmailError.SubjectMismatch)
                {
                    feedbackMessage = "Decisão Correta! O e-mail realmente tinha o assunto duvidoso. A ameaça foi neutralizada.";
                }
                else
                {
                    feedbackMessage = "Erro de Análise! Este e-mail, na verdade, tinha um assunto duvidoso.";
                }
                break;
            case EmailError.BodyMismatch:
                if (playerIsPhishing == EmailError.BodyMismatch)
                {
                    feedbackMessage = "Decisão Correta! O e-mail realmente tinha o corpo duvidoso. A ameaça foi neutralizada.";
                }
                else
                {
                    feedbackMessage = "Erro de Análise! Este e-mail, na verdade, tinha um corpo duvidoso.";
                }
                break;
            case EmailError.AttachmentMismatch:
                if (playerIsPhishing == EmailError.AttachmentMismatch)
                {
                    feedbackMessage = "Decisão Correta! O e-mail realmente tinha o anexo duvidoso. A ameaça foi neutralizada.";
                }
                else
                {
                    feedbackMessage = "Erro de Análise! Este e-mail, na verdade, tinha um anexo duvidoso.";
                }
                break;
        }

        if (!playerIsPhishing.Equals(currentEmail.error))
            DataController.CurrentErrorCount++;

        ShowFeedback(playerIsPhishing.Equals(currentEmail.error), feedbackMessage);
        RemoveEmail(currentEmail);

        if ((inbox.Count == 0 && DataController.CurrentDay >= DataController.MaxDay) || DataController.CurrentErrorCount >= DataController.MaxErrors)
        {
            transform.parent.GetComponentInParent<WindowController>().CloseWindow();
            string endingScene = (inbox.Count == 0) ? "Win" : "Gameover";
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

    public void ShowFeedback(bool acertou, string mensagem)
    {
        GameObject connectedAlert = Instantiate(alertWindow, AlertManagerController.AlertsLayer);
        GameObject content = connectedAlert.transform.GetChild(0).gameObject;
        TextMeshProUGUI txtTituloAviso = content.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI txtFeedback = content.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        connectedAlert.SetActive(false);

        if (content.transform.GetChild(2).TryGetComponent(out Button button))
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Destroy(connectedAlert));
        }

        txtFeedback.text = mensagem;

        if (!acertou)
        {
            txtTituloAviso.text = "[ALERTA DE SEGURANÇA]";
            txtTituloAviso.color = new Color(150 / 255f, 58 / 255f, 27 / 255f); // Vermelho

            if (DataController.CurrentErrorCount < DataController.MaxErrors)
            {
                txtFeedback.text += "\n\nREGISTRO: Você possui " + DataController.CurrentErrorCount + "/" + DataController.MaxErrors + " advertências.";
            }
            else
            {
                txtFeedback.text += "\n\nCRÍTICO: Você acumulou " + DataController.CurrentErrorCount + " advertências. VOCÊ FOI DEMITIDO!";
            }
        }
        else
        {
            txtTituloAviso.text = "[ANÁLISE CORRETA]";
            txtTituloAviso.color = new Color(53 / 255f, 128 / 255f, 19 / 255f); // Verde
        }

        connectedAlert.SetActive(true);
    }
}