using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlertManagerController : MonoBehaviour
{
    // Inspector: lista paginada de achados (string[], uma pagina por achado) e aviso curto de achado novo.
    public const string ShowFindingsAction = "ShowFindingsAlert";
    public const string ShowFindingNotificationAction = "ShowFindingNotification";

    public static Transform AlertsLayer { get; private set; }

    [Header("Alert Window")]
    [SerializeField] private GameObject alertWindow;

    [Header("Inspector")]
    [SerializeField] private FindingsAlertWindow findingsAlertWindow;
    // Prefab "Alert Notification": texto vem do prefab; o filho "Fechar" (Button) fecha.
    [SerializeField] private GameObject findingNotificationWindow;

    private void Awake()
    {
        AlertsLayer = transform;
        ActionController.SyncSignal<FeedbackData>("ShowFeedback", ShowFeedback);
        ActionController.SyncSignal("ShowNextDay", ShowNextDay);
        ActionController.SyncSignal<string[]>(ShowFindingsAction, ShowFindings);
        ActionController.SyncSignal(ShowFindingNotificationAction, ShowFindingNotification);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<FeedbackData>("ShowFeedback", ShowFeedback);
        ActionController.UnsyncSignal("ShowNextDay", ShowNextDay);
        ActionController.UnsyncSignal<string[]>(ShowFindingsAction, ShowFindings);
        ActionController.UnsyncSignal(ShowFindingNotificationAction, ShowFindingNotification);
    }

    private void ShowFeedback(FeedbackData feedback)
    {
        GameObject connectedAlert = Instantiate(alertWindow, AlertsLayer);
        GameObject content = connectedAlert.transform.GetChild(0).gameObject;
        TextMeshProUGUI txtTituloAviso = content.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI txtFeedback = content.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        connectedAlert.SetActive(false);

        if (content.transform.GetChild(2).TryGetComponent(out Button button))
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Destroy(connectedAlert));
        }

        txtFeedback.text = feedback.message;
        txtTituloAviso.text = feedback.isPositive ? "[ANÁLISE CORRETA]" : "[ALERTA DE SEGURANÇA]";
        
        if (!feedback.isPositive)
        {
            txtTituloAviso.color = new Color(150 / 255f, 58 / 255f, 27 / 255f); // Vermelho

            if (DataController.CurrentErrorCount < DataController.MaxErrors)
            {
                txtFeedback.text +=
                    "\n\nREGISTRO: Você possui "
                    + DataController.CurrentErrorCount
                    + "/"
                    + DataController.MaxErrors
                    + " advertências.";
            }
            else
            {
                txtFeedback.text +=
                    "\n\nCRÍTICO: Você acumulou "
                    + DataController.CurrentErrorCount
                    + " advertências. VOCÊ FOI DEMITIDO!";
            }
        }
        else
        {
            txtTituloAviso.color = new Color(53 / 255f, 128 / 255f, 19 / 255f); // Verde
        }

        connectedAlert.transform.SetAsLastSibling();
        connectedAlert.SetActive(true);
    }

    private void ShowNextDay()
    {
        GameObject connectedAlert = Instantiate(alertWindow, AlertsLayer);
        GameObject content = connectedAlert.transform.GetChild(0).gameObject;
        TextMeshProUGUI txtTituloAviso = content.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI txtFeedback = content.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        connectedAlert.SetActive(false);

        if (content.transform.GetChild(2).TryGetComponent(out Button button))
        {
            button.GetComponentInChildren<TextMeshProUGUI>().text = "Avançar Dia";
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => DataController.AdvanceDay());
            button.onClick.AddListener(() => Destroy(connectedAlert));
        }

        txtTituloAviso.text = "[Gerência]";
        txtTituloAviso.color = new Color(53 / 255f, 128 / 255f, 19 / 255f);
        txtFeedback.text = "Você terminou o seu expediente. Clique abaixo para avançar ao próximo dia.";

        connectedAlert.transform.SetAsFirstSibling();
        connectedAlert.SetActive(true);
    }

    private void ShowFindings(string[] pages)
    {
        if (findingsAlertWindow == null)
        {
            Debug.LogError("AlertManagerController: \"Findings Alert Window\" is not set. Drag \"Alert Achados\" into it.", this);
            return;
        }

        Instantiate(findingsAlertWindow, AlertsLayer).Show(pages);
    }

    private void ShowFindingNotification()
    {
        if (findingNotificationWindow == null)
        {
            Debug.LogError("AlertManagerController: \"Finding Notification Window\" is not set. Drag \"Alert Notification\" into it.", this);
            return;
        }

        GameObject notification = Instantiate(findingNotificationWindow, AlertsLayer);
        Transform close = notification.transform.Find("Fechar");
        if (close != null && close.TryGetComponent(out Button closeButton))
            closeButton.onClick.AddListener(() => Destroy(notification));
        else
            Debug.LogWarning("AlertManagerController: \"Fechar\" has no Button; the notification cannot be closed.", this);

        notification.SetActive(true);
    }
}
