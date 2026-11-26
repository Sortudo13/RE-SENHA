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
        ActionController.SyncSignal("DayChanged", AtualizarTexto);
        ActionController.SyncSignal("AdvanceDay", AdvanceDay);
        ActionController.SyncSignal("RetrocedeDay", RetrocedeDay);
        ActionController.SyncSignal("ResetDay", ResetDay);
        ActionController.SyncSignal("LoadDay", LoadDay);
        ActionController.SyncSignal("ExitGame", ExitGame);
        ActionController.SyncSignal("LoadMainMenu", LoadMainMenu);
    }

    private void OnDisable()
    {
        ActionController.UnsyncSignal("DayChanged", AtualizarTexto);
        ActionController.UnsyncSignal("AdvanceDay", AdvanceDay);
        ActionController.UnsyncSignal("RetrocedeDay", RetrocedeDay);
        ActionController.UnsyncSignal("ResetDay", ResetDay);
        ActionController.UnsyncSignal("LoadDay", LoadDay);
        ActionController.UnsyncSignal("ExitGame", ExitGame);
        ActionController.UnsyncSignal("LoadMainMenu", LoadMainMenu);
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

    private void RetrocedeDay()
    {
        LoadingScreen.Run(DataController.RetrocedeDay, "Retrocedendo o dia...");
    }

    private void LoadDay()
    {
        LoadingScreen.Run(DataController.LoadDay, "Carregando o dia...");
    }

    private void ResetDay()
    {
        DataController.ResetDayAndReload();
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f;
        LoadingScreen.LoadScene("MainMenu");
    }
}
