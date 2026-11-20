using UnityEngine;

// Conta do dia do chat, separada do fluxo da conversa (como EmailDecisionController x EmailController).
// O ChatController cuida de conversa, historico, save e tela; aqui ficam acerto/erro, feedback e fim do dia.
// Fica no mesmo objeto do ChatController e do ChatTimeSync (ChatSystem, cena Desktop).
// Quando o chat tiver achados, a ponte com o Inspector entra aqui.
public sealed class ChatDecisionController : MonoBehaviour
{
    public const string ConversationFinishedAction = "ChatConversationFinished";

    private const string DefaultCorrectFeedback = "Resposta correta! Você agiu de forma segura na conversa.";
    private const string DefaultWrongFeedback = "Resposta insegura! Essa escolha colocou a empresa em risco.";

    [Tooltip("Vazio usa o ChatTimeSync do mesmo objeto.")]
    [SerializeField]
    private ChatTimeSync chatTimeSync;

    private void Awake()
    {
        if (chatTimeSync == null)
            chatTimeSync = GetComponent<ChatTimeSync>();

        if (chatTimeSync == null)
            Debug.LogError("[ChatDecisionController] ChatTimeSync não encontrado.", this);
    }

    private void OnEnable()
    {
        DayProgressController.AddPendingCheck(HasPendingDecisions);
        ActionController.SyncSignal<string>(ConversationFinishedAction, OnConversationFinished);
    }

    private void OnDisable()
    {
        DayProgressController.RemovePendingCheck(HasPendingDecisions);
        ActionController.UnsyncSignal<string>(ConversationFinishedAction, OnConversationFinished);
    }

    // Chamado direto (nao por sinal) porque precisa devolver se deu GameOver, como o InspectionJudge.
    // Mesma ordem do email: conta, feedback e depois o fim do dia.
    // Devolve true se deu GameOver: o save foi apagado e o ChatController nao deve salvar nem seguir.
    public static bool RegisterChoice(ChatOption chosen)
    {
        if (chosen == null || chosen.outcome == ChatOptionOutcome.Neutral)
            return false;

        bool correct = chosen.outcome == ChatOptionOutcome.Correct;
        DayProgressController.RegisterAnswer(correct);

        string message = !string.IsNullOrWhiteSpace(chosen.outcomeFeedback)
            ? chosen.outcomeFeedback
            : correct ? DefaultCorrectFeedback : DefaultWrongFeedback;
        ActionController.SendAction("ShowFeedback", new FeedbackData(correct, message));

        return DayProgressController.CheckDayEnd();
    }

    // A conversa terminada pode ter sido a ultima pendente do dia.
    private void OnConversationFinished(string conversationId)
    {
        DayProgressController.CheckDayEnd();
    }

    // Pendente: conversa do dia que ja chegou, nao terminou e esta marcada com blocksDayEnd.
    // Quem escreve a conversa decide se ela segura o "Avancar Dia" (decidido em 2026-11-20).
    private bool HasPendingDecisions()
    {
        ChatInboxState inboxState = chatTimeSync != null ? chatTimeSync.State : null;
        if (inboxState == null)
            return false;

        int day = DataController.CurrentDay;
        foreach (var state in inboxState.All)
        {
            if (!state.hasArrived || state.isFinished || state.entry.arrivalDay != day)
                continue;

            if (state.conversation.blocksDayEnd)
                return true;
        }
        return false;
    }
}
