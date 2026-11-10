using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IconController : MonoBehaviour
{
    [Header("Icon Settings")]
    [SerializeField] private bool onDesktop;

    [Header("Window Settings")]
    [SerializeField] private WindowController windowPrefab;

    private GameObject connectedWindow;

    [Header("Icon Notification Badge")]
    [SerializeField] private Image notificationBadge;
    private string appName;

    private void Start() => ApplyDesktopSettings();
    private void OnValidate() => ApplyDesktopSettings();

    private void OnEnable()
    {
        ActionController.SyncSignal<string>("ShowNotificationBadge", ShowNotificationBadge);
        ActionController.SyncSignal<string>("HideNotificationBadge", HideNotificationBadge);
    }
    private void OnDisable()
    {
        ActionController.UnsyncSignal<string>("ShowNotificationBadge", ShowNotificationBadge);
        ActionController.UnsyncSignal<string>("HideNotificationBadge", HideNotificationBadge);
    }

    private void ApplyDesktopSettings()
    {
        if (!onDesktop)
            return;

        Image iconImage = GetComponentInChildren<Image>();
        if (iconImage != null && windowPrefab.AppIcon != null)
        {
            iconImage.sprite = windowPrefab.AppIcon;
        }

        TMP_Text iconText = GetComponentInChildren<TMP_Text>();
        if (iconText != null && windowPrefab.AppName != null)
        {
            iconText.text = appName = windowPrefab.AppName;

        }
    }

    public void OpenApp()
    {
        if (connectedWindow == null)
        {
            // Instancia o GameObject da janela na hierarquia
            connectedWindow = Instantiate(windowPrefab.gameObject, WindowManagerController.WindowsLayer);
        }
        else
        {
            // Se existe anteriormente, ativa e foca
            if (connectedWindow.TryGetComponent(out WindowController windowController))
            {
                windowController.OpenWindow();
                ActionController.SendAction($"ToFront_{windowController.AppID}");
            }
        }
        ActionController.SendAction("HideNotificationBadge", appName);
        
    }

    public void ShowNotificationBadge(string app)
    {
        if(app == appName) notificationBadge.gameObject.SetActive(true); 
    }

    public void HideNotificationBadge(string app)
    {
        if (app == appName) notificationBadge.gameObject.SetActive(false); 
    }
}