using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public class MovementController : MonoBehaviour,IPointerUpHandler, IPointerDownHandler, IDragHandler
{

    [Header("Movement config")]
    [SerializeField] private RectTransform targetTransform;
    [SerializeField] private PointerEventData.InputButton dragButton = PointerEventData.InputButton.Left;
    [SerializeField] private CursorStyle pressedCursorType = CursorStyle.Default;
    [SerializeField] [Range(0f,1f)] private float dragSensitivity = 0.5f;
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
        if (eventData.button == dragButton && bringToFront)
        {
            targetTransform.SetAsLastSibling();
            CursorController.Instance.SetToMode(pressedCursorType);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetTransform,
                eventData.position,
                eventData.pressEventCamera,
                out grabLocalOffset
            );
            CursorController.Instance.ShowVirtualCursor(true);
            SyncVirtualCursor(eventData.pressEventCamera);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == dragButton)
        {
            Vector3 worldGrabPoint = targetTransform.TransformPoint(grabLocalOffset);
            Vector2 finalScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, worldGrabPoint);
            Mouse.current?.WarpCursorPosition(finalScreenPos);
            CursorController.Instance.ShowVirtualCursor(false);

            CursorController.Instance.SetToMode(CursorStyle.Default);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == dragButton)
        {
            targetTransform.anchoredPosition += (eventData.delta * dragSensitivity) / canvas.scaleFactor;
            LimitInContainer();
            SyncVirtualCursor(eventData.pressEventCamera);
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

    private void SyncVirtualCursor(Camera cam)
    {
        Vector3 worldGrabPoint = targetTransform.TransformPoint(grabLocalOffset);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldGrabPoint);

        CursorController.Instance.UpdateVirtualCursorPosition(screenPoint);
    }
}