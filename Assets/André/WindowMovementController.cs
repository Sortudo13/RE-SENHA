using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class WindowMovementController : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Movement config")]
    [SerializeField] private RectTransform targetTransform;
    [SerializeField] private PointerEventData.InputButton dragButton = PointerEventData.InputButton.Left;
    private const bool bringToFront = true;
    private Canvas canvas;
    private RectTransform containerRect;

    private void Awake()
    {
        if (targetTransform == null)
            targetTransform = GetComponent<RectTransform>();
        
        canvas = transform.root.GetComponentInChildren<Canvas>();

        if (targetTransform.parent != null)
            containerRect = targetTransform.parent.GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == dragButton && bringToFront)
            targetTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == dragButton)
        {
            targetTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            LimitInContainer();
        }
        
    }

    // Use the object with Anchor Min = (0.5, 0.5), Anchor Max = (0.5, 0.5) and Pivot = (0.5, 0.5)
    public void LimitInContainer()
    {
        float metadeContainerX = containerRect.rect.width / 2f;
        float metadeContainerY = containerRect.rect.height / 2f;

        float metadeObjetoX = (targetTransform.rect.width * targetTransform.localScale.x) / 2f;
        float metadeObjetoY = (targetTransform.rect.height * targetTransform.localScale.y) / 2f;

        float limiteX = Mathf.Abs(metadeContainerX - metadeObjetoX);
        float limiteY = Mathf.Abs(metadeContainerY - metadeObjetoY);

        Vector2 posicaoAtual = targetTransform.anchoredPosition;

        posicaoAtual.x = Mathf.Clamp(posicaoAtual.x, -limiteX, limiteX);
        posicaoAtual.y = Mathf.Clamp(posicaoAtual.y, -limiteY, limiteY);

        targetTransform.anchoredPosition = posicaoAtual;
    }
}