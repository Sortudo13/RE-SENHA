using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private RectTransform rectTransform; 
    private Canvas canvas;
    private RectTransform canvasRect;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Pega o Canvas principal que envolve o jogo todo, ignorando Canvases de janelas
        canvas = transform.root.GetComponentInChildren<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {  
        rectTransform.SetAsLastSibling(); // Traz a janela para a frente
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move a janela usando o delta do rato e a escala do Canvas
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor; 
        
        // Trava a posição
        LimitarNaTela();
    }

    void LimitarNaTela()
    {
        // Usa o tamanho real do Canvas escalonado em vez do Screen.width
        float metadeTelaX = canvasRect.rect.width / 2f;
        float metadeTelaY = canvasRect.rect.height / 2f;

        // Calcula a metade da janela, respeitando a escala maluca (0.25) que a Unity gerou
        float metadeJanelaX = (rectTransform.rect.width * rectTransform.localScale.x) / 2f;
        float metadeJanelaY = (rectTransform.rect.height * rectTransform.localScale.y) / 2f;

        // O limite é a borda da tela menos o pedaço da janela
        float limiteX = metadeTelaX - metadeJanelaX;
        float limiteY = metadeTelaY - metadeJanelaY;

        // Pega a posição ancorada (cujo centro é 0,0)
        Vector2 posicaoAtual = rectTransform.anchoredPosition;

        // O Clamp vai perfeitamente de -limite (esquerda/baixo) até +limite (direita/cima)
        posicaoAtual.x = Mathf.Clamp(posicaoAtual.x, -limiteX, limiteX);
        posicaoAtual.y = Mathf.Clamp(posicaoAtual.y, -limiteY, limiteY);

        rectTransform.anchoredPosition = posicaoAtual;
    }
}