using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject painelGameOverPrincipal;
    public GameObject painelEstatisticas;

    [Header("Textos de Estatísticas")]
    public TextMeshProUGUI txtMotivoDerrota;
    public TextMeshProUGUI txtDiasSobrevividos;
    public TextMeshProUGUI txtAcertosErros;

    // Função chamada pelo GameManager quando o jogo acaba
    public void ExibirGameOver(string motivo, int dias, int acertos, int erros)
    {
        painelGameOverPrincipal.SetActive(true);
        painelEstatisticas.SetActive(false);

        // Preenche os dados baseados no progresso do jogador
        txtMotivoDerrota.text = "Motivo da Demissão: " + motivo;
        txtDiasSobrevividos.text = "Dias no Cargo: " + dias;
        txtAcertosErros.text = $"Decisões Corretas: {acertos} | Erros: {erros}";
    }

    // Conectado ao botão "Reiniciar"
    public void ReiniciarJogo()
    {
        // Alterar para recarregar a cena principal
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Conectado ao botão "Estatísticas"
    public void MostrarEstatisticas()
    {
        painelEstatisticas.SetActive(true);
    }

    // Opcional: Botão de "Voltar" dentro do painel de estatísticas
    public void EsconderEstatisticas()
    {
        painelEstatisticas.SetActive(false);
    }
}
