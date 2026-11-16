using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InspectionRuleSelectable : InspectionSelectable
{
    [SerializeField]
    private string subruleId;

    protected override string SelectionId => subruleId;
    protected override string HighlightAction =>
        InspectionSelectionController.RuleHighlightAction;

    public void Setup(string id)
    {
        subruleId = id;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (InspectionModeActive && !string.IsNullOrWhiteSpace(subruleId))
            ActionController.SendAction(
                InspectionSelectionController.RuleSelectedAction,
                new InspectionSelection(string.Empty, subruleId)
            );
    }

    protected override bool MatchesHighlight(string selectionId)
    {
        return string.Equals(subruleId, selectionId);
    }

    protected override bool MatchesSelection(InspectionSelection selection)
    {
        return !string.IsNullOrWhiteSpace(selection.SelectionId)
            && MatchesHighlight(selection.SelectionId);
    }
}
