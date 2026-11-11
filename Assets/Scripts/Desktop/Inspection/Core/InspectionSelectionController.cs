using UnityEngine;

public sealed class InspectionSelectionController : MonoBehaviour
{
    public const string FieldSelectedAction = "InspectionFieldSelected";
    public const string RuleSelectedAction = "InspectionRuleSelected";
    public const string FindingRegisteredAction = "InspectionFindingRegistered";
    public const string SelectionClearedAction = "InspectionSelectionCleared";
    public const string FieldHighlightAction = "InspectionFieldHighlight";
    public const string RuleHighlightAction = "InspectionRuleHighlight";

    [SerializeField]
    private InspectionEvaluationController evaluation;
    private string selectedField;
    private string selectedSubruleId;

    private void Awake()
    {
        ActionController.SyncSignal<InspectionSelection>(FieldSelectedAction, SelectField);
        ActionController.SyncSignal<InspectionSelection>(RuleSelectedAction, SelectRule);
        ActionController.SyncSignal<EmailSelectionContext>(
            InspectionEvaluationController.EmailSelectedAction,
            ClearSelectionForEmail
        );
        ActionController.SyncSignal<bool>(
            InspectionModeController.ModeChangedAction,
            ClearSelectionWhenModeEnds
        );
        ActionController.SyncSignal(
            InspectionModeController.StateRequestedAction,
            PublishHighlights
        );
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<InspectionSelection>(FieldSelectedAction, SelectField);
        ActionController.UnsyncSignal<InspectionSelection>(RuleSelectedAction, SelectRule);
        ActionController.UnsyncSignal<EmailSelectionContext>(
            InspectionEvaluationController.EmailSelectedAction,
            ClearSelectionForEmail
        );
        ActionController.UnsyncSignal<bool>(
            InspectionModeController.ModeChangedAction,
            ClearSelectionWhenModeEnds
        );
        ActionController.UnsyncSignal(
            InspectionModeController.StateRequestedAction,
            PublishHighlights
        );
    }

    public void SelectField(InspectionSelection selection)
    {
        if (selection == null)
            return;

        selectedField = selection.FieldId;
        TryRegisterSelection();
        if (string.Equals(selectedField, selection.FieldId, System.StringComparison.Ordinal))
            ActionController.SendAction(FieldHighlightAction, selection.FieldId);
    }

    public void SelectRule(InspectionSelection selection)
    {
        if (selection == null)
            return;

        selectedSubruleId = selection.SelectionId;
        TryRegisterSelection();
        if (string.Equals(selectedSubruleId, selection.SelectionId, System.StringComparison.Ordinal))
            ActionController.SendAction(RuleHighlightAction, selection.SelectionId);
    }

    public void ClearSelection()
    {
        selectedField = null;
        selectedSubruleId = null;
        ActionController.SendAction(SelectionClearedAction);
        ActionController.SendAction<string>(FieldHighlightAction, null);
        ActionController.SendAction<string>(RuleHighlightAction, null);
    }

    private void ClearSelectionForEmail(EmailSelectionContext context)
    {
        ClearSelection();
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
            ActionController.SendAction(FieldHighlightAction, selectedField);
        if (!string.IsNullOrWhiteSpace(selectedSubruleId))
            ActionController.SendAction(RuleHighlightAction, selectedSubruleId);
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

        if (!string.Equals(result.FieldId, selectedField, System.StringComparison.Ordinal))
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

        ActionController.SendAction(FindingRegisteredAction, selectedSubruleId);
        ClearSelection();
        ActionController.SendAction(InspectionModeController.DeactivateAction);
    }

    private void RejectSelection(string message)
    {
        ClearSelection();
        Debug.LogWarning($"InspectionSelectionController: {message}");
    }
}
