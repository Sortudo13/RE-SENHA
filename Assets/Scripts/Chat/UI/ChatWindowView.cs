using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// View da janela de chat. Expõe métodos para o ChatController popular a UI.
/// </summary>
public class ChatWindowView : MonoBehaviour
{
    [Header("Lista de conversas (painel esquerdo)")]
    [SerializeField]
    private Transform conversationListContent;

    [SerializeField]
    private ConversationListItemView conversationItemPrefab;

    [Header("Painel de chat (painel direito)")]
    [SerializeField]
    private GameObject emptyState;

    [SerializeField]
    private Image npcAvatarImage;

    [SerializeField]
    private TextMeshProUGUI npcNameText;

    [SerializeField]
    private Transform messageContent;

    [SerializeField]
    private Transform choiceContainer;

    [Header("Prefabs de mensagem")]
    [SerializeField]
    private GameObject bubbleNPCPrefab;

    [SerializeField]
    private GameObject bubblePlayerPrefab;

    [SerializeField]
    private GameObject bubbleTypingPrefab;

    [SerializeField]
    private GameObject choiceButtonPrefab;

    private readonly Dictionary<string, ConversationListItemView> _items = new();
    private GameObject _currentTypingBubble;

    private void Start()
    {
        ActionController.SendAction("ChatWindowCreated", this);
    }

    // ============================================================
    // Lista de conversas
    // ============================================================

    public void AddOrUpdateConversationItem(ChatInboxState.ConversationState state, string preview)
    {
        if (_items.TryGetValue(state.conversation.conversationId, out var item))
        {
            item.Bind(state, preview, _onConversationClicked);
            return;
        }

        var newItem = Instantiate(conversationItemPrefab, conversationListContent);
        newItem.Bind(state, preview, _onConversationClicked);
        _items[state.conversation.conversationId] = newItem;
    }

    private Action<string> _onConversationClicked;

    public void SetConversationClickHandler(Action<string> handler)
    {
        _onConversationClicked = handler;
    }

    // ============================================================
    // Painel direito
    // ============================================================
    public void ShowConversation(ChatConversation conversation)
    {
        emptyState.SetActive(false);

        npcNameText.text = conversation.npcName;
        if (conversation.npcAvatar != null)
            npcAvatarImage.sprite = conversation.npcAvatar;

        ClearMessages();
    }

    public void ShowEmptyState()
    {
        emptyState.SetActive(true);
        ClearMessages();
    }

    public void ClearMessages()
    {
        foreach (Transform child in messageContent)
            Destroy(child.gameObject);

        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);
    }

    // ============================================================
    // Bolhas de mensagem
    // ============================================================
    public void AppendNpcMessage(string text)
    {
        var bubble = Instantiate(bubbleNPCPrefab, messageContent);
        var tmp = bubble.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = text;
        ScrollToBottom();
    }

    public void AppendPlayerMessage(string text)
    {
        var bubble = Instantiate(bubblePlayerPrefab, messageContent);
        var tmp = bubble.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = text;
        ScrollToBottom();
    }

    public GameObject ShowTypingIndicator()
    {
        if (_currentTypingBubble != null)
            Destroy(_currentTypingBubble);

        _currentTypingBubble = Instantiate(bubbleTypingPrefab, messageContent);
        ScrollToBottom();
        return _currentTypingBubble;
    }

    public void HideTypingIndicator()
    {
        if (_currentTypingBubble != null)
        {
            Destroy(_currentTypingBubble);
            _currentTypingBubble = null;
        }
    }

    public void HideChoices()
    {
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);
    }

    // ============================================================
    // Escolhas
    // ============================================================
    public void ShowChoices(IReadOnlyList<ChatOption> options, Action<ChatOption> onChosen)
    {
        if (choiceContainer == null)
        {
            Debug.LogError("[ChatWindowView] ChoiceContainer não atribuído.", this);
            return;
        }

        if (choiceButtonPrefab == null)
        {
            Debug.LogError("[ChatWindowView] ChoiceButton prefab não atribuído.", this);
            return;
        }

        if (options == null || options.Count == 0)
        {
            Debug.LogWarning("[ChatWindowView] ShowChoices recebeu uma lista vazia.", this);
            HideChoices();
            return;
        }

        HideChoices();

        foreach (var opt in options)
        {
            var btn = Instantiate(choiceButtonPrefab, choiceContainer);
            var rt = btn.GetComponent<RectTransform>();
            if (rt != null)
                rt.localScale = Vector3.one;
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp == null)
            {
                Debug.LogError("[ChatWindowView] ChoiceButton não possui TextMeshProUGUI.", btn);
                continue;
            }
            tmp.text = opt.optionText;

            var b = btn.GetComponent<Button>();
            if (b == null)
            {
                Debug.LogError("[ChatWindowView] ChoiceButton não possui Button.", btn);
                continue;
            }

            var captured = opt;
            b.onClick.AddListener(() => onChosen.Invoke(captured));
        }

        if (choiceContainer is RectTransform choiceRect)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(choiceRect);

            var choicePanel = choiceRect.parent as RectTransform;
            if (choicePanel != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(choicePanel);

                if (choicePanel.parent is RectTransform chatArea)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(chatArea);
            }

            Canvas.ForceUpdateCanvases();
        }
    }

    // ============================================================
    // Auto-scroll
    // ============================================================
    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        var scrollRect = messageContent.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
