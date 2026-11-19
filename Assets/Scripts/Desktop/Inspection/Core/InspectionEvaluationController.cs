using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Guarda as superficies abertas (uma por app: o email aberto, o colaborador selecionado no
// Database...) e as quebras de regra do item em julgamento.
// Nao conhece nenhum app: cada app publica a propria superficie com PublishSurface e avisa
// com CloseSurface quando o item e fechado.
public sealed class InspectionEvaluationController : MonoBehaviour
{

    private readonly ManualRuleEvaluator evaluator = new();
    private readonly Dictionary<string, IInspectionSurface> surfaces =
        new(StringComparer.OrdinalIgnoreCase);
    private ManualRuleBookData manual;
    private readonly SignalSubscriptions signals = new();

    // Item em julgamento: a ultima superficie publicada que guarda achados (FindingStore != null).
    // Superficies sem FindingStore (ex.: Database) sao so referencia; consulte com TryGetSurface.
    public IInspectionSurface CurrentSurface { get; private set; }
    public IReadOnlyList<InspectionEvaluationResult> CurrentResults { get; private set; } =
        Array.Empty<InspectionEvaluationResult>();

    // Achados que o jogador ja registrou no item em julgamento.
    public IReadOnlyList<EvidenceData> CurrentFindings =>
        CurrentSurface?.FindingStore?.Findings ?? Array.Empty<EvidenceData>();

    public bool HasFindingStore => CurrentSurface?.FindingStore != null;

    // O ActionController so entrega o sinal se o tipo generico for exatamente IInspectionSurface;
    // enviar a classe concreta (ex.: EmailInspectionSurface) seria ignorado em silencio.
    public static void PublishSurface(IInspectionSurface surface)
    {
        if (surface == null)
        {
            Debug.LogWarning(
                "InspectionEvaluationController: PublishSurface(null) ignored. Use CloseSurface(sourceType)."
            );
            return;
        }

        ActionController.SendAction<IInspectionSurface>(InspectionSignals.SurfaceChanged, surface);
    }

    public static void CloseSurface(string sourceType)
    {
        ActionController.SendAction<string>(InspectionSignals.SurfaceClosed, sourceType);
    }

    private void Awake()
    {
        signals
            .On<IInspectionSurface>(InspectionSignals.SurfaceChanged, SetSurface)
            .On<string>(InspectionSignals.SurfaceClosed, RemoveSurface)
            .On<string>(InspectionSignals.FindingRegistered, OnFindingRegistered)
            .On(InspectionSignals.StateRequested, PublishFoundFields);
        try
        {
            manual = ManualRules.Load();
        }
        catch (InvalidOperationException exception)
        {
            Debug.LogError($"InspectionEvaluationController: {exception.Message}");
        }
    }

    private void OnDestroy()
    {
        signals.Clear();
    }

    public bool TryGetSurface(string sourceType, out IInspectionSurface surface)
    {
        surface = null;
        return !string.IsNullOrWhiteSpace(sourceType)
            && surfaces.TryGetValue(sourceType.Trim(), out surface);
    }

    private void SetSurface(IInspectionSurface surface)
    {
        if (surface == null || string.IsNullOrWhiteSpace(surface.SourceType))
            return;

        surfaces[surface.SourceType.Trim()] = surface;
        if (surface.FindingStore != null)
            SetCurrentSurface(surface);
    }

    private void RemoveSurface(string sourceType)
    {
        if (string.IsNullOrWhiteSpace(sourceType) || !surfaces.Remove(sourceType.Trim()))
            return;

        // Se outro app com achados continua aberto, ele volta a ser o item em julgamento.
        if (CurrentSurface != null
            && InspectionFields.Matches(CurrentSurface.SourceType, sourceType))
            SetCurrentSurface(surfaces.Values.FirstOrDefault(open => open.FindingStore != null));
    }

    private void SetCurrentSurface(IInspectionSurface surface)
    {
        CurrentSurface = surface;
        CurrentResults = CurrentSurface == null || manual == null
            ? Array.Empty<InspectionEvaluationResult>()
            : evaluator.Evaluate(CurrentSurface, manual, DataController.CurrentDay);
        PublishFoundFields();
    }

    private void OnFindingRegistered(string findingId)
    {
        PublishFoundFields();
    }

    // Um campo pode ter varios achados (ex.: Attachments em dias diferentes); vai uma vez so.
    // Saves antigos podem ter so o campo legado "field".
    private void PublishFoundFields()
    {
        string[] fields = CurrentFindings
            .Select(finding => string.IsNullOrWhiteSpace(finding?.fieldId)
                ? finding?.field
                : finding.fieldId)
            .Where(field => !string.IsNullOrWhiteSpace(field))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        ActionController.SendAction<string[]>(InspectionSignals.FoundFieldsChanged, fields);
    }

    public bool HasRegisteredFinding(string findingId)
    {
        return CurrentSurface?.FindingStore?.HasFinding(findingId) ?? false;
    }

    public bool TryGetFinding(string findingId, out InspectionEvaluationResult finding)
    {
        finding = CurrentResults.FirstOrDefault(
            result => string.Equals(result.FindingId, findingId, StringComparison.Ordinal)
        );
        return finding != null;
    }

    public bool RegisterFinding(string findingId)
    {
        return CurrentSurface?.FindingStore != null
            && TryGetFinding(findingId, out InspectionEvaluationResult result)
            && CurrentSurface.FindingStore.TryRegisterFinding(result);
    }
}
