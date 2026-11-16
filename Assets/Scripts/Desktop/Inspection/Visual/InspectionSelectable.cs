using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public abstract class InspectionSelectable : ActionButtonController,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField]
    private Color selectedColor = new(1f, 0.85f, 0.2f, 1f);

    // Destaque mais fraco enquanto o mouse esta em cima, so com o modo inspetor ligado.
    [SerializeField]
    private Color hoverColor = new(1f, 0.93f, 0.6f, 1f);

    // Combinacao campo + subregra recusada (nenhuma regra quebrada ali): pisca nesta cor.
    [SerializeField]
    private Color rejectedColor = new(0.9f, 0.25f, 0.2f, 1f);

    // Combinacao valida (quebra de regra encontrada e registrada): pisca nesta cor.
    [SerializeField]
    private Color acceptedColor = new(0.3f, 0.8f, 0.35f, 1f);

    // Quantidade e duracao das piscadas, para recusa e para achado.
    [SerializeField, Min(1), FormerlySerializedAs("rejectedBlinks")]
    private int flashBlinks = 2;

    [SerializeField, Min(0.01f), FormerlySerializedAs("rejectedBlinkSeconds")]
    private float flashBlinkSeconds = 0.12f;

    private Graphic graphic;
    private Color originalColor;
    private bool isSelected;
    private bool isHovered;
    private bool isFlashing;
    private Color flashColor;
    private Coroutine flashRoutine;

    protected bool InspectionModeActive { get; private set; }
    protected abstract string SelectionId { get; }
    protected abstract string HighlightAction { get; }

    protected virtual void Awake()
    {
        // Sem "??": objeto da Unity ausente pode ser um "null falso" que o ?? nao reconhece.
        if (!TryGetComponent(out graphic))
            graphic = GetComponentInChildren<Graphic>();
        if (graphic != null)
            originalColor = graphic.color;

        ActionController.SyncSignal<string>(HighlightAction, OnHighlight);
        ActionController.SyncSignal<bool>(
            InspectionModeController.ModeChangedAction,
            OnInspectionModeChanged
        );
        ActionController.SyncSignal(
            InspectionSelectionController.SelectionClearedAction,
            ClearHighlight
        );
        ActionController.SyncSignal<InspectionSelection>(
            InspectionSelectionController.SelectionRejectedAction,
            OnSelectionRejected
        );
        ActionController.SyncSignal<InspectionSelection>(
            InspectionSelectionController.SelectionAcceptedAction,
            OnSelectionAccepted
        );
    }

    // Start roda depois do Setup feito logo apos o Instantiate, entao o seletor ja tem seu ID
    // quando o estado atual (modo e highlights) chega.
    protected virtual void Start()
    {
        ActionController.SendAction(InspectionModeController.StateRequestedAction);
    }

    protected virtual void OnDestroy()
    {
        ActionController.UnsyncSignal<string>(HighlightAction, OnHighlight);
        ActionController.UnsyncSignal<bool>(
            InspectionModeController.ModeChangedAction,
            OnInspectionModeChanged
        );
        ActionController.UnsyncSignal(
            InspectionSelectionController.SelectionClearedAction,
            ClearHighlight
        );
        ActionController.UnsyncSignal<InspectionSelection>(
            InspectionSelectionController.SelectionRejectedAction,
            OnSelectionRejected
        );
        ActionController.UnsyncSignal<InspectionSelection>(
            InspectionSelectionController.SelectionAcceptedAction,
            OnSelectionAccepted
        );
    }

    // Objeto desativado (ex.: pagina do Manual trocada) nao recebe OnPointerExit,
    // e a Unity para as corrotinas dele.
    protected virtual void OnDisable()
    {
        isHovered = false;
        isFlashing = false;
        flashRoutine = null;
        ApplyColor();
    }

    public abstract void OnPointerClick(PointerEventData eventData);

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        ApplyColor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        ApplyColor();
    }

    protected abstract bool MatchesHighlight(string selectionId);

    // Diz se este seletor faz parte da combinacao campo + subregra informada.
    protected abstract bool MatchesSelection(InspectionSelection selection);

    protected void SetSelected(bool selected)
    {
        isSelected = selected;
        ApplyColor();
    }

    private void OnSelectionRejected(InspectionSelection selection)
    {
        Flash(selection, rejectedColor);
    }

    private void OnSelectionAccepted(InspectionSelection selection)
    {
        Flash(selection, acceptedColor);
    }

    private void Flash(InspectionSelection selection, Color color)
    {
        if (selection == null || !isActiveAndEnabled || !MatchesSelection(selection))
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Blink(color));
    }

    private IEnumerator Blink(Color color)
    {
        flashColor = color;
        WaitForSeconds wait = new(flashBlinkSeconds);
        for (int blink = 0; blink < flashBlinks; blink++)
        {
            isFlashing = true;
            ApplyColor();
            yield return wait;

            isFlashing = false;
            ApplyColor();
            yield return wait;
        }

        flashRoutine = null;
    }

    // Prioridade: piscando (recusa/achado) > selecionado > hover (so com o modo ligado) > cor original.
    private void ApplyColor()
    {
        if (graphic == null)
            return;

        if (isFlashing)
            graphic.color = flashColor;
        else if (isSelected)
            graphic.color = selectedColor;
        else if (isHovered && InspectionModeActive)
            graphic.color = hoverColor;
        else
            graphic.color = originalColor;
    }

    private void OnHighlight(string selectionId)
    {
        SetSelected(
            !string.IsNullOrWhiteSpace(selectionId)
            && MatchesHighlight(selectionId)
        );
    }

    private void OnInspectionModeChanged(bool isActive)
    {
        InspectionModeActive = isActive;
        if (!isActive)
            isSelected = false;

        // Liga/desliga o hover tambem quando o mouse ja estava em cima ao trocar o modo.
        ApplyColor();
    }

    private void ClearHighlight()
    {
        SetSelected(false);
    }
}
