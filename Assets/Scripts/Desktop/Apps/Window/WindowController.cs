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
    }

    private void ToFront()
    {
        transform.SetAsLastSibling();
    }

    public void OpenWindow()
    {
        ActionController.SendAction("OnWindowOpened");
        if (!gameObject.activeInHierarchy)
        {   
            ActionController.SendAction("AddActiveWindow");
            gameObject.SetActive(true);
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
        ActionController.SendAction("OnWindowOpened");
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

        // Dispara a ação global sem precisar saber quem vai tocar o som
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
