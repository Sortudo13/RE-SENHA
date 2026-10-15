using TMPro;
using UnityEngine;

public class DesktopCalendarController : MonoBehaviour
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
        ActionController.SyncSignal("DayChanged", AtualizarTexto);
    }

    private void OnDisable()
    {
        ActionController.UnsyncSignal("DayChanged", AtualizarTexto);
    }

    public void AtualizarTexto()
    {
        if (textTMPro != null)
            textTMPro.text = $"Dia {DataController.CurrentDay:D2}";
    }
}
