using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InspectionRuleTextSelectable : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    private RegraData rule;
    private SubregraData subrule;

    public void Initialize(RegraData ruleData, SubregraData subruleData)
    {
        rule = ruleData;
        subrule = subruleData;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (InspectionModeController.IsActive && rule != null)
            InspectionModeController.SelectRule(rule, subrule, transform as RectTransform);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InspectionModeController.IsActive)
            InspectionModeController.HighlightField(transform as RectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InspectionModeController.IsActive)
            InspectionModeController.ClearHighlight(transform as RectTransform);
    }
}
