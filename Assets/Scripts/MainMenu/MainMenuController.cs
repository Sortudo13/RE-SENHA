using UnityEngine.SceneManagement;

public class MainMenuController : UnityEngine.MonoBehaviour
{
    public void NovoJogo()
    {
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
        SceneManager.LoadScene("Desktop");
    }

    public void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
