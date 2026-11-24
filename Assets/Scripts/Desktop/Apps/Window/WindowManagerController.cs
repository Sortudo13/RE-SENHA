using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WindowManagerController : MonoBehaviour
{
    private static int activeWindowCount = 0;
    public static int ActiveWindows => activeWindowCount;
    public static Transform WindowsLayer { get; private set; }

    [Header("Templates de Janela")]
    [SerializeField]
    private WindowController windowEmail;

    [SerializeField]
    private WindowController windowManual;

    [SerializeField]
    private WindowController windowPDF;

    [SerializeField]
    private WindowController windowPNG;

    [SerializeField]
    private WindowController windowRG;

    [SerializeField]
    private WindowController windowChat;

    private void Awake()
    {
        // O contador e estatico e sobrevive a troca de cena; janelas destruidas junto com a cena
        // (ex.: ResetDayAndReload) nao descontam. Cena nova comeca sem janelas.
        activeWindowCount = 0;
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
                ActionController.SendAction<bool>("MinimizeAllWindow", true);
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
        gameObject.SetActive(!gameObject.activeSelf);
    }

    private void DeactivateDesktop()
    {
        gameObject.SetActive(false);
    }

    public void SwapWorkspace()
    {
        ActionController.SendAction(TutorialSignals.TaskbarUsed, "show_desktop");
        if (activeWindowCount > 0)
            SwapDesktop();
        else
            DeactivateDesktop();
    }

    public void DestroyWindows()
    {
        if (activeWindowCount > 0)
            ActionController.SendAction<bool>("MinimizeAllWindow", false);

        foreach (Transform child in transform)
            Destroy(child.gameObject);
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
        ActionController.SyncSignal<AttachmentContent>("InitPDF", InitPDF);
        ActionController.SyncSignal<AttachmentContent>("InitPNG", InitPNG);
        ActionController.SyncSignal<AttachmentContent>("InitRG", InitRG);
        ActionController.SyncSignal<AttachmentContent>("InitChat", InitChat);
    }

    private void CloseTemplateSignals()
    {
        ActionController.UnsyncSignal<string>("OpenURL", OpenURL);
        ActionController.UnsyncSignal<AttachmentContent>("InitEmail", InitEmail);
        ActionController.UnsyncSignal<AttachmentContent>("InitManual", InitManual);
        ActionController.UnsyncSignal<AttachmentContent>("InitPDF", InitPDF);
        ActionController.UnsyncSignal<AttachmentContent>("InitPNG", InitPNG);
        ActionController.UnsyncSignal<AttachmentContent>("InitRG", InitRG);
        ActionController.UnsyncSignal<AttachmentContent>("InitChat", InitChat);
    }

    private void OpenURL(string url)
    {
        Debug.Log("Abertura de url na janela do app Navegador.");
    }

    private void InitManual(AttachmentContent data)
    {
        GameObject window_manual = Instantiate(windowManual.gameObject, transform);
        if (data is ManualData manualData)
            window_manual.GetComponentInChildren<ManualWindowController>().OpenAt(manualData.app, manualData.ruleId);
    }

    private void InitEmail(AttachmentContent data)
    {
        GameObject window_email = Instantiate(windowEmail.gameObject, transform);
        if (data is EmailData emailData)
            window_email.GetComponentInChildren<EmailController>().ShowMail(emailData);
    }

    private void InitPDF(AttachmentContent data)
    {
        GameObject window_PDF = Instantiate(windowPDF.gameObject, transform);
        ActionController.SendAction(TutorialSignals.DocumentOpened, "PDF");
        if (data is PDFData pdfData)
            window_PDF.GetComponentInChildren<PDFController>().LoadDocument(pdfData.text);
    }

    private void InitPNG(AttachmentContent data)
    {
        GameObject window_PNG = Instantiate(windowPNG.gameObject, transform);
        ActionController.SendAction(TutorialSignals.DocumentOpened, "PNG");
        if (data is PNGData pngData)
            window_PNG.transform.Find("Área de Conteúdo/Image").GetComponentInChildren<Image>().sprite = pngData.GetImage();
    }

    private void InitRG(AttachmentContent data)
    {
        GameObject window_rg = Instantiate(windowRG.gameObject, transform);
        ActionController.SendAction(TutorialSignals.DocumentOpened, "RG");
        if (data is not RGData rgData)
        {
            Debug.LogWarning("InitRG: anexo sem dados de RG; o documento abre vazio.");
            return;
        }

        RGController rg = window_rg.GetComponentInChildren<RGController>(true);
        if (rg != null)
            rg.Show(rgData);
        else
            Debug.LogError("InitRG: RGController nao encontrado no Window RG (adicionar na raiz do RG.prefab).");
    }

    private void InitChat(AttachmentContent data)
    {
        GameObject window_chat = Instantiate(windowChat.gameObject, transform);
    }
}
