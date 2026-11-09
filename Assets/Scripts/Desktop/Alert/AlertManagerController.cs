using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlertManagerController : MonoBehaviour
{
    public static Transform AlertsLayer { get; private set; }

    [Header("Alert Window")]
    [SerializeField] private GameObject alertWindow;

    private void Awake()
    {
        AlertsLayer = transform;
        ActionController.SyncSignal<FeedbackData>("ShowFeedback", ShowFeedback);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<FeedbackData>("ShowFeedback", ShowFeedback);
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
}
