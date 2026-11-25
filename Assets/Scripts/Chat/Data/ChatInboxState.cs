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
        public List<string> shownScheduledNodeIds = new();
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
    /// Conversas de dias anteriores somem do chat. A conversa aparece no dia em que chega
    /// e nos dias em que tem uma mensagem agendada (scheduledDay).
    /// </summary>
    public static bool IsVisibleOnDay(ConversationState state, int day)
    {
        if (state?.entry == null || state.conversation == null)
            return false;
        if (state.entry.arrivalDay == day)
            return true;

        foreach (var node in state.conversation.nodes)
        {
            if (node != null && node.HasSchedule && node.scheduledDay == day)
                return true;
        }
        return false;
    }

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

    public List<string> ProcessScheduledNodes(int currentDay, int currentTotalMinutes)
    {
        var nodesToShow = new List<string>();

        foreach (var state in _states.Values)
        {
            // Conversa de outro dia esta escondida: nao recebe mensagem atrasada.
            if (!state.hasArrived || state.conversation == null || !IsVisibleOnDay(state, currentDay))
                continue;

            ChatNode nextDue = null;

            foreach (var node in state.conversation.nodes)
            {
                if (node == null || !node.HasSchedule)
                    continue;
                if (state.shownScheduledNodeIds.Contains(node.nodeId))
                    continue;

                bool timeReached =
                    (currentDay > node.scheduledDay)
                    || (
                        currentDay == node.scheduledDay
                        && currentTotalMinutes >= node.ScheduledTotalMinutes
                    );

                if (!timeReached)
                    continue;

                // Lógica Condicional (Defer)
                // Se o nó exige uma resposta e o jogador AINDA ESTÁ naquele nó (não respondeu), pula.
                if (
                    !string.IsNullOrEmpty(node.requiresAnswerToNodeId)
                    && state.currentNodeId == node.requiresAnswerToNodeId
                )
                {
                    continue;
                }

                // Regra da race condition: pega o nó mais antigo (menor tempo) que já venceu
                if (nextDue == null || node.ScheduledTotalMinutes < nextDue.ScheduledTotalMinutes)
                {
                    nextDue = node;
                }
            }

            if (nextDue != null)
            {
                state.shownScheduledNodeIds.Add(nextDue.nodeId);
                nodesToShow.Add($"{state.conversation.conversationId}:{nextDue.nodeId}");
            }
        }

        return nodesToShow;
    }

    public void MarkAsRead(string conversationId)
    {
        var s = Get(conversationId);
        if (s != null)
            s.hasUnread = false;
    }
}
