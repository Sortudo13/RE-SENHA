using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CreditosController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField]
    private GameObject painelCreditos; // Canvas_Creditos

    [SerializeField]
    private RectTransform textoCreditos; // o texto que rola

    [SerializeField]
    private Button botaoVoltar;

    [Header("Configuração do Scroll")]
    [SerializeField]
    private float velocidade = 60f; // pixels por segundo

    [SerializeField]
    private float pausaFinal = 2f; // segundos parado no fim antes de fechar

    private Coroutine rotinaScroll;

    private void Awake()
    {
        if (botaoVoltar != null)
            botaoVoltar.onClick.AddListener(FecharCreditos);

        if (painelCreditos != null)
            painelCreditos.SetActive(false);
    }

    public void AbrirCreditos()
    {
        if (painelCreditos == null || textoCreditos == null)
            return;

        painelCreditos.SetActive(true);

        // Posiciona o texto logo abaixo da tela e começa a rolar
        float alturaTela = ((RectTransform)painelCreditos.transform).rect.height;
        textoCreditos.anchoredPosition = new Vector2(0f, -alturaTela);

        if (rotinaScroll != null)
            StopCoroutine(rotinaScroll);

        rotinaScroll = StartCoroutine(RolarCreditos(alturaTela));
    }

    public void FecharCreditos()
    {
        if (rotinaScroll != null)
        {
            StopCoroutine(rotinaScroll);
            rotinaScroll = null;
        }

        if (painelCreditos != null)
            painelCreditos.SetActive(false);
    }

    private IEnumerator RolarCreditos(float alturaTela)
    {
        // Sobe o texto até que ele saia inteiro pelo topo da tela
        float alturaTexto = textoCreditos.rect.height;
        float alvo = alturaTexto + alturaTela; // deslocamento total até sumir

        float percorrido = 0f;

        while (percorrido < alvo)
        {
            float delta = velocidade * Time.unscaledDeltaTime; // unscaled: funciona mesmo com Time.timeScale = 0
            percorrido += delta;
            textoCreditos.anchoredPosition += Vector2.up * delta;
            yield return null;
        }

        // Pausa no final e fecha automaticamente
        yield return new WaitForSecondsRealtime(pausaFinal);
        FecharCreditos();
    }
}
