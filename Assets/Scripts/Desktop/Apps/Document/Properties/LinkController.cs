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
        // Verifica se o clique foi em cima de um link válido do TextMeshPro
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(p_TextMeshPro, eventData.position, eventData.pressEventCamera);

        if (linkIndex != -1) // -1 significa que não clicou em um link
        {
            TMP_LinkInfo linkInfo = p_TextMeshPro.textInfo.linkInfo[linkIndex];

            // Pega o ID que você definiu dentro das aspas da tag <link="id">
            string linkID = linkInfo.GetLinkID();

            // Executa a ação baseada no ID clicado
            ActionController.SendAction(linkID);
        }
    }
}
// Uso: <color=#0000FF><link="ID_DA_ACAO">Texto Clicável</link></color>
//
// Para imagens, crie um TextMeshPro -> Sprite Asset e mova para "Assets/TextMesh Pro/Resources/Sprite Assets/"
// Uso: <sprite name=\"NOME_SPRITE\" size=100%>