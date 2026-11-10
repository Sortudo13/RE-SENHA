using UnityEngine;
using UnityEngine.UI;

// Sem uso desde 2026-11-09: o Inspector nao bloqueia mais a interface.
// Mantido apenas porque o componente ainda esta no objeto InspectionOverlay da cena Desktop;
// remover este script junto com esse objeto quando a cena puder ser editada.

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
