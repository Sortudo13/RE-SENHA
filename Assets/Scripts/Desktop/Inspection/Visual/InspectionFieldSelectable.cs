using UnityEngine;
using UnityEngine.EventSystems;

public sealed class InspectionFieldSelectable : InspectionSelectable
{
    // App dono do campo (SourceType da superficie, ex.: "database"). Vazio = item em julgamento
    // (ex.: o email aberto). Preencha nos apps de referencia, para o campo nao se confundir
    // com um campo de mesmo nome em outro app.
    [SerializeField]
    private string source;

    [SerializeField]
    private string field;

    // "database/nome" quando ha fonte; so "Sender" quando nao ha.
    private string QualifiedField => InspectionFields.Qualify(source, field);

    protected override string SelectionId => QualifiedField;
    protected override string HighlightAction =>
        InspectionSelectionController.FieldHighlightAction;

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (InspectionModeActive && !string.IsNullOrWhiteSpace(field))
            ActionController.SendAction(
                InspectionSelectionController.FieldSelectedAction,
                new InspectionSelection(QualifiedField, QualifiedField)
            );
    }

    protected override bool MatchesHighlight(string selectionId)
    {
        return string.Equals(QualifiedField, selectionId);
    }

    protected override bool MatchesSelection(InspectionSelection selection)
    {
        return !string.IsNullOrWhiteSpace(selection.FieldId) && MatchesHighlight(selection.FieldId);
    }
}
