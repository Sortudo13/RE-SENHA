using System.Collections.Generic;
using UnityEngine;

// So decide o que mostrar; as janelas sao criadas pelo AlertManagerController.
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

    // Achado novo: aviso curto (Alert Notification). A lista paginada (Alert Achados) abre
    // pelo botao do Email que publica ShowInspectionFindings.
    private void OnFindingRegistered(string findingId)
    {
        ActionController.SendAction(AlertManagerController.ShowFindingNotificationAction);
    }

    public void ShowCurrentFindings()
    {
        if (evaluation == null || evaluation.CurrentEmail == null)
            return;

        ActionController.SendAction(
            AlertManagerController.ShowFindingsAction,
            BuildPages(evaluation.CurrentEmail.evidences)
        );
    }

    // Uma pagina por achado. Lista vazia: o FindingsAlertWindow mostra "Nenhum achado registrado.".
    private static string[] BuildPages(List<EvidenceData> findings)
    {
        if (findings == null)
            return new string[0];

        string[] pages = new string[findings.Count];
        for (int index = 0; index < findings.Count; index++)
        {
            EvidenceData finding = findings[index];
            pages[index] = $"[{finding.field}] {finding.rule}\n{finding.evidence}\n{finding.explanation}";
        }

        return pages;
    }
}
