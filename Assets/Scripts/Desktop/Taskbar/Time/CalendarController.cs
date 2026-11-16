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
    }

    private void OnEnable()
    {
        ActionController.SyncSignal("Resetar Jogo", ResetGame);
        ActionController.SyncSignal("DayChanged", AtualizarTexto);
        ActionController.SyncSignal("AdvanceDay", AdvanceDay);
    }

    private void OnDisable()
    {
        ActionController.UnsyncSignal("Resetar Jogo", ResetGame);
        ActionController.UnsyncSignal("DayChanged", AtualizarTexto);
        ActionController.UnsyncSignal("AdvanceDay", AdvanceDay);
    }

    private void AtualizarTexto()
    {
        if (textTMPro != null)
            textTMPro.text = $"Dia {DataController.CurrentDay:D2}";
    }

    private void AdvanceDay()
    {
        LoadingScreen.Run(DataController.AdvanceDay, "Avançando o dia...");
    }

    private void ResetGame()
    {
        DataController.ResetGame();
        // Debug show notification badge
        ActionController.SendAction("ShowNotificationBadge", "Email");
        ActionController.SendAction("ShowNotificationBadge", "Banco de Dados");
        ActionController.SendAction("ShowNotificationBadge", "Requisi��es");
        ActionController.SendAction("ShowNotificationBadge", "Manual");
    }
}
