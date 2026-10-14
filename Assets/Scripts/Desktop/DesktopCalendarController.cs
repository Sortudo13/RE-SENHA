using TMPro;
using UnityEngine;

public class DesktopCalendarController : MonoBehaviour
{
    private TextMeshProUGUI textTMPro;

    void Awake()
    {
        if (TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI component))
            textTMPro = component;

        DataController.OnDayChanged += AtualizarTexto;
        AtualizarTexto();
    }

    public void AtualizarTexto()
    {
        textTMPro.text = $"Dia {DataController.CurrentDay:D2}";
    }
}
