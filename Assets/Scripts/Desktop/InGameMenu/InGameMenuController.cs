using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenuController : MonoBehaviour
{
    [Header("Referências dos Botões")]
    [SerializeField]
    private Button exitButton;

    [SerializeField]
    private Button returnToMenuButton;

    [Header("Overlay de Pausa")]
    [SerializeField]
    private GameObject pauseOverlay;

    [Header("Configuração")]
    [SerializeField]
    private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        // Registra os cliques dos botões
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
    }

    private void OnDestroy()
    {
        // Remove os listeners para evitar vazamentos
        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnExitClicked);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.RemoveListener(OnReturnToMenuClicked);
    }

    // ============================================================
    // Ativação/Desativação do menu (pausa e retomada)
    // ============================================================
    private void OnEnable()
    {
        Time.timeScale = 0f;
        if (pauseOverlay != null) pauseOverlay.SetActive(true);
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
    }

    // ============================================================
    // Ações dos botões
    // ============================================================
    private void OnExitClicked()
    {
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

        // Salva o estado do jogo antes de sair
        ActionController.SendAction("SaveGame");

        // Carrega a cena do menu principal
        SceneManager.LoadScene(mainMenuSceneName);
    }
}