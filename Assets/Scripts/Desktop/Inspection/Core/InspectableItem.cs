using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Jeito facil de ligar um app ao Inspector: vai na raiz da janela do app. Le os valores dos
// campos direto dos InspectionFieldSelectable filhos, entao o nome do campo so precisa bater
// entre o prefab e o "field" da regra no ManualRules.json. Nao precisa de classe de superficie.
//
//   inspectable.Open(item.id, item.evidences);         // ao abrir o item, depois de preencher a tela
//   inspectable.Close();                               // ao fechar o item
//   inspectable.Judge(rejeitou, () => Remover(item));  // no botao de decisao
//
// Para valores que nao estao em um texto da tela (ex.: anexos), escreva uma superficie em
// codigo, como a do Email (Apps/Email).
[DisallowMultipleComponent]
public sealed class InspectableItem : MonoBehaviour
{
    [Tooltip("SourceType do app, igual ao \"app\" das regras no ManualRules.json (email, ticket, chat).")]
    [SerializeField]
    private string app;

    [Tooltip("App de consulta (ex.: Database): nao e julgado nem guarda achados.")]
    [SerializeField]
    private bool referenceOnly;

    [Tooltip("Textos do feedback do Judge. Os padroes falam em \"o item\".")]
    [SerializeField]
    private InspectionFeedbackTexts feedbackTexts = new();

    private InspectionEvaluationController evaluation;
    private InspectableItemSurface openSurface;
    private bool validated;

    // Item aberto quando a janela foi desativada (minimizar usa SetActive(false)); volta no OnEnable.
    private bool reopenOnEnable;
    private string openItemId;
    private List<EvidenceData> openEvidences;

    public string App => app?.Trim();
    public bool IsOpen => openSurface != null;

    // Publica o item aberto. Chame DEPOIS de preencher os textos da tela; se a tela mudar
    // depois (ex.: mensagem nova no chat), chame de novo.
    // evidences: a lista de achados do dado do item (vai no save). Ignorada se referenceOnly.
    public void Open(string itemId, List<EvidenceData> evidences)
    {
        if (string.IsNullOrWhiteSpace(App))
        {
            Debug.LogError($"InspectableItem em '{name}': preencha o campo App.", this);
            return;
        }

        if (!referenceOnly && evidences == null)
        {
            Debug.LogError(
                $"InspectableItem '{App}': Open precisa da lista de achados do item "
                    + "(crie antes com \"item.evidences ??= new();\").",
                this
            );
            return;
        }

        IInspectionFindingStore store = referenceOnly
            ? null
            : new ListFindingStore(App, itemId, evidences);
        openItemId = itemId;
        openEvidences = evidences;
        openSurface = new InspectableItemSurface(App, ReadFields(), store);

        if (!validated)
        {
            validated = true;
            Validate();
        }

        InspectionEvaluationController.PublishSurface(openSurface);
    }

    public void Close()
    {
        if (openSurface == null)
            return;

        openSurface = null;
        InspectionEvaluationController.CloseSurface(App);
    }

    // Decisao do jogador sobre o item aberto (rejected = bloqueou/rejeitou). Confere, conta o
    // erro, mostra o feedback, chama removeItem e ve o fim do dia (InspectionJudge).
    public bool Judge(bool rejected, Action removeItem)
    {
        if (referenceOnly)
        {
            Debug.LogError($"InspectableItem '{App}': app de consulta nao e julgado.", this);
            return false;
        }

        InspectionEvaluationController controller = FindEvaluation();
        if (openSurface == null || controller == null || controller.CurrentSurface != openSurface)
        {
            Debug.LogWarning(
                $"InspectableItem '{App}': Judge sem item aberto (chame Open antes).",
                this
            );
            return false;
        }

        return InspectionJudge.Judge(controller, rejected, feedbackTexts, removeItem);
    }

    // Fechar ou minimizar a janela tira o item do Inspector; ao reativar, ele volta.
    private void OnDisable()
    {
        reopenOnEnable = openSurface != null;
        Close();
    }

    private void OnEnable()
    {
        if (!reopenOnEnable)
            return;

        reopenOnEnable = false;
        Open(openItemId, openEvidences);
    }

    // Campos do item: os seletores ativos sem fonte (ou com fonte = este app). Campo repetido
    // (ex.: varios baloes "message" no chat) junta os valores, um por linha.
    private Dictionary<string, string> ReadFields()
    {
        Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);
        foreach (InspectionFieldSelectable selectable in GetSelectables())
        {
            if (!selectable.TryReadValue(out string value))
                continue;

            string field = selectable.Field.Trim();
            values[field] = values.TryGetValue(field, out string previous)
                ? previous + "\n" + value
                : value;
        }

