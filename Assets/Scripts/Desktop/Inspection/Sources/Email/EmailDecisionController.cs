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

        // Regra do dia: vence quem responde todos os itens do dia sem chegar a MaxErrors (3) erros.
        // Hoje so os emails contam; tickets e mensagens de chat devem entrar nesta mesma conta
        // quando tiverem fluxo de decisao (o ideal e um controlador de progresso do dia compartilhado).
        // A derrota e verificada primeiro: errar o ultimo email com o 3o erro e derrota, nao vitoria.
        if (DataController.CurrentErrorCount >= DataController.MaxErrors)
        {
            EndGame("GameOver");
            return;
        }

        int remainingEmails = save.emails.Count;
        if (remainingEmails == 0)
        {
            if (DataController.CurrentDay >= DataController.MaxDay)
            {
                EndGame("GameWin");
                return;
            }

            DataController.AdvanceDay();
            // PENDENTE (Andre): com mais de um dia (MaxDay > 1), o EmailController precisa recarregar a lista
            // depois de AdvanceDay(), ou continua mostrando a inbox antiga ate a janela ser reaberta.
            // Sugestao: no DataController, disparar "DayChanged" depois de SaveEmails e o Email escutar esse sinal.
        }
    }

    // A troca de cena destroi a janela do Email; antes ela era fechada aqui, tocando o som de fechar janela.
    private static void EndGame(string endingScene)
    {
        DataController.ResetDays();
        SceneManager.LoadScene(endingScene);
    }
}
