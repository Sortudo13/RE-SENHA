using UnityEngine;
using TMPro;

public class EmailItemUI : MonoBehaviour
{
    public TextMeshProUGUI txtAssunto;
    public TextMeshProUGUI txtRemetente;

    private EmailData dadosDoEmail;
    private EmailManager manager;

    // Configura os dados visuais deste item na lista
    public void Configurar(EmailData dados, EmailManager managerRef)
    {
        dadosDoEmail = dados;
        manager = managerRef;

        txtAssunto.text = dados.assunto;
        txtRemetente.text = dados.remetente;
    }

    // Método chamado quando o jogador clica neste e-mail da lista
    public void AoClicarNoEmail()
    {
        manager.ExibirEmailCompleto(dadosDoEmail);
    }
}