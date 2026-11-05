using UnityEngine;
using UnityEngine.UI;

public sealed class InspectionOverlayRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    public bool IsRaycastLocationValid(Vector2 screenPosition, Camera eventCamera)
    {
        return !InspectionSelectableRegistry.Contains(screenPosition, eventCamera);
    }
}
