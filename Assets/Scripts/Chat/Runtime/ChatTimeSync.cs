using UnityEngine;

public class ChatTimeSync : MonoBehaviour
{
    [SerializeField]
    private ChatInbox inbox;

    private ChatInboxState _state;
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
            }
        }
    }

    private void OnTimeOrDayChanged()
    {
        if (_state == null)
            return;

        int day = DataController.CurrentDay;
        int totalMinutes = ClockController.CurrentHour * 60 + ClockController.CurrentMinute;
        var arrived = _state.ProcessTime(day, totalMinutes);

        foreach (var s in arrived)
            ActionController.SendAction("ChatConversationArrived", s.conversation.conversationId);
    }

}
