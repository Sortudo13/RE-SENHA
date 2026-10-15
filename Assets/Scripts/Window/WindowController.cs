using UnityEngine;
using System;

public class WindowController : MonoBehaviour
{
    public event Action OnDestroyedObject;

    private void Awake()
    {
        ActionController.SyncSignal("MinimizeAllWindow", MinimizeWindow);
    }

    public void OpenWindow()
    {
        if (!this.gameObject.activeInHierarchy)
        {
            ActionController.SendAction("AddActiveWindow");
            this.gameObject.SetActive(true);
        }
        this.transform.SetAsLastSibling();
    }

    public void MinimizeWindow()
    {
        if (this.gameObject.activeSelf)
        {
            ActionController.SendAction("SubActiveWindow");
            this.gameObject.SetActive(false);
        }
    }

    public void SwapWindow()
    {
        if (this.gameObject.activeInHierarchy)
            this.MinimizeWindow();
        else
            this.OpenWindow();
    }

    public void CloseWindow()
    {
        this.MinimizeWindow();
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal("MinimizeAllWindow", MinimizeWindow);
        OnDestroyedObject?.Invoke();
        OnDestroyedObject = null;
    }
}
