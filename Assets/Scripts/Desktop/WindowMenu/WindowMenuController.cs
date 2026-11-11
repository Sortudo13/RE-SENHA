using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(WindowController))]
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

    private WindowController _window;

    private void Awake()
    {
        _window = GetComponent<WindowController>();

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
    }

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
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDestroy()
    {
        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnExitClicked);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.RemoveListener(OnReturnToMenuClicked);
    }
}
