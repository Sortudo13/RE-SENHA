using System;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;

public class WindowController : MonoBehaviour
{
    [SerializeField]
    private Sprite windowIcon;

    [SerializeField]
    private string windowName;

    [Header("Nome da Janela (para exibição no header)")]
    [SerializeField]
    [Tooltip(
        "Nome que aparece no header da janela. Se não for fornecido, o nome do app será usado."
    )]
    private string windowHeaderName;
    private TextMeshProUGUI headerText;

    [Header("Comportamento")]
    [SerializeField]
    private bool showInTaskbar = true; //para casos como o Window Menu, que não deve aparecer na taskbar

    [SerializeField]
    private bool minimizeWithAll = true;

    private string myID;

    public string AppID => myID;
    public string AppName => windowName;
    public Sprite AppIcon => windowIcon;

    [SerializeField]
    private Animator animator;

    private void Awake()
    {
        myID = $"Win_{gameObject.GetEntityId()}";
        UpdateHeaderText();
    }

    private void OnValidate()
    {
        UpdateHeaderText();
    }

    private void UpdateHeaderText()
    {
        if (headerText == null)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name != "WindowHeaderName")
                    continue;

                headerText = child.GetComponent<TextMeshProUGUI>();
                break;
            }
        }

        if (headerText == null)
            return;

        headerText.text = string.IsNullOrWhiteSpace(windowHeaderName)
            ? windowName
            : windowHeaderName;
    }

    private void Start()
    {
        if (IsWindowMenu())
            return;

        ActionController.SendAction("AddActiveWindow");

        if (showInTaskbar)
        {
            WindowData data = new(myID, windowIcon, windowName);
            ActionController.SendAction("WindowCreated", data);
        }

        if (ShouldMinimizeWithAll())
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
        animator.SetTrigger("Open");
    }

    public void MinimizeWindow()
    {
        animator.SetTrigger("Minimize");
    }

    public void MinimizeWindow_AfterAnimation()
    { 
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
        if (IsWindowMenu())
        {
            Destroy(gameObject);
            return;
        }
        MinimizeWindow();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (ShouldMinimizeWithAll())
            ActionController.UnsyncSignal("MinimizeAllWindow", MinimizeWindow);
        ActionController.UnsyncSignal($"Swap_{myID}", SwapWindow);
        ActionController.UnsyncSignal($"ToFront_{myID}", ToFront);

        ActionController.SendAction($"WindowDestroyed_{myID}");
    }

    private bool ShouldMinimizeWithAll()
    {
        return minimizeWithAll && !IsWindowMenu();
    }

    private bool IsWindowMenu()
    {
        return GetComponentInChildren<WindowMenuController>() != null;
    }
}
