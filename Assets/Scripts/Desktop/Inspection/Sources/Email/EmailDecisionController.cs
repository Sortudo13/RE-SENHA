using UnityEngine;
using UnityEngine.SceneManagement;

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

        EmailData email = evaluation.CurrentEmail;
        SaveData save = evaluation.CurrentSave;
        if (email == null || save == null)
            return;

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
        ActionController.SendAction("RemoveEmail", email);

        int remainingEmails = save.emails.Count;
        if (
            (remainingEmails == 0 && DataController.CurrentDay >= DataController.MaxDay)
            || DataController.CurrentErrorCount >= DataController.MaxErrors
        )
        {
            ActionController.SendAction("CloseEmailWindow");
            string endingScene = (remainingEmails == 0) ? "GameWin" : "GameOver";
            DataController.ResetDays();
            SceneManager.LoadScene(endingScene);
        }
        if (remainingEmails == 0)
        {
            DataController.AdvanceDay();
            ActionController.SendAction("ReloadInbox");
        }
    }
}
