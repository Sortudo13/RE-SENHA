using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    // Componente que guarda o Transform (Propriedades e valores espaciais de elementos da UI)
    private RectTransform rectTransform; 
    private Canvas canvas;
    public Vector2 tamanhoTela = new Vector2(Screen.width, Screen.height);

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPointerDown(PointerEventData eventData)
    {  
        rectTransform.SetAsLastSibling(); // Trás o objeto pra frente
    }
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor; // Adiciona na posição do nosso objeto o quanto o mouse se mover no último frame
        LimitarNaTela();
    }
    void LimitarNaTela()
    {
        // Obtém a resolução atual da tela ou do Canvas
        Vector2 tamanhoTela = new Vector2(Screen.width, Screen.height);

        // Se o seu Canvas estiver no modo 'Scale With Screen Size', use o tamanho do Canvas:
        // Vector2 tamanhoTela = canvas.GetComponent<RectTransform>().rect.size;

        // Calcula a metade do tamanho do objeto para considerar as bordas
        float metadeLargura = (rectTransform.rect.width * rectTransform.lossyScale.x) / 2f;
        float metadeAltura = (rectTransform.rect.height * rectTransform.lossyScale.y) / 2f;

        // Define os limites mínimos e máximos permitidos na tela
        float minX = metadeLargura;
        float maxX = tamanhoTela.x - metadeLargura;
        float minY = metadeAltura;
        float maxY = tamanhoTela.y - metadeAltura;

        // Obtém a posição atual do objeto em coordenadas de tela (Screen Space)
        Vector3 posicaoNaTela = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);

        // Limita as posições X e Y dentro das bordas da tela
        posicaoNaTela.x = Mathf.Clamp(posicaoNaTela.x, minX, maxX);
        posicaoNaTela.y = Mathf.Clamp(posicaoNaTela.y, minY, maxY);

        // Converte de volta para a posição do mundo e aplica no objeto
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, posicaoNaTela, null, out Vector3 posicaoMundo))
        {
            rectTransform.position = posicaoMundo;
        }
    }
}
