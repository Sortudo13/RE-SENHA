using UnityEngine;

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

        // Os botoes so aparecem com um email aberto; a checagem evita contar uma decisao sem email.
        EmailData email = evaluation.CurrentEmail;
        if (email == null)
            return;

        bool emailHasFindings = evaluation.CurrentResults.Count > 0;
        // Bloquear exige ao menos uma evidencia registrada com o Inspector; sem ela conta como erro
        // mesmo que o email quebre regras (o jogador tem que mostrar o motivo do bloqueio).
        bool hasEvidence = email.evidences != null && email.evidences.Count > 0;
        bool blockedWithoutEvidence = blockEmail && emailHasFindings && !hasEvidence;
        bool correctDecision = blockEmail == emailHasFindings && !blockedWithoutEvidence;
        string feedbackMessage = correctDecision
            ? blockEmail
                ? "Decisão correta! O e-mail possui achados de segurança."
                : "Decisão correta! Nenhuma regra do Manual foi violada."
            : blockedWithoutEvidence
                ? "Erro de análise! Você bloqueou o e-mail sem registrar nenhuma evidência. Use o Inspetor para apontar a regra quebrada antes de bloquear."
            : blockEmail
                ? "Erro de análise! O e-mail não possui achados aplicáveis."
                : "Erro de análise! O e-mail possui achados de segurança.";

        DayProgressController.RegisterAnswer(correctDecision);
        ActionController.SendAction("ShowFeedback", new FeedbackData(correctDecision, feedbackMessage));
        ActionController.SendAction("RemoveEmail", email);
        DayProgressController.CheckDayEnd();
    }
}
