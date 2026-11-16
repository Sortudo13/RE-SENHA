using System.Text.RegularExpressions;
using TMPro;
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

    // So usados pelo InspectableItem, que le o valor do campo direto da tela.
    // Texto que mostra o valor; vazio = o primeiro texto dentro deste objeto.
    [SerializeField]
    private TMP_Text valueText;

    // Marcado: "Solicitante: Joao" vira "Joao" (tira tudo ate o primeiro ":").
    // Desmarque se o valor puder ter ":" sem rotulo (ex.: um horario "08:30").
    [SerializeField]
    private bool ignoreLabel = true;

    private static readonly Regex RichTextTag = new("<[^>]*>", RegexOptions.Compiled);

    public string Field => field;
    public string Source => source;

    // "database/nome" quando ha fonte; so "Sender" quando nao ha.
    private string QualifiedField => InspectionFields.Qualify(source, field);

    // Valor mostrado na tela agora. Sem rotulo, o texto vai com as tags de rich text
    // (um <link="..."> escondido continua visivel para as regras).
    public bool TryReadValue(out string value)
    {
        value = null;
        TMP_Text text = valueText != null ? valueText : GetComponentInChildren<TMP_Text>(true);
        if (text == null)
            return false;

        value = text.text ?? string.Empty;
        if (ignoreLabel)
        {
            string plain = RichTextTag.Replace(value, string.Empty);
            int separator = plain.IndexOf(':');
            if (separator >= 0)
                value = plain[(separator + 1)..];
        }

        value = value.Trim();
        return true;
    }

    protected override string SelectionId => QualifiedField;
    protected override string HighlightAction =>
        InspectionSignals.FieldHighlight;

    protected override void Awake()
    {
        base.Awake();
        Signals.On<string[]>(InspectionSignals.FoundFieldsChanged, OnFoundFieldsChanged);
    }

    // Os achados sao do item em julgamento, cujos campos nao tem fonte ("Sender"); um campo de
    // app de referencia ("database/nome") nunca casa com eles.
    private void OnFoundFieldsChanged(string[] foundFields)
    {
        bool found = false;
        if (foundFields != null && !string.IsNullOrWhiteSpace(field))
            foreach (string foundField in foundFields)
                if (InspectionFields.Matches(foundField, QualifiedField))
                {
                    found = true;
                    break;
                }

        SetFound(found);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (InspectionModeActive && !string.IsNullOrWhiteSpace(field))
            ActionController.SendAction(
                InspectionSignals.FieldSelected,
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
