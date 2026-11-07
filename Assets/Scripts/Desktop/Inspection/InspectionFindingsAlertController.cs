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
        TextMeshProUGUI title = FindText(alert, "Título");
        TextMeshProUGUI details = FindText(alert, "Conteúdo");
        Button rulesButton = alert.GetComponentInChildren<Button>(true);

        if (title == null || details == null || rulesButton == null)
        {
            Debug.LogError(
                "InspectionFindingsAlertController: o prefab de achados precisa "
                + "conter título, conteúdo e botão."
            );
            Destroy(alert);
            return;
        }

        title.text = "[ACHADOS DO INSPETOR]";
        details.text = BuildSummary();
        ConfigureButton(
            rulesButton,
            "Ver regras",
            () => ShowDetails(alert, rulesButton, details)
        );

        alert.SetActive(true);
    }

    private static TextMeshProUGUI FindText(GameObject alert, string objectName)
    {
        foreach (TextMeshProUGUI text in alert.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.gameObject.name == objectName)
                return text;
        }

        return null;
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
