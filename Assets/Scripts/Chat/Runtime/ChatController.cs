using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatController : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField]
    private ChatInbox inbox;

    [SerializeField]
    private ChatTimeSync chatTimeSync;

    private ChatWindowView _windowView;
    private ChatInboxState _inboxState;
    private ChatInboxState.ConversationState _activeConversation;
    private Coroutine _npcFlowCoroutine;

    private readonly Dictionary<string, List<ChatMessageEntry>> _history = new();

    private void Awake()
    {
        if (chatTimeSync == null)
        {
            Debug.LogError("[ChatController] ChatTimeSync não atribuído!");
            return;
        }

        chatTimeSync.EnsureState();
        _inboxState = chatTimeSync.State;
        if (_inboxState == null)
            return;

        // Importa o estado salvo do JSON
        if (DataController.CurrentSave != null && DataController.CurrentSave.chatStates != null)
            ImportState(DataController.CurrentSave.chatStates);
    }

    private void OnEnable()
    {
        if (_inboxState == null)
            return;

        ActionController.SyncSignal("ChatInboxReset", OnInboxReset);
        ActionController.SyncSignal("SaveGame", SyncChatStateToSave);
        ActionController.SyncSignal<ChatWindowView>("ChatWindowCreated", OnWindowCreated);
        ActionController.SyncSignal<string>("ChatConversationArrived", OnConversationArrived);
    }

    private void OnDisable()
    {
        ActionController.UnsyncSignal("ChatInboxReset", OnInboxReset);
        ActionController.UnsyncSignal("SaveGame", SyncChatStateToSave);
        ActionController.UnsyncSignal<ChatWindowView>("ChatWindowCreated", OnWindowCreated);
        ActionController.UnsyncSignal<string>("ChatConversationArrived", OnConversationArrived);
    }

    // ============================================================
    // Janela criada
    // ============================================================
    private void OnWindowCreated(ChatWindowView window)
    {
        _windowView = window;
        _windowView.SetConversationClickHandler(OnConversationClicked);

        foreach (var state in _inboxState.All)
        {
            if (state.hasArrived)
            {
                string preview = GetPreviewForState(state);
                _windowView.AddOrUpdateConversationItem(state, preview);
            }
        }
    }

    // ============================================================
    // Relógio
    // ============================================================
    private void OnConversationArrived(string conversationId)
    {
        var state = _inboxState.Get(conversationId);
        if (state == null)
            return;

        string preview = GetPreviewForState(state);
        if (_windowView != null)
            _windowView.AddOrUpdateConversationItem(state, preview);
    }

    private void OnInboxReset()
    {
        if (_npcFlowCoroutine != null)
            StopCoroutine(_npcFlowCoroutine);

        _inboxState = chatTimeSync.State;

        _history.Clear();
        _activeConversation = null;

        if (DataController.CurrentSave != null)
        {
            DataController.CurrentSave.chatStates ??= new List<ChatSaveState>();
            DataController.CurrentSave.chatStates.Clear();
            DataController.SavingGame();
        }

        if (_windowView != null)
            _windowView.ShowEmptyState();
    }

    // ============================================================
    // Clique na lista
    // ============================================================
    private void OnConversationClicked(string conversationId)
    {
        if (_windowView == null)
            return;

        var state = _inboxState.Get(conversationId);
        if (state == null || !state.hasArrived)
            return;

        if (_activeConversation != null && _activeConversation != state)
        {
            if (_npcFlowCoroutine != null)
                StopCoroutine(_npcFlowCoroutine);
            _windowView.HideTypingIndicator();
        }

        state.hasUnread = false;
        SyncChatStateToSave();

        if (_windowView != null)
            _windowView.AddOrUpdateConversationItem(state, GetPreviewForState(state));

        _activeConversation = state;
        _windowView.ShowConversation(state.conversation);

        RebuildHistory(state);

        if (state.isFinished)
        {
            _windowView.HideChoices();
            return;
        }

        var currentNode = state.conversation.GetNode(state.currentNodeId);
        if (currentNode == null)
        {
            Debug.LogError($"[ChatController] Nó '{state.currentNodeId}' não encontrado.");
            return;
        }

        if (!_history.TryGetValue(state.conversation.conversationId, out var hist))
        {
            hist = new List<ChatMessageEntry>();
            _history[state.conversation.conversationId] = hist;
        }

        if (hist.Count == 0)
            PlayNode(currentNode);
        else
            ShowChoicesIfAny(currentNode);
    }

    // ============================================================
    // Fluxo de nós
    // ============================================================
    private void PlayNode(ChatNode node)
    {
        if (_activeConversation == null || _windowView == null)
            return;

        if (_activeConversation.currentNodeId != node.nodeId)
        {
            _activeConversation.currentNodeId = node.nodeId;
            SyncChatStateToSave();
        }

        if (_npcFlowCoroutine != null)
            StopCoroutine(_npcFlowCoroutine);
        _npcFlowCoroutine = StartCoroutine(NpcMessageFlow(node));
    }

    private IEnumerator NpcMessageFlow(ChatNode node)
    {
        _windowView.HideChoices();
        _windowView.ShowTypingIndicator();
        yield return new WaitForSeconds(node.typingDelay);
        _windowView.HideTypingIndicator();

        _windowView.AppendNpcMessage(node.npcMessage);
        RegisterHistory(node.npcMessage, true);
        SyncChatStateToSave();

        if (node.isEndNode)
        {
            _activeConversation.isFinished = true;
            SyncChatStateToSave();
            _windowView.HideChoices();
            yield break;
        }

        if (node.options != null && node.options.Count > 0)
        {
            _windowView.ShowChoices(node.options, opt => OnChoiceChosen(node, opt));
        }
        else if (!string.IsNullOrEmpty(node.nextNodeIdIfNoChoice))
        {
            var next = _activeConversation.conversation.GetNode(node.nextNodeIdIfNoChoice);
            if (next != null)
                PlayNode(next);
        }
    }

    private void ShowChoicesIfAny(ChatNode node)
    {
        if (node.options != null && node.options.Count > 0)
        {
            _windowView.ShowChoices(node.options, opt => OnChoiceChosen(node, opt));
        }
        else if (!string.IsNullOrEmpty(node.nextNodeIdIfNoChoice))
        {
            var next = _activeConversation.conversation.GetNode(node.nextNodeIdIfNoChoice);
            if (next != null)
                PlayNode(next);
        }
    }

    private void OnChoiceChosen(ChatNode fromNode, ChatOption chosen)
    {
        if (_activeConversation == null || _windowView == null)
            return;

        _windowView.HideChoices();
        _windowView.AppendPlayerMessage(chosen.optionText);
        RegisterHistory(chosen.optionText, false);

        _activeConversation.currentNodeId = chosen.nextNodeId;
        SyncChatStateToSave();

        if (chosen.securityScoreDelta != 0)
        {
            ActionController.SendAction("ChatSecurityScoreDelta", chosen.securityScoreDelta);
        }

        var next = _activeConversation.conversation.GetNode(chosen.nextNodeId);
        if (next == null)
        {
            Debug.LogWarning($"[ChatController] Nó '{chosen.nextNodeId}' inexistente.");
            return;
        }

        PlayNode(next);
    }

    // ============================================================
    // Histórico
    // ============================================================
    private void RegisterHistory(string text, bool isFromNpc)
    {
        if (_activeConversation == null)
            return;

        string id = _activeConversation.conversation.conversationId;
        if (!_history.TryGetValue(id, out var list))
        {
            list = new List<ChatMessageEntry>();
            _history[id] = list;
        }
        list.Add(new ChatMessageEntry { isFromNpc = isFromNpc, text = text });
    }

    private void RebuildHistory(ChatInboxState.ConversationState state)
    {
        if (!_history.TryGetValue(state.conversation.conversationId, out var list))
            return;

        foreach (var entry in list)
        {
            if (entry.isFromNpc)
                _windowView.AppendNpcMessage(entry.text);
            else
                _windowView.AppendPlayerMessage(entry.text);
        }
    }

    private string GetPreviewForState(ChatInboxState.ConversationState state)
    {
        if (_history.TryGetValue(state.conversation.conversationId, out var list) && list.Count > 0)
            return list[list.Count - 1].text;

        var start = state.conversation.GetStartNode();
        return start != null ? start.npcMessage : "Nova conversa";
    }

    // ============================================================
    // Persistência (integração com DataController)
    // ============================================================
    public List<ChatSaveState> ExportState()
    {
        var list = new List<ChatSaveState>();
        if (_inboxState == null)
            return list;

        foreach (var state in _inboxState.All)
        {
            var saveState = new ChatSaveState
            {
                conversationId = state.conversation.conversationId,
                currentNodeId = state.currentNodeId,
                isFinished = state.isFinished,
                hasArrived = state.hasArrived,
                hasUnread = state.hasUnread,
                messageHistory = new List<ChatMessageEntry>(),
            };

            if (_history.TryGetValue(state.conversation.conversationId, out var messages))
            {
                foreach (var entry in messages)
                {
                    saveState.messageHistory.Add(
                        new ChatMessageEntry { isFromNpc = entry.isFromNpc, text = entry.text }
                    );
                }
            }

            list.Add(saveState);
        }
        return list;
    }

    public void ImportState(List<ChatSaveState> savedStates)
    {
        if (savedStates == null || savedStates.Count == 0)
            return;

        foreach (var saved in savedStates)
        {
            if (saved == null)
                continue;

            var state = _inboxState.Get(saved.conversationId);
            if (state == null)
                continue;

            if (state.conversation.GetNode(saved.currentNodeId) != null)
                state.currentNodeId = saved.currentNodeId;
            else
                Debug.LogWarning(
                    $"[ChatController] Nó salvo '{saved.currentNodeId}' não encontrado. Usando o nó inicial."
                );

            state.isFinished = saved.isFinished;
            state.hasArrived = saved.hasArrived;
            state.hasUnread = saved.hasUnread;

            // Reconstrói o _history a partir do save
            if (saved.messageHistory != null && saved.messageHistory.Count > 0)
            {
                var messages = new List<ChatMessageEntry>();
                foreach (var entry in saved.messageHistory)
                {
                    messages.Add(
                        new ChatMessageEntry { isFromNpc = entry.isFromNpc, text = entry.text }
                    );
                }
                _history[saved.conversationId] = messages;
            }
        }
    }

    private void SyncChatStateToSave()
    {
        if (_inboxState == null)
        {
            Debug.LogError("[ChatController] Save ignorado: _inboxState nulo.");
            return;
        }

        if (DataController.CurrentSave == null)
        {
            Debug.LogError("[ChatController] Save ignorado: CurrentSave nulo.");
            return;
        }

        DataController.CurrentSave.chatStates = ExportState();
        DataController.SavingGame();
    }
}
