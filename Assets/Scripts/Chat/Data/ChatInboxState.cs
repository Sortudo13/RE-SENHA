using System.Collections.Generic;

public class ChatInboxState
{
    public class ConversationState
    {
        public ChatConversation conversation;
        public ChatInbox.Entry entry; // guarda horário/ordem
        public bool hasArrived;
        public bool hasUnread;
        public bool isFinished;
        public string currentNodeId; // nó atual da sessão
    }

    private readonly Dictionary<string, ConversationState> _states = new();
    private readonly ChatInbox _inbox;
    private int _lastProcessedTotalMinutes = -1;

    public ChatInboxState(ChatInbox inbox)
    {
        _inbox = inbox;
        foreach (var entry in inbox.GetSortedEntries())
        {
            if (entry?.conversation == null)
                continue;

            _states[entry.conversation.conversationId] = new ConversationState
            {
                conversation = entry.conversation,
                entry = entry,
                hasArrived = false,
                hasUnread = false,
                isFinished = false,
                currentNodeId = entry.conversation.startNodeId,
            };
        }
    }

    public ConversationState Get(string conversationId) =>
        _states.TryGetValue(conversationId, out var s) ? s : null;

    public IEnumerable<ConversationState> All => _states.Values;

    /// <summary>
    /// Retorna apenas as conversas que acabaram de chegar neste minuto.
    /// Chamadas repetidas com o mesmo minuto retornam lista vazia.
    /// </summary>
    public List<ConversationState> ProcessTime(int currentTotalMinutes)
    {
        var result = new List<ConversationState>();
        if (currentTotalMinutes <= _lastProcessedTotalMinutes)
            return result;
        _lastProcessedTotalMinutes = currentTotalMinutes;

        foreach (var state in _states.Values)
        {
            if (state.hasArrived)
                continue;

            if (
                state.entry.ArrivalTotalMinutes < 8 * 60
                || state.entry.ArrivalTotalMinutes > 16 * 60
            )
                continue;

            if (currentTotalMinutes < state.entry.ArrivalTotalMinutes)
                continue;

            state.hasArrived = true;
            state.hasUnread = true;
            result.Add(state);
        }

        return result;
    }

    public void MarkAsRead(string conversationId)
    {
        var s = Get(conversationId);
        if (s != null)
            s.hasUnread = false;
    }
}
