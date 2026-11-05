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
    private InspectionEvaluationController inspectionEvaluation;
    private Transform listBar;

    [Header("List Config")]
    [SerializeField] private GameObject listButton;

    [Header("Alert Window")]
    [SerializeField] private GameObject alertWindow;

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
        inspectionEvaluation = GetComponent<InspectionEvaluationController>();
        ListenSignals();
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<string>("OpenAttachment", OpenAttachment);
        ActionController.UnsyncSignal<EmailData>("AddEmail", AddEmail);
        ActionController.UnsyncSignal<EmailData>("RemoveEmail", RemoveEmail);
        UnlistenSignals();
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

            //Colocar trecho de configuração visual do botão nele próprio.
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
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    public void LoadEmail(EmailData email)
    {
        if (currentEmail != email)
            currentEmail = email;
        else
            currentEmail = null;

        ActionController.SendAction("CurrentEmailChanged", new InspectionEmailContext(currentSave, currentEmail));

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

    // Gerenciador de Avisos
    public void ShowFeedback(bool acertou, string mensagem)
    {
        GameObject connectedAlert = Instantiate(alertWindow, AlertManagerController.AlertsLayer);
        GameObject content = connectedAlert.transform.GetChild(0).gameObject;
        TextMeshProUGUI txtTituloAviso = content
            .transform.GetChild(0)
            .GetComponent<TextMeshProUGUI>();
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
                txtFeedback.text +=
                    "\n\nREGISTRO: Você possui "
                    + DataController.CurrentErrorCount
                    + "/"
                    + DataController.MaxErrors
                    + " advertências.";
            }
            else
            {
                txtFeedback.text +=
                    "\n\nCRÍTICO: Você acumulou "
                    + DataController.CurrentErrorCount
                    + " advertências. VOCÊ FOI DEMITIDO!";
            }
        }
        else
        {
            txtTituloAviso.text = "[ANÁLISE CORRETA]";
            txtTituloAviso.color = new Color(53 / 255f, 128 / 255f, 19 / 255f); // Verde
        }

        connectedAlert.SetActive(true);
    }


    //Tirar daqui pra baixo e levar pro inspetor

    private void EvaluateDecision(bool blockEmail)
    {
        if (currentEmail == null)
            return;

        bool emailHasFindings =
            inspectionEvaluation != null && inspectionEvaluation.CurrentResults.Count > 0;
        bool correctDecision = blockEmail == emailHasFindings;
        string feedbackMessage = correctDecision
            ? blockEmail
                ? "Decisão correta! O e-mail possui achados de segurança."
                : "Decisão correta! Nenhuma regra do Manual foi violada."
            : blockEmail
                ? "Erro de análise! O e-mail não possui achados aplicáveis."
                : "Erro de análise! O e-mail possui achados de segurança.";

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
            string endingScene = (inbox.Count == 0) ? "GameWin" : "GameOver";
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

    private void ListenSignals()
    {
        ActionController.SyncSignal("ApproveCurrentEmail", ApproveCurrentEmail);
        ActionController.SyncSignal("BlockCurrentEmail", BlockCurrentEmail);
    }

    private void UnlistenSignals()
    {
        ActionController.UnsyncSignal("ApproveCurrentEmail", ApproveCurrentEmail);
        ActionController.UnsyncSignal("BlockCurrentEmail", BlockCurrentEmail);
    }


    private void ApproveCurrentEmail()
    {
        ActionController.SendAction("OnApprovalButtonClicked");
        EvaluateDecision(false);
    }

    private void BlockCurrentEmail()
    {
        EvaluateDecision(true);
    }
}
