using System;
using System.Collections.Generic;
using System.Linq;

public sealed class ManualRuleEvaluationResult : InspectionEvaluationResult
{
    public string SubruleId { get; }
    public string Field { get; }
    public string Rule { get; }
    public string Subrule { get; }

    public ManualRuleEvaluationResult(
        string subruleId,
        string field,
        string ruleId,
        string rule,
        string subrule,
        string evidence,
        string explanation,
        string reason
    ) : base(
        subruleId,
        "manual-content",
        field,
        ruleId,
        rule,
        evidence,
        explanation,
        reason
    )
    {
        SubruleId = subruleId;
        Field = field;
        Rule = rule;
        Subrule = subrule;
    }
}

public sealed class ManualRuleEvaluator
{
    private readonly Dictionary<string, Func<IInspectionFieldSource, ManualRuleCheckData, string>> checks =
        new()
        {
            ["SenderDomain"] = EvaluateSenderDomain,
            ["SubjectTerms"] = EvaluateSubjectTerms,
            ["BodyTerms"] = EvaluateBodyTerms,
            ["AttachmentExtension"] = EvaluateAttachmentExtension,
        };
    private readonly Dictionary<string, string> requiredCapabilities =
        new()
        {
            ["SenderDomain"] = "sender",
            ["SubjectTerms"] = "subject",
            ["BodyTerms"] = "body",
            ["AttachmentExtension"] = "attachments",
        };

    public IReadOnlyList<InspectionEvaluationResult> Evaluate(
        IInspectionSurface surface,
        ManualRuleBookData manual,
        int currentDay
    )
    {
        if (surface == null)
            throw new ArgumentNullException(nameof(surface));
        if (surface.FieldSource == null)
            throw new InvalidOperationException("Inspection surface field source is missing.");
        if (surface.Capabilities == null)
            throw new InvalidOperationException("Inspection surface capabilities are missing.");
        if (manual == null)
            throw new ArgumentNullException(nameof(manual));
        if (manual.rules == null)
            throw new InvalidOperationException("Manual rules collection is missing.");

        List<InspectionEvaluationResult> results = new();
        foreach (ManualRuleData rule in ManualRepository.GetActiveRules(manual, currentDay))
        {
            if (rule == null)
                continue;

            foreach (ManualSubruleData subrule in rule.subrules)
            {
                if (subrule == null || subrule.check == null)
                    throw new InvalidOperationException(
                        $"Rule '{rule.id}' contains an invalid subrule."
                    );
                if (subrule.finding == null)
                    throw new InvalidOperationException(
                        $"Subrule '{subrule.id}' contains no finding data."
                    );

                if (!checks.TryGetValue(
                        subrule.check.type,
                        out Func<IInspectionFieldSource, ManualRuleCheckData, string> check
                    ))
                    throw new InvalidOperationException(
                        $"Unsupported check type '{subrule.check.type}' in subrule '{subrule.id}'."
                    );

                if (!surface.Capabilities.Contains(requiredCapabilities[subrule.check.type]))
                    continue;

                string evidence = check(surface.FieldSource, subrule.check);
                if (evidence == null)
                    continue;

                ManualFindingData finding = subrule.finding;
                results.Add(
                    new ManualRuleEvaluationResult(
                        subrule.id,
                        rule.field,
                        rule.id,
                        finding.rule,
                        subrule.description,
                        string.IsNullOrWhiteSpace(finding.evidence) ? evidence : finding.evidence,
                        finding.explanation,
                        finding.reason
                    )
                );
            }
        }

        return results;
    }

    public IReadOnlyList<InspectionEvaluationResult> Evaluate(
        EmailData email,
        ManualRuleBookData manual,
        int currentDay
    )
    {
        return Evaluate(new EmailInspectionSurface(email), manual, currentDay);
    }

    private static string EvaluateSenderDomain(
        IInspectionFieldSource fields,
        ManualRuleCheckData check
    )
    {
        RequireValues(check.allowedDomains, check.type, "allowedDomains");
        if (!fields.TryGetText("sender", out string sender))
            return null;

        int separator = sender?.LastIndexOf('@') ?? -1;
        if (separator < 0 || separator == sender.Length - 1)
            return sender ?? string.Empty;

        string domain = sender[(separator + 1)..].Trim().ToLowerInvariant();
        bool allowed = check.allowedDomains.Any(
            allowedDomain =>
                string.Equals(
                    domain,
                    NormalizeDomain(allowedDomain),
                    StringComparison.OrdinalIgnoreCase
                )
        );
        return allowed ? null : domain;
    }

    private static string EvaluateSubjectTerms(
        IInspectionFieldSource fields,
        ManualRuleCheckData check
    )
    {
        RequireValues(check.terms, check.type, "terms");
        return FindTerm(
            fields.TryGetText("subject", out string subject) ? subject : null,
            check.terms,
            check.caseSensitive
        );
    }

    private static string EvaluateBodyTerms(
        IInspectionFieldSource fields,
        ManualRuleCheckData check
    )
    {
        if (check.terms == null || check.terms.Length == 0)
            RequireValues(check.suspiciousDomains, check.type, "terms or suspiciousDomains");

        string term = FindTerm(
            fields.TryGetText("body", out string body) ? body : null,
            check.terms,
            check.caseSensitive
        );
        if (term != null)
            return term;

        return FindTerm(body, check.suspiciousDomains, check.caseSensitive);
    }

    private static string EvaluateAttachmentExtension(
        IInspectionFieldSource fields,
        ManualRuleCheckData check
    )
    {
        RequireValues(check.forbiddenExtensions, check.type, "forbiddenExtensions");
        foreach (InspectionAttachmentDescriptor attachment in fields.GetAttachments())
        {
            if (attachment == null || string.IsNullOrWhiteSpace(attachment.Extension))
                continue;

            string extension = attachment.Extension.Trim().TrimStart('.');
            bool forbidden = check.forbiddenExtensions.Any(
                item => string.Equals(
                    extension,
                    item.Trim().TrimStart('.'),
                    StringComparison.OrdinalIgnoreCase
                )
            );
            if (forbidden)
                return extension;
        }

        return null;
    }

    private static string FindTerm(string value, string[] terms, bool caseSensitive)
    {
        if (string.IsNullOrEmpty(value) || terms == null)
            return null;

        StringComparison comparison = caseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
        foreach (string term in terms)
        {
            if (!string.IsNullOrEmpty(term) && value.IndexOf(term, comparison) >= 0)
                return term;
        }

        return null;
    }

    private static string NormalizeDomain(string domain)
    {
        return domain?.Trim().TrimStart('@').ToLowerInvariant() ?? string.Empty;
    }

    private static void RequireValues(string[] values, string checkType, string name)
    {
        if (values == null || values.Length == 0 || values.All(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException(
                $"Check '{checkType}' requires a non-empty '{name}' parameter."
            );
    }
}
