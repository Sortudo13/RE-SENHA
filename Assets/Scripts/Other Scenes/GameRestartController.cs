using UnityEngine.SceneManagement;

public class GameRestartController : UnityEngine.MonoBehaviour
{
    public void RestartGame()
    {
        DataController.ResetDays();
        SceneManager.LoadScene("Desktop");
    }
}
