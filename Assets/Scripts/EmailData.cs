using UnityEngine;

[CreateAssetMenu(fileName = "NovoEmail", menuName = "JogoCyber/Novo Email")]
public class EmailData : ScriptableObject
{
    public string remetente;
    public string assunto;
    [TextArea(5, 10)]
    public string corpoEmail;
    public string nomeAnexo;
    
    [Header("Gabarito de Segurança")]
    public bool ehPhishing; // Se verdadeiro, o jogador DEVE reportar
    public string explicacaoErro; // Ex: "O domínio do remetente falso empresa-segura.co em vez de empresa.com"
}