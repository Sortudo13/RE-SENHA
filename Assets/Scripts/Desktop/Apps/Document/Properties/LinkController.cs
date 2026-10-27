using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LinkController : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI p_TextMeshPro;

    void Awake()
    {
        p_TextMeshPro = GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        p_TextMeshPro.ForceMeshUpdate();

        // Verifica se o clique foi em cima de um link válido do TextMeshPro
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(p_TextMeshPro, eventData.position, eventData.pressEventCamera);

        if (linkIndex != -1) // -1 significa que não clicou em um link
        {
            TMP_LinkInfo linkInfo = p_TextMeshPro.textInfo.linkInfo[linkIndex];

            // Pega o ID que você definiu dentro das aspas da tag <link="id">
            string linkID = linkInfo.GetLinkID();
            string[] command = linkID.Split('|',2);

            // Executa a ação baseada no ID clicado
            ActionController.SendAction(command[0], command[1]);
        }
    }
}
/*
Uso: <color=#0000FF><link="ID_DA_ACAO">Texto Clicável</link></color>

Para imagens, crie um TextMeshPro -> Sprite Asset e mova para "Assets/TextMesh Pro/Resources/Sprite Assets/"
Uso: <sprite name=\"NOME_SPRITE\" size=100%>


O TextMesh Pro suporta uma grande variedade de tags de Rich Text inspiradas no HTML e no XML. Com elas, você pode modificar o estilo, a cor, o tamanho e até a posição do texto caractere por caractere.
Aqui estão as principais tags divididas por categoria:

Estilização de Texto

• <b>Negrito</b> – Deixa o texto em negrito.
• <i>Itálico</i> – Deixa o texto em itálico.
• <s>Riscado</s> – Cria uma linha horizontal sobre o texto (strikethrough).
• <u>Sublinhado</u> – Como vimos, adiciona uma linha abaixo do texto.
• <sup>Sobrescrito</sup> – Eleva o texto (ex: m²).
• <sub>Subscrito</sub> – Rebaixa o texto (ex: H₂O).
• <mark=#FFFF00AA>Destacado</mark> – Adiciona um fundo colorido ao texto (como um marca-texto).

Cores (Color)

• <color=yellow>Texto Amarelo</color> – Muda a cor por nome.
• <color=#FF0000>Texto Vermelho</color> – Usa código Hexadecimal.
• Texto <alpha=#88>Semistransparente – Altera apenas a opacidade (transparência) do texto de forma rápida.

Tamanho e Alinhamento

• <size=48>Texto Grande</size> – Define o tamanho absoluto da fonte em pixels/pontos.
• <size=+10>Maior</size> ou <size=-5>Menor</size> – Ajusta o tamanho de forma relativa ao tamanho atual.
• <align=center>Texto Centralizado</align> – Alinha o trecho especificamente à esquerda, direita, centro ou justificado.

Posição e Espaçamento (Avançado)

• <line-height=150%>Espaçamento</line-height> – Controla a altura da linha atual.
• <voffset=1em>Texto Flutuante</voffset> – Desloca o texto verticalmente (ótimo para alinhar ícones manuais).
• <indent=15%>Texto recuado</indent> – Cria uma margem esquerda para o parágrafo.

Recursos Exclusivos (Não HTML)

• <sprite=index> – Insere um ícone/emoji diretamente na linha do texto (gerado por um Sprite Asset do TMP).
• <noparse><b>Isso não fica em negrito</b></noparse> – Faz o TMP ignorar as tags e exibi-las como texto puro na tela.
 */