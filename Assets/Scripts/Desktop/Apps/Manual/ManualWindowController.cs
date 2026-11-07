using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManualWindowController : MonoBehaviour
{
    private static readonly Dictionary<string, string> FieldLabels = new()
    {
        ["Sender"] = "Remetente",
        ["Subject"] = "Assunto",
        ["Body"] = "Corpo",
        ["Attachments"] = "Anexos",
    };

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

    private bool inspectionModeActive;

    void Start()
    {
        ActionController.SyncSignal<bool>(
            InspectionModeController.ModeChangedAction,
            SetInspectionMode
        );
        CarregarRegras();
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<bool>(
            InspectionModeController.ModeChangedAction,
            SetInspectionMode
        );
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
                txtItem.text = $"{FieldLabel(regra.field)}: {regra.title}";

            ManualRuleData regraAtual = regra;
            Button button = novoItem.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => ExibirDetalhes(regraAtual));
                button.interactable = !inspectionModeActive;
            }
        }

        // Exibe a primeira regra por padrão
        if (regrasAtivas.Count > 0)
        {
            ExibirDetalhes(regrasAtivas[0]);
        }
    }

    private void SetInspectionMode(bool active)
    {
        inspectionModeActive = active;
        if (containerRegras == null)
            return;

        foreach (Button button in containerRegras.GetComponentsInChildren<Button>(true))
            button.interactable = !active;
    }

    private void ExibirDetalhes(ManualRuleData regra)
    {
        if (txtTituloRegra != null) 
            txtTituloRegra.text = regra.title;

        ManualSubruleData subrule = regra.subrules[0];
        if (txtDescricaoRegra != null)
        {
            string subrulesText = string.Join(
                "\n",
                regra.subrules.ConvertAll(item => item.description)
            );
            txtDescricaoRegra.text =
                $"{regra.description}\n\nCampo do e-mail: {FieldLabel(regra.field)}\n\n{subrulesText}";
        }

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

    private static string FieldLabel(string field)
    {
        if (string.IsNullOrWhiteSpace(field))
            return "Campo";

        return FieldLabels.TryGetValue(field, out string label) ? label : field;
    }
}