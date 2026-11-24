using UnityEngine;

public class SwapController : MonoBehaviour
{
    [SerializeField]
    private bool sendsPauseMenuTutorial;

    public void Swap()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        if (gameObject.activeSelf && sendsPauseMenuTutorial)
            ActionController.SendAction(TutorialSignals.PauseMenuOpened, "");
    }
}
