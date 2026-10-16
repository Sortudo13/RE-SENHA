using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ManualManager : MonoBehaviour
{
    [Header("Dados das Regras")]
    public ManualData bancoDeRegras;
    public int diaAtualDoJogo = 1;

    [Header("Componentes de Interface")]
    public GameObject janelaManual;
    public Transform containerListaRegras;
    public GameObject prefabItemRegra;

    [Header("Painel de Detalhes (Lado Direito)")]
    public TextMeshProUGUI txtTituloRegra;
    public TextMeshProUGUI txtDescricaoRegra;
    public TextMeshProUGUI txtStatusVigor;

    void Start()
    {
        // Começa fechado na área de trabalho
        if (janelaManual != null)
            janelaManual.SetActive(false);
            
        CarregarRegrasDoDia();
    }

    // Chamado quando o jogador clica no ícone da Área de Trabalho
    public void AbrirJanelaManual()
    {
        janelaManual.SetActive(true);
    }

    // Chamado pelo botão [X] fechar da janela
    public void FecharJanelaManual()
    {
        janelaManual.SetActive(false);
    }

    public void CarregarRegrasDoDia()
    {
        // Limpa lista anterior
        foreach (Transform child in containerListaRegras)
        {
            Destroy(child.gameObject);
        }

        List<RegraData> regrasAtivas = bancoDeRegras.ObterRegrasAtivas(diaAtualDoJogo);

        foreach (RegraData regra in regrasAtivas)
        {
            GameObject novoBotao = Instantiate(prefabItemRegra, containerListaRegras);
            
            // Configura o texto do botão
            TextMeshProUGUI txtBotao = novoBotao.GetComponentInChildren<TextMeshProUGUI>();
            if (txtBotao != null)
                txtBotao.text = $"{regra.idRegra}: {regra.titulo}";

            // Adiciona evento de clique para exibir os detalhes
            RegraData regraCapturada = regra; // Evita problema de closure em loops
            novoBotao.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ExibirDetalhesRegra(regraCapturada));
        }

        if (regrasAtivas.Count > 0)
        {
            ExibirDetalhesRegra(regrasAtivas[0]);
        }
    }

    public void ExibirDetalhesRegra(RegraData regra)
    {
        txtTituloRegra.text = regra.titulo;
        txtDescricaoRegra.text = regra.descricao;
        txtStatusVigor.text = $"<color=green>● EM VIGOR (Nível {regra.nivelIntroducao})</color>";
    }
}