using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailManager : MonoBehaviour
{
    [Header("Lista de E-mails do Dia")]
    public List<EmailData> caixaDeEntrada;

    [Header("Referências da Interface (Painel Direito)")]
    public TextMeshProUGUI txtRemetente;
    public TextMeshProUGUI txtAssunto;
    public TextMeshProUGUI txtCorpo;
    public TextMeshProUGUI txtAnexo;

    [Header("Painel de Avisos / Feedback")]
    public GameObject painelAvisos;
    public TextMeshProUGUI txtTituloAviso;
    public TextMeshProUGUI txtFeedback;

    [Header("Configuração da Lista (Painel Esquerdo)")]
    public Transform containerDaLista; // Onde os botões de e-mail serão criados
    public GameObject prefabItemEmail; // O modelo do botão de e-mail

    private EmailData emailAbertoAtual;

    void Start()
    {
        // Garante que o painel de avisos comece oculto
        if (painelAvisos != null)
        {
            painelAvisos.SetActive(false);
        }

        GerarListaDeEmails();
    }

    public void GerarListaDeEmails()
    {
        // Limpa botões antigos se houver
        foreach (Transform child in containerDaLista)
        {
            Destroy(child.gameObject);
        }

        // Instancia um botão para cada e-mail da lista
        foreach (EmailData email in caixaDeEntrada)
        {
            GameObject novoBotao = Instantiate(prefabItemEmail, containerDaLista);

            // Pega o script do botão e envia os dados
            EmailItemUI itemUI = novoBotao.GetComponent<EmailItemUI>();
            if (itemUI != null)
            {
                itemUI.ConfigurarItem(email, this);
            }
        }
    }

    public void ExibirEmailCompleto(EmailData email)
    {
        emailAbertoAtual = email;

        if (txtRemetente != null)
            txtRemetente.text = "<b>De:</b> " + email.remetente;
        if (txtAssunto != null)
            txtAssunto.text = "<b>Assunto:</b> " + email.assunto;
        if (txtCorpo != null)
            txtCorpo.text = email.corpoEmail;
        if (txtAnexo != null)
        {
            txtAnexo.text = string.IsNullOrEmpty(email.nomeAnexo)
                ? "Sem anexos"
                : "Anexo: " + email.nomeAnexo;
        }
    }

    // Chamado pelo botão "Marcar como Seguro"
    public void DecisaoMarcarSeguro()
    {
        ProcessarDecisao(false); // false = o jogador acha que NÃO é phishing
    }

    // Chamado pelo botão "Reportar Phishing"
    public void DecisaoReportarPhishing()
    {
        ProcessarDecisao(true); // true = o jogador acha que É phishing
    }

    private void ProcessarDecisao(bool jogadorMarcouPhishing)
    {
        if (emailAbertoAtual == null)
            return;

        // Compara a escolha do jogador com o atributo ehPhishing do EmailData
        if (jogadorMarcouPhishing == emailAbertoAtual.ehPhishing)
        {
            // Acertou!
            string mensagemSucesso = jogadorMarcouPhishing
                ? "Excelente! Ameaça de phishing neutralizada com sucesso."
                : "Correto! O e-mail foi validado como comunicação legítima.";

            ExibirFeedback(true, mensagemSucesso);
        }
        else
        {
            // Errou!
            string mensagemErro = jogadorMarcouPhishing
                ? "Falso Positivo! Você bloqueou uma comunicação legítima do sistema."
                : "ALERTA DE SEGURANÇA! "
                    + (
                        string.IsNullOrEmpty(emailAbertoAtual.explicacaoErro)
                            ? "Sua análise estava incorreta."
                            : emailAbertoAtual.explicacaoErro
                    );

            ExibirFeedback(false, mensagemErro);
        }
    }

    private void ExibirFeedback(bool acertou, string mensagem)
    {
        if (txtFeedback != null)
            txtFeedback.text = mensagem;

        if (txtTituloAviso != null)
        {
            if (acertou)
            {
                txtTituloAviso.text = "[ ANÁLISE CORRETA ]";
                txtTituloAviso.color = new Color(0f, 1f, 0.4f); // Verde Neon
            }
            else
            {
                txtTituloAviso.text = "[ ALERTA DE SEGURANÇA ]";
                txtTituloAviso.color = new Color(1f, 0.2f, 0.2f); // Vermelho Neon
            }
        }

        if (painelAvisos != null)
            painelAvisos.SetActive(true); // Exibe o popup centralizado
    }
}
