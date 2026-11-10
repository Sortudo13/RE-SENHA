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
            txtDescription.text = subrule.description;
    }
}
