using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class InspectionEvaluationController : MonoBehaviour
{
    public const string EmailSelectedAction = "CurrentEmailChanged";
    public const string FindingsEvaluatedAction = "InspectionFindingsEvaluated";

    [SerializeField]
    private string manualResourcePath = ManualRulesLoader.DefaultResourcePath;

    private readonly ManualRuleEvaluator evaluator = new();
    private ManualRuleBookData manual;
    private SaveData currentSave;
    private EmailData currentEmail;
    private IInspectionSurface currentSurface;
    private IReadOnlyList<InspectionEvaluationResult> currentResults =
        Array.Empty<InspectionEvaluationResult>();

    public EmailData CurrentEmail => currentEmail;
    public IInspectionSurface CurrentSurface => currentSurface;
    public IReadOnlyList<InspectionEvaluationResult> CurrentResults => currentResults;

    public bool HasFindingStore => currentSurface?.FindingStore != null;

    public bool HasRegisteredFinding(string findingId)
    {
        return currentSurface?.FindingStore?.HasFinding(findingId) ?? false;
    }

    private void Awake()
    {
        ActionController.SyncSignal<InspectionEmailContext>(EmailSelectedAction, SetEmailContext);
        LoadManual();
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<InspectionEmailContext>(
            EmailSelectedAction,
            SetEmailContext
        );
    }

    public void SetEmail(EmailData email)
    {
        currentSave = null;
        currentEmail = email;
        currentSurface = currentEmail == null
            ? null
            : new EmailInspectionSurface(currentEmail);
        EvaluateCurrentSurface();
    }

    public void SetEmailContext(InspectionEmailContext context)
    {
        currentSave = context?.Save;
        currentEmail = context?.Email;
        currentSurface = currentEmail == null
            ? null
            : new EmailInspectionSurface(
                currentEmail,
                new EmailInspectionFindingStore(currentSave, currentEmail)
            );
        EvaluateCurrentSurface();
    }

    public void SetSurface(IInspectionSurface surface)
    {
        currentSave = null;
        currentEmail = null;
        currentSurface = surface;
        EvaluateCurrentSurface();
    }

    public void ClearEmail()
    {
        ClearSurface();
    }

    public void ClearSurface()
    {
        currentSave = null;
        currentEmail = null;
        currentSurface = null;
        EvaluateCurrentSurface();
    }

    public void EvaluateCurrentSurface()
    {
        if (currentSurface == null)
        {
            currentResults = Array.Empty<InspectionEvaluationResult>();
            ActionController.SendAction(FindingsEvaluatedAction, currentResults);
            return;
        }

        if (manual == null)
            LoadManual();

        if (manual == null)
            throw new InvalidOperationException(
                "Inspection rules could not be loaded before evaluating the current surface."
            );

        currentResults = evaluator.Evaluate(
            currentSurface,
            manual,
            DataController.CurrentDay
        );
        ActionController.SendAction(FindingsEvaluatedAction, currentResults);
    }

    public bool TryGetFinding(string findingId, out InspectionEvaluationResult finding)
    {
        if (string.IsNullOrWhiteSpace(findingId))
        {
            finding = null;
            return false;
        }

        foreach (InspectionEvaluationResult result in currentResults)
        {
            if (string.Equals(result.FindingId, findingId, StringComparison.Ordinal))
            {
                finding = result;
                return true;
            }
        }

        finding = null;
        return false;
    }

    public bool RegisterFinding(string findingId)
    {
        if (currentSurface?.FindingStore == null)
            return false;

        if (!TryGetFinding(findingId, out InspectionEvaluationResult result))
            return false;

        return currentSurface.FindingStore.TryRegisterFinding(result);
    }

    private void LoadManual()
    {
        try
        {
            manual = ManualRepository.Load(manualResourcePath);
        }
        catch (InvalidOperationException exception)
        {
            Debug.LogError($"InspectionEvaluationController: {exception.Message}");
            manual = null;
            currentResults = Array.Empty<InspectionEvaluationResult>();
        }
    }
}

public sealed class InspectionEmailContext
{
    public SaveData Save { get; }
    public EmailData Email { get; }

    public InspectionEmailContext(SaveData save, EmailData email)
    {
        Save = save;
        Email = email;
    }
}
