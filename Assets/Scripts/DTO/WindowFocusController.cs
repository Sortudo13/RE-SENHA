using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class WindowFocusController : MonoBehaviour
{
    private readonly List<RaycastResult> raycastResults = new();
    private PointerEventData pointerData;
    private EventSystem cachedEventSystem;

    private void Update()
    {
        if (TryGetPointerDownPosition(out Vector2 screenPosition))
            FocusWindowAt(screenPosition);
    }

    private bool TryGetPointerDownPosition(out Vector2 position)
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame ||
                Mouse.current.middleButton.wasPressedThisFrame)
            {
                position = Mouse.current.position.ReadValue();
                return true;
            }
        }
        else if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            position = Pointer.current.position.ReadValue();
            return true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            position = Input.mousePosition;
            return true;
        }
#endif
        position = default;
        return false;
    }

    private void FocusWindowAt(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return;

        if (pointerData == null || cachedEventSystem != EventSystem.current)
        {
            cachedEventSystem = EventSystem.current;
            pointerData = new PointerEventData(cachedEventSystem);
        }

        pointerData.position = screenPosition;
        raycastResults.Clear();

        EventSystem.current.RaycastAll(pointerData, raycastResults);

        if (raycastResults.Count == 0)
            return;

        GameObject topHit = raycastResults[0].gameObject;
        WindowController targetWindow = topHit.GetComponentInParent<WindowController>();

        if (targetWindow != null)
            targetWindow.ToFront();
    }
}