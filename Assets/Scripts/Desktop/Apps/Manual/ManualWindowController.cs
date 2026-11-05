using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManualWindowController : MonoBehaviour
{
    [Header("Dados do Manual")]
    [SerializeField]
    private string manualResourcePath = ManualRulesLoader.DefaultResourcePath;

    [SerializeField]
    public ManualData bancoDeRegras;

    private ManualRuleBookData manual;
    public int nivelAtual = 1;

    [Header("Referências - Painel Esquerdo")]
    public Transform containerRegras;
    public GameObject prefabItemRegra;

    [Header("Referências - Painel Direito")]
    public TextMeshProUGUI txtTituloRegra;
    public TextMeshProUGUI txtDescricaoRegra;
    public TextMeshProUGUI txtStatusVigor;

    [SerializeField]
    private InspectionRuleSelectable selectableSubrule;

    void Start()
    {
        CarregarRegras();
    }

    public void CarregarRegras()
    {
        try
        {
            manual = ManualRulesLoader.Load(manualResourcePath);
        }
        catch (System.InvalidOperationException exception)
        {
            Debug.LogError($"ManualWindowController: {exception.Message}");
            return;
        }

        // Limpa instâncias anteriores do container
        foreach (Transform child in containerRegras)
        {
            Destroy(child.gameObject);
        }

        var regrasAtivas = manual.rules.FindAll(regra =>
            regra.introductionDay <= nivelAtual
        );

        foreach (ManualRuleData regra in regrasAtivas)
        {
            GameObject novoItem = Instantiate(prefabItemRegra, containerRegras);

            TextMeshProUGUI txtItem = novoItem.GetComponentInChildren<TextMeshProUGUI>();
            if (txtItem != null)
                txtItem.text = $"{regra.id}: {regra.title}";

            ManualRuleData regraAtual = regra;
            Button button = novoItem.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => ExibirDetalhes(regraAtual));
        }

        // Exibe a primeira regra por padrão
        if (regrasAtivas.Count > 0)
        {
            ExibirDetalhes(regrasAtivas[0]);
        }
    }

    private void ExibirDetalhes(ManualRuleData regra)
    {
        if (txtTituloRegra != null) 
            txtTituloRegra.text = regra.title;

        ManualSubruleData subrule = regra.subrules[0];
        if (txtDescricaoRegra != null) 
            txtDescricaoRegra.text = $"{regra.description}\n{subrule.description}";

        if (txtStatusVigor != null) 
            txtStatusVigor.text = $"<color=#00FF66>● EM VIGOR (Nível {regra.introductionDay})</color>";

        if (selectableSubrule == null)
        {
            Debug.LogError(
                "ManualWindowController: configure selectableSubrule with "
                + "InspectionRuleSelectable on the subrule field."
            );
            return;
        }

        selectableSubrule.Setup(subrule.id);
    }
}