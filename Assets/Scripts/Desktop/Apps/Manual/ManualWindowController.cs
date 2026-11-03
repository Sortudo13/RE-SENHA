using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManualWindowController : MonoBehaviour
{
    [Header("Dados do Manual")]
    public ManualData bancoDeRegras;
    public int nivelAtual = 1;

    [Header("Referências - Painel Esquerdo")]
    public Transform containerRegras;
    public GameObject prefabItemRegra;

    [Header("Referências - Painel Direito")]
    public TextMeshProUGUI txtTituloRegra;
    public TextMeshProUGUI txtDescricaoRegra;
    public TextMeshProUGUI txtStatusVigor;
    public GameObject prefabSubregra;
    private RegraData regraExibida;
    private GameObject subrulesContainer;

    void Start()
    {
        CarregarRegras();
    }

    public void CarregarRegras()
    {
        if (bancoDeRegras == null) return;

        // Limpa instâncias anteriores do container
        foreach (Transform child in containerRegras)
        {
            Destroy(child.gameObject);
        }

        List<RegraData> regrasAtivas = bancoDeRegras.ObterRegrasAtivas(nivelAtual);

        foreach (RegraData regra in regrasAtivas)
        {
            RegraData regraAtual = regra;
            CreateRuleItem(regraAtual, $"{regra.idRegra}: {regra.titulo}");
        }

        // Exibe a primeira regra por padrão
        if (regrasAtivas.Count > 0)
        {
            RegraData primeiraRegra = regrasAtivas[0];
            ExibirDetalhes(primeiraRegra);
        }
    }

    private void CreateRuleItem(RegraData regra, string label)
    {
        GameObject novoItem = Instantiate(prefabItemRegra, containerRegras);
        TextMeshProUGUI txtItem = novoItem.GetComponentInChildren<TextMeshProUGUI>();
        if (txtItem != null)
        {
            txtItem.text = label;
        }

        Button ruleButton = novoItem.GetComponent<Button>();
        if (ruleButton != null)
            ruleButton.onClick.AddListener(() => ExibirDetalhes(regra));
    }

    public void ExibirDetalhes(RegraData regra)
    {
        regraExibida = regra;
        if (txtTituloRegra != null) 
        {
            txtTituloRegra.text = regra.titulo;
        }

        if (txtDescricaoRegra != null) 
            txtDescricaoRegra.text = regra.descricao;

        if (txtStatusVigor != null) 
            txtStatusVigor.text = $"<color=#00FF66>● EM VIGOR (Nível {regra.nivelIntroducao})</color>";

        CreateSubrulesPage(regra);
    }

    private void CreateSubrulesPage(RegraData regra)
    {
        if (subrulesContainer != null)
            Destroy(subrulesContainer);

        subrulesContainer = new GameObject(
            "Subregras",
            typeof(RectTransform),
            typeof(VerticalLayoutGroup)
        );
        RectTransform containerRect = subrulesContainer.GetComponent<RectTransform>();
        containerRect.SetParent(txtDescricaoRegra.transform.parent, false);
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.pivot = new Vector2(0.5f, 1f);
        containerRect.offsetMin = new Vector2(20f, 45f);
        containerRect.offsetMax = new Vector2(-20f, -105f);
        containerRect.SetAsLastSibling();

        VerticalLayoutGroup layout = subrulesContainer.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 14f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        SubregraData subregra = regra.subregras != null && regra.subregras.Count > 0
            ? regra.subregras[0]
            : null;
        if (subregra == null)
            return;

        if (prefabSubregra == null)
        {
            Debug.LogError("ManualWindowController requires a subrule item prefab.");
            return;
        }

        GameObject subruleObject = Instantiate(prefabSubregra, subrulesContainer.transform);
        TextMeshProUGUI text = subruleObject.GetComponentInChildren<TextMeshProUGUI>();
        if (text == null)
        {
            Debug.LogError("The subrule item prefab requires a TextMeshProUGUI child.");
            return;
        }

        text.text = $"{subregra.idSubregra}: {subregra.descricao}";

        InspectionRuleTextSelectable selectable =
            text.gameObject.GetComponent<InspectionRuleTextSelectable>();
        if (selectable == null)
            selectable = text.gameObject.AddComponent<InspectionRuleTextSelectable>();
        selectable.Initialize(regra, subregra);
    }
}