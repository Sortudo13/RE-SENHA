using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TicketController : MonoBehaviour
{
    private TicketData currentTicket;
    private SaveData currentSave;
    private List<TicketData> tickets;

    [Header("List Config")]
    [SerializeField]
    private GameObject listButton;

    [Header("Interface References")]
    [SerializeField]
    private GameObject ticketInterface;

    [SerializeField]
    private Transform listBar;

    [SerializeField]
    private GameObject requesterField;

    [SerializeField]
    private GameObject bodyField;

    [SerializeField]
    private GameObject decisionField;

    [Header("Feedback")]
    [SerializeField]
    private GameObject alertWindow;

    private void Start()
    {
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
        {
            Debug.LogError(
                "TicketController: decisionField precisa conter um TMP_Dropdown e um Button."
            );
            return;
        }

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
            TextMeshProUGUI label = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = ticket.requester;

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
        TicketDecision answer =
            dropdown.value == 0 ? TicketDecision.Approve : TicketDecision.Reject;
        bool correct = answer == currentTicket.decision;
        string feedback = correct
            ? "Decisão correta! O chamado foi avaliado conforme as políticas de TI."
            : "Decisão incorreta. Consulte o manual de TI antes de avaliar chamados semelhantes.";
        if (!string.IsNullOrWhiteSpace(currentTicket.reason))
            feedback += $"\n\nJustificativa: {currentTicket.reason}";

        if (!correct)
            DataController.CurrentErrorCount++;

        ActionController.SendAction("ShowFeedback", new FeedbackData(correct, feedback));

        tickets.Remove(currentTicket);
        DataController.SaveEmails(currentSave);
        currentTicket = null;
        HideTicketDetails();
        UpdateTicketSidebar();
    }
}
