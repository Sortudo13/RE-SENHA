using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PDFController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI documentText;
    public TextMeshProUGUI pageCounterText;
    public Button nextButton;
    public Button prevButton;

    private int currentPage = 1;
    private int totalPages = 1;

    void Awake()
    {
        // Configura os botões
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PrevPage);

        string document = "Sem Texto.";
        LoadDocument(document);
    }

    // Método principal para injetar o texto na janela
    public void LoadDocument(string content)
    {
        documentText.text = content;

        // Força o TMP a recalcular as malhas e quebras de texto imediatamente.
        // Sem isso, a contagem de páginas retorna 0 no mesmo frame em que o texto é alterado.
        documentText.ForceMeshUpdate();

        // Obtém o total de páginas geradas pelo TMP
        totalPages = documentText.textInfo.pageCount;

        // Reseta para a primeira página
        currentPage = 1;
        UpdatePageDisplay();
    }

    public void NextPage()
    {
        if (currentPage < totalPages)
        {
            currentPage++;
            UpdatePageDisplay();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            UpdatePageDisplay();
        }
    }

    private void UpdatePageDisplay()
    {
        // Diz ao TextMeshPro qual página renderizar
        documentText.pageToDisplay = currentPage;

        // Atualiza o contador (ex: "1 / 4")
        if (pageCounterText != null)
            pageCounterText.text = $"{currentPage} / {totalPages}";

        // Habilita ou desabilita os botões dependendo da página
        prevButton.gameObject.SetActive(currentPage > 1);
        nextButton.gameObject.SetActive(currentPage < totalPages);
    }
}
