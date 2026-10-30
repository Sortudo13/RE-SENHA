using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TicketController : MonoBehaviour
{
    private TicketData currentTicket;
    private SaveData currentSave;
    private List<TicketData> tickets;

    [Header("List Config")]
    [SerializeField] private GameObject listButton;

    [Header("Feedback")]
    [SerializeField] private GameObject alertWindow;

    private Transform listBar;
    private GameObject ticketInterface;
    private GameObject requesterField;
    private GameObject subjectField;
    private GameObject bodyField;
    private GameObject decisionField;

    private void Start()
    {
        ticketInterface = transform.GetChild(0).gameObject;
        listBar = transform.GetChild(1).GetChild(0);

        requesterField = ticketInterface.transform.GetChild(1).gameObject;
        subjectField = ticketInterface.transform.GetChild(3).gameObject;
        bodyField = ticketInterface.transform.GetChild(6).gameObject;
        decisionField = ticketInterface.transform.GetChild(7).gameObject;
        ticketInterface.transform.GetChild(5).gameObject.SetActive(false);

        HideTicketDetails();

        currentSave = DataController.LoadEmails();
        tickets = currentSave.tickets;
        if (tickets == null)
            tickets = DataController.LoadDailyJson(DataController.CurrentDay).tickets;
        tickets ??= new List<TicketData>();
        currentSave.tickets = tickets;
        UpdateTicketSidebar();
        ConfigureDecisionControl();
    }

    private void OnDestroy()
    {
        if (decisionField == null)
            return;

        Button submitButton = decisionField.GetComponentInChildren<Button>(true);
        if (submitButton != null)
            submitButton.onClick.RemoveListener(ValidateAnswer);
    }

    private void HideTicketDetails()
    {
        foreach (Transform child in ticketInterface.transform)
            child.gameObject.SetActive(false);
    }

    private void ConfigureDecisionControl()
    {
        TMP_Dropdown dropdown = decisionField.GetComponentInChildren<TMP_Dropdown>(true);
        Button submitButton = decisionField.GetComponentInChildren<Button>(true);
        if (dropdown == null || submitButton == null)
            return;

        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string> { "Aprovar", "Rejeitar" });
        dropdown.value = 0;
        dropdown.RefreshShownValue();
        dropdown.onValueChanged.AddListener(_ => UpdateDecisionButton());

        submitButton.onClick.RemoveListener(ValidateAnswer);
        submitButton.onClick.AddListener(ValidateAnswer);
        UpdateDecisionButton();
    }

    private void UpdateDecisionButton()
    {
        Button submitButton = decisionField.GetComponentInChildren<Button>(true);
        TMP_Dropdown dropdown = decisionField.GetComponentInChildren<TMP_Dropdown>(true);
        if (submitButton == null || dropdown == null)
            return;

        bool approving = dropdown.value == 0;
        if (submitButton.TryGetComponent(out Image image))
            image.color = approving
                ? new Color(13 / 255f, 190 / 255f, 0)
                : new Color(124 / 255f, 41 / 255f, 41 / 255f);

        TextMeshProUGUI label = submitButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = approving ? "Aprovar" : "Rejeitar";
    }

    private void UpdateTicketSidebar()
    {
        foreach (Transform child in listBar)
            Destroy(child.gameObject);

        foreach (TicketData ticket in tickets)
        {
            GameObject buttonObject = Instantiate(listButton, listBar);
            TextMeshProUGUI label = new GameObject("Texto").AddComponent<TextMeshProUGUI>();
            label.transform.SetParent(buttonObject.transform, false);
            label.text = ticket.subject;
            label.enableAutoSizing = true;
            label.fontSizeMin = 30;
            label.fontSizeMax = 50;
            label.color = Color.black;
            label.alignment = TextAlignmentOptions.Center;

            RectTransform rect = label.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            if (buttonObject.TryGetComponent(out Button button))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => LoadTicket(ticket));
            }
        }
    }

    private void LoadTicket(TicketData ticket)
    {
        currentTicket = currentTicket == ticket ? null : ticket;
        if (currentTicket == null)
        {
            HideTicketDetails();
            return;
        }

        requesterField.GetComponentInChildren<TextMeshProUGUI>().text =
            $"Solicitante: {currentTicket.requester}";
        subjectField.GetComponentInChildren<TextMeshProUGUI>().text =
            $"Assunto: {currentTicket.subject}";
        bodyField.GetComponentInChildren<TextMeshProUGUI>().text =
            $"Descrição:\n{currentTicket.body}";

        foreach (Transform child in ticketInterface.transform)
            child.gameObject.SetActive(true);
    }

    private void ValidateAnswer()
    {
        if (currentTicket == null)
            return;

        TMP_Dropdown dropdown = decisionField.GetComponentInChildren<TMP_Dropdown>(true);
        TicketDecision answer = dropdown.value == 0 ? TicketDecision.Approve : TicketDecision.Reject;
        bool correct = answer == currentTicket.decision;
        string feedback = correct
            ? "Decisão correta! A requisição foi avaliada conforme as regras da empresa."
            : "Decisão incorreta. Consulte o manual da empresa antes de avaliar requisições semelhantes.";
        if (!string.IsNullOrWhiteSpace(currentTicket.reason))
            feedback += $"\n\nJustificativa: {currentTicket.reason}";

        if (!correct)
            DataController.CurrentErrorCount++;

        ShowFeedback(correct, feedback);
        tickets.Remove(currentTicket);
        DataController.SaveEmails(currentSave);
        currentTicket = null;
        HideTicketDetails();
        UpdateTicketSidebar();

        if (tickets.Count == 0)
        {
            if (DataController.CurrentErrorCount >= DataController.MaxErrors)
            {
                transform.parent.GetComponentInParent<WindowController>().CloseWindow();
                DataController.ResetDays();
                SceneManager.LoadScene("Gameover");
            }
            else if (DataController.CurrentDay >= DataController.MaxDay)
            {
                transform.parent.GetComponentInParent<WindowController>().CloseWindow();
                DataController.ResetDays();
                SceneManager.LoadScene("GameWin");
            }
        }
    }

    private void ShowFeedback(bool correct, string message)
    {
        if (alertWindow == null)
        {
            Debug.LogWarning("TicketController: o prefab de feedback não foi configurado.");
            return;
        }

        GameObject alert = Instantiate(alertWindow, AlertManagerController.AlertsLayer);
        Transform content = alert.transform.GetChild(0);
        content.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            correct ? "[ANÁLISE CORRETA]" : "[ALERTA DE SEGURANÇA]";
        content.GetChild(1).GetComponent<TextMeshProUGUI>().text = message;

        if (content.GetChild(2).TryGetComponent(out Button closeButton))
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => Destroy(alert));
        }

        alert.SetActive(true);
    }
}
