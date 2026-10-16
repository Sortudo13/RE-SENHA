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
        DataController.ResetDays();
    }
}
