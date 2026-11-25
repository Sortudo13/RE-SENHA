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
    private const string ChatAppName = "Chat";

    private readonly Dictionary<string, List<ChatMessageEntry>> _history = new();

    private void Awake()
    {
        Debug.Log("[ChatController] Awake: INICIANDO");
        _history.Clear();
        _activeConversation = null;

        if (chatTimeSync == null)
        {
            Debug.LogError("[ChatController] ChatTimeSync não atribuído!");
            return;
        }

        chatTimeSync.EnsureState();
        _inboxState = chatTimeSync.State;

        // Importa o histórico visual das mensagens
        if (DataController.CurrentSave != null && DataController.CurrentSave.chatStates != null)
            ImportHistoryOnly(DataController.CurrentSave.chatStates);

        Debug.Log("[ChatController] Awake: _history limpo, _activeConversation=null");
    }

    private void OnEnable()
    {
        ActionController.SyncSignal("SaveGame", SyncChatStateToSave);
        ActionController.SyncSignal<ChatWindowView>("ChatWindowCreated", OnWindowCreated);
        ActionController.SyncSignal<string>("ChatConversationArrived", OnConversationArrived);
        ActionController.SyncSignal<string>("ChatScheduledNodeArrived", OnScheduledNodeArrived);
    }

    private void OnDisable()
    {
        ActionController.UnsyncSignal("SaveGame", SyncChatStateToSave);
        ActionController.UnsyncSignal<ChatWindowView>("ChatWindowCreated", OnWindowCreated);
        ActionController.UnsyncSignal<string>("ChatConversationArrived", OnConversationArrived);
        ActionController.UnsyncSignal<string>("ChatScheduledNodeArrived", OnScheduledNodeArrived);
    }

    private IEnumerator Start()
    {
        if (_inboxState == null)
            yield break;

        // A chegada pode ter sido processada antes do IconController assinar o sinal,
        // e conversas não lidas também podem ter sido restauradas do save.
        yield return null;

        foreach (var state in _inboxState.All)
        {
            if (IsVisibleToday(state) && state.hasUnread)
            {
                Debug.Log($"[BadgeDebug] ChatController disparando ShowNotificationBadge('{ChatAppName}') em Start (reconciliação de conversas não lidas).", this);
                ActionController.SendAction("ShowNotificationBadge", ChatAppName);
                yield break;
            }
        }
    }

    // So as conversas do dia aparecem; as de dias anteriores continuam no save, mas somem da lista.
    private static bool IsVisibleToday(ChatInboxState.ConversationState state)
    {
        return state != null
            && state.hasArrived
            && ChatInboxState.IsVisibleOnDay(state, DataController.CurrentDay);
    }

    private void OnScheduledNodeArrived(string payload)
    {
        // Payload: "conversationId:nodeId"
        var parts = payload.Split(':');
        if (parts.Length != 2)
            return;

        string conversationId = parts[0];
        string nodeId = parts[1];

        var state = _inboxState.Get(conversationId);
        if (state == null)
            return;

        var node = state.conversation.GetNode(nodeId);
        if (node == null)
            return;

        // A referência da view permanece após minimizar a janela; só é visível enquanto ativa.
        if (_activeConversation == state && _windowView != null && _windowView.isActiveAndEnabled)
        {
            state.isFinished = false;
            state.currentNodeId = nodeId;
            PlayNode(node);
            return;
        }

        // Se a conversa está fechada, atualiza a lista e marca como não lida
        state.isFinished = false;
        state.currentNodeId = nodeId;
        state.hasUnread = true;
        Debug.Log($"[BadgeDebug] ChatController: conversa '{conversationId}' marcada como não lida. Disparando ShowNotificationBadge.", this);

        Debug.Log($"[BadgeDebug] ChatController disparando ShowNotificationBadge('{ChatAppName}') em OnScheduledNodeArrived.", this);
        ActionController.SendAction("ShowNotificationBadge", ChatAppName);

        // Adiciona ao histórico visual
        if (!_history.TryGetValue(conversationId, out var list))
        {
            list = new List<ChatMessageEntry>();
            _history[conversationId] = list;
        }
        list.Add(new ChatMessageEntry { isFromNpc = true, text = node.npcMessage });

        // Atualiza a UI da lista de conversas
        if (_windowView != null)
            _windowView.AddOrUpdateConversationItem(state, node.npcMessage);

        SyncChatStateToSave();
    }

    // ============================================================
    // Janela criada
    // ============================================================
    private void OnWindowCreated(ChatWindowView window)
    {
        _windowView = window;
        _windowView.SetConversationClickHandler(OnConversationClicked);

        ActionController.SendAction("HideNotificationBadge", ChatAppName);

        if (_inboxState == null)
            return;

        foreach (var state in _inboxState.All)
        {
            if (IsVisibleToday(state))
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
        Debug.Log($"[BadgeDebug] ChatController recebeu ChatConversationArrived('{conversationId}'). inboxState inicializado={_inboxState != null}.", this);
        var state = _inboxState.Get(conversationId);
        if (state == null)
        {
            Debug.LogWarning($"[BadgeDebug] ChatController não encontrou a conversa '{conversationId}' no inbox.", this);
            return;
        }

        Debug.Log($"[BadgeDebug] ChatController: conversa '{conversationId}' marcada como não lida. Disparando ShowNotificationBadge. hasUnread={state.hasUnread}", this);
        string preview = GetPreviewForState(state);
        if (_windowView != null)
            _windowView.AddOrUpdateConversationItem(state, preview);

        Debug.Log($"[BadgeDebug] ChatController disparando ShowNotificationBadge('{ChatAppName}') em OnConversationArrived.", this);
        ActionController.SendAction("ShowNotificationBadge", ChatAppName);
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
        if (state == null || !IsVisibleToday(state))
            return;

        ActionController.SendAction(TutorialSignals.ChatOpened, conversationId);

        if (_activeConversation != null && _activeConversation != state)
        {
            if (_npcFlowCoroutine != null)
                StopCoroutine(_npcFlowCoroutine);
            _windowView.HideTypingIndicator();
        }

        state.hasUnread = false;
        SyncChatStateToSave();

        bool anyUnread = false;
        foreach (var s in _inboxState.All)
        {
            if (s.hasUnread && IsVisibleToday(s))
            {
                anyUnread = true;
                break;
            }
        }
        if (!anyUnread)
            ActionController.SendAction("HideNotificationBadge", ChatAppName);

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
            var missingNodeId = state.currentNodeId;
            currentNode = state.conversation.GetStartNode();
            if (currentNode != null)
            {
                state.currentNodeId = currentNode.nodeId;
                Debug.LogWarning(
                    $"[ChatController] Nó '{missingNodeId}' não encontrado em '{state.conversation.conversationId}', usando o inicial."
                );
            }
            else
            {
                Debug.LogError(
                    $"[ChatController] Nenhum nó inicial válido em '{state.conversation.conversationId}'. startNodeId='{state.conversation.startNodeId}', nodes.Count={state.conversation.nodes?.Count ?? 0}."
                );
                return;
            }
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
        StartCoroutine(SendEmailLater(node.emailTriggerId, node.emailDelay));

        if (node.isEndNode)
        {
            _activeConversation.isFinished = true;
            SyncChatStateToSave();
            _windowView.HideChoices();
            // O ChatDecisionController confere o fim do dia (pode ter sido a ultima pendente).
            ActionController.SendAction(
                ChatDecisionController.ConversationFinishedAction,
                _activeConversation.conversation.conversationId
            );
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

    // O SenderController entrega o email do JSON do dia com esse triggerId.
    private IEnumerator SendEmailLater(string triggerId, float delay)
    {
        if (string.IsNullOrWhiteSpace(triggerId))
            yield break;
        yield return new WaitForSeconds(delay);
        ActionController.SendAction("TriggerEmail", triggerId);
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

        ActionController.SendAction(
            TutorialSignals.ChoiceMade,
            _activeConversation.conversation.conversationId
        );

        _windowView.HideChoices();
        _windowView.AppendPlayerMessage(chosen.optionText);
        RegisterHistory(chosen.optionText, false);

        _activeConversation.currentNodeId = chosen.nextNodeId;
        StartCoroutine(SendEmailLater(chosen.emailTriggerId, chosen.emailDelay));

        // Conta antes de salvar, para o erro ir junto no save (SavingGame grava CurrentErrorCount).
        // GameOver apaga o save e troca de cena: nao salvar nem seguir (salvaria no save novo).
        if (ChatDecisionController.RegisterChoice(chosen))
            return;

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
                shownScheduledNodeIds = new List<string>(state.shownScheduledNodeIds), // NOVO
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
            state.shownScheduledNodeIds =
                saved.shownScheduledNodeIds != null
                    ? new List<string>(saved.shownScheduledNodeIds)
                    : new List<string>();

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

    private void ImportHistoryOnly(List<ChatSaveState> savedStates)
    {
        if (savedStates == null)
            return;

        foreach (var saved in savedStates)
        {
            if (saved?.messageHistory == null || saved.messageHistory.Count == 0)
                continue;

            var list = new List<ChatMessageEntry>();
            foreach (var entry in saved.messageHistory)
                list.Add(new ChatMessageEntry { isFromNpc = entry.isFromNpc, text = entry.text });

            _history[saved.conversationId] = list;
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
