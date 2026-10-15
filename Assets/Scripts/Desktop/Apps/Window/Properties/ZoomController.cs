using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ZoomController : MonoBehaviour, IScrollHandler
{
    [Header("Zoom config")]
    public float zoomSpeed = 0.01f;
    public float zoomMin = 0.01f;
    public float zoomMax = 3f;

    public void OnScroll(PointerEventData eventData)
    {
        bool ctrlPressed = Keyboard.current != null &&(Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);

        if (ctrlPressed)
        {
            float direcaoScroll = eventData.scrollDelta.y;

            foreach (Transform child in transform)
            {
                Vector3 newEscale = child.localScale + (direcaoScroll * zoomSpeed * Vector3.one);

                newEscale.x = Mathf.Clamp(newEscale.x, zoomMin, zoomMax);
                newEscale.y = Mathf.Clamp(newEscale.y, zoomMin, zoomMax);
                newEscale.z = 1f;

                child.localScale = newEscale;
            }
        }
    }
}