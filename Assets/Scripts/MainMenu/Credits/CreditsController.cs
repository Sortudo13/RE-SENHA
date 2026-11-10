using System.Collections;
using TMPro;
using UnityEngine;

public class CreditosController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField]
    private RectTransform textoCreditos; // RectTransform do TextMeshProUGUI

    [SerializeField]
    private GameObject canvasMainMenu; // Referência ao Canvas do Main Menu

    private TextMeshProUGUI _textoTMP;

    [Header("Configuração do Scroll")]
    [SerializeField]
    private float velocidade = 60f; // pixels por segundo

    [SerializeField]
    private float pausaFinal = 2f; // segundos parado no fim antes de fechar

    private Coroutine rotinaScroll;

    private void Awake()
    {
        // Pega o componente TMP no mesmo objeto do RectTransform
        _textoTMP = textoCreditos.GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        IniciarScroll();
    }

    private void OnDisable()
    {
        if (rotinaScroll != null)
        {
            StopCoroutine(rotinaScroll);
            rotinaScroll = null;
        }
    }

    private void IniciarScroll()
    {
        if (textoCreditos == null || _textoTMP == null)
            return;

        // Força o TMP a recalcular o layout para o preferredHeight ficar correto
        _textoTMP.ForceMeshUpdate();

        // Altura visível da tela (o próprio Canvas de créditos)
        float alturaTela = ((RectTransform)transform).rect.height;

        // Posiciona o texto logo abaixo da tela
        textoCreditos.anchoredPosition = new Vector2(0f, -alturaTela);

        if (rotinaScroll != null)
            StopCoroutine(rotinaScroll);

        rotinaScroll = StartCoroutine(RolarCreditos(alturaTela));
    }

    private IEnumerator RolarCreditos(float alturaTela)
    {
        // Altura real do texto (TMP)
        float alturaTexto = _textoTMP.preferredHeight;
        float alvo = alturaTexto + alturaTela; // deslocamento total até sumir

        float percorrido = 0f;

        while (percorrido < alvo)
        {
            float delta = velocidade * Time.unscaledDeltaTime;
            percorrido += delta;
            textoCreditos.anchoredPosition += Vector2.up * delta;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(pausaFinal);

        // Fecha o Canvas de créditos (este GameObject) e abre o Main Menu
        gameObject.SetActive(false);
        canvasMainMenu.SetActive(true);
    }
}
