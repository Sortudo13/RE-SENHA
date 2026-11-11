using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InspectionFieldSelectable : InspectionSelectable
{
    [SerializeField]
    private string field;

    protected override string SelectionId => field;
    protected override string HighlightAction =>
        InspectionSelectionController.FieldHighlightAction;

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (InspectionModeActive && !string.IsNullOrWhiteSpace(field))
            ActionController.SendAction(
                InspectionSelectionController.FieldSelectedAction,
                new InspectionSelection(field, field)
            );
    }

    protected override bool MatchesHighlight(string selectionId)
    {
        return string.Equals(field, selectionId);
    }
}
