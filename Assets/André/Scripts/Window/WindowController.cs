using UnityEngine;
using System;

public class WindowController : MonoBehaviour
{
    public event Action OnDestroyedObject;
    private DesktopWindowController manager;

    private void Awake()
    {
        manager = GetComponentInParent<DesktopWindowController>(true);
    }

    public void OpenWindow()
    {
        manager.gameObject.SetActive(true);
        this.transform.SetAsLastSibling();
        this.gameObject.SetActive(true);
    }

    public void MinimizeWindow()
    {
        this.gameObject.SetActive(false);
    }

    public void SwapWindow()
    {
        if (!(manager.gameObject.activeSelf && this.gameObject.activeSelf))
        {
            this.OpenWindow();
        } else
        {
            this.MinimizeWindow();
        }
    }

    public void CloseWindow()
    {
        this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        OnDestroyedObject?.Invoke();
        OnDestroyedObject = null;
    }
    private void OnEnable()
    {
        manager.gameObject.SetActive(true);
        manager.OnWindowStatusChanged(true);
    }

    private void OnDisable()
    {
        manager.OnWindowStatusChanged(false);
    }
}
