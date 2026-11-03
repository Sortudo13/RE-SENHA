using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class EmailInspectionAlertController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private TextMeshProUGUI findingsText;

    [SerializeField]
    private Button closeButton;

    public void Initialize(IReadOnlyList<InspectionFinding> findings)
    {
        Transform content = transform.childCount > 0 ? transform.GetChild(0) : null;
        if (titleText == null && content != null && content.childCount > 0)
            titleText = content.GetChild(0).GetComponent<TextMeshProUGUI>();
        if (findingsText == null && content != null && content.childCount > 1)
            findingsText = content.GetChild(1).GetComponent<TextMeshProUGUI>();
        if (closeButton == null)
            closeButton = GetComponentInChildren<Button>();

        if (titleText != null)
            titleText.text = "[REGRAS ENCONTRADAS]";

        if (findingsText != null)
        {
            var message = new StringBuilder();
            foreach (InspectionFinding finding in findings)
            {
                message.AppendLine($"- {finding.Rule}");
                message.AppendLine($"  Campo: {finding.Field}");
                message.AppendLine($"  Evidência: {finding.Evidence}");
                message.AppendLine($"  {finding.Explanation}");
                if (!string.IsNullOrWhiteSpace(finding.Reason))
                    message.AppendLine($"  Justificativa: {finding.Reason}");
                message.AppendLine();
            }

            findingsText.text = message.ToString().TrimEnd();
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }
    }

    private void Close()
    {
        AlertManagerController.CloseAlert(gameObject);
    }
}
