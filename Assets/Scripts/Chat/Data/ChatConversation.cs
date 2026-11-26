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

    [Header("Fim do dia")]
    [Tooltip(
        "Se marcado, o \"Avançar Dia\" só aparece depois que esta conversa terminar (no dia em que ela chega)."
    )]
    public bool blocksDayEnd;

    /// <summary>True se algum nó tem opção Correta/Errada (conta na conta do dia).</summary>
    public bool HasJudgedOption()
    {
        foreach (var node in nodes)
        {
            if (node?.options == null)
                continue;

            foreach (var option in node.options)
                if (option != null && option.outcome != ChatOptionOutcome.Neutral)
                    return true;
        }
        return false;
    }

    // Cache interno (não serializado)
    private Dictionary<string, ChatNode> _nodeLookup;

    private void OnEnable()
    {
        BuildLookup();
    }

    // Flag para ignorar o OnValidate durante a geração
    public static bool IsGenerating = false;

    private void OnValidate()
    {
        if (IsGenerating)
            return;

        BuildLookup();

        if (nodes == null || nodes.Count == 0)
            return;
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null)
                return;
            ValidateReferences();
        };
        #endif
    }

    private void ValidateReferences()
    {
        if (nodes == null || nodes.Count == 0)
            return;

        if (!blocksDayEnd && HasJudgedOption())
        {
            Debug.LogWarning(
                $"[{name}] Tem opção Correta/Errada, mas não segura o fim do dia (blocksDayEnd desmarcado).",
                this
            );
        }

        if (string.IsNullOrEmpty(startNodeId))
        {
            Debug.LogWarning($"[{name}] startNodeId vazio.", this);
        }
        else if (!NodeExists(startNodeId))
        {
            Debug.LogError($"[{name}] startNodeId '{startNodeId}' não existe.", this);
        }

        foreach (var node in nodes)
        {
            if (node == null)
                continue;

            if (node.options != null)
            {
                foreach (var opt in node.options)
                {
                    if (string.IsNullOrEmpty(opt.nextNodeId))
                        continue;

                    if (!NodeExists(opt.nextNodeId))
                    {
                        Debug.LogError(
                            $"[{name}] Nó '{node.nodeId}' aponta para '{opt.nextNodeId}' inexistente.",
                            this
                        );
                    }
                }
            }

            if (
                !string.IsNullOrEmpty(node.nextNodeIdIfNoChoice)
                && !NodeExists(node.nextNodeIdIfNoChoice)
            )
            {
                Debug.LogError(
                    $"[{name}] Nó '{node.nodeId}' aponta (no choice) para '{node.nextNodeIdIfNoChoice}' inexistente.",
                    this
                );
            }
        }
    }

    // Helper que varre a lista real, sem usar cache
    private bool NodeExists(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId))
            return false;
        foreach (var n in nodes)
        {
            if (n != null && n.nodeId == nodeId)
                return true;
        }
        return false;
    }

    private void BuildLookup()
    {
        _nodeLookup = new Dictionary<string, ChatNode>();
        if (nodes == null)
            return;

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

    public void RefreshCache()
    {
        BuildLookup();
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
}
