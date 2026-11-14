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
    // Payload: IInspectionSurface. Use PublishSurface para enviar.
    public const string SurfaceChangedAction = "InspectionSurfaceChanged";
    // Payload: string com o SourceType da superficie fechada. Use CloseSurface para enviar.
    public const string SurfaceClosedAction = "InspectionSurfaceClosed";

    private readonly ManualRuleEvaluator evaluator = new();
    private readonly Dictionary<string, IInspectionSurface> surfaces =
        new(StringComparer.OrdinalIgnoreCase);
    private ManualRuleBookData manual;

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

        ActionController.SendAction<IInspectionSurface>(SurfaceChangedAction, surface);
    }

    public static void CloseSurface(string sourceType)
    {
        ActionController.SendAction<string>(SurfaceClosedAction, sourceType);
    }

    private void Awake()
    {
        ActionController.SyncSignal<IInspectionSurface>(SurfaceChangedAction, SetSurface);
        ActionController.SyncSignal<string>(SurfaceClosedAction, RemoveSurface);
        try
        {
            manual = ManualRepository.Load();
        }
        catch (InvalidOperationException exception)
        {
            Debug.LogError($"InspectionEvaluationController: {exception.Message}");
        }
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<IInspectionSurface>(SurfaceChangedAction, SetSurface);
        ActionController.UnsyncSignal<string>(SurfaceClosedAction, RemoveSurface);
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
