using UnityEngine;

public class ChatTimeSync : MonoBehaviour
{
    [SerializeField]
    private ChatInbox inbox;

    private ChatInboxState _state;
    private bool _hasLoggedInitialTimeSync;
    public ChatInboxState State => _state;

    private void Awake()
    {
        Debug.Log("[ChatTimeSync] Awake");
        EnsureState();
    }

    private void OnEnable()
    {
        ActionController.SyncSignal("TimeUpdated", OnTimeOrDayChanged);
        ActionController.SyncSignal("DayChanged", OnTimeOrDayChanged);
    }

    private void OnDisable()
    {
        ActionController.UnsyncSignal("TimeUpdated", OnTimeOrDayChanged);
        ActionController.UnsyncSignal("DayChanged", OnTimeOrDayChanged);
    }

    public void EnsureState()
    {
        if (_state != null)
            return;

        _state = new ChatInboxState(inbox);
        int savedStateCount = DataController.CurrentSave?.chatStates?.Count ?? 0;
        Debug.Log($"[ChatTimeSync] Importando {savedStateCount} estados");

        // Importa o estado salvo (hasArrived, currentNodeId, etc.)
        if (DataController.CurrentSave != null && DataController.CurrentSave.chatStates != null)
        {
            foreach (var saved in DataController.CurrentSave.chatStates)
            {
                if (saved == null)
                    continue;
                var conv = _state.Get(saved.conversationId);
                if (conv == null)
                    continue;

                if (conv.conversation.GetNode(saved.currentNodeId) != null)
                    conv.currentNodeId = saved.currentNodeId;
                else
                    Debug.LogWarning(
                        $"[ChatTimeSync] Nó '{saved.currentNodeId}' não encontrado. Usando nó inicial."
                    );

                conv.isFinished = saved.isFinished;
                conv.hasArrived = saved.hasArrived;
                conv.hasUnread = saved.hasUnread;
                // Sem isso, ao carregar o save a mensagem agendada ja mostrada chegava de novo.
                if (saved.shownScheduledNodeIds != null)
                    conv.shownScheduledNodeIds = new System.Collections.Generic.List<string>(
                        saved.shownScheduledNodeIds
                    );
            }
        }
    }

    private void OnTimeOrDayChanged()
    {
        int day = DataController.CurrentDay;
        int totalMinutes = (ClockController.CurrentHour * 60) + ClockController.CurrentMinute;

        if (!_hasLoggedInitialTimeSync)
        {
            Debug.Log($"[BadgeDebug] ChatTimeSync recebeu primeiro TimeUpdated/DayChanged: dia={day}, horário={ClockController.CurrentHour:D2}:{ClockController.CurrentMinute:D2}, estado inicializado={_state != null}.", this);
            _hasLoggedInitialTimeSync = true;
        }

        // Processa chegadas normais
        var arrivedConversations = State.ProcessTime(day, totalMinutes);
        if (arrivedConversations.Count > 0)
            Debug.Log($"[BadgeDebug] ChatTimeSync encontrou conversas chegando: {string.Join(", ", arrivedConversations.ConvertAll(c => c.conversation.conversationId))}.", this);
        foreach (var conversation in arrivedConversations)
        {
            string conversationId = conversation.conversation.conversationId;
            Debug.Log($"[BadgeDebug] ChatTimeSync disparando ChatConversationArrived('{conversationId}').", this);
            ActionController.SendAction("ChatConversationArrived", conversationId);
        }

        // Processa nós agendados
        var scheduledNodes = State.ProcessScheduledNodes(day, totalMinutes);
        if (scheduledNodes.Count > 0)
            Debug.Log($"[BadgeDebug] ChatTimeSync encontrou nós agendados: {string.Join(", ", scheduledNodes)}.", this);
        foreach (var payload in scheduledNodes)
        {
            Debug.Log($"[BadgeDebug] ChatTimeSync disparando ChatScheduledNodeArrived('{payload}').", this);
            ActionController.SendAction("ChatScheduledNodeArrived", payload);
        }
    }
}
