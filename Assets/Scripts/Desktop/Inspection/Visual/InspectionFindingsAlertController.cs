using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class InspectionFindingsAlertController : MonoBehaviour
{
    public const string ShowFindingsAction = "ShowInspectionFindings";

    [SerializeField]
    private InspectionEvaluationController evaluation;

    // Prefab "Alert Notification": filhos "Fechar" (com Button) e "Conteúdo" > "Título" e "Conteúdo".
    [SerializeField]
    private GameObject findingNotificationPrefab;

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

    // Achado novo: notificacao curta so com ele. A lista completa (Alert Achados) continua
    // no botao do Email que publica ShowInspectionFindings.
    private void OnFindingRegistered(string findingId)
    {
        ShowFindingNotification();
    }

    private void ShowFindingNotification()
    {
        if (findingNotificationPrefab == null)
        {
            Debug.LogError(
                "InspectionFindingsAlertController: \"Finding Notification Prefab\" is not set (Inspector.prefab). Drag \"Alert Notification\" into it.",
                this
            );
            return;
        }

        if (AlertManagerController.AlertsLayer == null)
        {
            Debug.LogError("InspectionFindingsAlertController: AlertManagerController.AlertsLayer is missing.", this);
            return;
        }

        GameObject notification = Instantiate(
            findingNotificationPrefab,
            AlertManagerController.AlertsLayer
        );

        // So a mensagem: o titulo do Alert base ("Titulo") fica escondido.
        Transform title = notification.transform.Find("Conteúdo/Título");
        Transform message = notification.transform.Find("Conteúdo/Conteúdo");
        if (title != null)
            title.gameObject.SetActive(false);
        if (message != null && message.TryGetComponent(out TextMeshProUGUI messageText))
            messageText.text = "Um achado foi encontrado";
        else
            Debug.LogWarning(
                "InspectionFindingsAlertController: \"Conteúdo/Conteúdo\" text was not found in the notification."
            );

        Transform close = notification.transform.Find("Fechar");
        if (close != null && close.TryGetComponent(out Button closeButton))
            closeButton.onClick.AddListener(() => Destroy(notification));
        else
            Debug.LogWarning(
                "InspectionFindingsAlertController: \"Fechar\" has no Button; the notification cannot be closed."
            );

        notification.SetActive(true);
    }

    public void ShowCurrentFindings()
    {
        if (evaluation?.CurrentEmail == null)
            return;

        List<EvidenceData> findings = evaluation.CurrentEmail.evidences;
        ActionController.SendAction(
            AlertManagerController.ShowFindingsAction,
            new FindingsAlertData(
                "[ACHADOS DO INSPETOR]",
                BuildSummary(findings),
                BuildDetails(findings)
            )
        );
    }

    private static string BuildSummary(List<EvidenceData> findings)
    {
        int count = findings?.Count ?? 0;
        return $"O Inspector registrou {count} achado(s).\n"
            + "Use \"Ver regras\" para consultar os detalhes.";
    }

    private static string BuildDetails(List<EvidenceData> findings)
    {
        StringBuilder text = new();
        if (findings == null)
            return string.Empty;

        foreach (EvidenceData finding in findings)
        {
            text.AppendLine($"[{finding.field}] {finding.rule}");
            text.AppendLine(finding.evidence);
            text.AppendLine(finding.explanation);
            text.AppendLine();
        }

        return text.ToString();
    }
}
