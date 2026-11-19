using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChatConversation", menuName = "Re:Senha/Chat/Chat Conversation")]
public class ChatConversation : ScriptableObject
{
    [Header("Identificação")]
    public string conversationId;

    [Header("NPC padrão da conversa")]
    public string npcName;
    public Sprite npcAvatar;

    [Header("Nós")]
    public List<ChatNode> nodes = new List<ChatNode>();

    [Tooltip("nodeId do nó onde a conversa começa.")]
    public string startNodeId;

    // Cache interno (não serializado)
    private Dictionary<string, ChatNode> _nodeLookup;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void OnValidate()
    {
        BuildLookup();
        ValidateReferences();
    }

    private void BuildLookup()
    {
        _nodeLookup = new Dictionary<string, ChatNode>();
        foreach (var node in nodes)
        {
            if (node == null || string.IsNullOrEmpty(node.nodeId))
                continue;

            if (_nodeLookup.ContainsKey(node.nodeId))
            {
                Debug.LogError(
                    $"[ChatConversation] ID duplicado '{node.nodeId}' em '{name}'.",
                    this
                );
                continue;
            }
            _nodeLookup[node.nodeId] = node;
        }
    }

    /// <summary>Retorna o nó pelo ID, ou null se não existir.</summary>
    public ChatNode GetNode(string id)
    {
        if (_nodeLookup == null)
            BuildLookup();

        if (string.IsNullOrEmpty(id))
            return null;

        return _nodeLookup.TryGetValue(id, out var node) ? node : null;
    }

    public ChatNode GetStartNode()
    {
        return GetNode(startNodeId);
    }

    private void ValidateReferences()
    {
        if (string.IsNullOrEmpty(startNodeId))
        {
            Debug.LogWarning($"[{name}] startNodeId vazio.", this);
        }
        else if (GetNode(startNodeId) == null)
        {
            Debug.LogError($"[{name}] startNodeId '{startNodeId}' não existe.", this);
        }

        foreach (var node in nodes)
        {
            if (node == null)
                continue;

            foreach (var opt in node.options)
            {
                if (string.IsNullOrEmpty(opt.nextNodeId))
                {
                    Debug.LogWarning(
                        $"[{name}] Nó '{node.nodeId}' tem opção sem nextNodeId.",
                        this
                    );
                    continue;
                }
                if (GetNode(opt.nextNodeId) == null)
                {
                    Debug.LogError(
                        $"[{name}] Nó '{node.nodeId}' aponta para '{opt.nextNodeId}' inexistente.",
                        this
                    );
                }
            }
        }
    }
}
