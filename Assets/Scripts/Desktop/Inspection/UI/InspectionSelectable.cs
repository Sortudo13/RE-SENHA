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

    // Borda do selecionado: diferencia selecionado de hover pela forma, nao so pelo tom de
    // amarelo (os dois sao parecidos demais, inclusive para quem tem daltonismo).
    [SerializeField]
    private Color selectedOutlineColor = new(0.45f, 0.3f, 0f, 1f);

    [SerializeField, Min(0f)]
    private float selectedOutlineWidth = 3f;

    // Destaque mais fraco enquanto o mouse esta em cima, so com o modo inspetor ligado.
    [SerializeField]
    private Color hoverColor = new(1f, 0.93f, 0.6f, 1f);

    // Combinacao campo + subregra recusada (nenhuma regra quebrada ali): pisca nesta cor.
    [SerializeField]
    private Color rejectedColor = new(0.9f, 0.25f, 0.2f, 1f);

    // Combinacao valida (quebra de regra encontrada e registrada): pisca nesta cor.
    [SerializeField]
    private Color acceptedColor = new(0.3f, 0.8f, 0.35f, 1f);

    // Campo que ja tem achado registrado no item aberto. Fica ate o item trocar; nao bloqueia
    // o clique, porque o mesmo campo pode quebrar mais de uma regra.
    [SerializeField]
    private Color foundColor = new(0.55f, 0.85f, 0.55f, 0.6f);

    // Quantidade e duracao das piscadas, para recusa e para achado.
    [SerializeField, Min(1), FormerlySerializedAs("rejectedBlinks")]
    private int flashBlinks = 2;

    [SerializeField, Min(0.01f), FormerlySerializedAs("rejectedBlinkSeconds")]
    private float flashBlinkSeconds = 0.12f;

    // Pista que nao depende de cor: recusa treme na horizontal, achado da um "pulo" de escala.
    // Assim aceito x recusado se distinguem mesmo para quem nao separa vermelho de verde.
    [SerializeField, Min(0f)]
    private float rejectedShakeDistance = 6f;

    [SerializeField, Min(0.01f)]
    private float rejectedShakeSeconds = 0.3f;

    [SerializeField, Min(1f)]
    private float acceptedPulseScale = 1.06f;

    [SerializeField, Min(0.01f)]
    private float acceptedPulseSeconds = 0.25f;

    private Graphic graphic;
    private Outline selectedOutline;
    private Color originalColor;
    private bool isSelected;
    private bool isHovered;
    private bool isFlashing;
    private bool isFound;
    private Color flashColor;
    private Coroutine flashRoutine;
    private Coroutine motionRoutine;
    private bool isMoving;
    private Vector3 restPosition;
    private Vector3 restScale;

    protected readonly SignalSubscriptions Signals = new();
    protected bool InspectionModeActive { get; private set; }
    protected abstract string SelectionId { get; }
    protected abstract string HighlightAction { get; }

    protected virtual void Awake()
    {
        // Sem "??": objeto da Unity ausente pode ser um "null falso" que o ?? nao reconhece.
        if (!TryGetComponent(out graphic))
            graphic = GetComponentInChildren<Graphic>();
        if (graphic != null)
        {
            originalColor = graphic.color;

            // Criada aqui (e nao no prefab) para valer em todo seletor sem editar os prefabs.
            selectedOutline = graphic.gameObject.AddComponent<Outline>();
            selectedOutline.effectColor = selectedOutlineColor;
            selectedOutline.effectDistance = new Vector2(selectedOutlineWidth, -selectedOutlineWidth);
            selectedOutline.useGraphicAlpha = false;
            selectedOutline.enabled = false;
        }

        Signals
            .On<string>(HighlightAction, OnHighlight)
            .On<bool>(InspectionSignals.ModeChanged, OnInspectionModeChanged)
            .On(InspectionSignals.SelectionCleared, ClearHighlight)
            .On<InspectionSelection>(InspectionSignals.SelectionRejected, OnSelectionRejected)
            .On<InspectionSelection>(InspectionSignals.SelectionAccepted, OnSelectionAccepted);
    }

    // Start roda depois do Setup feito logo apos o Instantiate, entao o seletor ja tem seu ID
    // quando o estado atual (modo e highlights) chega.
    protected virtual void Start()
    {
        ActionController.SendAction(InspectionSignals.StateRequested);
    }

    // As subclasses inscrevem sinais extras em Signals no Awake; tudo sai aqui.
    protected virtual void OnDestroy()
    {
        Signals.Clear();
    }

    // Objeto desativado (ex.: pagina do Manual trocada) nao recebe OnPointerExit,
    // e a Unity para as corrotinas dele.
    protected virtual void OnDisable()
    {
        isHovered = false;
        isFlashing = false;
        flashRoutine = null;
        ResetMotion();
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

    protected void SetFound(bool found)
    {
        isFound = found;
        ApplyColor();
    }

    private void OnSelectionRejected(InspectionSelection selection)
    {
        if (Flash(selection, rejectedColor))
            StartMotion(Shake());
    }

    private void OnSelectionAccepted(InspectionSelection selection)
    {
        if (Flash(selection, acceptedColor))
            StartMotion(Pulse());
    }

    // Devolve se este seletor participa da combinacao (e portanto piscou).
    private bool Flash(InspectionSelection selection, Color color)
    {
        if (selection == null || !isActiveAndEnabled || !MatchesSelection(selection))
            return false;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Blink(color));
        return true;
    }

    // Guarda a posicao/escala de repouso antes de mexer, para sempre voltar exatamente a ela.
    private void StartMotion(IEnumerator motion)
    {
        if (motionRoutine != null)
            StopCoroutine(motionRoutine);
        ResetMotion();

        restPosition = transform.localPosition;
        restScale = transform.localScale;
        isMoving = true;
        motionRoutine = StartCoroutine(motion);
    }

    private void ResetMotion()
    {
        motionRoutine = null;
        if (!isMoving)
            return;

        transform.localPosition = restPosition;
        transform.localScale = restScale;
        isMoving = false;
    }

    // Vai e volta 3 vezes na horizontal, perdendo forca ate parar.
    private IEnumerator Shake()
    {
        for (float elapsed = 0f; elapsed < rejectedShakeSeconds; elapsed += Time.deltaTime)
        {
            float progress = elapsed / rejectedShakeSeconds;
            float offset =
                Mathf.Sin(progress * Mathf.PI * 6f) * rejectedShakeDistance * (1f - progress);
            transform.localPosition = restPosition + new Vector3(offset, 0f, 0f);
            yield return null;
        }

        ResetMotion();
    }

    // Cresce ate acceptedPulseScale e volta ao tamanho normal.
    private IEnumerator Pulse()
    {
        for (float elapsed = 0f; elapsed < acceptedPulseSeconds; elapsed += Time.deltaTime)
        {
            float progress = elapsed / acceptedPulseSeconds;
            float scale = 1f + (acceptedPulseScale - 1f) * Mathf.Sin(progress * Mathf.PI);
            transform.localScale = restScale * scale;
            yield return null;
        }

        ResetMotion();
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

    // Prioridade: piscando (recusa/achado) > selecionado > hover (so com o modo ligado)
    // > ja tem achado > cor original.
    private void ApplyColor()
    {
        if (graphic == null)
            return;

        // A borda segue a selecao mesmo durante a piscada, que so troca o preenchimento.
        if (selectedOutline != null)
            selectedOutline.enabled = isSelected;

        if (isFlashing)
            graphic.color = flashColor;
        else if (isSelected)
            graphic.color = selectedColor;
        else if (isHovered && InspectionModeActive)
            graphic.color = hoverColor;
        else if (isFound)
            graphic.color = foundColor;
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
