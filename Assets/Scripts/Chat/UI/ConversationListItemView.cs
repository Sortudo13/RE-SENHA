using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// View de uma linha da lista de conversas.
/// Instanciado pelo ChatController para cada entrada do ChatInbox.
/// </summary>
public class ConversationListItemView : MonoBehaviour
{
    [Header("Referências visuais")]
    [SerializeField]
    private Image avatarImage;

    [SerializeField]
    private TextMeshProUGUI npcNameText;

    [SerializeField]
    private TextMeshProUGUI previewText;

    [SerializeField]
    private TextMeshProUGUI timeText;

    [SerializeField]
    private GameObject unreadDot;

    private Button _button;
    private string _conversationId;
    private Action<string> _onClick;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(HandleClick);
    }

    public void Bind(ChatInboxState.ConversationState state, string preview, Action<string> onClick)
    {
        _conversationId = state.conversation.conversationId;
        _onClick = onClick;

        npcNameText.text = state.conversation.npcName;
        previewText.text = preview;
        timeText.text = $"{state.entry.arrivalHour:D2}:{state.entry.arrivalMinute:D2}";
        unreadDot.SetActive(state.hasUnread);

        if (state.conversation.npcAvatar != null)
            avatarImage.sprite = state.conversation.npcAvatar;
    }

    private void HandleClick()
    {
        if (_onClick != null)
            _onClick.Invoke(_conversationId);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(HandleClick);
    }
}
