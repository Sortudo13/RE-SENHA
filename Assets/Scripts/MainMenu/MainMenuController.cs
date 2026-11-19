using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void NovoJogo()
    {
        // Limpa o save do disco
        DataController.ResetGame();

        // Avisa todos os sistemas que o jogo foi resetado
        ActionController.SendAction("Resetar Jogo");

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