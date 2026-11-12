using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class MovementController
    : MonoBehaviour,
        IPointerUpHandler,
        IPointerDownHandler,
        IDragHandler
{
    [Header("Movement config")]
    [SerializeField]
    private RectTransform targetTransform;

    [SerializeField]
    private PointerEventData.InputButton dragButton = PointerEventData.InputButton.Left;

    [SerializeField]
    private CursorStyle pressedCursorType = CursorStyle.Default;

    [Header("Fluidez")]
    [SerializeField]
    [Tooltip("Quanto maior, mais rápido a janela alcança o mouse. 15 = suave, 30 = responsivo.")]
    private float followSpeed = 22f;

    private const bool bringToFront = true;
    private Canvas canvas;
    private RectTransform containerRect;
    private Vector2 minPosition;
    private Vector2 maxPosition;
    private float canvasScaleFactor = 1f;
    private bool isDragging;

    // Posição alvo (onde a janela DEVE estar) e posição do último cursor sincronizado
    private Vector2 targetPosition;
    private Vector2 lastCursorPos;

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
        if (eventData.button != dragButton)
            return;

        if (bringToFront)
            targetTransform.SetAsLastSibling();

        CursorController.Instance.SetToMode(pressedCursorType);

        CacheDragConstraints();

        // Alvo começa na posição atual da janela
        targetPosition = targetTransform.anchoredPosition;

        isDragging = true;

        CursorController.Instance.ShowVirtualCursor(true);
        SyncVirtualCursor(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != dragButton)
            return;

        isDragging = false;

        // Garante que a janela chegue exatamente ao alvo ao soltar
        targetTransform.anchoredPosition = targetPosition;

        CursorController.Instance.ShowVirtualCursor(false);
        CursorController.Instance.SetToMode(CursorStyle.Default);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != dragButton || !isDragging || containerRect == null)
            return;

        // Atualiza o ALVO (não move a janela diretamente)
        targetPosition += eventData.delta / canvasScaleFactor;
        targetPosition.x = Mathf.Clamp(targetPosition.x, minPosition.x, maxPosition.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minPosition.y, maxPosition.y);

        SyncVirtualCursor(eventData.position);
    }

    private void LateUpdate()
    {
        if (!isDragging)
            return;

        // Suavização exponencial independente de FPS
        float t = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
        targetTransform.anchoredPosition = Vector2.Lerp(
            targetTransform.anchoredPosition,
            targetPosition,
            t
        );
    }

    public void LimitInContainer()
    {
        CacheDragConstraints();

        Vector2 posicaoAtual = targetTransform.anchoredPosition;
        posicaoAtual.x = Mathf.Clamp(posicaoAtual.x, minPosition.x, maxPosition.x);
        posicaoAtual.y = Mathf.Clamp(posicaoAtual.y, minPosition.y, maxPosition.y);

        targetTransform.anchoredPosition = posicaoAtual;

        // Mantém o alvo coerente com a posição limitada
        targetPosition = posicaoAtual;
    }

    private void CacheDragConstraints()
    {
        canvasScaleFactor = canvas != null ? canvas.scaleFactor : 1f;

        float halfContainerWidth = containerRect.rect.width / 2f;
        float halfContainerHeight = containerRect.rect.height / 2f;
        float halfTargetWidth = targetTransform.rect.width * targetTransform.localScale.x / 2f;
        float halfTargetHeight = targetTransform.rect.height * targetTransform.localScale.y / 2f;

        minPosition = new Vector2(
            Mathf.Abs(halfContainerWidth - halfTargetWidth) * -1f,
            Mathf.Abs(halfContainerHeight - halfTargetHeight) * -1f
        );
        maxPosition = -minPosition;
    }

    //recebe a posição do mouse, não o ponto de grab da janela
    private void SyncVirtualCursor(Vector2 screenPosition)
    {
        // Evita chamadas redundantes quando o mouse não se moveu
        if ((screenPosition - lastCursorPos).sqrMagnitude < 0.01f)
            return;

        lastCursorPos = screenPosition;
        CursorController.Instance.UpdateVirtualCursorPosition(screenPosition);
    }
}
