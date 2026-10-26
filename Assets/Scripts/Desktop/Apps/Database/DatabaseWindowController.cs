using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DatabaseWindowController : MonoBehaviour
{
    [Header("Pesquisa e Lista (Painel Esquerdo)")]
    [SerializeField] private TMP_InputField inputPesquisa;
    [SerializeField] private Transform containerLista;
    [SerializeField] private GameObject prefabItemLista;

    [Header("Painéis de Detalhes (Painel Direito)")]
    [SerializeField] private GameObject painelDetalhesConteudo;
    [SerializeField] private GameObject painelVazio;

    [Header("Campos Cadastrais")]
    [SerializeField] private TextMeshProUGUI txtNome;
    [SerializeField] private TextMeshProUGUI txtMatricula;
    [SerializeField] private TextMeshProUGUI txtCargo;
    [SerializeField] private TextMeshProUGUI txtDepartamento;
    [SerializeField] private TextMeshProUGUI txtEmail;
    [SerializeField] private TextMeshProUGUI txtStatus;

    [Header("Campos de Documento (RG)")]
    [SerializeField] private TextMeshProUGUI txtRGNumero;
    [SerializeField] private TextMeshProUGUI txtRGExpedicao;
    [SerializeField] private TextMeshProUGUI txtRGNascimento;
    [SerializeField] private TextMeshProUGUI txtRGLocalidade;

    private List<EmployeeData> allEmployees = new();
    private List<EmployeeData> filteredEmployees = new();
    private EmployeeData selectedEmployee;
    private bool initialized;

    private void Awake()
    {
        if (inputPesquisa != null)
        {
            inputPesquisa.onValueChanged.AddListener(OnSearchChanged);
        }
    }

    private void Start()
    {
        CarregarBancoDeDados();
        AtualizarLista();
        initialized = true;
    }

    private void OnDestroy()
    {
        if (inputPesquisa != null)
        {
            inputPesquisa.onValueChanged.RemoveListener(OnSearchChanged);
        }
    }

    private void OnEnable()
    {
        if (initialized)
        {
            AtualizarLista();
        }
    }

    public void CarregarBancoDeDados()
    {
        int dia = DataController.CurrentDay;
        TextAsset jsonAsset = Resources.Load<TextAsset>($"Database/Database_Dia_{dia}");

        if (jsonAsset == null)
        {
            jsonAsset = Resources.Load<TextAsset>("Database/Database_Base");
        }

#if UNITY_EDITOR
        if (jsonAsset == null)
        {
            jsonAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Database/Database_Base.json");
        }
#endif

        if (jsonAsset != null && !string.IsNullOrEmpty(jsonAsset.text))
        {
            EmployeeDatabaseSave data = JsonUtility.FromJson<EmployeeDatabaseSave>(jsonAsset.text);
            allEmployees = data?.employees ?? new List<EmployeeData>();
            Debug.Log($"[DatabaseWindow] {allEmployees.Count} colaboradores carregados com sucesso do JSON.");
        }
        else
        {
            Debug.LogError("[DatabaseWindow] Nenhum arquivo JSON do banco de dados foi encontrado em Resources/Database.");
            allEmployees = new List<EmployeeData>();
        }
    }

    private void OnSearchChanged(string query)
    {
        FiltrarLista(query);
    }

    private void AtualizarLista()
    {
        FiltrarLista(inputPesquisa != null ? inputPesquisa.text : string.Empty);
    }

    private void FiltrarLista(string query)
    {
        string termo = (query ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(termo))
        {
            filteredEmployees = new List<EmployeeData>(allEmployees);
        }
        else
        {
            filteredEmployees = allEmployees.FindAll(emp =>
                Contem(emp.nome, termo) ||
                Contem(emp.matricula, termo) ||
                Contem(emp.email, termo) ||
                Contem(emp.departamento, termo) ||
                Contem(emp.cargo, termo) ||
                (emp.rg != null && Contem(emp.rg.numero, termo))
            );
        }

        RenderizarLista();

        if (filteredEmployees.Count > 0)
        {
            // Mantem selecionado se ainda estiver na lista filtrada, senao seleciona o primeiro
            if (selectedEmployee == null || !filteredEmployees.Contains(selectedEmployee))
            {
                SelecionarFuncionario(filteredEmployees[0]);
            }
            else
            {
                SelecionarFuncionario(selectedEmployee);
            }
        }
        else
        {
            ExibirVazio();
        }
    }

    private static bool Contem(string valor, string termo)
    {
        return !string.IsNullOrEmpty(valor) &&
               valor.IndexOf(termo, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void RenderizarLista()
    {
        if (containerLista != null)
        {
            for (int i = containerLista.childCount - 1; i >= 0; i--)
            {
                Transform child = containerLista.GetChild(i);
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }

        if (prefabItemLista == null)
        {
            Debug.LogError("[DatabaseWindow] prefabItemLista não está vinculado no Inspector!");
            return;
        }

        if (containerLista == null)
        {
            Debug.LogError("[DatabaseWindow] containerLista não está vinculado no Inspector!");
            return;
        }

        foreach (EmployeeData emp in filteredEmployees)
        {
            GameObject itemObj = Instantiate(prefabItemLista, containerLista);
            itemObj.SetActive(true);
            if (itemObj.TryGetComponent(out DatabaseItemListController itemCtrl))
            {
                itemCtrl.Setup(emp, SelecionarFuncionario);
            }
        }

        if (containerLista is RectTransform rt)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
    }

    public void SelecionarFuncionario(EmployeeData emp)
    {
        selectedEmployee = emp;

        if (emp == null)
        {
            ExibirVazio();
            return;
        }

        if (painelDetalhesConteudo != null)
            painelDetalhesConteudo.SetActive(true);

        if (painelVazio != null)
            painelVazio.SetActive(false);

        // Preenche campos corporativos
        if (txtNome != null) txtNome.text = emp.nome;
        if (txtMatricula != null) txtMatricula.text = $"Matrícula: {emp.matricula}";
        if (txtCargo != null) txtCargo.text = $"Cargo: {emp.cargo}";
        if (txtDepartamento != null) txtDepartamento.text = $"Departamento: {emp.departamento}";
        if (txtEmail != null) txtEmail.text = $"E-mail: {emp.email}";

        if (txtStatus != null)
        {
            string corHex = emp.status switch
            {
                "Ativo" => "#6BBE31",     // Verde
                "Férias" => "#FFBF4C",    // Amarelo
                "Demitido" => "#FF3333",  // Vermelho
                _ => "#FFFFFF"
            };
            txtStatus.text = $"<color={corHex}>Status: {emp.status.ToUpper()}</color>";
        }

        // Preenche dados do RG
        if (emp.rg != null)
        {
            if (txtRGNumero != null) txtRGNumero.text = $"RG: {emp.rg.numero}";
            if (txtRGExpedicao != null) txtRGExpedicao.text = $"Expedição: {emp.rg.dataExpedicao}";
            if (txtRGNascimento != null) txtRGNascimento.text = $"Nascimento: {emp.rg.dataNascimento}";
            if (txtRGLocalidade != null) txtRGLocalidade.text = $"Emissão: {emp.rg.naturalidade} - {emp.rg.estadoEmissor}";
        }
        else
        {
            if (txtRGNumero != null) txtRGNumero.text = "RG: Não cadastrado";
            if (txtRGExpedicao != null) txtRGExpedicao.text = "Expedição: -";
            if (txtRGNascimento != null) txtRGNascimento.text = "Nascimento: -";
            if (txtRGLocalidade != null) txtRGLocalidade.text = "Emissão: -";
        }

        if (painelDetalhesConteudo != null && painelDetalhesConteudo.TryGetComponent(out RectTransform pRt))
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(pRt);
        }

    }

    private void ExibirVazio()
    {
        selectedEmployee = null;
        if (painelDetalhesConteudo != null)
            painelDetalhesConteudo.SetActive(false);

        if (painelVazio != null)
            painelVazio.SetActive(true);
    }

}
