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
            manual = ManualRepository.Load();
        }
        catch (InvalidOperationException exception)
        {
            Debug.LogError($"ManualWindowController: {exception.Message}");
            return;
        }

        ShowApp(appTabs.Count > 0 ? appTabs[0].app : null);
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
        IReadOnlyList<ManualRuleData> rules = ManualRepository.GetActiveRules(
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
            txtRuleField.text = $"[{FieldLabel(rule.field)}]";
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
        if (string.IsNullOrWhiteSpace(field))
            return "Campo";

        return FieldLabels.TryGetValue(field, out string label) ? label : field;
    }
}
