using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IconController : MonoBehaviour
{
    [Header("Icon Settings")]
    [SerializeField] private bool onDesktop;
    [SerializeField] private int dayToAppear = 1;

    [Header("Window Settings")]
    [SerializeField] private WindowController windowPrefab;
    private GameObject connectedWindow;

    [Header("Icon Notification Badge")]
    [SerializeField] private Image notificationBadge;

    private void Start() => CheckExistence();
    private void OnValidate() => ApplyDesktopSettings();
    private void Awake() => ActionController.SyncSignal("DayChanged", CheckExistence);
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
    private void OnDestroy() => ActionController.UnsyncSignal("DayChanged", CheckExistence);

    private void ApplyDesktopSettings()
    {
        if (!onDesktop || windowPrefab == null)
            return;

        Image iconImage = GetComponentInChildren<Image>();
        if (iconImage != null && windowPrefab.AppIcon != null)
            iconImage.sprite = windowPrefab.AppIcon;

        TMP_Text iconText = GetComponentInChildren<TMP_Text>();
        if (iconText != null && windowPrefab.AppName != null)
            iconText.text = windowPrefab.AppName;
    }

    private void CheckExistence() => gameObject.SetActive(dayToAppear <= DataController.CurrentDay);

    public void OpenApp()
    {
        if (connectedWindow == null)
            connectedWindow = Instantiate(windowPrefab.gameObject, WindowManagerController.WindowsLayer);
        else
            if (connectedWindow.TryGetComponent(out WindowController windowController))
            {
                windowController.OpenWindow();
                ActionController.SendAction($"ToFront_{windowController.AppID}");
            }
        
        ActionController.SendAction("HideNotificationBadge", windowPrefab.AppName);
    }

    private void ShowNotificationBadge(string app)
    {
        if(app == windowPrefab.AppName) notificationBadge.gameObject.SetActive(true); 
    }

    private void HideNotificationBadge(string app)
    {
        if (app == windowPrefab.AppName) notificationBadge.gameObject.SetActive(false); 
    }
}