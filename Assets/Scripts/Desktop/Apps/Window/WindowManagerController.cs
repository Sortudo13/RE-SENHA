using Unity.VisualScripting;
using UnityEngine;

public class WindowManagerController : MonoBehaviour
{
    private int activeWindowCount = 0;
    public static Transform WindowsLayer { get; private set; }

    [Header("Templates de Janela")]
    [SerializeField] private WindowController windowEmail;
    [SerializeField] private WindowController windowManual;
    [SerializeField] private WindowController windowRG;

    private void Awake()
    {
        WindowsLayer = transform;
        ActionController.SyncSignal("AddActiveWindow", AddWindowCount);
        ActionController.SyncSignal("SubActiveWindow", SubWindowCount);

        OpenTemplateSignals();
    }
    private void AddWindowCount ()
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

    private void SubWindowCount ()
    {
        activeWindowCount--;
        if (activeWindowCount < 0) activeWindowCount = 0;
    }

    private void ActivateDesktop()
    {
        gameObject.SetActive(true);
    }

    private void SwapDesktop()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    private void DeactivateDesktop()
    {
        gameObject.SetActive(false);
    }

    public void SwapWorkspace()
    {
        if (activeWindowCount > 0)
            SwapDesktop();
        else
            DeactivateDesktop();
    }

    private void OpenTemplateSignals()
    {
        ActionController.SyncSignal("InitEmail", InitEmail);
        ActionController.SyncSignal("InitManual", InitManual);
        ActionController.SyncSignal("InitRG", InitRG);
    }

    private void CloseTemplateSignals()
    {
        ActionController.UnsyncSignal("InitEmail", InitEmail);
        ActionController.UnsyncSignal("InitManual", InitManual);
        ActionController.UnsyncSignal("InitRG", InitRG);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal("AddActiveWindow", AddWindowCount);
        ActionController.UnsyncSignal("SubActiveWindow", SubWindowCount);

        CloseTemplateSignals();
    }

    private void InitEmail()
    {
        Instantiate(windowEmail.gameObject, transform);
    }

    private void InitManual()
    {
        Instantiate(windowManual.gameObject, transform);
    }

    private void InitRG() //Passar dados para preencher o RG
    {
        Instantiate(windowRG.gameObject, transform);
    }
}
