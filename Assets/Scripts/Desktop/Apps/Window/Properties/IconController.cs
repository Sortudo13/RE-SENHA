using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IconController : MonoBehaviour
{
    [Header("Icon Settings")]
    [SerializeField]
    private bool onDesktop;

    [SerializeField]
    private int dayToAppear = 1;

    [Header("Window Settings")]
    [SerializeField]
    private WindowController windowPrefab;
    private GameObject connectedWindow;

    [Header("Icon Notification Badge")]
    [SerializeField]
    private Image notificationBadge;

    [SerializeField]
    private bool debugBadge = true;

    private void Start()
    {
        if (debugBadge)
            Debug.Log($"[BadgeDebug] IconController.Start. GameObject ativo={gameObject.activeSelf}. AppName='{windowPrefab?.AppName}'", this);
        CheckExistence();
    }

    private void OnValidate() => ApplyDesktopSettings();

    private void Awake()
    {
        if (debugBadge)
            Debug.Log($"[BadgeDebug] IconController.Awake. GameObject ativo={gameObject.activeSelf}. AppName='{windowPrefab?.AppName}'", this);
        ActionController.SyncSignal("DayChanged", CheckExistence);
    }

    private void OnEnable()
    {
        ActionController.SyncSignal<string>("ShowNotificationBadge", ShowNotificationBadge);
        ActionController.SyncSignal<string>("HideNotificationBadge", HideNotificationBadge);
        if (debugBadge)
            Debug.Log($"[BadgeDebug] IconController.OnEnable registrou sinais. AppName='{windowPrefab?.AppName}'. notificationBadge={(notificationBadge != null ? notificationBadge.name : "NULL")}", this);
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
            connectedWindow = Instantiate(
                windowPrefab.gameObject,
                WindowManagerController.WindowsLayer
            );
        else if (connectedWindow.TryGetComponent(out WindowController windowController))
        {
            windowController.OpenWindow();
            ActionController.SendAction($"ToFront_{windowController.AppID}");
        }

        ActionController.SendAction("HideNotificationBadge", windowPrefab.AppName);
        ActionController.SendAction(TutorialSignals.WindowOpened, windowPrefab.AppName);
    }

    private void ShowNotificationBadge(string app)
    {
        if (debugBadge)
            Debug.Log($"[BadgeDebug] ShowNotificationBadge chamado com app='{app}'. Meu AppName='{windowPrefab?.AppName}'. Match={app == windowPrefab?.AppName}. badge atribuído={notificationBadge != null}. badge ativo={notificationBadge?.gameObject.activeSelf}", this);
        if (app == windowPrefab.AppName)
            notificationBadge.gameObject.SetActive(true);
    }

    private void HideNotificationBadge(string app)
    {
        if (debugBadge)
            Debug.Log($"[BadgeDebug] HideNotificationBadge chamado com app='{app}'. Meu AppName='{windowPrefab?.AppName}'. Match={app == windowPrefab?.AppName}", this);
        if (app == windowPrefab.AppName)
            notificationBadge.gameObject.SetActive(false);
    }
}
