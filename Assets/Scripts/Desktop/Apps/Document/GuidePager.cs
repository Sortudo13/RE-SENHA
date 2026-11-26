using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Paginas do Window Guide (guia.pdf), com setas e contador como o Leitor de PDF.
// Cada filho de "pages" e uma pagina; so a atual fica ativa. Para criar uma pagina nova,
// duplique uma "Pagina N" dentro de Folha/Paginas no Window Guide.prefab.
public class GuidePager : MonoBehaviour
{
    [SerializeField]
    private Transform pages;

    [SerializeField]
    private Button nextButton;

    [SerializeField]
    private Button prevButton;

    // Opcional: "1 / 8".
    [SerializeField]
    private TMP_Text pageCounter;

    private int currentPage;

    private void Awake()
    {
        if (pages == null) Debug.LogError("[GuidePager] Campo 'pages' nao foi arrastado.", this);
        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);
        ShowPage(0);
    }

    public void NextPage() => ShowPage(currentPage + 1);

    public void PrevPage() => ShowPage(currentPage - 1);

    private void ShowPage(int index)
    {
        int total = pages != null ? pages.childCount : 0;
        currentPage = Mathf.Clamp(index, 0, Mathf.Max(0, total - 1));

        for (int i = 0; i < total; i++)
            pages.GetChild(i).gameObject.SetActive(i == currentPage);

        if (pageCounter != null) pageCounter.text = total > 0 ? $"{currentPage + 1} / {total}" : string.Empty;
        if (prevButton != null) prevButton.gameObject.SetActive(currentPage > 0);
        if (nextButton != null) nextButton.gameObject.SetActive(currentPage < total - 1);
    }
}
