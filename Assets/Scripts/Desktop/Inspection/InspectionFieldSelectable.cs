using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class InspectionFieldSelectable : ActionButtonController, IPointerClickHandler
{
    [SerializeField]
    private string field;

    [SerializeField]
    private Color selectedColor = new(1f, 0.85f, 0.2f, 1f);

    private Graphic graphic;
    private Color originalColor;
    private bool inspectionModeActive;

    private void Awake()
    {
        InspectionSelectableRegistry.Register(transform as RectTransform);
        graphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>();
        if (graphic != null)
            originalColor = graphic.color;

        ActionController.SyncSignal<string>(
            InspectionSelectionController.FieldHighlightAction,
            OnFieldSelected
        );
        ActionController.SyncSignal<bool>(
            InspectionModeController.ModeChangedAction,
            OnInspectionModeChanged
        );
        ActionController.SyncSignal(
            InspectionSelectionController.SelectionClearedAction,
            ClearHighlight
        );
    }

    private void OnDestroy()
    {
        InspectionSelectableRegistry.Unregister(transform as RectTransform);
        ActionController.UnsyncSignal<string>(
            InspectionSelectionController.FieldHighlightAction,
            OnFieldSelected
        );
        ActionController.UnsyncSignal<bool>(
            InspectionModeController.ModeChangedAction,
            OnInspectionModeChanged
        );
        ActionController.UnsyncSignal(
            InspectionSelectionController.SelectionClearedAction,
            ClearHighlight
        );
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inspectionModeActive && !string.IsNullOrWhiteSpace(field))
            SendAction(InspectionSelectionController.FieldSelectedAction, field);
    }

    private void OnFieldSelected(string selectedField)
    {
        SetSelected(
            !string.IsNullOrWhiteSpace(selectedField)
            && string.Equals(field, selectedField)
        );
    }

    private void OnInspectionModeChanged(bool isActive)
    {
        inspectionModeActive = isActive;
        if (!isActive)
            SetSelected(false);
    }

    private void SetSelected(bool selected)
    {
        if (graphic != null)
            graphic.color = selected ? selectedColor : originalColor;
    }

    private void ClearHighlight()
    {
        SetSelected(false);
    }
}
