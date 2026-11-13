using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public sealed class ManualRuleEvaluator
{
    private sealed class CheckDefinition
    {
        public string Capability { get; }
        public Func<IInspectionFieldSource, ManualRuleCheckData, string> Evaluate { get; }

        public CheckDefinition(
            string capability,
            Func<IInspectionFieldSource, ManualRuleCheckData, string> evaluate
        )
        {
            Capability = capability;
            Evaluate = evaluate;
        }
    }

    public const string EvaluatorId = "manual-content";

    // Tipos de verificacao aceitos no ManualRules.json e a capacidade que cada um exige da superficie.
    private static readonly Dictionary<string, CheckDefinition> Checks = new()
    {
        ["SenderDomain"] = new("sender", EvaluateSenderDomain),
        ["SubjectTerms"] = new("subject", EvaluateSubjectTerms),
        ["BodyTerms"] = new("body", EvaluateBodyTerms),
        ["BodyLinkDomains"] = new("body", EvaluateBodyLinkDomains),
        ["AttachmentExtension"] = new("attachments", EvaluateAttachmentExtension),
    };

    public static bool SupportsCheck(string type)
    {
        return type != null && Checks.ContainsKey(type);
    }

    // O ManualRulesLoader ja valida IDs, subregras, check, finding e tipos ao carregar o JSON.
    public IReadOnlyList<InspectionEvaluationResult> Evaluate(
        IInspectionSurface surface,
        ManualRuleBookData manual,
        int currentDay
    )
    {
        if (surface == null)
            throw new ArgumentNullException(nameof(surface));

        List<InspectionEvaluationResult> results = new();
        foreach (ManualRuleData rule in ManualRepository.GetActiveRules(manual, currentDay, surface.SourceType))
        {
            foreach (ManualSubruleData subrule in rule.subrules)
            {
                CheckDefinition check = Checks[subrule.check.type];
                if (!surface.Capabilities.Contains(check.Capability))
                    continue;

                string evidence = check.Evaluate(surface.FieldSource, subrule.check);
                if (evidence == null)
                    continue;

                ManualFindingData finding = subrule.finding;
                results.Add(
                    new InspectionEvaluationResult(
                        subrule.id,
                        EvaluatorId,
                        rule.field,
                        rule.id,
                        finding.rule,
                        string.IsNullOrWhiteSpace(finding.evidence) ? evidence : finding.evidence,
                        finding.explanation,
                        finding.reason
                    )
                );
            }
        }

        return results;
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

        return FindSuspiciousHost(body, check.suspiciousDomains);
    }

    // Hosts (com ou sem esquema) presentes no texto, inclusive dentro de tags <link="...">.
    private static readonly Regex HostPattern = new(
        @"(?<![A-Za-z0-9-])(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z]{2,}(?![A-Za-z0-9-])",
        RegexOptions.Compiled
    );

    // Compara dominios pelo final do host, e nao como substring: ".ru" acusa "site.ru",
    // mas nao "www.rural.com"; "empresa.com" acusa "empresa.com" e "mail.empresa.com".
    // Dominios nao diferenciam maiusculas, por isso caseSensitive nao se aplica aqui.
    private static string FindSuspiciousHost(string value, string[] suspiciousDomains)
    {
        if (string.IsNullOrEmpty(value) || suspiciousDomains == null)
            return null;

        foreach (Match match in HostPattern.Matches(value))
        {
            string host = match.Value.ToLowerInvariant();
            foreach (string domain in suspiciousDomains)
            {
                if (string.IsNullOrWhiteSpace(domain))
                    continue;

                string suffix = domain.Trim().ToLowerInvariant();
                bool matches = suffix.StartsWith(".")
                    ? host.EndsWith(suffix, StringComparison.Ordinal)
                    : host == suffix || host.EndsWith("." + suffix, StringComparison.Ordinal);
                if (matches)
                    return host;
            }
        }

        return null;
    }

    // Lista branca de links
    // Pega o destino real de <link="OpenURL|..."> mesmo quando o texto visivel do link mostra um endereco da empresa.
    private static string EvaluateBodyLinkDomains(
        IInspectionFieldSource fields,
        ManualRuleCheckData check
    )
    {
        RequireValues(check.allowedDomains, check.type, "allowedDomains");
        if (!fields.TryGetText("body", out string body) || string.IsNullOrEmpty(body))
            return null;

        foreach (Match match in HostPattern.Matches(body))
        {
            string host = match.Value.ToLowerInvariant();
            bool allowed = check.allowedDomains.Any(allowedDomain =>
            {
                string domain = NormalizeDomain(allowedDomain);
                return domain.Length > 0
                    && (host == domain || host.EndsWith("." + domain, StringComparison.Ordinal));
            });
            if (!allowed)
                return host;
        }

        return null;
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
