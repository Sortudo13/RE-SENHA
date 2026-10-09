using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailController : MonoBehaviour
{
    [Header("Lista de E-mails do Dia")]
    [SerializeField] private List<DataEmail> caixaDeEntrada;

    [Header("Referências da Interface (Painel Direito)")]
    [SerializeField] private GameObject painelLeitura;
    private TextMeshProUGUI txtRemetente;
    private TextMeshProUGUI txtAssunto;
    private TextMeshProUGUI txtCorpo;
    private TextMeshProUGUI txtAnexo;

    [Header("Painel de Avisos / Feedback")]
    [SerializeField] private GameObject painelAvisos;
    private TextMeshProUGUI txtTituloAviso;
    private TextMeshProUGUI txtFeedback;

    [Header("Configuração da Lista (Painel Esquerdo)")]
    [SerializeField] private Transform containerDaLista;
    [SerializeField] private GameObject prefabItemEmail;
    private DataEmail emailAbertoAtual;

    [Header("Progresso do Jogador")]
    [SerializeField] private int advertenciasMaximas = 3;
    private int advertenciasAtuais = 0;

    void Start()
    {
        if (painelAvisos != null)
        {
            painelAvisos.SetActive(false);
            txtTituloAviso = painelAvisos.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            txtFeedback = painelAvisos.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        }

        if (painelLeitura != null)
        {
            painelLeitura.SetActive(true);
            txtRemetente = painelLeitura.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            txtAssunto = painelLeitura.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            txtCorpo = painelLeitura.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            txtAnexo = painelLeitura.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
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
        foreach (DataEmail email in caixaDeEntrada)
        {
            GameObject newButton = Instantiate(prefabItemEmail, containerDaLista);

            GameObject newText = new("Texto");
            newText.transform.SetParent(newButton.transform, false);

            TextMeshProUGUI text = newText.AddComponent<TextMeshProUGUI>();
            text.text = email.assunto;
            text.fontSize = 20;
            text.color = Color.black; // Altere para a cor desejada
            text.alignment = TextAlignmentOptions.Center;

            RectTransform rect = newText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0); // Âncora inferior esquerda
            rect.anchorMax = new Vector2(1, 1); // Âncora superior direita
            rect.offsetMin = Vector2.zero;      // Zera Left e Bottom
            rect.offsetMax = Vector2.zero;      // Zera Right e Top

            if (newButton.TryGetComponent(out Button botao))        
            {
                botao.onClick.RemoveAllListeners();
                botao.onClick.AddListener(() => ExibirEmailCompleto(email));
            }
        }
    }

    public void ExibirEmailCompleto(DataEmail email)
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

        if (jogadorMarcouPhishing == emailAbertoAtual.ehPhishing)
        {
            ExibirFeedback(true, "Decisão Correta! A ameaça foi neutralizada.");
        }
        else
        {
            advertenciasAtuais++;

            if (advertenciasAtuais >= advertenciasMaximas)
            {
                ExibirFeedback(
                    false,
                    $"ALERTA CRÍTICO: Você acumulou {advertenciasAtuais} advertências. VOCÊ FOI DEMITIDO!"
                );
            }
            else
            {
                ExibirFeedback(
                    false,
                    $"Erro de Análise! Advertência {advertenciasAtuais}/{advertenciasMaximas} registrada."
                );
            }
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

    public void FecharPainelAvisosEAvancar()
    {
        if (painelAvisos != null)
            painelAvisos.SetActive(false);

        // Remove o e-mail processado da lista
        if (emailAbertoAtual != null && caixaDeEntrada.Contains(emailAbertoAtual))
        {
            caixaDeEntrada.Remove(emailAbertoAtual);
        }

        emailAbertoAtual = null;

        // Regera a lista de botões à esquerda
        GerarListaDeEmails();

        // Limpa a tela de leitura
        if (txtRemetente != null)
            txtRemetente.text = "";
        if (txtAssunto != null)
            txtAssunto.text = "";
        if (txtCorpo != null)
            txtCorpo.text = "";
        if (txtAnexo != null)
            txtAnexo.text = "";
    }
}
