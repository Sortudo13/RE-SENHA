using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEmail", menuName = "EmailData/NewEmail")]
public class EmailData: ScriptableObject
{
    [Header("Email")]
    public string sender;
    public string subject;
    [TextArea(5, 10)]
    public string body;               //TODO: Permitir a formatação do corpo do email, incluindo negrito, itálico, sublinhado, cores, links e imagens. (uso de tags HTML ou Markdown)
    public List<string> attachments;  //TODO: Permitir anexo de documentos, imagens ou links. Se não houver anexo, deixar em branco.

    [Header("Answer")]
    public EmailError error;
    public string reason;            //TODO: Adicionar opção para coletar o local da discrepância a fim de permitir a seleção durante a análise do email.
}

public enum EmailError
{
    None,
    GeneralMismatch,
    SenderMismatch,
    SubjectMismatch,
    BodyMismatch,
    AttachmentMismatch
}