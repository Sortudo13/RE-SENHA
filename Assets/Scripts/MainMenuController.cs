using UnityEngine.SceneManagement;

public class MainMenuController : UnityEngine.MonoBehaviour
{
    public void NovoJogo()
    {
        DataController.DeleteSaveFile();
        DataController.ResetDays();
        AbrirDesktop();
    }

    public void Carregar()
    {
        SaveData save = DataController.LoadEmails();
        DataController.CurrentDay = save.current_day;
        DataController.CurrentErrorCount = save.error_count;
        AbrirDesktop();
    }

    private void AbrirDesktop()
    {
        SceneManager.LoadScene("Desktop");
    }
}
