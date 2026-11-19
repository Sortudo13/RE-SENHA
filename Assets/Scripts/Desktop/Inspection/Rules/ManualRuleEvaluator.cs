using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public sealed class ManualRuleEvaluator
{
    // Evaluate recebe (campos, parametros do check, campo a verificar) e devolve a evidencia
    // encontrada ou null.
    private sealed class CheckDefinition
    {
        // Campo fixo que o verificador le; null = generico, le o "field" da regra.
        public string Capability { get; }
        public Func<IInspectionFieldSource, ManualRuleCheckData, string, string> Evaluate { get; }

        public CheckDefinition(
            string capability,
            Func<IInspectionFieldSource, ManualRuleCheckData, string, string> evaluate
        )
        {
            Capability = capability;
            Evaluate = evaluate;
        }

        // Verificador especifico do Email (campo fixo, ignora o "field" da regra).
        public CheckDefinition(
            string capability,
            Func<IInspectionFieldSource, ManualRuleCheckData, string> evaluate
        )
            : this(capability, (fields, check, _) => evaluate(fields, check)) { }
    }

    public const string EvaluatorId = "manual-content";

    // Tipos de verificacao aceitos no ManualRules.json e a capacidade que cada um exige da superficie.
    // Os genericos (capacidade null) servem para qualquer app: leem o "field" da regra.
    private static readonly Dictionary<string, CheckDefinition> Checks = new()
    {
        ["Contains"] = new(null, EvaluateContains),
        ["DomainNotAllowed"] = new(null, EvaluateDomainNotAllowed),
        ["SenderDomain"] = new(InspectionFields.Sender, EvaluateSenderDomain),
        ["SubjectTerms"] = new(InspectionFields.Subject, EvaluateSubjectTerms),
        ["BodyTerms"] = new(InspectionFields.Body, EvaluateBodyTerms),
        ["BodyLinkDomains"] = new(InspectionFields.Body, EvaluateBodyLinkDomains),
        ["AttachmentExtension"] = new(InspectionFields.Attachments, EvaluateAttachmentExtension),
    };

    public static bool SupportsCheck(string type)
    {
        return type != null && Checks.ContainsKey(type);
    }

    // Campo que a superficie precisa ter para a subregra ser avaliada: o campo fixo do
    // verificador ou, nos genericos, o "field" da regra. Null se o tipo nao existe.
    public static string GetRequiredField(ManualRuleData rule, ManualSubruleData subrule)
    {
        string type = subrule?.check?.type;
        if (type == null || !Checks.TryGetValue(type, out CheckDefinition check))
            return null;

        return check.Capability ?? rule?.field;
    }

    // O ManualRules ja valida IDs, subregras, check, finding e tipos ao carregar o JSON.

    // Generico: o campo contem um dos termos.
    private static string EvaluateContains(
        IInspectionFieldSource fields,
        ManualRuleCheckData check,
        string field
    )
    {
        RequireValues(check.terms, check.type, "terms");
        return FindTerm(
            fields.TryGetText(field, out string value) ? value : null,
            check.terms,
            check.caseSensitive
        );
    }

    // Generico: o endereco de email do campo nao e de um dominio permitido. Campo vazio ou sem
    // "@" acusa (nao da para confirmar o dominio), como no SenderDomain.
    private static string EvaluateDomainNotAllowed(
        IInspectionFieldSource fields,
        ManualRuleCheckData check,
        string field
    )
    {
        RequireValues(check.allowedDomains, check.type, "allowedDomains");
        if (!fields.TryGetText(field, out string value))
            return null;

        return FindDomainNotAllowed(value, check.allowedDomains);
    }
    public IReadOnlyList<InspectionEvaluationResult> Evaluate(
        IInspectionSurface surface,
        ManualRuleBookData manual,
        int currentDay
    )
    {
        if (surface == null)
            throw new ArgumentNullException(nameof(surface));

        List<InspectionEvaluationResult> results = new();
        foreach (ManualRuleData rule in ManualRules.GetActiveRules(manual, currentDay, surface.SourceType))
        {
            foreach (ManualSubruleData subrule in rule.subrules)
            {
                CheckDefinition check = Checks[subrule.check.type];
                string field = check.Capability ?? rule.field;
                if (!InspectionFields.Contains(surface.Capabilities, field))
                    continue;

                string evidence = check.Evaluate(surface.FieldSource, subrule.check, field);
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
        if (!fields.TryGetText(InspectionFields.Sender, out string sender))
            return null;

        return FindDomainNotAllowed(sender, check.allowedDomains);
    }

    private static string FindDomainNotAllowed(string address, string[] allowedDomains)
    {
        int separator = address?.LastIndexOf('@') ?? -1;
        if (separator < 0 || separator == address.Length - 1)
            return address ?? string.Empty;

        string domain = address[(separator + 1)..].Trim().ToLowerInvariant();
        bool allowed = allowedDomains.Any(
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
            fields.TryGetText(InspectionFields.Subject, out string subject) ? subject : null,
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
            fields.TryGetText(InspectionFields.Body, out string body) ? body : null,
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
        if (!fields.TryGetText(InspectionFields.Body, out string body) || string.IsNullOrEmpty(body))
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
