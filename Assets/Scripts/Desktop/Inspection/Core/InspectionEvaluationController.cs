using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class InspectionEvaluationController : MonoBehaviour
{
    public const string EmailSelectedAction = "CurrentEmailChanged";

    private readonly ManualRuleEvaluator evaluator = new();
    private ManualRuleBookData manual;
    private IInspectionSurface currentSurface;

    public EmailData CurrentEmail { get; private set; }
    public SaveData CurrentSave { get; private set; }
    public IReadOnlyList<InspectionEvaluationResult> CurrentResults { get; private set; } =
        Array.Empty<InspectionEvaluationResult>();

    public bool HasFindingStore => currentSurface?.FindingStore != null;

    private void Awake()
    {
        ActionController.SyncSignal<EmailSelectionContext>(EmailSelectedAction, SetEmailContext);
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
        ActionController.UnsyncSignal<EmailSelectionContext>(EmailSelectedAction, SetEmailContext);
    }

    private void SetEmailContext(EmailSelectionContext context)
    {
        CurrentEmail = context?.Email;
        currentSurface = CurrentEmail == null
            ? null
            : new EmailInspectionSurface(
                CurrentEmail,
                new EmailInspectionFindingStore(CurrentEmail)
            );

        CurrentResults = currentSurface == null || manual == null
            ? Array.Empty<InspectionEvaluationResult>()
            : evaluator.Evaluate(currentSurface, manual, DataController.CurrentDay);
    }

    public bool HasRegisteredFinding(string findingId)
    {
        return currentSurface?.FindingStore?.HasFinding(findingId) ?? false;
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
        return currentSurface?.FindingStore != null
            && TryGetFinding(findingId, out InspectionEvaluationResult result)
            && currentSurface.FindingStore.TryRegisterFinding(result);
    }
}
