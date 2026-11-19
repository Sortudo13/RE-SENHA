using UnityEngine;

public sealed class EmailDecisionController : MonoBehaviour
{

    [SerializeField]
    private InspectionEvaluationController evaluation;
    private readonly SignalSubscriptions signals = new();


    private void Awake()
    {
        signals
            .On(InspectionSignals.ApproveEmail, ApproveCurrentEmail)
            .On(InspectionSignals.BlockEmail, BlockCurrentEmail)
            .On<EmailData>(InspectionSignals.CurrentEmailChanged, PublishEmailSurface);
    }

    private void OnDestroy()
    {
        signals.Clear();
    }

    // Ponte Email -> Inspector: o EmailController nao conhece o Inspector, entao a superficie
    // do email aberto e criada e publicada aqui.
    private static void PublishEmailSurface(EmailData email)
    {
        if (email == null)
        {
            InspectionEvaluationController.CloseSurface(EmailInspectionSurface.Source);
            return;
        }

        email.evidences ??= new System.Collections.Generic.List<EvidenceData>();
        InspectionEvaluationController.PublishSurface(
            new EmailInspectionSurface(
                email,
                new ListFindingStore(EmailInspectionSurface.Source, string.Empty, email.evidences)
            )
        );
    }

    private static readonly InspectionFeedbackTexts FeedbackTexts = new()
    {
        correctReject = "Decisão correta! O e-mail possui achados de segurança.",
        correctApprove = "Decisão correta! Nenhuma regra do Manual foi violada.",
        rejectedWithoutEvidence =
            "Erro de análise! Você bloqueou o e-mail sem registrar nenhuma evidência. Use o Inspetor para apontar a regra quebrada antes de bloquear.",
        wrongReject = "Erro de análise! O e-mail não possui achados aplicáveis.",
        wrongApprove = "Erro de análise! O e-mail possui achados de segurança.",
    };

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
        EmailData email = (evaluation.CurrentSurface as EmailInspectionSurface)?.Email;
        if (email == null)
            return;

        InspectionJudge.Judge(
            evaluation,
            blockEmail,
            FeedbackTexts,
            () => ActionController.SendAction("RemoveEmail", email)
        );
    }
}
