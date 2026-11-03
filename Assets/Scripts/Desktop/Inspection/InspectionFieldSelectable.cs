using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InspectionFieldSelectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private InspectionField field;

    public void Initialize(InspectionField inspectionField)
    {
        field = inspectionField;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!InspectionModeController.IsActive)
            return;

        InspectionModeController.HighlightField(transform as RectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!InspectionModeController.IsActive)
            return;

        InspectionModeController.ClearHighlight(transform as RectTransform);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!InspectionModeController.IsActive)
            return;

        InspectionModeController.SelectField(field, transform as RectTransform);
    }

    private void OnDisable()
    {
        InspectionModeController.ClearHighlight(transform as RectTransform);
    }
}
