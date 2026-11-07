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
    private Transform subruleContainer;
    private readonly List<GameObject> generatedSubrules = new();

    void Start()
    {
        ActionController.SyncSignal<bool>(
            InspectionModeController.ModeChangedAction,
            SetInspectionMode
        );
        CreateSubruleContainer();
        CarregarRegras();
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<bool>(
            InspectionModeController.ModeChangedAction,
            SetInspectionMode
        );
        ClearSubruleItems();
    }

    public void CarregarRegras()
    {
        try
        {
            manual = ManualRepository.Load(manualResourcePath);
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

        IReadOnlyList<ManualRuleData> regrasAtivas = ManualRepository.GetActiveRules(
            manual,
            nivelAtual
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

        if (txtStatusVigor != null) 
            txtStatusVigor.text = $"<color=#00FF66>● EM VIGOR (Nível {regra.introductionDay})</color>";

        if (selectableSubrule == null || subruleContainer == null)
        {
            Debug.LogError(
                "ManualWindowController: subrule template and container are required."
            );
            return;
        }

        ClearSubruleItems();
        for (int index = 0; index < regra.subrules.Count; index++)
        {
            ManualSubruleData subrule = regra.subrules[index];
            InspectionRuleSelectable item = index == 0
                ? selectableSubrule
                : Instantiate(selectableSubrule, subruleContainer);

            if (index > 0)
                generatedSubrules.Add(item.gameObject);

            item.Setup(subrule.id);
            TextMeshProUGUI text = item.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = index == 0
                    ? $"{regra.description}\n\n{subrule.description}"
                    : subrule.description;
            }

            ConfigureSubruleLayout(item);
        }
    }

    private void CreateSubruleContainer()
    {
        if (selectableSubrule == null || subruleContainer != null)
            return;

        RectTransform template = selectableSubrule.transform as RectTransform;
        if (template == null || template.parent == null)
            return;

        GameObject containerObject = new("SubruleList");
        RectTransform container = containerObject.AddComponent<RectTransform>();
        container.SetParent(template.parent, false);
        container.anchorMin = template.anchorMin;
        container.anchorMax = template.anchorMax;
        container.pivot = template.pivot;
        container.anchoredPosition = template.anchoredPosition;
        container.sizeDelta = template.sizeDelta;
        container.localScale = Vector3.one;

        VerticalLayoutGroup layout = containerObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        subruleContainer = container;
        template.SetParent(subruleContainer, false);
        template.anchorMin = new Vector2(0f, 1f);
        template.anchorMax = new Vector2(1f, 1f);
        template.pivot = new Vector2(0.5f, 1f);
        template.anchoredPosition = Vector2.zero;
        template.sizeDelta = new Vector2(0f, 0f);
    }

    private void ClearSubruleItems()
    {
        foreach (GameObject item in generatedSubrules)
        {
            if (item != null)
                Destroy(item);
        }

        generatedSubrules.Clear();
    }

    private static void ConfigureSubruleLayout(InspectionRuleSelectable item)
    {
        ContentSizeFitter fitter = item.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = item.gameObject.AddComponent<ContentSizeFitter>();

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private static string FieldLabel(string field)
    {
        if (string.IsNullOrWhiteSpace(field))
            return "Campo";

        return FieldLabels.TryGetValue(field, out string label) ? label : field;
    }
}