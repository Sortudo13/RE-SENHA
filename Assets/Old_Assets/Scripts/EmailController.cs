using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class EmailController : MonoBehaviour
{
    private EmailData currentEmail = null;

    [Header("Daily Emails")]
    [SerializeField] private List<EmailData> inbox;

    [Header("List Config")]
    [SerializeField] private Transform listBar;
    [SerializeField] private GameObject listButton;

    [Header("Email's Reading Panel")]
    [SerializeField] private GameObject emailInterface;
    private TextMeshProUGUI emailSender;
    private TextMeshProUGUI emailSubject;
    private TextMeshProUGUI emailBody;
    private TextMeshProUGUI emailAttachments;

    [Header("Alert Window")]
    [SerializeField] private GameObject alertWindow;
    [SerializeField] private Transform alertLayer;
    private GameObject connectedAlert;
    private TextMeshProUGUI txtTituloAviso;
    private TextMeshProUGUI txtFeedback;

    private int advertenciasAtuais = 0;
    private const int advertenciasMaximas = 3;

    void Start()
    {
        connectedAlert = Instantiate(alertWindow, alertLayer);
        txtTituloAviso = connectedAlert.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        txtFeedback = connectedAlert.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        connectedAlert.SetActive(false);

        if (connectedAlert.transform.GetChild(2).TryGetComponent(out Button button))
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HideFeedback);
        }

        if (emailInterface != null)
        {
            emailSender = emailInterface.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            emailSubject = emailInterface.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            emailBody = emailInterface.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            emailAttachments = emailInterface.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            emailInterface.transform.GetChild(4).gameObject.SetActive(false);
        }

        UpdateEmailPanel();
    }

    public void UpdateEmailPanel()
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
            rect.anchorMin = new Vector2(0, 0); // Âncora inferior esquerda
            rect.anchorMax = new Vector2(1, 1); // Âncora superior direita
            rect.offsetMin = Vector2.zero;      // Zera Left e Bottom
            rect.offsetMax = Vector2.zero;      // Zera Right e Top

            if (newButton.TryGetComponent(out Button botao))        
            {
                botao.onClick.RemoveAllListeners();
                botao.onClick.AddListener(() => SwapEmail(email));
            }
        }
    }

    public void SwapEmail(EmailData email)
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
            emailInterface.transform.GetChild(4).gameObject.SetActive(true);
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
            emailInterface.transform.GetChild(4).gameObject.SetActive(false);
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
        switch (currentEmail.error)
        {
            case EmailError.None:
                if (playerIsPhishing == EmailError.None)
                {
                    ShowFeedback(true, "Decisão Correta! O e-mail é realmente seguro.");
                }
                else
                {
                    ShowFeedback(false, "Erro de Análise! Este e-mail, na verdade, é seguro. Pelo menos manteve a vigilância.");
                }
                break;
            case EmailError.GeneralMismatch:
                if (playerIsPhishing == EmailError.GeneralMismatch)
                {
                    ShowFeedback(true, "Decisão Correta! O e-mail realmente tinha uma discrepância. A ameaça foi neutralizada.");
                }
                else
                {
                    ShowFeedback(false, "Erro de Análise! Este e-mail, na verdade, tinha uma discrepância.");
                }
                break;
            case EmailError.SenderMismatch:
                if (playerIsPhishing == EmailError.SenderMismatch)
                {
                    ShowFeedback(true, "Decisão Correta! O e-mail realmente tinha o remetente duvidoso. A ameaça foi neutralizada.");
                }
                else
                {
                    ShowFeedback(false, "Erro de Análise! Este e-mail, na verdade, tinha um remetente duvidoso.");
                }
                break;
            case EmailError.SubjectMismatch:
                if (playerIsPhishing == EmailError.SubjectMismatch)
                {
                    ShowFeedback(true, "Decisão Correta! O e-mail realmente tinha o assunto duvidoso. A ameaça foi neutralizada.");
                }
                else
                {
                    ShowFeedback(false, "Erro de Análise! Este e-mail, na verdade, tinha um assunto duvidoso.");
                }
                break;
            case EmailError.BodyMismatch:
                if (playerIsPhishing == EmailError.BodyMismatch)
                {
                    ShowFeedback(true, "Decisão Correta! O e-mail realmente tinha o corpo duvidoso. A ameaça foi neutralizada.");
                }
                else
                {
                    ShowFeedback(false, "Erro de Análise! Este e-mail, na verdade, tinha um corpo duvidoso.");
                }
                break;
            case EmailError.AttachmentMismatch:
                if (playerIsPhishing == EmailError.AttachmentMismatch)
                {
                    ShowFeedback(true, "Decisão Correta! O e-mail realmente tinha o anexo duvidoso. A ameaça foi neutralizada.");
                }
                else
                {
                    ShowFeedback(false, "Erro de Análise! Este e-mail, na verdade, tinha um anexo duvidoso.");
                }
                break;
        }
    }

    public void ShowFeedback(bool acertou, string mensagem)
    {
        txtFeedback.text = mensagem;

        if (!acertou)
        {
            txtTituloAviso.text = "[ ALERTA DE SEGURANÇA ]";
            txtTituloAviso.color = new Color(1f, 0.2f, 0.2f); // Vermelho Neon
            advertenciasAtuais++;
            if (advertenciasAtuais < advertenciasMaximas)
            {
                txtFeedback.text += "\n\nREGISTRO: Você possui " + advertenciasAtuais + "/" + advertenciasMaximas + " advertências.";
            }
            else
            {
                txtFeedback.text += "\n\nCRÍTICO: Você acumulou " + advertenciasAtuais + " advertências. VOCÊ FOI DEMITIDO!";
            }
        } else
        {
            txtTituloAviso.text = "[ ANÁLISE CORRETA ]";
            txtTituloAviso.color = new Color(0f, 1f, 0.4f); // Verde Neon
        }

        if (connectedAlert != null)
            connectedAlert.SetActive(true);
    }

    public void HideFeedback()
    {
        if (connectedAlert != null)
            connectedAlert.SetActive(false);

        if (currentEmail != null && inbox.Contains(currentEmail))
        {
            inbox.Remove(currentEmail);
            SwapEmail(currentEmail);
        }

        UpdateEmailPanel();
    }

    private void OnDestroy()
    {
        //TODO: Fazer envio de alerta para componente dos alertas
    }
}
