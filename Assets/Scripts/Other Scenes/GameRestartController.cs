using UnityEngine.SceneManagement;

public class GameRestartController : UnityEngine.MonoBehaviour
{
    public void RestartGame()
    {
        DataController.ResetGame();
        SceneManager.LoadScene("Desktop");
    }
}
