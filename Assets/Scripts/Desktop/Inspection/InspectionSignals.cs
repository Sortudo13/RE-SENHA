using System;
using System.Collections.Generic;

// Todos os sinais (ActionController) do Inspector, num lugar so.
// O ActionController so entrega um sinal se o tipo do payload de quem envia for identico ao
// de quem escuta; por isso cada constante diz o payload.
public static class InspectionSignals
{
    // ---- Item inspecionado (o app avisa; use PublishSurface/CloseSurface) ----

    // Payload IInspectionSurface. Envie com InspectionEvaluationController.PublishSurface.
    public const string SurfaceChanged = "InspectionSurfaceChanged";

    // Payload string (SourceType da superficie fechada). Envie com CloseSurface.
    public const string SurfaceClosed = "InspectionSurfaceClosed";

    // ---- Modo inspetor ----

    // Sem payload. Botao (ActionButtonController) ou Espaco: liga/desliga o modo.
    public const string ToggleMode = "ToggleInspectionMode";

    // Sem payload. Desliga o modo (ex.: depois de registrar um achado).
    public const string DeactivateMode = "DeactivateInspectionMode";

    // Payload bool. O modo mudou.
    public const string ModeChanged = "InspectionModeChanged";

    // Sem payload. Um seletor novo pede o estado atual; quem tem estado (modo, destaques,
    // campos com achado) reenvia.
    public const string StateRequested = "InspectionStateRequested";

    // ---- Selecao campo + subregra (internos, entre seletores e InspectionSelectionController) ----

    // Payload InspectionSelection. Clique em um campo / em uma subregra do Manual.
    public const string FieldSelected = "InspectionFieldSelected";
    public const string RuleSelected = "InspectionRuleSelected";

    // Payload string (id selecionado ou null). Pinta o campo / a subregra selecionada.
    public const string FieldHighlight = "InspectionFieldHighlight";
    public const string RuleHighlight = "InspectionRuleHighlight";

    // Sem payload. Limpa a selecao.
    public const string SelectionCleared = "InspectionSelectionCleared";

    // Payload InspectionSelection (FieldId = campo, SelectionId = subregra). Pisca vermelho / verde.
    public const string SelectionRejected = "InspectionSelectionRejected";
    public const string SelectionAccepted = "InspectionSelectionAccepted";

    // ---- Achados ----

    // Payload string (id do achado). Achado novo registrado no item em julgamento.
    public const string FindingRegistered = "InspectionFindingRegistered";

    // Payload string[] (campos). Campos do item em julgamento que ja tem achado.
    public const string FoundFieldsChanged = "InspectionFoundFieldsChanged";

    // Sem payload. Botao "Ver regras" do app: abre a lista de achados do item.
    public const string ShowFindings = "ShowInspectionFindings";

    // ---- Email ----

    // Payload EmailData (null ao fechar). Enviado pelo EmailController.
    public const string CurrentEmailChanged = "CurrentEmailChanged";

    // Sem payload. Botoes Aprovar / Bloquear do Email.
    public const string ApproveEmail = "ApproveCurrentEmail";
    public const string BlockEmail = "BlockCurrentEmail";
}

// Inscricao em sinais sem repetir cada um no OnDestroy:
//   signals.On(InspectionSignals.ToggleMode, Toggle).On<bool>(InspectionSignals.ModeChanged, OnMode);
//   ... e no OnDestroy: signals.Clear();
public sealed class SignalSubscriptions
{
    private readonly List<Action> unsubscribe = new();

    public SignalSubscriptions On(string signal, Action handler)
    {
        ActionController.SyncSignal(signal, handler);
        unsubscribe.Add(() => ActionController.UnsyncSignal(signal, handler));
        return this;
    }

    public SignalSubscriptions On<T>(string signal, Action<T> handler)
    {
        ActionController.SyncSignal(signal, handler);
        unsubscribe.Add(() => ActionController.UnsyncSignal(signal, handler));
        return this;
    }

    public void Clear()
    {
        foreach (Action action in unsubscribe)
            action();
        unsubscribe.Clear();
    }
}
