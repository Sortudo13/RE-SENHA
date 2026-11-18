using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void NovoJogo()
    {
        // Limpa o save do disco
        DataController.ResetGame();
        AbrirDesktop();
    }

    public void Carregar()
    {
        DataController.LoadingGame();
        AbrirDesktop();
    }

    private void AbrirDesktop()
    {
        LoadingScreen.LoadScene("Desktop");
    }

    public void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        UnityEngine.Application.Quit();
#endif
    }
}