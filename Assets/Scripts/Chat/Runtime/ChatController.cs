using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orquestra a lógica de chat: escuta o relógio, gerencia o estado da inbox,
/// executa o fluxo de nós e conversa com o ChatWindowView para atualizar a UI.
/// </summary>
public class ChatController : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField]
    private ChatInbox inbox;

    private ChatWindowView _windowView;
    private ChatInboxState _inboxState;
    private ChatInboxState.ConversationState _activeConversation;
    private Coroutine _npcFlowCoroutine;
    private readonly Dictionary<string, List<string>> _history = new();

    [SerializeField]
    private ChatTimeSync chatTimeSync;

    private void Awake()
    {
        if (chatTimeSync != null)
            _inboxState = chatTimeSync.State;
        else
        {
            Debug.LogError("[ChatController] ChatTimeSync não atribuído!");
            return;
        }

        ActionController.SyncSignal("Resetar Jogo", OnResetGame);
        ActionController.SyncSignal<ChatWindowView>("ChatWindowCreated", OnWindowCreated);
        ActionController.SyncSignal<string>("ChatConversationArrived", OnConversationArrived);
    }

    private void OnDisable()
    {
        ActionController.UnsyncSignal("Resetar Jogo", OnResetGame);
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

        // Reconstrói a lista com as conversas que já chegaram
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

        // Só atualiza a UI se a janela estiver aberta
        if (_windowView != null)
            _windowView.AddOrUpdateConversationItem(state, preview);
    }

    private void OnResetGame()
    {
        if (_npcFlowCoroutine != null)
            StopCoroutine(_npcFlowCoroutine);

        if (chatTimeSync != null && chatTimeSync.State != null)
            _inboxState = chatTimeSync.State;
        else
            _inboxState = new ChatInboxState(inbox); // fallback

        _history.Clear();
        _activeConversation = null;

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
            hist = new List<string>();
            _history[state.conversation.conversationId] = hist;
        }

        if (hist.Count == 0)
        {
            PlayNode(currentNode);
        }
        else
        {
            ShowChoicesIfAny(currentNode);
        }
    }

    // ============================================================
    // Fluxo de nós
    // ============================================================
    private void PlayNode(ChatNode node)
    {
        if (_activeConversation == null || _windowView == null)
            return;
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
        RegisterHistory(node.npcMessage);

        if (node.isEndNode)
        {
            _activeConversation.isFinished = true;
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
        RegisterHistory(chosen.optionText);

        _activeConversation.currentNodeId = chosen.nextNodeId;

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
    private void RegisterHistory(string text)
    {
        if (_activeConversation == null)
            return;

        string id = _activeConversation.conversation.conversationId;
        if (!_history.TryGetValue(id, out var list))
        {
            list = new List<string>();
            _history[id] = list;
        }
        list.Add(text);
    }

    private void RebuildHistory(ChatInboxState.ConversationState state)
    {
        if (!_history.TryGetValue(state.conversation.conversationId, out var list))
            return;

        for (int i = 0; i < list.Count; i++)
        {
            if (i % 2 == 0)
                _windowView.AppendNpcMessage(list[i]);
            else
                _windowView.AppendPlayerMessage(list[i]);
        }
    }

    private string GetPreviewForState(ChatInboxState.ConversationState state)
    {
        if (_history.TryGetValue(state.conversation.conversationId, out var list) && list.Count > 0)
            return list[list.Count - 1];

        var start = state.conversation.GetStartNode();
        return start != null ? start.npcMessage : "Nova conversa";
    }
}
