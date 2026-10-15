using UnityEngine;

public class DesktopWindowController : MonoBehaviour
{
    private int activeWindowCount = 0;

    private void Awake()
    {
        ActionController.SyncSignal("AddActiveWindow", AddWindowCount);
        ActionController.SyncSignal("SubActiveWindow", SubWindowCount);
    }
    private void AddWindowCount ()
    {
        if (!gameObject.activeSelf)
        {
            if (activeWindowCount > 0)
            {
                ActionController.SendAction("MinimizeAllWindow");
            }
            ActivateDesktop();
        }

        activeWindowCount++;
    }

    private void SubWindowCount ()
    {
        activeWindowCount--;
        if (activeWindowCount < 0) activeWindowCount = 0;
    }

    private void ActivateDesktop()
    {
        gameObject.SetActive(true);
    }

    private void SwapDesktop()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    private void DeactivateDesktop()
    {
        gameObject.SetActive(false);
    }

    public void SwapWorkspace()
    {
        if (activeWindowCount > 0)
            SwapDesktop();
        else
            DeactivateDesktop();
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal("AddActiveWindow", AddWindowCount);
        ActionController.UnsyncSignal("SubActiveWindow", SubWindowCount);
    }
}
