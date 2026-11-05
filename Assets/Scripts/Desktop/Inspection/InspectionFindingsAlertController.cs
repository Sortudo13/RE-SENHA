using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class InspectionFindingsAlertController : MonoBehaviour
{
    public const string ShowFindingsAction = "ShowInspectionFindings";

    [SerializeField]
    private GameObject findingsAlert;

    [SerializeField]
    private InspectionEvaluationController evaluation;

    private void Awake()
    {
        ActionController.SyncSignal<string>(
            InspectionSelectionController.FindingRegisteredAction,
            ShowFindingsAlert
        );
        ActionController.SyncSignal(ShowFindingsAction, ShowCurrentFindings);
    }

    public void ShowCurrentFindings()
    {
        ShowFindingsAlert(string.Empty);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<string>(
            InspectionSelectionController.FindingRegisteredAction,
            ShowFindingsAlert
        );
        ActionController.UnsyncSignal(ShowFindingsAction, ShowCurrentFindings);
    }

    private void ShowFindingsAlert(string subruleId)
    {
        if (findingsAlert == null || evaluation?.CurrentEmail == null)
            return;

        GameObject alert = Instantiate(findingsAlert, AlertManagerController.AlertsLayer);
        Transform content = alert.transform.GetChild(0);
        TextMeshProUGUI title = content.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI details = content.GetChild(1).GetComponent<TextMeshProUGUI>();
        Button rulesButton = content.GetChild(2).GetComponent<Button>();

        title.text = "[ACHADOS DO INSPETOR]";
        details.text = BuildSummary();
        ConfigureButton(
            rulesButton,
            "Ver regras",
            () => ShowDetails(alert, rulesButton, details)
        );

        alert.SetActive(true);
    }

    private string BuildSummary()
    {
        int count = evaluation.CurrentEmail.inspectionFindings?.Count ?? 0;
        return $"O Inspector registrou {count} achado(s).\n"
            + "Use \"Ver regras\" para consultar os detalhes.";
    }

    private void ShowDetails(
        GameObject alert,
        Button rulesButton,
        TextMeshProUGUI details
    )
    {
        StringBuilder text = new();
        foreach (InspectionFindingData finding in evaluation.CurrentEmail.inspectionFindings)
        {
            text.AppendLine($"[{finding.field}] {finding.rule}");
            text.AppendLine(finding.evidence);
            text.AppendLine(finding.explanation);
            text.AppendLine();
        }

        details.text = text.ToString();
        ConfigureButton(rulesButton, "Fechar", () => Destroy(alert));
    }

    private static void ConfigureButton(
        Button button,
        string label,
        UnityEngine.Events.UnityAction action
    )
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = label;
    }

}
