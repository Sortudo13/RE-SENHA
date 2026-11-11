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

    private const bool bringToFront = true;
    private Canvas canvas;
    private RectTransform containerRect;
    private Vector2 grabLocalOffset;

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

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetTransform,
            eventData.position,
            eventData.pressEventCamera,
            out grabLocalOffset
        );

        CursorController.Instance.ShowVirtualCursor(true);
        SyncVirtualCursor(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != dragButton)
            return;

        CursorController.Instance.ShowVirtualCursor(false);
        CursorController.Instance.SetToMode(CursorStyle.Default);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != dragButton || containerRect == null)
            return;

        Vector2 localPoint;
        if (
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                containerRect,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            )
        )
        {
            targetTransform.anchoredPosition = localPoint - grabLocalOffset;
            LimitInContainer();
        }

        SyncVirtualCursor(eventData.position);
    }

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

    //recebe a posição do mouse, não o ponto de grab da janela
    private void SyncVirtualCursor(Vector2 screenPosition)
    {
        CursorController.Instance.UpdateVirtualCursorPosition(screenPosition);
    }
}
