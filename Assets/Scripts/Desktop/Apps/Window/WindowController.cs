using UnityEngine;
using System;
using TMPro;

public class WindowController : MonoBehaviour
{
    [SerializeField] private Sprite windowIcon;
    [SerializeField] private string windowName;
    private string myID;

    public string AppID => myID;
    public string AppName => windowName;
    public Sprite AppIcon => windowIcon;

    private void Awake()
    {
        myID = $"Win_{gameObject.GetEntityId()}";
    }

    private void Start()
    {
        ActionController.SendAction("AddActiveWindow");

        WindowData data = new(myID, windowIcon, windowName);
        ActionController.SendAction("WindowCreated", data);

        ActionController.SyncSignal("MinimizeAllWindow", MinimizeWindow);
        ActionController.SyncSignal($"Swap_{myID}", SwapWindow);
        ActionController.SyncSignal($"ToFront_{myID}", ToFront);
        ActionController.SendAction("OnWindowOpened");
    }

    private void ToFront()
    {
        transform.SetAsLastSibling();
    }

    public void OpenWindow()
    {
        if (!gameObject.activeInHierarchy)
        {   
            ActionController.SendAction("AddActiveWindow");
            gameObject.SetActive(true);
            ActionController.SendAction("OnWindowOpened");
        }
        ToFront();
    }

    public void MinimizeWindow()
    {
        if (gameObject.activeSelf)
        {
            ActionController.SendAction("SubActiveWindow");
            gameObject.SetActive(false);
        }
    }

    public void SwapWindow()
    {
        if (gameObject.activeInHierarchy)
        {
            MinimizeWindow();
        }
        else
        {
            OpenWindow();
        }
    }

    public void CloseWindow()
    {
        ActionController.SendAction("OnWindowClosed");
        MinimizeWindow();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal("MinimizeAllWindow", MinimizeWindow);
        ActionController.UnsyncSignal($"Swap_{myID}", SwapWindow);
        ActionController.UnsyncSignal($"ToFront_{myID}", ToFront);

        ActionController.SendAction($"WindowDestroyed_{myID}");
    }
}
