using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlertManagerController : MonoBehaviour
{
    public const string ShowFindingsAction = "ShowFindingsAlert";

    public static Transform AlertsLayer { get; private set; }

    [Header("Alert Window")]
    [SerializeField] private GameObject alertWindow;
    [SerializeField] private GameObject findingsAlertWindow;

    private void Awake()
    {
        AlertsLayer = transform;
        ActionController.SyncSignal<FeedbackData>("ShowFeedback", ShowFeedback);
        ActionController.SyncSignal("ShowNextDay", ShowNextDay);
        ActionController.SyncSignal<FindingsAlertData>(ShowFindingsAction, ShowFindings);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<FeedbackData>("ShowFeedback", ShowFeedback);
        ActionController.UnsyncSignal("ShowNextDay", ShowNextDay);
        ActionController.UnsyncSignal<FindingsAlertData>(ShowFindingsAction, ShowFindings);
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
        }

        txtTituloAviso.text = "[Gerência]";
        txtTituloAviso.color = new Color(53 / 255f, 128 / 255f, 19 / 255f);
        txtFeedback.text = "Você terminou o seu expediente. Clique abaixo para avançar ao próximo dia.";

        connectedAlert.SetActive(true);
    }

    private void ShowFindings(FindingsAlertData findings)
    {
        GameObject connectedAlert = Instantiate(findingsAlertWindow, AlertsLayer);
        Transform content = connectedAlert.transform.GetChild(0);
        TextMeshProUGUI title = content.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI message = content.GetChild(1).GetComponent<TextMeshProUGUI>();
        Button button = content.GetChild(2).GetComponent<Button>();
        TextMeshProUGUI buttonLabel = button.GetComponentInChildren<TextMeshProUGUI>();

        title.text = findings.title;
        message.text = findings.summary;
        buttonLabel.text = "Ver regras";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            message.text = findings.details;
            buttonLabel.text = "Fechar";
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Destroy(connectedAlert));
        });

        connectedAlert.SetActive(true);
    }
}

public class FindingsAlertData
{
    public string title;
    public string summary;
    public string details;

    public FindingsAlertData(string title, string summary, string details)
    {
        this.title = title;
        this.summary = summary;
        this.details = details;
    }
}
