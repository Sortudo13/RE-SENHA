using UnityEngine;

/// <summary>
/// Escuta o relógio e propaga chegadas de conversas via ActionController.
/// </summary>
public class ChatTimeSync : MonoBehaviour
{
    [SerializeField]
    private ChatInbox inbox;

    private ChatInboxState _state;

    public ChatInboxState State => _state;

    private void Awake()
    {
        _state = new ChatInboxState(inbox);
        ActionController.SyncSignal("TimeUpdated", OnTimeUpdated);
        ActionController.SyncSignal("Resetar Jogo", OnResetGame);
    }

    private void OnDisable()
    {
        ActionController.UnsyncSignal("TimeUpdated", OnTimeUpdated);
        ActionController.UnsyncSignal("Resetar Jogo", OnResetGame);
    }

    private void OnTimeUpdated()
    {
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
