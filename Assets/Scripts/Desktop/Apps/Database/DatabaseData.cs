using System;
using System.Collections.Generic;

[Serializable]
public class EmployeeDatabaseSave
{
    public List<EmployeeData> employees = new();
}

[Serializable]
public class EmployeeData
{
    public string id;
    public string matricula;
    public string nome;
    public string email;
    public string departamento;
    public string cargo;
    public string status; // "Ativo", "Férias", "Demitido"
    public EmployeeRGData rg;
    // Caminho dentro de Resources/Images, sem extensao (ex.: "NPCs/npc_2"). Usado pelo RG.
    public string foto;
}

[Serializable]
public class EmployeeRGData
{
    public string numero;
    public string dataExpedicao;
    public string dataNascimento;
    public string naturalidade;
    public string estadoEmissor;
    public string estadoEmissao; // Estado por extenso 
}

