public class GameRestartController : UnityEngine.MonoBehaviour
{
    public void RestartGame()
    {
        DataController.ResetGame();
        LoadingScreen.LoadScene("Desktop");
    }
}
