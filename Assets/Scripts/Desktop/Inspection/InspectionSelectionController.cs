using UnityEngine;

public sealed class InspectionSelectionController : MonoBehaviour
{
    public const string FieldSelectedAction = "InspectionFieldSelected";
    public const string RuleSelectedAction = "InspectionRuleSelected";
    public const string FindingRegisteredAction = "InspectionFindingRegistered";
    public const string SelectionRejectedAction = "InspectionSelectionRejected";
    public const string SelectionClearedAction = "InspectionSelectionCleared";
    public const string FieldHighlightAction = "InspectionFieldHighlight";
    public const string RuleHighlightAction = "InspectionRuleHighlight";

    [SerializeField]
    private InspectionEvaluationController evaluation;
    private string selectedField;
    private string selectedSubruleId;

    private void Awake()
    {
        ActionController.SyncSignal<string>(FieldSelectedAction, SelectField);
        ActionController.SyncSignal<InspectionRuleSelection>(
            RuleSelectedAction,
            SelectRule
        );
        ActionController.SyncSignal<InspectionEmailContext>(
            InspectionEvaluationController.EmailSelectedAction,
            ClearSelectionForEmail
        );
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<string>(FieldSelectedAction, SelectField);
        ActionController.UnsyncSignal<InspectionRuleSelection>(
            RuleSelectedAction,
            SelectRule
        );
        ActionController.UnsyncSignal<InspectionEmailContext>(
            InspectionEvaluationController.EmailSelectedAction,
            ClearSelectionForEmail
        );
    }

    public void SelectField(string field)
    {
        selectedField = field;
        TryRegisterSelection();
        if (string.Equals(selectedField, field, System.StringComparison.Ordinal))
            ActionController.SendAction(FieldHighlightAction, field);
    }

    public void SelectRule(InspectionRuleSelection selection)
    {
        selectedSubruleId = selection?.subruleId;
        TryRegisterSelection();
        if (selection != null
            && string.Equals(selectedSubruleId, selection.subruleId, System.StringComparison.Ordinal))
            ActionController.SendAction(RuleHighlightAction, selection);
    }

    public void ClearSelection()
    {
        selectedField = null;
        selectedSubruleId = null;
        ActionController.SendAction(SelectionClearedAction);
        ActionController.SendAction<string>(FieldHighlightAction, null);
        ActionController.SendAction<InspectionRuleSelection>(RuleHighlightAction, null);
    }

    private void ClearSelectionForEmail(InspectionEmailContext context)
    {
        ClearSelection();
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
                out ManualRuleEvaluationResult result
            ))
        {
            RejectSelection("The selected rule does not apply to the current email.");
            return;
        }

        if (!string.Equals(result.Field, selectedField, System.StringComparison.Ordinal))
        {
            RejectSelection("The selected field does not match the selected rule.");
            return;
        }

        if (!evaluation.HasCurrentSave)
        {
            RejectSelection("The current email save context is unavailable.");
            return;
        }

        if (evaluation.HasRegisteredFinding(selectedSubruleId))
        {
            RejectSelection(
                $"The finding '{selectedSubruleId}' was already registered for this email."
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
        ActionController.SendAction(SelectionRejectedAction, message);
    }
}
