using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("IDs das Dicas no CompanionTips.json")]
    [SerializeField]
    private string tipWelcome = "tutorial_welcome";

    [SerializeField]
    private string tipOpenEmail = "tutorial_open_email";

    [SerializeField]
    private string tipReadEmail = "tutorial_read_email";

    [SerializeField]
    private string tipFinished = "tutorial_finished";

    [SerializeField]
    private string tipManualOpened = "manual_first_open";

    [SerializeField]
    private string tipChatOpened = "first_chat_open";

    [SerializeField]
    private string tipInspectionModeEntered = "first_inspection_mode";

    [SerializeField]
    private string tipDecisionMade = "first_decision_pending";

    [SerializeField]
    private string tipDocumentOpened = "first_document_open";

    [SerializeField]
    private string tipAlertReceived = "first_alert_received";

    private enum Step
    {
        None,
        WaitOpenEmail,
        WaitReadEmail,
        Done,
    }

    private Step currentStep = Step.None;

    private void OnEnable()
    {
        ActionController.SyncSignal<string>(TutorialSignals.WindowOpened, OnWindowOpened);
        ActionController.SyncSignal<string>(TutorialSignals.EmailRead, OnEmailRead);
        ActionController.SyncSignal<string>(TutorialSignals.ChatOpened, OnChatOpened);
        ActionController.SyncSignal<string>(TutorialSignals.ChoiceMade, OnDecisionMade);
        ActionController.SyncSignal<string>(TutorialSignals.DecisionMade, OnDecisionMade);
        ActionController.SyncSignal<string>(TutorialSignals.InspectionModeEntered, OnInspectionModeEntered);
        ActionController.SyncSignal<string>(TutorialSignals.DocumentOpened, OnDocumentOpened);
        ActionController.SyncSignal<string>(TutorialSignals.AlertReceived, OnAlertReceived);
        ActionController.SyncSignal<string>(CompanionSignals.TipFinished, OnTipFinished);
    }

    private void OnDisable()
    {
        ActionController.UnsyncSignal<string>(TutorialSignals.WindowOpened, OnWindowOpened);
        ActionController.UnsyncSignal<string>(TutorialSignals.EmailRead, OnEmailRead);
        ActionController.UnsyncSignal<string>(TutorialSignals.ChatOpened, OnChatOpened);
        ActionController.UnsyncSignal<string>(TutorialSignals.ChoiceMade, OnDecisionMade);
        ActionController.UnsyncSignal<string>(TutorialSignals.DecisionMade, OnDecisionMade);
        ActionController.UnsyncSignal<string>(TutorialSignals.InspectionModeEntered, OnInspectionModeEntered);
        ActionController.UnsyncSignal<string>(TutorialSignals.DocumentOpened, OnDocumentOpened);
        ActionController.UnsyncSignal<string>(TutorialSignals.AlertReceived, OnAlertReceived);
        ActionController.UnsyncSignal<string>(CompanionSignals.TipFinished, OnTipFinished);
    }

    private void Start()
    {
        // Espera a cena carregar e verifica se já viu o tutorial
        if (CompanionTips.WasSeen(tipWelcome))
            return;

        // Pequeno delay para a UI do desktop terminar de carregar
        Invoke(nameof(StartTutorial), 1.5f);
    }

    private void StartTutorial()
    {
        currentStep = Step.WaitOpenEmail;
        ShowTipOnce(tipWelcome);
    }

    private void OnTipFinished(string tipId)
    {
        // Quando o jogador fecha a boas-vindas, mostra a próxima instrução
        if (tipId == tipWelcome)
        {
            ShowTipOnce(tipOpenEmail);
        }
        else if (tipId == tipReadEmail)
        {
            currentStep = Step.Done;
            ShowTipOnce(tipFinished);
        }
    }

    private void OnWindowOpened(string appName)
    {
        if (!string.IsNullOrWhiteSpace(appName)
            && appName.IndexOf("Manual", StringComparison.OrdinalIgnoreCase) >= 0)
            ShowTipOnce(tipManualOpened);

        if (currentStep == Step.WaitOpenEmail && appName == "Email")
        {
            currentStep = Step.WaitReadEmail;
            ShowTipOnce(tipReadEmail);
        }
    }

    private void OnEmailRead(string emailSubject)
    {
        if (currentStep == Step.WaitReadEmail)
        {
            // O jogador leu o e-mail. Avança.
            currentStep = Step.Done;
            ShowTipOnce(tipFinished);
        }
    }

    private void OnChatOpened(string conversationId) => ShowTipOnce(tipChatOpened);

    private void OnDecisionMade(string decisionId) => ShowTipOnce(tipDecisionMade);

    private void OnInspectionModeEntered(string source) => ShowTipOnce(tipInspectionModeEntered);

    private void OnDocumentOpened(string documentType) => ShowTipOnce(tipDocumentOpened);

    private void OnAlertReceived(string alertType) => ShowTipOnce(tipAlertReceived);

    private static void ShowTipOnce(string tipId)
    {
        if (!CompanionTips.WasSeen(tipId))
            CompanionSignals.Show(tipId);
    }
}
