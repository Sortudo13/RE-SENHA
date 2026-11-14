using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Fica na raiz do prefab "Alert Achados". Mostra um achado por pagina; o titulo e o do prefab.
public sealed class FindingsAlertWindow : MonoBehaviour
{
    private const string EmptyMessage = "Nenhum achado registrado.";

    [SerializeField]
    private TextMeshProUGUI message;

    [SerializeField]
    private Button previousButton;

    [SerializeField]
    private Button nextButton;

    [SerializeField]
    private Button closeButton;

    private string[] pages;
    private int page;

    private void Awake()
    {
        if (previousButton != null)
            previousButton.onClick.AddListener(ShowPreviousPage);
        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextPage);
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnDestroy()
    {
        if (previousButton != null)
            previousButton.onClick.RemoveListener(ShowPreviousPage);
        if (nextButton != null)
            nextButton.onClick.RemoveListener(ShowNextPage);
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }

    public void Show(string[] findingPages)
    {
        pages = findingPages != null && findingPages.Length > 0
            ? findingPages
            : new[] { EmptyMessage };
        ShowPage(0);
        gameObject.SetActive(true);
    }

    private void ShowPreviousPage()
    {
        ShowPage(page - 1);
    }

    private void ShowNextPage()
    {
        ShowPage(page + 1);
    }

    private void ShowPage(int index)
    {
        if (pages == null)
            return;

        page = Mathf.Clamp(index, 0, pages.Length - 1);
        if (message != null)
            message.text = pages[page];
        if (previousButton != null)
            previousButton.interactable = page > 0;
        if (nextButton != null)
            nextButton.interactable = page < pages.Length - 1;
    }

    private void Close()
    {
        Destroy(gameObject);
    }
}
