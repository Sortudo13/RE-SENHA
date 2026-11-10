using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ManualRuleCardController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtField;
    [SerializeField] private TextMeshProUGUI txtTitle;
    [SerializeField] private TextMeshProUGUI txtDescription;
    [SerializeField] private Button button;

    private ManualRuleData rule;
    private Action<ManualRuleData> onSelect;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(Select);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(Select);
    }

    public void Setup(ManualRuleData rule, string fieldLabel, Action<ManualRuleData> onSelect)
    {
        this.rule = rule;
        this.onSelect = onSelect;

        if (txtField != null)
            txtField.text = $"[{fieldLabel}]";
        if (txtTitle != null)
            txtTitle.text = rule?.title ?? string.Empty;
        if (txtDescription != null)
            txtDescription.text = rule?.description ?? string.Empty;
    }

    private void Select()
    {
        if (rule != null)
            onSelect?.Invoke(rule);
    }
}
