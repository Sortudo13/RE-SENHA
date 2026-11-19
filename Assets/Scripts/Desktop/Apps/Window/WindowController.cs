using System;
using TMPro;
using UnityEngine;

public class WindowController : MonoBehaviour
{
    [SerializeField] private Sprite windowIcon;
    [SerializeField] private string windowName;
    [SerializeField] private string windowHeaderName;

    private string myID;
    public string AppID => myID;
    public string AppName => windowName;
    public Sprite AppIcon => windowIcon;

    [SerializeField] private Animator animator;
    public int countActions = 0;

    private void Awake()
    {
        myID = $"Win_{gameObject.GetEntityId()}";
        UpdateHeaderText();
    }

    private void Start()
    {
        ActionController.SendAction("AddActiveWindow");

        WindowData data = new(myID, windowIcon, windowName);
        ActionController.SendAction("WindowCreated", data);

        ActionController.SyncSignal<bool>("MinimizeAllWindow", MinimizeWindow);
        ActionController.SyncSignal($"Swap_{myID}", SwapWindow);
        ActionController.SyncSignal($"ToFront_{myID}", ToFront);
        ActionController.SendAction("OnWindowOpened");

        if (WindowManagerController.ActiveWindows > DataController.MaxWindows)
        {
            ActionController.SendAction("ShowBlockedWindow");
            CloseWindow();
        }
    }

    public void ToFront()
    {
        if (transform.parent == null)
            return;

        int lastIndex = transform.parent.childCount - 1;
        if (transform.GetSiblingIndex() < lastIndex)
            transform.SetAsLastSibling();
    }

    public void OpenWindow()
    {
        countActions++;
        if (!gameObject.activeInHierarchy)
        {
            ActionController.SendAction("AddActiveWindow");
            gameObject.SetActive(true);
            ActionController.SendAction("OnWindowOpened");
        }
        ToFront();
        animator.SetTrigger(Animator.StringToHash("Open"));
    }

    public async void MinimizeWindow(bool animate = true)
    {
        int currentAction = ++countActions;
        if (animate)
        {
            animator.SetTrigger(Animator.StringToHash("Minimize"));
            await Awaitable.NextFrameAsync();
            float duration = animator.GetCurrentAnimatorStateInfo(0).length;
            await Awaitable.WaitForSecondsAsync(duration);
        }

        if (this == null || currentAction != countActions)
            return;

        if (gameObject.activeSelf)
        {
            ActionController.SendAction("SubActiveWindow");
            ActionController.SendAction("HideNotificationBadge", AppName);
            gameObject.SetActive(false);
        }
    }

    public void SwapWindow()
    {
        if (gameObject.activeInHierarchy)
            MinimizeWindow();
        else
            OpenWindow();
    }

    public void CloseWindow()
    {
        ActionController.SendAction("OnWindowClosed");
        MinimizeWindow(false);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<bool>("MinimizeAllWindow", MinimizeWindow);

        ActionController.UnsyncSignal($"Swap_{myID}", SwapWindow);
        ActionController.UnsyncSignal($"ToFront_{myID}", ToFront);

        ActionController.SendAction($"WindowDestroyed_{myID}");
    }

    private void UpdateHeaderText()
    {
        if (transform.Find("Barra Superior").Find("WindowHeaderName").TryGetComponent<TextMeshProUGUI>(out var textComponent))
            textComponent.text = string.IsNullOrWhiteSpace(windowHeaderName) ? windowName : windowHeaderName;
    }
}
