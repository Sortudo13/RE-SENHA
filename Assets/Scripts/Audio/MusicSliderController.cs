using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MusicSliderController : MonoBehaviour
{
    [Tooltip("Se vazio, usa o Slider do próprio GameObject.")]
    [SerializeField]
    private Slider musicSlider;

    [Tooltip("Chave do PlayerPrefs usada pelo AudioManager.")]
    [SerializeField]
    private string musicVolumePrefKey = "MusicVolume";

    [Tooltip("Volume inicial caso não exista valor salvo.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float defaultMusicVolume = 0.3f;

    private void Awake()
    {
        if (musicSlider == null)
            musicSlider = GetComponent<Slider>();

        musicSlider.minValue = 0f;
        musicSlider.maxValue = 1f;
        musicSlider.wholeNumbers = false;
    }

    private void OnEnable()
    {
        if (musicSlider == null)
            return;

        float volumeSalvo = PlayerPrefs.HasKey(musicVolumePrefKey)
            ? PlayerPrefs.GetFloat(musicVolumePrefKey)
            : defaultMusicVolume;

        musicSlider.SetValueWithoutNotify(Mathf.Clamp01(volumeSalvo));
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
    }

    private void OnDisable()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
    }

    private void OnMusicSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }
}
