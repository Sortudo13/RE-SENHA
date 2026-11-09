using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class InspectionFindingsAlertController : MonoBehaviour
{
    public const string ShowFindingsAction = "ShowInspectionFindings";

    [SerializeField]
    private InspectionEvaluationController evaluation;

    private void Awake()
    {
        ActionController.SyncSignal<string>(
            InspectionSelectionController.FindingRegisteredAction,
            OnFindingRegistered
        );
        ActionController.SyncSignal(ShowFindingsAction, ShowCurrentFindings);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<string>(
            InspectionSelectionController.FindingRegisteredAction,
            OnFindingRegistered
        );
        ActionController.UnsyncSignal(ShowFindingsAction, ShowCurrentFindings);
    }

    private void OnFindingRegistered(string findingId)
    {
        ShowCurrentFindings();
    }

    public void ShowCurrentFindings()
    {
        if (evaluation?.CurrentEmail == null)
            return;

        List<InspectionFindingData> findings = evaluation.CurrentEmail.inspectionFindings;
        ActionController.SendAction(
            AlertManagerController.ShowFindingsAction,
            new FindingsAlertData(
                "[ACHADOS DO INSPETOR]",
                BuildSummary(findings),
                BuildDetails(findings)
            )
        );
    }

    private static string BuildSummary(List<InspectionFindingData> findings)
    {
        int count = findings?.Count ?? 0;
        return $"O Inspector registrou {count} achado(s).\n"
            + "Use \"Ver regras\" para consultar os detalhes.";
    }

    private static string BuildDetails(List<InspectionFindingData> findings)
    {
        StringBuilder text = new();
        if (findings == null)
            return string.Empty;

        foreach (InspectionFindingData finding in findings)
        {
            text.AppendLine($"[{finding.field}] {finding.rule}");
            text.AppendLine(finding.evidence);
            text.AppendLine(finding.explanation);
            text.AppendLine();
        }

        return text.ToString();
    }
}
