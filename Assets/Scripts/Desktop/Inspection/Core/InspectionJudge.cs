using System;
using UnityEngine;

// Textos do feedback de uma decisao. Os padroes servem para qualquer app; cada app pode
// passar os proprios (o Email passa os dele).
[Serializable]
public sealed class InspectionFeedbackTexts
{
    [TextArea]
    public string correctReject = "Decisão correta! O item possui achados de segurança.";

    [TextArea]
    public string correctApprove = "Decisão correta! Nenhuma regra do Manual foi violada.";

    [TextArea]
    public string rejectedWithoutEvidence =
        "Erro de análise! Você rejeitou o item sem registrar nenhuma evidência. Use o Inspetor para apontar a regra quebrada antes de rejeitar.";

    [TextArea]
    public string wrongReject = "Erro de análise! O item não possui achados aplicáveis.";

    [TextArea]
    public string wrongApprove = "Erro de análise! O item possui achados de segurança.";
}

// Decisao de aprovar/rejeitar o item em julgamento, igual para todos os apps:
// confere a resposta, conta acerto/erro, mostra o feedback, remove o item e ve o fim do dia.
public static class InspectionJudge
{
    // rejected: o jogador bloqueou/rejeitou o item. removeItem: tira o item respondido da lista
    // do app (chamado depois do feedback e antes de conferir o fim do dia).
    // Devolve false se nao ha item em julgamento (nada e contado).
    public static bool Judge(
        InspectionEvaluationController evaluation,
        bool rejected,
        InspectionFeedbackTexts texts,
        Action removeItem
    )
    {
        if (evaluation == null)
        {
            Debug.LogError("InspectionJudge: evaluation reference is missing.");
            return false;
        }

        if (evaluation.CurrentSurface == null)
            return false;

        texts ??= new InspectionFeedbackTexts();

        bool hasBreaks = evaluation.CurrentResults.Count > 0;
        // Rejeitar exige ao menos uma evidencia registrada com o Inspector; sem ela conta como
        // erro mesmo que o item quebre regras (o jogador tem que mostrar o motivo).
        bool hasEvidence = evaluation.CurrentFindings.Count > 0;
        bool rejectedWithoutEvidence = rejected && hasBreaks && !hasEvidence;
        bool correct = rejected == hasBreaks && !rejectedWithoutEvidence;

        string message = correct
            ? rejected ? texts.correctReject : texts.correctApprove
            : rejectedWithoutEvidence ? texts.rejectedWithoutEvidence
            : rejected ? texts.wrongReject
            : texts.wrongApprove;

        DayProgressController.RegisterAnswer(correct);
        ActionController.SendAction("ShowFeedback", new FeedbackData(correct, message));
        removeItem?.Invoke();
        DayProgressController.CheckDayEnd();
        return true;
    }
}
