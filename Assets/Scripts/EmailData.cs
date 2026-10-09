using UnityEngine;

[CreateAssetMenu(fileName = "NovoEmail", menuName = "EmailData/New Email")]
public class EmailData : ScriptableObject
{
    [Header("Email")]
    public string remetente;
    public string assunto;
    [TextArea(5, 10)]
    public string corpoEmail;       //TODO: Permitir a formatação do corpo do email, incluindo negrito, itálico, sublinhado, cores, links e imagens. (uso de tags HTML ou Markdown)
    public string nomeAnexo;        //TODO: Permitir anexo de documentos, imagens ou links. Se não houver anexo, deixar em branco.

    [Header("Gabarito de Segurança")]
    public bool ehPhishing;
    public string explicacaoErro;   //TODO: Adicionar opção para coletar o local da discrepância a fim de permitir a seleção durante a análise do email.
}