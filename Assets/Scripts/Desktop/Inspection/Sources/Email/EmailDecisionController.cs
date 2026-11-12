using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public sealed class EmailDecisionController : MonoBehaviour
{
    public const string ApproveAction = "ApproveCurrentEmail";
    public const string BlockAction = "BlockCurrentEmail";

    [SerializeField]
    private InspectionEvaluationController evaluation;

    private void Awake()
    {
        ActionController.SyncSignal(ApproveAction, ApproveCurrentEmail);
        ActionController.SyncSignal(BlockAction, BlockCurrentEmail);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal(ApproveAction, ApproveCurrentEmail);
        ActionController.UnsyncSignal(BlockAction, BlockCurrentEmail);
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

    private void EvaluateDecision(bool blockEmail)
    {
        if (evaluation == null)
        {
            Debug.LogError("EmailDecisionController: evaluation reference is missing.");
            return;
        }

        bool emailHasFindings = evaluation.CurrentResults.Count > 0;
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

        ActionController.SendAction("ShowFeedback", new FeedbackData(correctDecision, feedbackMessage));
        ActionController.SendAction("RemoveEmail", evaluation.CurrentEmail);

        FinishedDay();
    }

    private void FinishedDay()
    {
        if (DataController.CurrentErrorCount >= DataController.MaxErrors)
        {
            DataController.ResetGame();
            SceneManager.LoadScene("GameOver");
        }

        if (!ClockController.IsStillWorking)
        {
            int remainingCompanyEmails = DataController.CurrentSave.currentMailBoxes.Find(box => box.mailBox == "Company").emails.Count;
            int availableCompanyEmails = DataController.CurrentSave.availableMails.Count(mail => mail.mailBox == "Company");
            int remainingTickets = DataController.CurrentSave.tickets.Count;

            bool hasPendingEmails = remainingCompanyEmails > 0 || availableCompanyEmails > 0;
            bool hasPendingTickets = remainingTickets > 0 && false;

            if (!hasPendingEmails && !hasPendingTickets)
                ActionController.SendAction("ShowNextDay");
        }
    }
}
