using TMPro;
using UnityEngine;

[RequireComponent(typeof(InspectionRuleSelectable))]
public sealed class ManualSubruleItemController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtDescription;
    [SerializeField] private InspectionRuleSelectable selectable;

    public void Setup(ManualSubruleData subrule)
    {
        if (subrule == null)
            return;

        if (selectable == null)
            selectable = GetComponent<InspectionRuleSelectable>();

        selectable.Setup(subrule.id);
        if (txtDescription != null)
        {
            txtDescription.text = subrule.description;

            RectTransform itemRect = (RectTransform)transform;
            RectTransform descriptionRect = txtDescription.rectTransform;
            descriptionRect.anchorMin = Vector2.zero;
            descriptionRect.anchorMax = Vector2.one;
            descriptionRect.offsetMin = new Vector2(12f, 8f);
            descriptionRect.offsetMax = new Vector2(-12f, -8f);

            float textWidth = Mathf.Max(0f, itemRect.rect.width - 24f);
            float textHeight = txtDescription
                .GetPreferredValues(subrule.description, textWidth, float.PositiveInfinity)
                .y;
            itemRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                Mathf.Max(itemRect.rect.height, textHeight + 16f)
            );
        }
    }
}
