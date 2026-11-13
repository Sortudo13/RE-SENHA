using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChatNode", menuName = "Re:Senha/Chat/Chat Node")]
public class ChatNode : ScriptableObject
{
    [Header("Identificação")]
    [Tooltip("ID único DENTRO da conversa. Ex.: 'inicio', 'pede_senha', 'fim_negou'.")]
    public string nodeId;

    [Header("Mensagem do NPC")]
    [Tooltip("Nome exibido no header da janela (ou no nó, se quiser variar).")]
    public string npcName;

    [Tooltip("Avatar do NPC.")]
    public Sprite npcAvatar;

    [Tooltip("Mensagem do NPC. Suporta rich text do TextMeshPro.")]
    [TextArea(3, 10)]
    public string npcMessage;

    [Header("Ritmo")]
    [Tooltip("Segundos de 'digitando...' antes da mensagem aparecer.")]
    [Min(0f)]
    public float typingDelay = 1.5f;

    [Header("Fluxo")]
    [Tooltip("Opções de resposta. Se vazio, usa nextNodeIdIfNoChoice.")]
    public List<ChatOption> options = new List<ChatOption>();

    [Tooltip("Se não houver opções, avança para este nó automaticamente.")]
    public string nextNodeIdIfNoChoice;

    [Tooltip("Marca este nó como final da conversa.")]
    public bool isEndNode;
}
