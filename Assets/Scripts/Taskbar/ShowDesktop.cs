using UnityEngine;

public class ShowDesktop : MonoBehaviour
{
    public void MinimizeAllWindows()
    {
        Window[] openWindows = FindObjectsByType<Window>();

        foreach (Window window in openWindows)
        {
            window.MinimizeWindow();
        }
    }
}
