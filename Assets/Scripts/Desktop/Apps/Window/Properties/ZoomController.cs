using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ZoomController : MonoBehaviour, IScrollHandler
{
    [Header("Zoom config")]
    public float zoomSpeed = 0.01f;
    public float zoomMin = 0.01f;
    public float zoomMax = 3f;

    [Header("Botoes (+ / -)")]
    [Tooltip("Quanto cada clique em ZoomIn/ZoomOut muda a escala.")]
    public float buttonStep = 0.1f;

    public void OnScroll(PointerEventData eventData)
    {
        bool ctrlPressed = Keyboard.current != null &&(Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);

        if (ctrlPressed)
            ApplyZoom(eventData.scrollDelta.y * zoomSpeed);
    }

    public void ZoomIn()
    {
        ApplyZoom(buttonStep);
    }

    public void ZoomOut()
    {
        ApplyZoom(-buttonStep);
    }

    private void ApplyZoom(float amount)
    {
        foreach (Transform child in transform)
        {
            Vector3 newEscale = child.localScale + (amount * Vector3.one);

            newEscale.x = Mathf.Clamp(newEscale.x, zoomMin, zoomMax);
            newEscale.y = Mathf.Clamp(newEscale.y, zoomMin, zoomMax);
            newEscale.z = 1f;

            child.localScale = newEscale;
        }
    }
}