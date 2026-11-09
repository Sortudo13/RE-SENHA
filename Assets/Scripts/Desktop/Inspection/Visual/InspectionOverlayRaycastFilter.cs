using UnityEngine;
using UnityEngine.UI;

public sealed class InspectionOverlayRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    [SerializeField]
    private InspectionSelectableRegistry registry;

    private void Awake()
    {
        if (registry == null)
            registry = GetComponent<InspectionSelectableRegistry>();
    }

    public bool IsRaycastLocationValid(Vector2 screenPosition, Camera eventCamera)
    {
        return registry == null
            || !registry.Contains(screenPosition, eventCamera);
    }
}
