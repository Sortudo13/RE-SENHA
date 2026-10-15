using UnityEngine;
using UnityEngine.EventSystems;

public class CursorSwap : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler, IPointerMoveHandler
{
    [Header("Cursor Types")]
    [SerializeField] private CursorStyle enterCursorType = CursorStyle.Keep;
    [SerializeField] private CursorStyle exitCursorType = CursorStyle.Keep;
    [SerializeField] private CursorStyle upCursorType = CursorStyle.Keep;
    [SerializeField] private CursorStyle downCursorType = CursorStyle.Keep;
    [SerializeField] private CursorStyle clickCursorType = CursorStyle.Keep;
    [SerializeField] private CursorStyle moveCursorType = CursorStyle.Keep;


    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorController.Instance.SetToMode(enterCursorType);
    }

    public void OnPointerExit(PointerEventData  eventData)
    {
        CursorController.Instance.SetToMode(exitCursorType);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CursorController.Instance.SetToMode(upCursorType);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CursorController.Instance.SetToMode(downCursorType);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CursorController.Instance.SetToMode(clickCursorType);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        CursorController.Instance.SetToMode(moveCursorType);
    }
}