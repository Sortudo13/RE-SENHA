using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DesktopIconController : MonoBehaviour
{
    [Header("Icon Settings")]
    [SerializeField] private bool onDesktop;
    [SerializeField] private Sprite appIcon;
    [SerializeField] private string appName;


    [Header("Window Settings")]
    [SerializeField] private GameObject window;
    [SerializeField] private Transform layer;

    [Header("Taskbar Settings")]
    [SerializeField] private GameObject taskbarButton;
    [SerializeField] private Transform taskbarLayer;

    private GameObject connectedWindow;
    private GameObject connectedButton;

    private void Start()
    {
        ApplyDesktopSettings();
    }

    private void OnValidate()
    {
        ApplyDesktopSettings();
    }

    private void ApplyDesktopSettings()
    {
        if (!onDesktop)
            return;

        Image iconImage = GetComponentInChildren<Image>();
        if (iconImage != null && appIcon != null)
        {
            iconImage.sprite = appIcon;
        }

        TMP_Text iconText = GetComponentInChildren<TMP_Text>();
        if (iconText != null)
        {
            iconText.text = appName;
        }
    }

    public void OpenApp()
    {
        if (connectedWindow == null)
        {
            connectedWindow = Instantiate(window, layer);
            connectedButton = Instantiate(taskbarButton, taskbarLayer);

            if (connectedWindow.TryGetComponent(out WindowController windowController))
            {
                if (connectedButton.TryGetComponent(out Image buttonImage))
                {
                    if (appIcon != null)
                        buttonImage.sprite = appIcon;
                }

                if (connectedButton.TryGetComponent(out Button button))
                {
                    button.onClick.AddListener(windowController.SwapWindow);
                }

                windowController.OnDestroyedObject += DestroyButton;
            }
        }
        else
        {
            if (connectedWindow.TryGetComponent(out WindowController windowController))
            {
                windowController.OpenWindow();
            }
            connectedButton.transform.SetAsFirstSibling();
        }
    }

    private void DestroyButton()
    {
        Destroy(connectedButton);
    }
}