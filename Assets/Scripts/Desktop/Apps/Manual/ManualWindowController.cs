using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManualWindowController : MonoBehaviour
{
    [Serializable]
    private sealed class AppTab
    {
        public string app;
        public string label;
        public Button button;
        public Image background;
    }

    private static readonly Dictionary<string, string> FieldLabels = new()
    {
        ["Sender"] = "Remetente",
        ["Subject"] = "Assunto",
        ["Body"] = "Corpo",
        ["Attachments"] = "Anexos",
    };

    [Header("Barra de aplicativos")]
    [SerializeField] private GameObject sidebar;
    [SerializeField] private List<AppTab> appTabs = new();
    [SerializeField] private Color tabNormalColor = new(0.78f, 0.78f, 0.78f, 1f);
    [SerializeField] private Color tabSelectedColor = new(0.63f, 0.73f, 0.88f, 1f);

    [Header("Pagina de regras")]
    [SerializeField] private GameObject rulesPage;
    [SerializeField] private TextMeshProUGUI txtRulesTitle;
    [SerializeField] private ScrollRect rulesScroll;
    [SerializeField] private Transform rulesContainer;
    [SerializeField] private GameObject emptyRulesMessage;
    [SerializeField] private ManualRuleCardController ruleCardPrefab;

    [Header("Pagina de subregras")]
    [SerializeField] private GameObject subrulesPage;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI txtRuleTitle;
    [SerializeField] private TextMeshProUGUI txtRuleField;
    [SerializeField] private TextMeshProUGUI txtRuleDescription;
    [SerializeField] private ScrollRect subrulesScroll;
    [SerializeField] private Transform subrulesContainer;
    [SerializeField] private ManualSubruleItemController subruleItemPrefab;

    private ManualRuleBookData manual;
    private string selectedApp;

    // Destino pedido por OpenAt (anexo de email). OpenAt roda logo apos o Instantiate,
    // antes do Start carregar as regras, entao o destino fica guardado ate o Start.
    private string pendingApp;
    private string pendingRuleId;

    private void Awake()
    {
        foreach (AppTab tab in appTabs)
        {
            if (tab?.button == null)
                continue;

            string app = tab.app;
            tab.button.onClick.AddListener(() => ShowApp(app));
        }

        if (backButton != null)
            backButton.onClick.AddListener(ShowRulesPage);

        ActionController.SyncSignal("DayChanged", RefreshRules);
    }

    private void OnDestroy()
    {
        foreach (AppTab tab in appTabs)
        {
            if (tab?.button != null)
                tab.button.onClick.RemoveAllListeners();
        }

        if (backButton != null)
            backButton.onClick.RemoveListener(ShowRulesPage);

        ActionController.UnsyncSignal("DayChanged", RefreshRules);
    }

    private void Start()
    {
        try
        {
            manual = ManualRules.Load();
        }
        catch (InvalidOperationException exception)
        {
            Debug.LogError($"ManualWindowController: {exception.Message}");
            return;
        }

        string defaultApp = appTabs.Count > 0 ? appTabs[0].app : null;
        ShowApp(string.IsNullOrWhiteSpace(pendingApp) ? defaultApp : pendingApp);

        if (!string.IsNullOrWhiteSpace(pendingRuleId))
            OpenRule(pendingRuleId);

        pendingApp = null;
        pendingRuleId = null;

        CompanionSignals.Show("manual_first_open");
    }

    // Abre o Manual numa aba e, opcionalmente, direto numa regra. Chamado pelo
    // WindowManagerController ao abrir um anexo de Manual (ManualData).
    public void OpenAt(string app, string ruleId)
    {
        if (manual == null)
        {
            pendingApp = app;
            pendingRuleId = ruleId;
            return;
        }

        if (!string.IsNullOrWhiteSpace(app))
            ShowApp(app);
        if (!string.IsNullOrWhiteSpace(ruleId))
            OpenRule(ruleId);
    }

    // So abre regras ja liberadas no dia atual; regra de dia futuro ou id errado fica na lista.
    private void OpenRule(string ruleId)
    {
        foreach (ManualRuleData rule in manual.rules)
        {
            if (rule.id != ruleId)
                continue;

            if (rule.introductionDay > DataController.CurrentDay)
            {
                Debug.LogWarning(
                    $"ManualWindowController: regra '{ruleId}' so libera no dia {rule.introductionDay}."
                );
                return;
            }

            if (!string.Equals(rule.app, selectedApp, StringComparison.OrdinalIgnoreCase))
                ShowApp(rule.app);
            ShowRule(rule);
            return;
        }

        Debug.LogWarning($"ManualWindowController: regra '{ruleId}' nao existe no ManualRules.json.");
    }

    public void ShowApp(string app)
    {
        selectedApp = app;
        foreach (AppTab tab in appTabs)
        {
            if (tab?.background != null)
                tab.background.color = tab.app == app ? tabSelectedColor : tabNormalColor;
        }

        if (txtRulesTitle != null)
            txtRulesTitle.text = $"Regras - {AppLabel(app)}";

        ShowRulesPage();
        RefreshRules();
    }

    private void RefreshRules()
    {
        if (manual == null || rulesContainer == null || ruleCardPrefab == null)
            return;

        ClearChildren(rulesContainer);
        IReadOnlyList<ManualRuleData> rules = ManualRules.GetActiveRules(
            manual,
            DataController.CurrentDay,
            selectedApp
        );

        foreach (ManualRuleData rule in rules)
        {
            ManualRuleCardController card = Instantiate(ruleCardPrefab, rulesContainer);
            card.Setup(rule, FieldLabel(rule.field), ShowRule);
        }

        if (emptyRulesMessage != null)
            emptyRulesMessage.SetActive(rules.Count == 0);

        ResetScroll(rulesScroll);
    }

    private void ShowRule(ManualRuleData rule)
    {
        if (rule == null || subrulesContainer == null || subruleItemPrefab == null)
            return;

        if (txtRuleTitle != null)
            txtRuleTitle.text = rule.title;
        if (txtRuleField != null)
        {
            string label = FieldLabel(rule.field);
            txtRuleField.text = label != null ? $"[{label}]" : string.Empty;
            txtRuleField.gameObject.SetActive(label != null);
        }
        if (txtRuleDescription != null)
            txtRuleDescription.text = rule.description;

        ClearChildren(subrulesContainer);
        foreach (ManualSubruleData subrule in rule.subrules)
        {
            ManualSubruleItemController item = Instantiate(subruleItemPrefab, subrulesContainer);
            item.Setup(subrule);
        }

        SetPage(showSubrules: true);
        ResetScroll(subrulesScroll);
    }

    private void ShowRulesPage()
    {
        SetPage(showSubrules: false);
    }

    private void SetPage(bool showSubrules)
    {
        if (sidebar != null)
            sidebar.SetActive(!showSubrules);
        if (rulesPage != null)
            rulesPage.SetActive(!showSubrules);
        if (subrulesPage != null)
            subrulesPage.SetActive(showSubrules);
    }

    private static void ClearChildren(Transform container)
    {
        for (int index = container.childCount - 1; index >= 0; index--)
        {
            GameObject child = container.GetChild(index).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    private static void ResetScroll(ScrollRect scroll)
    {
        if (scroll == null || !scroll.gameObject.activeInHierarchy)
            return;

        if (scroll.content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
        scroll.verticalNormalizedPosition = 1f;
    }

    private string AppLabel(string app)
    {
        foreach (AppTab tab in appTabs)
        {
            if (tab != null && tab.app == app && !string.IsNullOrWhiteSpace(tab.label))
                return tab.label;
        }

        return app ?? string.Empty;
    }

    private static string FieldLabel(string field)
    {
        // Sem campo (regra de chat/ticket): sem rotulo.
        if (string.IsNullOrWhiteSpace(field))
            return null;

        return FieldLabels.TryGetValue(field, out string label) ? label : field;
    }
}
