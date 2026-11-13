using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class NewMovementController : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IDragHandler
{
    [Header("Movement config")]
    [SerializeField] private RectTransform targetTransform;
    [SerializeField] private PointerEventData.InputButton dragButton = PointerEventData.InputButton.Left;
    [SerializeField] private CursorStyle pressedCursorType = CursorStyle.Hand;
    [SerializeField][Range(0f, 1f)] private float dragSensitivity = 1f;

    [Header("Fluidez")]
    [SerializeField] private float followSpeed = 22f;
    [SerializeField] private float cursorEdgeMargin = 15f;

    private Canvas canvas;
    private RectTransform containerRect;
    private Vector2 grabLocalOffset;
    private float limiteX;
    private float limiteY;

    // Variáveis para fluidez
    private bool isDragging;
    private Vector2 targetPosition;
    private Camera dragCamera;

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
        if (eventData.button == dragButton)
        {
            targetTransform.SetAsLastSibling();
            CursorController.Instance.SetToMode(pressedCursorType);
            UpdateCacheLimits();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetTransform,
                eventData.position,
                eventData.pressEventCamera,
                out grabLocalOffset
            );

            // Inicia os controles de fluidez
            isDragging = true;
            targetPosition = targetTransform.anchoredPosition;
            dragCamera = eventData.pressEventCamera;

            CursorController.Instance.ShowVirtualCursor(true);
            SyncVirtualCursor(dragCamera);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == dragButton)
        {
            isDragging = false;

            // Garante que a janela chegue exatamente ao alvo ao soltar o clique
            targetTransform.anchoredPosition = targetPosition;

            // Usa a posição já amortecida para o cursor real reaparecer exatamente onde o virtual estava
            Vector2 finalScreenPos = GetSoftClampedCursorScreenPosition(eventData.pressEventCamera);
            Mouse.current?.WarpCursorPosition(finalScreenPos);

            CursorController.Instance.ShowVirtualCursor(false);
            CursorController.Instance.SetToMode(CursorStyle.Default);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == dragButton)
        {
            // Atualiza o ALVO (usando a sensibilidade), não a posição física da janela
            targetPosition += (eventData.delta * dragSensitivity) / canvas.scaleFactor;

            if (CursorController.Instance != null && CursorController.Instance.CurrentStyle != pressedCursorType)
                CursorController.Instance.SetToMode(pressedCursorType);

            LimitTargetPosition();
        }
    }

    private void LateUpdate()
    {
        if (!isDragging) return;

        // Suavização exponencial independente de FPS
        float t = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
        targetTransform.anchoredPosition = Vector2.Lerp(targetTransform.anchoredPosition, targetPosition, t);

        // Sincroniza o cursor virtual junto com o movimento suave da janela
        if (dragCamera != null)
        {
            SyncVirtualCursor(dragCamera);
        }
    }

    // Limitador adaptado para barrar o 'Alvo' antes de mover a janela
    private void LimitTargetPosition()
    {
        targetPosition.x = Mathf.Clamp(targetPosition.x, -limiteX, limiteX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, -limiteY, limiteY);
    }

    private void SyncVirtualCursor(Camera cam)
    {
        Vector2 screenPoint = GetSoftClampedCursorScreenPosition(cam);
        CursorController.Instance.UpdateVirtualCursorPosition(screenPoint);
    }

    private Vector2 GetSoftClampedCursorScreenPosition(Camera cam)
    {
        Vector3 worldGrabPoint = targetTransform.TransformPoint(grabLocalOffset);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldGrabPoint);

        if (containerRect == null)
            return screenPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            containerRect,
            screenPoint,
            cam,
            out Vector2 localInContainer
        );

        // Converte o tamanho do cursor (em pixels de tela) para o espaço local do container
        Vector2 minPad = Vector2.zero;
        Vector2 maxPad = Vector2.zero;
        if (CursorController.Instance != null)
        {
            CursorController.Instance.GetVirtualCursorExtents(out Vector2 minExt, out Vector2 maxExt);
            float scale = canvas != null ? canvas.scaleFactor : 1f;
            minPad = minExt / scale;
            maxPad = maxExt / scale;
        }

        Rect bounds = containerRect.rect;
        localInContainer.x = SoftClamp(localInContainer.x, bounds.xMin + minPad.x, bounds.xMax - maxPad.x, cursorEdgeMargin);
        localInContainer.y = SoftClamp(localInContainer.y, bounds.yMin + minPad.y, bounds.yMax - maxPad.y, cursorEdgeMargin);

        Vector3 clampedWorld = containerRect.TransformPoint(localInContainer);
        return RectTransformUtility.WorldToScreenPoint(cam, clampedWorld);
    }

    private void UpdateCacheLimits()
    {
        float metadeContainerX = containerRect.rect.width / 2f;
        float metadeContainerY = containerRect.rect.height / 2f;

        float metadeObjetoX = (targetTransform.rect.width * targetTransform.localScale.x) / 2f;
        float metadeObjetoY = (targetTransform.rect.height * targetTransform.localScale.y) / 2f;

        limiteX = Mathf.Abs(metadeContainerX - metadeObjetoX);
        limiteY = Mathf.Abs(metadeContainerY - metadeObjetoY);
    }

    private float SoftClamp(float value, float min, float max, float margin)
    {
        // Garante que a margem nunca ultrapasse metade do tamanho do container
        margin = Mathf.Clamp(margin, 0f, (max - min) * 0.5f);
        if (margin <= 0f) return Mathf.Clamp(value, min, max);

        float innerMin = min + margin;
        float innerMax = max - margin;

        if (value > innerMax)
        {
            float excess = value - innerMax;
            return innerMax + margin * (excess / (excess + margin));
        }

        if (value < innerMin)
        {
            float excess = innerMin - value;
            return innerMin - margin * (excess / (excess + margin));
        }

        return value;
    }
}