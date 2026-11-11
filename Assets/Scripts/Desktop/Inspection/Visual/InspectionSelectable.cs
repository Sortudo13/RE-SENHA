using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class InspectionSelectable : ActionButtonController, IPointerClickHandler
{
    [SerializeField]
    private Color selectedColor = new(1f, 0.85f, 0.2f, 1f);

    private Graphic graphic;
    private Color originalColor;

    protected bool InspectionModeActive { get; private set; }
    protected abstract string SelectionId { get; }
    protected abstract string HighlightAction { get; }

    protected virtual void Awake()
    {
        graphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>();
        if (graphic != null)
            originalColor = graphic.color;

        ActionController.SyncSignal<string>(HighlightAction, OnHighlight);
        ActionController.SyncSignal<bool>(
            InspectionModeController.ModeChangedAction,
            OnInspectionModeChanged
        );
        ActionController.SyncSignal(
            InspectionSelectionController.SelectionClearedAction,
            ClearHighlight
        );
    }

    // Start roda depois do Setup feito logo apos o Instantiate, entao o seletor ja tem seu ID
    // quando o estado atual (modo e highlights) chega.
    protected virtual void Start()
    {
        ActionController.SendAction(InspectionModeController.StateRequestedAction);
    }

    protected virtual void OnDestroy()
    {
        ActionController.UnsyncSignal<string>(HighlightAction, OnHighlight);
        ActionController.UnsyncSignal<bool>(
            InspectionModeController.ModeChangedAction,
            OnInspectionModeChanged
        );
        ActionController.UnsyncSignal(
            InspectionSelectionController.SelectionClearedAction,
            ClearHighlight
        );
    }

    public abstract void OnPointerClick(PointerEventData eventData);

    protected abstract bool MatchesHighlight(string selectionId);

    protected void SetSelected(bool selected)
    {
        if (graphic != null)
            graphic.color = selected ? selectedColor : originalColor;
    }

    private void OnHighlight(string selectionId)
    {
        SetSelected(
            !string.IsNullOrWhiteSpace(selectionId)
            && MatchesHighlight(selectionId)
        );
    }

    private void OnInspectionModeChanged(bool isActive)
    {
        InspectionModeActive = isActive;
        if (!isActive)
            SetSelected(false);
    }

    private void ClearHighlight()
    {
        SetSelected(false);
    }
}
