using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailController : MonoBehaviour
{
    private EmailData currentEmail = null;
    private SaveData currentSave = null;
    private List<EmailData> inbox = null;

    [Header("List Config")]
    [SerializeField] private Transform listBar;
    [SerializeField] private GameObject listButton;

    [Header("Alert Window")]
    [SerializeField] private GameObject alertWindow;
    [SerializeField] private Transform alertLayer;

    [Header("Email's Reading Panel")]
    [SerializeField] private GameObject emailInterface;
    private TextMeshProUGUI emailSender;
    private TextMeshProUGUI emailSubject;
    private TextMeshProUGUI emailBody;
    private TextMeshProUGUI emailAttachments;
    private GameObject emailButtons;

    void Start()
    {
        emailSender = emailInterface.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        emailSubject = emailInterface.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        emailBody = emailInterface.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        emailAttachments = emailInterface.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        emailButtons = emailInterface.transform.GetChild(4).gameObject;
        emailButtons.SetActive(false);

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
                emailSender.text = "<b>De:</b> " + currentEmail.sender;
            if (emailSubject != null)
                emailSubject.text = "<b>Assunto:</b> " + currentEmail.subject;
            if (emailBody != null)
                emailBody.text = currentEmail.body;
            if (emailAttachments != null)
            {
                if (currentEmail.attachments.Count > 0)
                {
                    emailAttachments.text = "Anexos: ";
                    foreach (string attachment in currentEmail.attachments)
                    {
                        emailAttachments.text += attachment + ", ";
                    }
                    emailAttachments.text = emailAttachments.text.TrimEnd(',', ' ');
                }
                else
                {
                    emailAttachments.text = "Sem anexos";
                }
            }
            emailButtons.SetActive(true);
        }
        else
        {
            if (emailSender != null)
                emailSender.text = "";
            if (emailSubject != null)
                emailSubject.text = "";
            if (emailBody != null)
                emailBody.text = "";
            if (emailAttachments != null)
                emailAttachments.text = "";
            emailButtons.SetActive(false);
        }
    }

    public void ValidateAnswer(string playerAnswer)
    {
        if (currentEmail == null)
            return;

        var playerIsPhishing = playerAnswer switch
        {
            "None" => EmailError.None,
            "SenderMismatch" => EmailError.SenderMismatch,
            "SubjectMismatch" => EmailError.SubjectMismatch,
            "BodyMismatch" => EmailError.BodyMismatch,
            "AttachmentMismatch" => EmailError.AttachmentMismatch,
            "GeneralMismatch" => EmailError.GeneralMismatch,
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
            case EmailError.GeneralMismatch:
                if (playerIsPhishing == EmailError.GeneralMismatch)
                {
                    feedbackMessage = "Decisão Correta! O e-mail realmente tinha uma discrepância. A ameaça foi neutralizada.";
                }
                else
                {
                    feedbackMessage = "Erro de Análise! Este e-mail, na verdade, tinha uma discrepância.";
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

    }

    public void ShowFeedback(bool acertou, string mensagem)
    {
        GameObject connectedAlert = Instantiate(alertWindow, alertLayer);
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