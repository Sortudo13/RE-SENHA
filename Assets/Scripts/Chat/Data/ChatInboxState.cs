using System.Collections.Generic;

public class ChatInboxState
{
    public class ConversationState
    {
        public ChatConversation conversation;
        public ChatInbox.Entry entry;
        public bool hasArrived;
        public bool hasUnread;
        public bool isFinished;
        public string currentNodeId;
    }

    private readonly Dictionary<string, ConversationState> _states = new();
    private readonly ChatInbox _inbox;
    private int _lastProcessedDay = -1;
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
    /// Processa chegada de conversas com base no dia e minuto atuais.
    /// Conversas cujo arrivalDay < currentDay e que ainda não chegaram são
    /// descartadas (não chegam retroativamente).
    /// </summary>
    public List<ConversationState> ProcessTime(int currentDay, int currentTotalMinutes)
    {
        var result = new List<ConversationState>();

        // Só processa se o dia ou o minuto mudou
        if (currentDay == _lastProcessedDay && currentTotalMinutes <= _lastProcessedTotalMinutes)
            return result;

        _lastProcessedDay = currentDay;
        _lastProcessedTotalMinutes = currentTotalMinutes;

        foreach (var state in _states.Values)
        {
            if (state.hasArrived)
                continue;

            // Não retroativo: se o dia atual passou do dia agendado, descarta
            if (currentDay > state.entry.arrivalDay)
                continue;

            // Ainda não é o dia
            if (currentDay < state.entry.arrivalDay)
                continue;

            // Mesmo dia: verifica o minuto
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
