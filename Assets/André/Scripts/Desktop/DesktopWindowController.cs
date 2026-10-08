using UnityEngine;

public class DesktopWindowController : MonoBehaviour
{
    private int activeWindowCount = 0;
    public void OnWindowStatusChanged(bool isActive)
    {
        activeWindowCount += isActive ? 1 : -1;

        if (activeWindowCount < 0) activeWindowCount = 0;
    }
    public void SwapWorkspace()
    {

        if (!(gameObject.activeSelf && activeWindowCount == 0))
        {
            gameObject.SetActive(!gameObject.activeSelf);
        } else
        {
            gameObject.SetActive(false);
        }
    }

    public bool HasActiveWindow => activeWindowCount > 0;
}
