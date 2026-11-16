using System;
using System.Collections.Generic;

// Superficie de inspecao do Database: o colaborador selecionado na janela.
// E so referencia para comparar com outro app (ticket, email...): nao guarda achados,
// por isso FindingStore e null e ela nunca vira o item em julgamento.
public sealed class DatabaseInspectionSurface : IInspectionSurface, IInspectionFieldSource
{
    public const string Source = "database";

    public const string Nome = "nome";
    public const string Matricula = "matricula";
    public const string Cargo = "cargo";
    public const string Departamento = "departamento";
    public const string Email = "email";
    public const string Status = "status";
    public const string RgNumero = "rgNumero";
    public const string RgExpedicao = "rgExpedicao";
    public const string RgNascimento = "rgNascimento";
    public const string RgLocalidade = "rgLocalidade";

    public EmployeeData Employee { get; }
    public string SourceType => Source;
    public IReadOnlyCollection<string> Capabilities { get; } =
        new[]
        {
            Nome,
            Matricula,
            Cargo,
            Departamento,
            Email,
            Status,
            RgNumero,
            RgExpedicao,
            RgNascimento,
            RgLocalidade,
        };
    public IInspectionFieldSource FieldSource => this;
    public IInspectionFindingStore FindingStore => null;

    public DatabaseInspectionSurface(EmployeeData employee)
    {
        Employee = employee;
    }

    // Devolve o dado puro, sem o rotulo que a janela mostra ("Cargo: ...").
    public bool TryGetText(string fieldId, out string value)
    {
        value = null;
        if (Employee == null)
            return false;

        if (InspectionFields.Matches(fieldId, Nome))
            value = Employee.nome;
        else if (InspectionFields.Matches(fieldId, Matricula))
            value = Employee.matricula;
        else if (InspectionFields.Matches(fieldId, Cargo))
            value = Employee.cargo;
        else if (InspectionFields.Matches(fieldId, Departamento))
            value = Employee.departamento;
        else if (InspectionFields.Matches(fieldId, Email))
            value = Employee.email;
        else if (InspectionFields.Matches(fieldId, Status))
            value = Employee.status;
        else if (InspectionFields.Matches(fieldId, RgNumero))
            value = Employee.rg?.numero;
        else if (InspectionFields.Matches(fieldId, RgExpedicao))
            value = Employee.rg?.dataExpedicao;
        else if (InspectionFields.Matches(fieldId, RgNascimento))
            value = Employee.rg?.dataNascimento;
        else if (InspectionFields.Matches(fieldId, RgLocalidade))
            value = Employee.rg == null
                ? null
                : $"{Employee.rg.naturalidade} - {Employee.rg.estadoEmissor}";
        else
            return false;

        return true;
    }

    public IReadOnlyList<InspectionAttachmentDescriptor> GetAttachments()
    {
        return Array.Empty<InspectionAttachmentDescriptor>();
    }
}
