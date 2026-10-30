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
    public TextMeshProUGUI txtEmailsAnalisados;

    // Função chamada pelo GameManager quando o jogo acaba
    public void ExibirGameOver(string motivo, int dias, int acertos, int erros)
    {
        painelGameOverPrincipal.SetActive(true);
        painelEstatisticas.SetActive(false);

        // Preenche os dados baseados no progresso do jogador
        txtMotivoDerrota.text = "Motivo da Demissão: " + motivo;
        txtDiasSobrevividos.text = "Dias no Cargo: " + dias;
        txtEmailsAnalisados.text = $"Emails Analisados: {acertos + erros}";
    }

    // Conectado ao botão "Reiniciar"
    public void ReiniciarJogo()
    {
        DataController.ResetDays();
        SceneManager.LoadScene("Desktop");
    }

    // Conectado ao botão "Estatísticas"
    public void MostrarEstatisticas()
    {
        // Esconde a tela principal de Game Over
        painelGameOverPrincipal.SetActive(false); 
        
        // Mostra a tela de Estatísticas no lugar
        painelEstatisticas.SetActive(true);
    }

    // Conectado ao botão "Voltar" dentro do painel de estatísticas
    public void EsconderEstatisticas()
    {
        // Esconde a tela de Estatísticas
        painelEstatisticas.SetActive(false);
        
        // Traz a tela principal de Game Over de volta
        painelGameOverPrincipal.SetActive(true);
    }
}
