using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EmailManager : MonoBehaviour
{
    [Header("Lista de E-mails do Dia")]
    public List<EmailData> caixaDeEntrada;

    [Header("Referências da Interface (Painel Direito)")]
    public TextMeshProUGUI txtRemetente;
    public TextMeshProUGUI txtAssunto;
    public TextMeshProUGUI txtCorpo;
    public TextMeshProUGUI txtAnexo;
    public GameObject painelAvisos;
    public TextMeshProUGUI txtFeedback;

    [Header("Configuração da Lista (Painel Esquerdo)")]
    public Transform containerDaLista; // Onde os botões de e-mail serão criados
    public GameObject prefabItemEmail; // O modelo do botão de e-mail

    private EmailData emailAbertoAtual;

    void Start()
    {
        GerarListaDeEmails();
    }

    void GerarListaDeEmails()
    {
        foreach (EmailData email in caixaDeEntrada)
        {
            // Cria um novo botão na lista da esquerda
            GameObject novoBotao = Instantiate(prefabItemEmail, containerDaLista);
            novoBotao.GetComponent<EmailItemUI>().Configurar(email, this);
        }
    }

    public void ExibirEmailCompleto(EmailData email)
    {
        emailAbertoAtual = email;

        txtRemetente.text = "<b>De:</b> " + email.remetente;
        txtAssunto.text = "<b>Assunto:</b> " + email.assunto;
        txtCorpo.text = email.corpoEmail;
        txtAnexo.text = string.IsNullOrEmpty(email.nomeAnexo) ? "Sem anexos" : "Anexo: " + email.nomeAnexo;
    }

    // Conectar ao botão "Marcar como Seguro"
    public void DecisaoMarcarSeguro()
    {
        if (emailAbertoAtual == null) return;

        if (!emailAbertoAtual.ehPhishing)
        {
            MostrarFeedback("<color=green>Correto! O e-mail era legítimo.</color>");
        }
        else
        {
            MostrarFeedback("<color=red>ALERTA DE SEGURANÇA! " + emailAbertoAtual.explicacaoErro + "</color>");
        }
    }

    // Conectar ao botão "Reportar Phishing / Suspeito"
    public void DecisaoReportarPhishing()
    {
        if (emailAbertoAtual == null) return;

        if (emailAbertoAtual.ehPhishing)
        {
            MostrarFeedback("<color=green>Excelente! Ameaça neutralizada com sucesso.</color>");
        }
        else
        {
            MostrarFeedback("<color=orange>Falso Positivo! Você bloqueou uma comunicação legítima.</color>");
        }
    }

    void MostrarFeedback(string mensagem)
    {
        painelAvisos.SetActive(true);
        txtFeedback.text = mensagem;
    }
}