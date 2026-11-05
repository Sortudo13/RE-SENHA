using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class InspectionEvaluationController : MonoBehaviour
{
    public const string EmailSelectedAction = EmailContextPublisher.EmailContextChangedAction;
    public const string FindingsEvaluatedAction = "InspectionFindingsEvaluated";

    [SerializeField]
    private string manualResourcePath = ManualRulesLoader.DefaultResourcePath;

    private readonly ManualRuleEvaluator evaluator = new();
    private ManualRuleBookData manual;
    private SaveData currentSave;
    private EmailData currentEmail;
    private IReadOnlyList<ManualRuleEvaluationResult> currentResults =
        Array.Empty<ManualRuleEvaluationResult>();

    public EmailData CurrentEmail => currentEmail;
    public IReadOnlyList<ManualRuleEvaluationResult> CurrentResults => currentResults;

    public bool HasCurrentSave => currentSave != null;

    public bool HasRegisteredFinding(string subruleId)
    {
        if (currentEmail?.inspectionFindings == null)
            return false;

        return currentEmail.inspectionFindings.Exists(
            finding => finding != null && finding.subruleId == subruleId
        );
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
        EvaluateCurrentEmail();
    }

    public void SetEmailContext(InspectionEmailContext context)
    {
        currentSave = context?.Save;
        currentEmail = context?.Email;
        EvaluateCurrentEmail();
    }

    public void ClearEmail()
    {
        SetEmail(null);
    }

    public void EvaluateCurrentEmail()
    {
        if (currentEmail == null)
        {
            currentResults = Array.Empty<ManualRuleEvaluationResult>();
            ActionController.SendAction(FindingsEvaluatedAction, currentResults);
            return;
        }

        if (manual == null)
            LoadManual();

        if (manual == null)
            throw new InvalidOperationException(
                "Inspection rules could not be loaded before evaluating the current email."
            );

        currentResults = evaluator.Evaluate(currentEmail, manual, DataController.CurrentDay);
        ActionController.SendAction(FindingsEvaluatedAction, currentResults);
    }

    public bool TryGetFinding(
        string subruleId,
        out ManualRuleEvaluationResult finding
    )
    {
        if (string.IsNullOrWhiteSpace(subruleId))
        {
            finding = null;
            return false;
        }

        foreach (ManualRuleEvaluationResult result in currentResults)
        {
            if (string.Equals(result.SubruleId, subruleId, StringComparison.Ordinal))
            {
                finding = result;
                return true;
            }
        }

        finding = null;
        return false;
    }

    public bool RegisterFinding(string subruleId)
    {
        if (currentEmail == null || currentSave == null)
            return false;

        if (!TryGetFinding(subruleId, out ManualRuleEvaluationResult result))
            return false;

        currentEmail.inspectionFindings ??= new List<InspectionFindingData>();
        if (currentEmail.inspectionFindings.Exists(
                finding => finding.subruleId == result.SubruleId
            ))
            return false;

        currentEmail.inspectionFindings.Add(
            new InspectionFindingData
            {
                subruleId = result.SubruleId,
                field = result.Field,
                rule = result.Rule,
                evidence = result.Evidence,
                explanation = result.Explanation,
                reason = result.Reason,
            }
        );
        DataController.SaveEmails(currentSave);
        return true;
    }

    private void LoadManual()
    {
        try
        {
            manual = ManualRulesLoader.Load(manualResourcePath);
        }
        catch (InvalidOperationException exception)
        {
            Debug.LogError($"InspectionEvaluationController: {exception.Message}");
            manual = null;
            currentResults = Array.Empty<ManualRuleEvaluationResult>();
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
