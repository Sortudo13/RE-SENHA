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

        int remainingEmails = DataController.CurrentSave.currentMailBoxes.Find(box => box.mailBox == "Company").emails.Count;
        int availableEmails = DataController.CurrentSave.availableMails.Count;
        if (
            (remainingEmails == 0 && availableEmails == 0 && DataController.CurrentDay >= DataController.MaxDay)
            || DataController.CurrentErrorCount >= DataController.MaxErrors
        )
        {
            // A troca de cena destroi a janela do Email; antes ela era fechada aqui, tocando o som de fechar janela.
            string endingScene = (remainingEmails == 0 && availableEmails == 0) ? "GameWin" : "GameOver";
            DataController.ResetGame();
            SceneManager.LoadScene(endingScene);
        }
        if (remainingEmails == 0)
        {
            DataController.AdvanceDay();
            // PENDENTE (Andre): com mais de um dia (MaxDay > 1), o EmailController precisa recarregar a lista
            // depois de AdvanceDay(), ou continua mostrando a inbox antiga ate a janela ser reaberta.
            // Sugestao: no DataController, disparar "DayChanged" depois de SaveEmails e o Email escutar esse sinal.
        }
    }
}
