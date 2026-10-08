using UnityEngine;
using TMPro; // Ou UnityEngine.UI se usar Text tradicional
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EmailItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtAssunto;
    private EmailData dadosDoEmail;
    private EmailManager manager;

    // Esta função é chamada pelo EmailManager na hora do Instantiate
    public void ConfigurarItem(EmailData email, EmailManager emailManager)
    {
        dadosDoEmail = email;
        manager = emailManager;

        // Atualiza o texto do botão com o assunto do e-mail
        if (txtAssunto != null)
        {
            txtAssunto.text = email.assunto; 
        }

        // Adiciona a escuta do clique via código (evita ter que configurar no Inspector)
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(AoClicar);
    }

    private void AoClicar()
    {
        // Envia os dados deste e-mail para o painel de leitura
        if (manager != null)
        {
            manager.ExibirEmailCompleto(dadosDoEmail);
        }
    }
}