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

        // Regra sem campo (chat, ticket: sem Inspetor) nao mostra o rotulo.
        bool hasField = !string.IsNullOrWhiteSpace(fieldLabel);
        if (txtField != null)
        {
            txtField.text = hasField ? $"[{fieldLabel}]" : string.Empty;
            txtField.gameObject.SetActive(hasField);
        }
        if (txtTitle != null)
            txtTitle.text = rule?.title ?? string.Empty;
        if (txtDescription != null)
            txtDescription.text = rule?.description ?? string.Empty;

        RectTransform cardRect = (RectTransform)transform;
        float textWidth = Mathf.Max(0f, cardRect.rect.width - 24f);
        float contentHeight = 40f;
        if (txtField != null && hasField)
            contentHeight += txtField.GetPreferredValues(txtField.text, textWidth, float.PositiveInfinity).y;
        if (txtTitle != null)
            contentHeight += txtTitle.GetPreferredValues(txtTitle.text, textWidth, float.PositiveInfinity).y;
        if (txtDescription != null)
            contentHeight += txtDescription.GetPreferredValues(
                txtDescription.text,
                textWidth,
                float.PositiveInfinity
            ).y;

        cardRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            Mathf.Max(cardRect.rect.height, contentHeight)
        );
    }

    private void Select()
    {
        if (rule != null)
            onSelect?.Invoke(rule);
    }
}
