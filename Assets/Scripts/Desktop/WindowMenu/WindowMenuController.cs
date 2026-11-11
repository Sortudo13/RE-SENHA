using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WindowMenuController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField]
    private Button exitButton;

    [SerializeField]
    private Button returnToMenuButton;

    [Header("Configuração")]
    [SerializeField]
    private string mainMenuSceneName = "MainMenu";

    [SerializeField]
    private bool pauseGameWhileOpen = true;

    private void Awake()
    {
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
    }

    private void OnEnable()
    {
        if (pauseGameWhileOpen)
            Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        if (pauseGameWhileOpen)
            Time.timeScale = 1f;
    }

    private void OnExitClicked()
    {
        // Fecha o jogo (build) ou para o Play Mode (editor)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnReturnToMenuClicked()
    {
        // Garante que o tempo volte ao normal antes de trocar de cena
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDestroy()
    {
        // Remove listeners para evitar referências pendentes
        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnExitClicked);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.RemoveListener(OnReturnToMenuClicked);
    }
}
