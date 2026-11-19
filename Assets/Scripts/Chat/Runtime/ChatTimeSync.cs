using UnityEngine;

/// <summary>
/// Escuta o relógio e propaga chegadas de conversas via ActionController.
/// </summary>
public class ChatTimeSync : MonoBehaviour
{
    [SerializeField] private ChatInbox inbox;
    private ChatInboxState _state;

    public ChatInboxState State => _state;

    private void Awake()
    {
        EnsureState();
    }

    private void OnEnable()
    {
        ActionController.SyncSignal("TimeUpdated", OnTimeUpdated);
        ActionController.SyncSignal("Resetar Jogo", OnResetGame);
    }

    public void EnsureState()
    {
        if (_state != null) return;

        if (inbox == null)
        {
            Debug.LogError("[ChatTimeSync] ChatInbox não atribuído.", this);
            return;
        }

        _state = new ChatInboxState(inbox);

        if (DataController.CurrentSave != null && DataController.CurrentSave.chatStates != null)
        {
            foreach (var saved in DataController.CurrentSave.chatStates)
            {
                if (saved == null) continue;
                var convState = _state.Get(saved.conversationId);
                if (convState == null)
                    continue;

                if (convState.conversation.GetNode(saved.currentNodeId) != null)
                    convState.currentNodeId = saved.currentNodeId;
                else
                    Debug.LogWarning($"[ChatTimeSync] Nó '{saved.currentNodeId}' não encontrado.");

                convState.isFinished = saved.isFinished;
                convState.hasArrived = saved.hasArrived;
                convState.hasUnread = saved.hasUnread;
            }
        }
    }

    private void OnDisable()
    {
        ActionController.UnsyncSignal("TimeUpdated", OnTimeUpdated);
        ActionController.UnsyncSignal("Resetar Jogo", OnResetGame);
    }

    private void OnTimeUpdated()
    {
        if (_state == null)
            return;

        int total = ClockController.CurrentHour * 60 + ClockController.CurrentMinute;
        var arrived = _state.ProcessTime(total);

        foreach (var s in arrived)
        {
            ActionController.SendAction("ChatConversationArrived", s.conversation.conversationId);
        }
    }

    private void OnResetGame()
    {
        _state = new ChatInboxState(inbox);
        ActionController.SendAction("ChatInboxReset");
    }
}
