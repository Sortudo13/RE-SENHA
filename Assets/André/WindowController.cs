using UnityEngine;
using System;

public class WindowController : MonoBehaviour
{
    public event Action OnDestroyedObject;

    public void OpenWindow()
    {
        this.gameObject.SetActive(true);
    }

    public void MinimizeWindow()
    {
        this.gameObject.SetActive(false);
    }

    public void CloseWindow()
    {
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        OnDestroyedObject?.Invoke();
    }
}