        return values;
    }

    private IEnumerable<InspectionFieldSelectable> GetSelectables()
    {
        return GetComponentsInChildren<InspectionFieldSelectable>(false)
            .Where(
                selectable =>
                    !string.IsNullOrWhiteSpace(selectable.Field)
                    && (
                        string.IsNullOrWhiteSpace(selectable.Source)
                        || InspectionFields.Matches(selectable.Source, App)
                    )
            );
    }

    private InspectionEvaluationController FindEvaluation()
    {
        if (evaluation == null)
            evaluation = FindAnyObjectByType<InspectionEvaluationController>();
        if (evaluation == null)
            Debug.LogError("InspectableItem: nenhum InspectionEvaluationController na cena (Inspector.prefab).", this);
        return evaluation;
    }

    // Avisos no Console para os erros que antes passavam em silencio. Roda no primeiro Open.
    private void Validate()
    {
        if (!ManualRules.IsSupportedApp(App))
            Debug.LogWarning(
                $"InspectableItem em '{name}': app '{App}' nao esta em ManualRules.SupportedApps; "
                    + "nenhuma regra vai valer para ele.",
                this
            );

        foreach (InspectionFieldSelectable selectable in GetSelectables())
        {
            if (!selectable.TryReadValue(out _))
                Debug.LogWarning(
                    $"InspectableItem '{App}': o campo '{selectable.Field}' ({selectable.name}) "
                        + "nao tem texto (preencha Value Text ou ponha um texto dentro dele).",
                    selectable
                );
        }

        if (referenceOnly)
            return;

        ManualRuleBookData manual;
        try
        {
            manual = ManualRules.Load();
        }
        catch (InvalidOperationException)
        {
            return; // o InspectionEvaluationController ja avisa
        }

        IReadOnlyCollection<string> capabilities = openSurface.Capabilities;
        foreach (ManualRuleData rule in manual.rules.Where(rule => InspectionFields.Matches(rule.app, App)))
        {
            foreach (ManualSubruleData subrule in rule.subrules)
            {
                string required = ManualRuleEvaluator.GetRequiredField(rule, subrule);
                if (required != null && !InspectionFields.Contains(capabilities, required))
                    Debug.LogWarning(
                        $"InspectableItem '{App}': a subregra '{subrule.id}' le o campo '{required}', "
                            + $"mas '{name}' nao tem um InspectionFieldSelectable ativo com esse Field. "
                            + "Essa regra nunca vai acusar.",
                        this
                    );
            }
        }
    }

    [ContextMenu("Log inspection values")]
    private void LogInspectionValues()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("InspectableItem: use no Play, com um item aberto.", this);
            return;
        }

        Dictionary<string, string> current = ReadFields();
        string fields = current.Count == 0
            ? "  (nenhum campo)"
            : string.Join("\n", current.Select(pair => $"  {pair.Key} = \"{pair.Value}\""));

        InspectionEvaluationController controller = FindEvaluation();
        bool judged = controller != null && openSurface != null && controller.CurrentSurface == openSurface;
        string results = judged
            ? controller.CurrentResults.Count == 0
                ? "  (nenhuma regra quebrada)"
                : string.Join(
                    "\n",
                    controller.CurrentResults.Select(result => $"  {result.FindingId} no campo {result.FieldId}")
                )
            : "  (item nao esta em julgamento)";

        Debug.Log(
            $"InspectableItem '{App}' ({(IsOpen ? "aberto" : "fechado")})\nCampos lidos da tela agora:\n{fields}\n"
                + $"Regras quebradas (do ultimo Open):\n{results}",
            this
        );
    }

    // Superficie montada com os valores lidos da tela no Open.
    private sealed class InspectableItemSurface : IInspectionSurface, IInspectionFieldSource
    {
        private readonly Dictionary<string, string> values;

        public string SourceType { get; }
        public IReadOnlyCollection<string> Capabilities { get; }
        public IInspectionFieldSource FieldSource => this;
        public IInspectionFindingStore FindingStore { get; }

        public InspectableItemSurface(
            string sourceType,
            Dictionary<string, string> values,
            IInspectionFindingStore findingStore
        )
        {
            SourceType = sourceType;
            this.values = values;
            Capabilities = values.Keys.ToArray();
            FindingStore = findingStore;
        }

        public bool TryGetText(string fieldId, out string value)
        {
            value = null;
            return !string.IsNullOrWhiteSpace(fieldId) && values.TryGetValue(fieldId.Trim(), out value);
        }

        public IReadOnlyList<InspectionAttachmentDescriptor> GetAttachments() =>
            Array.Empty<InspectionAttachmentDescriptor>();
    }
}
