using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManualWindowController : MonoBehaviour
{
    [Header("Dados do Manual")]
    public ManualData bancoDeRegras;
    public int nivelAtual = 1;

    [Header("Referências - Painel Esquerdo")]
    public Transform containerRegras;
    public GameObject prefabItemRegra;

    [Header("Referências - Painel Direito")]
    public TextMeshProUGUI txtTituloRegra;
    public TextMeshProUGUI txtDescricaoRegra;
    public TextMeshProUGUI txtStatusVigor;

    void Start()
    {
        CarregarRegras();
    }

    public void CarregarRegras()
    {
        if (bancoDeRegras == null) return;

        // Limpa instâncias anteriores do container
        foreach (Transform child in containerRegras)
        {
            Destroy(child.gameObject);
        }

        List<RegraData> regrasAtivas = bancoDeRegras.ObterRegrasAtivas(nivelAtual);

        foreach (RegraData regra in regrasAtivas)
        {
            GameObject novoItem = Instantiate(prefabItemRegra, containerRegras);
            
            TextMeshProUGUI txtItem = novoItem.GetComponentInChildren<TextMeshProUGUI>();
            if (txtItem != null)
            {
                txtItem.text = $"{regra.idRegra}: {regra.titulo}";
            }

            RegraData regraAtual = regra;
            novoItem.GetComponent<Button>().onClick.AddListener(() => ExibirDetalhes(regraAtual));
        }

        // Exibe a primeira regra por padrão
        if (regrasAtivas.Count > 0)
        {
            ExibirDetalhes(regrasAtivas[0]);
        }
    }

    public void ExibirDetalhes(RegraData regra)
    {
        if (txtTituloRegra != null) 
            txtTituloRegra.text = regra.titulo;

        if (txtDescricaoRegra != null) 
            txtDescricaoRegra.text = regra.descricao;

        if (txtStatusVigor != null) 
            txtStatusVigor.text = $"<color=#00FF66>● EM VIGOR (Nível {regra.nivelIntroducao})</color>";
    }
}