using Unity.VisualScripting;
using UnityEngine;

public class WindowManagerController : MonoBehaviour
{
    private int activeWindowCount = 0;
    private bool desktopWindowsHidden;
    public static Transform WindowsLayer { get; private set; }

    [Header("Templates de Janela")]
    [SerializeField]
    private WindowController windowEmail;

    [SerializeField]
    private WindowController windowManual;

    [SerializeField]
    private WindowController windowRG;

    [SerializeField]
    private WindowController windowMenu;

    private void Awake()
    {
        WindowsLayer = transform;
        ActionController.SyncSignal("AddActiveWindow", AddWindowCount);
        ActionController.SyncSignal("SubActiveWindow", SubWindowCount);
        ActionController.SyncSignal("DestroyWindows", DestroyWindows);
        ActionController.SyncSignal("DayChanged", DestroyWindows);

        OpenTemplateSignals();
    }

    private void AddWindowCount()
    {
        if (!gameObject.activeSelf)
        {
            if (activeWindowCount > 0)
            {
                ActionController.SendAction("MinimizeAllWindow");
            }
            ActivateDesktop();
        }

        activeWindowCount++;
    }

    private void SubWindowCount()
    {
        activeWindowCount--;
        if (activeWindowCount < 0)
            activeWindowCount = 0;
    }

    private void ActivateDesktop()
    {
        gameObject.SetActive(true);
    }

    private void SwapDesktop()
    {
        desktopWindowsHidden = !desktopWindowsHidden;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform window = transform.GetChild(i);
            if (window.GetComponentInChildren<WindowMenuController>() == null)
                window.gameObject.SetActive(!desktopWindowsHidden);
        }
    }

    private void DeactivateDesktop()
    {
        gameObject.SetActive(false);
    }

    public void SwapWorkspace()
    {
        CloseOpenWindowMenu();

        if (activeWindowCount > 0)
            SwapDesktop();
        else
            DeactivateDesktop();
    }

    public void DestroyWindows()
    {
        if (activeWindowCount > 0)
            ActionController.SendAction("MinimizeAllWindow");

        foreach (Transform child in transform)
            Destroy(child.gameObject);
            
    }
    private void CloseOpenWindowMenu()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            WindowController window = transform
                .GetChild(i)
                .GetComponentInChildren<WindowController>();

            if (
                window != null
                && window.GetComponentInChildren<WindowMenuController>() != null
            )
            {
                window.CloseWindow();
                return;
            }
        }
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal("AddActiveWindow", AddWindowCount);
        ActionController.UnsyncSignal("SubActiveWindow", SubWindowCount);
        ActionController.UnsyncSignal("DestroyWindows", DestroyWindows);
        ActionController.UnsyncSignal("DayChanged", DestroyWindows);

        CloseTemplateSignals();
    }

    // Implantação dos templates para abrir demais documentos / prefabs de janela
    private void OpenTemplateSignals()
    {
        ActionController.SyncSignal<string>("OpenURL", OpenURL);
        ActionController.SyncSignal<AttachmentContent>("InitEmail", InitEmail);
        ActionController.SyncSignal<AttachmentContent>("InitManual", InitManual);
        ActionController.SyncSignal<AttachmentContent>("InitRG", InitRG);
        ActionController.SyncSignal("OpenWindowMenu", OpenWindowMenu);
    }

    private void CloseTemplateSignals()
    {
        ActionController.UnsyncSignal<string>("OpenURL", OpenURL);
        ActionController.UnsyncSignal<AttachmentContent>("InitEmail", InitEmail);
        ActionController.UnsyncSignal<AttachmentContent>("InitManual", InitManual);
        ActionController.UnsyncSignal<AttachmentContent>("InitRG", InitRG);
        ActionController.UnsyncSignal("OpenWindowMenu", OpenWindowMenu);
    }

    private void OpenURL(string url)
    {
        Debug.Log("Abertura de url na janela do app Navegador.");
    }

    private void InitEmail(AttachmentContent data)
    {
        Instantiate(windowEmail.gameObject, transform);
    }

    private void InitManual(AttachmentContent data)
    {
        Instantiate(windowManual.gameObject, transform);
    }

    private void InitRG(AttachmentContent data)
    {
        Instantiate(windowRG.gameObject, transform);
    }

    private void OpenWindowMenu()
    {
        ActivateDesktop();
        Instantiate(windowMenu.gameObject, transform);
    }
}
