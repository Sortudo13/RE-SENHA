using System;
using UnityEngine;

// Como a escolha conta na conta do dia: Neutra nao conta; Errada da advertencia, igual ao email.
public enum ChatOptionOutcome
{
    Neutral,
    Correct,
    Wrong,
}

[Serializable]
public class ChatOption
{
    [Tooltip("Mensagem completa que o jogador envia no chat ao escolher.")]
    public string optionText;

    [Tooltip("Resumo mostrado no botão de escolha. Vazio usa o optionText.")]
    public string shortText;

    // Texto do botão: o resumo, se houver; senão a mensagem completa.
    public string ButtonText => string.IsNullOrWhiteSpace(shortText) ? optionText : shortText;

    [Tooltip("ID do próximo ChatNode para onde esta escolha leva.")]
    public string nextNodeId;

    [Tooltip("Neutra não conta. Correta/Errada contam como resposta do dia; Errada dá advertência.")]
    public ChatOptionOutcome outcome = ChatOptionOutcome.Neutral;

    [Tooltip("Texto do feedback para Correta/Errada. Vazio usa o texto padrão.")]
    [TextArea]
    public string outcomeFeedback;

    [Tooltip("Impacto na pontuação de segurança (pode ser negativo).")]
    public int securityScoreDelta;

    [Tooltip("Tag interna para logs/relatórios (não aparece no jogo).")]
    public string feedbackTag;

    [Tooltip("triggerId de um email do JSON do dia, que chega emailDelay segundos depois desta escolha.")]
    public string emailTriggerId;
    public float emailDelay = 3f;
}
