using TMPro;
using UnityEngine;

public class CalendarController : MonoBehaviour
{
    private TextMeshProUGUI textTMPro;

    void Awake()
    {
        if (TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI component))
            textTMPro = component;

        AtualizarTexto();
        ActionController.SyncSignal("Resetar Jogo", ResetGame);
    }

    private void OnEnable()
    {
        ActionController.SyncSignal("DayChanged", AtualizarTexto);
    }

    private void OnDisable()
    {
        ActionController.UnsyncSignal("DayChanged", AtualizarTexto);
    }

    private void AtualizarTexto()
    {
        if (textTMPro != null)
            textTMPro.text = $"Dia {DataController.CurrentDay:D2}";
    }

    private void ResetGame()
    {
        DataController.ResetGame();
        // Debug show notification badge
        ActionController.SendAction("ShowNotificationBadge", "Email");
        ActionController.SendAction("ShowNotificationBadge", "Banco de Dados");
        ActionController.SendAction("ShowNotificationBadge", "Requisições");
        ActionController.SendAction("ShowNotificationBadge", "Manual");
    }
}
