using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    [SerializeField]
    private Image fadeImage;

    [SerializeField]
    private float defaultDuration = 0.8f;

    public bool IsFading { get; private set; }

    private void Awake()
    {
        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0);
    }

    public void ResetFade()
    {
        StopAllCoroutines(); // para qualquer fade em andamento
        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0);
        IsFading = false;
    }

    public IEnumerator Fade(float startAlpha, float endAlpha, float duration = -1f)
    {
        if (fadeImage == null)
            yield break;

        if (duration <= 0f)
            duration = defaultDuration;

        IsFading = true;
        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
        IsFading = false;
    }
}
