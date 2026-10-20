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

    private void OnDestroy()
    {
        ActionController.UnsyncSignal("AddActiveWindow", AddWindowCount);
        ActionController.UnsyncSignal("SubActiveWindow", SubWindowCount);

        CloseTemplateSignals();
    }

    // Implantação dos templates para abrir demais documentos / prefabs de janela
    private void OpenTemplateSignals()
    {
        ActionController.SyncSignal<string>("OpenURL", OpenURL);
        ActionController.SyncSignal<AttachmentContent>("InitEmail", InitEmail);
        ActionController.SyncSignal<AttachmentContent>("InitManual", InitManual);
        ActionController.SyncSignal<AttachmentContent>("InitRG", InitRG);
    }

    private void CloseTemplateSignals()
    {
        ActionController.UnsyncSignal<string>("OpenURL", OpenURL);
        ActionController.UnsyncSignal<AttachmentContent>("InitEmail", InitEmail);
        ActionController.UnsyncSignal<AttachmentContent>("InitManual", InitManual);
        ActionController.UnsyncSignal<AttachmentContent>("InitRG", InitRG);
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
}
