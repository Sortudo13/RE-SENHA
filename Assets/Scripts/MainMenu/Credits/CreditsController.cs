using System.Collections;
using TMPro;
using UnityEngine;

public class CreditsController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField]
    private RectTransform creditsText;

    [SerializeField]
    private GameObject mainMenuCanvas;

    [SerializeField]
    private FadeController fadeController;

    private TextMeshProUGUI _textTMP;

    [Header("Configuração do Scroll")]
    [SerializeField]
    private float speed = 60f;

    [SerializeField]
    private float finalPause = 2f;

    [SerializeField]
    private float fadeDuration = 0.8f;

    [SerializeField]
    private float startOffset = 150f;

    [SerializeField]
    private float centerOffset = 0f;

    private Coroutine scrollRoutine;
    private bool _isClosing;

    private void Awake()
    {
        _textTMP = creditsText.GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        _isClosing = false;

        if (fadeController != null)
            StartCoroutine(fadeController.Fade(1f, 0f, fadeDuration));

        StartScroll();
    }

    private void OnDisable()
    {
        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }
    }

    private void StartScroll()
    {
        if (creditsText == null || _textTMP == null)
            return;

        _textTMP.ForceMeshUpdate();

        creditsText.anchoredPosition = new Vector2(0f, -startOffset);

        if (scrollRoutine != null)
            StopCoroutine(scrollRoutine);

        scrollRoutine = StartCoroutine(ScrollCredits());
    }

    private IEnumerator ScrollCredits()
    {
        float textHeight = _textTMP.preferredHeight;
        float target = startOffset + textHeight + centerOffset;
        float traveled = 0f;

        while (traveled < target)
        {
            float delta = speed * Time.unscaledDeltaTime;
            traveled = Mathf.Min(traveled + delta, target);
            creditsText.anchoredPosition += Vector2.up * delta;
            yield return null;
        }

        creditsText.anchoredPosition = new Vector2(0f, target - startOffset);

        yield return new WaitForSecondsRealtime(finalPause);

        // Fim natural do scroll → usa o fade
        CloseCreditsWithFade();
    }

    // Botão Voltar chama este método → volta instantaneamente
    public void CloseCredits()
    {
        if (_isClosing)
            return;

        _isClosing = true;

        // Para o scroll imediatamente
        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }

        // Garante que a tela não fique coberta por um fade residual
        if (fadeController != null)
            fadeController.ResetFade();

        // Ativa o menu e desativa o Credits na hora
        if (mainMenuCanvas != null && !mainMenuCanvas.activeSelf)
            mainMenuCanvas.SetActive(true);

        gameObject.SetActive(false);
    }

    // Fim do scroll → fade out → menu → fade in
    private void CloseCreditsWithFade()
    {
        if (_isClosing)
            return;

        _isClosing = true;

        StartCoroutine(FadeOutAndReturnToMenu());
    }

    private IEnumerator FadeOutAndReturnToMenu()
    {
        if (fadeController != null)
            yield return fadeController.StartCoroutine(fadeController.Fade(0f, 1f, fadeDuration));

        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }

        if (mainMenuCanvas != null && !mainMenuCanvas.activeSelf)
            mainMenuCanvas.SetActive(true);

        if (fadeController != null)
            yield return fadeController.StartCoroutine(fadeController.Fade(1f, 0f, fadeDuration));

        gameObject.SetActive(false);
    }
}
