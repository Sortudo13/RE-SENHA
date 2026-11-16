using UnityEngine;

public sealed class InspectionSelectionController : MonoBehaviour
{

    [SerializeField]
    private InspectionEvaluationController evaluation;
    private readonly SignalSubscriptions signals = new();
    private string selectedField;
    private string selectedSubruleId;

    private void Awake()
    {
        signals
            .On<InspectionSelection>(InspectionSignals.FieldSelected, SelectField)
            .On<InspectionSelection>(InspectionSignals.RuleSelected, SelectRule)
            .On<IInspectionSurface>(InspectionSignals.SurfaceChanged, ClearSelectionForSurface)
            .On<string>(InspectionSignals.SurfaceClosed, ClearSelectionForClosedSurface)
            .On<bool>(InspectionSignals.ModeChanged, ClearSelectionWhenModeEnds)
            .On(InspectionSignals.StateRequested, PublishHighlights);
    }

    private void OnDestroy()
    {
        signals.Clear();
    }

    public void SelectField(InspectionSelection selection)
    {
        if (selection == null)
            return;

        // Clicar de novo no campo ja selecionado desmarca so ele.
        if (string.Equals(selectedField, selection.FieldId, System.StringComparison.Ordinal))
        {
            selectedField = null;
            ActionController.SendAction<string>(InspectionSignals.FieldHighlight, null);
            return;
        }

        selectedField = selection.FieldId;
        WarnIfFieldIsUnknown(selectedField);
        TryRegisterSelection();
        if (string.Equals(selectedField, selection.FieldId, System.StringComparison.Ordinal))
            ActionController.SendAction(InspectionSignals.FieldHighlight, selection.FieldId);
    }

    public void SelectRule(InspectionSelection selection)
    {
        if (selection == null)
            return;

        // Clicar de novo na subregra ja selecionada desmarca so ela.
        if (string.Equals(selectedSubruleId, selection.SelectionId, System.StringComparison.Ordinal))
        {
            selectedSubruleId = null;
            ActionController.SendAction<string>(InspectionSignals.RuleHighlight, null);
            return;
        }

        selectedSubruleId = selection.SelectionId;
        TryRegisterSelection();
        if (string.Equals(selectedSubruleId, selection.SelectionId, System.StringComparison.Ordinal))
            ActionController.SendAction(InspectionSignals.RuleHighlight, selection.SelectionId);
    }

    public void ClearSelection()
    {
        selectedField = null;
        selectedSubruleId = null;
        ActionController.SendAction(InspectionSignals.SelectionCleared);
        ActionController.SendAction<string>(InspectionSignals.FieldHighlight, null);
        ActionController.SendAction<string>(InspectionSignals.RuleHighlight, null);
    }

    // Trocar de item (outro email, outro ticket...) descarta a combinacao parcial do item anterior.
    // Um app de referencia (sem achados, ex.: Database) trocar de item so limpa a selecao
    // se o campo selecionado for dele; assim da para escolher um campo do item em julgamento
    // e depois procurar o colaborador no Database.
    private void ClearSelectionForSurface(IInspectionSurface surface)
    {
        if (surface == null
            || surface.FindingStore != null
            || SelectedFieldBelongsTo(surface.SourceType))
            ClearSelection();
    }

    private void ClearSelectionForClosedSurface(string sourceType)
    {
        if (selectedField != null || selectedSubruleId != null)
            ClearSelection();
    }

    private bool SelectedFieldBelongsTo(string sourceType)
    {
        InspectionFields.Split(selectedField, out string source, out _);
        return InspectionFields.Matches(source, sourceType);
    }

    // Erro de configuracao comum: o "field" (ou o "source") do seletor no prefab nao existe
    // na superficie do app.
    private void WarnIfFieldIsUnknown(string qualifiedField)
    {
        if (evaluation == null)
            return;

        InspectionFields.Split(qualifiedField, out string source, out string field);
        IInspectionSurface surface = evaluation.CurrentSurface;
        if (source != null && !evaluation.TryGetSurface(source, out surface))
        {
            Debug.LogWarning(
                $"InspectionSelectionController: no open surface for source '{source}'. "
                    + "Check the 'source' of the InspectionFieldSelectable and PublishSurface in the app."
            );
            return;
        }

        if (surface != null && !InspectionFields.Contains(surface.Capabilities, field))
            Debug.LogWarning(
                $"InspectionSelectionController: field '{field}' is not a capability of the "
                    + $"'{surface.SourceType}' surface. Check the 'field' of the InspectionFieldSelectable."
            );
    }

    // A selecao so existe enquanto o modo esta ativo; assim, republicar os highlights
    // nunca destaca um seletor com o modo desligado.
    private void ClearSelectionWhenModeEnds(bool isActive)
    {
        if (!isActive && (selectedField != null || selectedSubruleId != null))
            ClearSelection();
    }

    // Seletores criados depois da selecao (ex.: subregras ao reabrir uma regra no Manual)
    // recebem o destaque atual.
    private void PublishHighlights()
    {
        if (!string.IsNullOrWhiteSpace(selectedField))
            ActionController.SendAction(InspectionSignals.FieldHighlight, selectedField);
        if (!string.IsNullOrWhiteSpace(selectedSubruleId))
            ActionController.SendAction(InspectionSignals.RuleHighlight, selectedSubruleId);
    }

    private void TryRegisterSelection()
    {
        if (string.IsNullOrWhiteSpace(selectedField)
            || string.IsNullOrWhiteSpace(selectedSubruleId))
            return;

        if (evaluation == null)
        {
            RejectSelection("Inspection evaluation controller was not found.");
            return;
        }

        if (!evaluation.TryGetFinding(
                selectedSubruleId,
                out InspectionEvaluationResult result
            ))
        {
            RejectSelection("The selected rule does not apply to the current surface.");
            return;
        }

        if (!InspectionFields.Matches(result.FieldId, selectedField))
        {
            RejectSelection("The selected field does not match the selected rule.");
            return;
        }

        if (!evaluation.HasFindingStore)
        {
            RejectSelection("The current inspection finding store is unavailable.");
            return;
        }

        if (evaluation.HasRegisteredFinding(selectedSubruleId))
        {
            RejectSelection(
                $"The finding '{selectedSubruleId}' was already registered for this surface."
            );
            return;
        }

        if (!evaluation.RegisterFinding(selectedSubruleId))
        {
            RejectSelection("The selected finding could not be saved.");
            return;
        }

        // Os seletores da combinacao piscam em verde antes de a selecao ser limpa.
        ActionController.SendAction(
            InspectionSignals.SelectionAccepted,
            new InspectionSelection(selectedField, selectedSubruleId)
        );
        ActionController.SendAction(InspectionSignals.FindingRegistered, selectedSubruleId);
        ClearSelection();
        ActionController.SendAction(InspectionSignals.DeactivateMode);
    }

    private void RejectSelection(string message)
    {
        // Avisa os seletores escolhidos antes de limpar, para eles piscarem em vermelho.
        ActionController.SendAction(
            InspectionSignals.SelectionRejected,
            new InspectionSelection(selectedField, selectedSubruleId)
        );
        ClearSelection();
        Debug.LogWarning($"InspectionSelectionController: {message}");
    }
}
