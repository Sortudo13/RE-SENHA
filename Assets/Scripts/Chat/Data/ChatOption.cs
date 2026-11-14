using System;
using UnityEngine;

[Serializable]
public class ChatOption
{
    [Tooltip("Texto que aparece no botão de escolha.")]
    public string optionText;

    [Tooltip("ID do próximo ChatNode para onde esta escolha leva.")]
    public string nextNodeId;

    [Tooltip("Impacto na pontuação de segurança (pode ser negativo).")]
    public int securityScoreDelta;

    [Tooltip("Tag interna para logs/relatórios (não aparece no jogo).")]
    public string feedbackTag;
}
