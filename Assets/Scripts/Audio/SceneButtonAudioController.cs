using UnityEngine;
using UnityEngine.UI;

public class SceneButtonAudioController : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip sceneOpenSound;

    private AudioSource fallbackAudioSource;
    private Button[] buttons;

    private void Awake()
    {
        if (AudioManager.Instance == null)
            PlaySound(sceneOpenSound);

        buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button button in buttons)
            button.onClick.AddListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        PlaySound(clickSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.TocarSFX(clip);
            return;
        }

        if (fallbackAudioSource == null)
        {
            fallbackAudioSource = gameObject.AddComponent<AudioSource>();
            fallbackAudioSource.playOnAwake = false;
        }

        fallbackAudioSource.PlayOneShot(clip);
    }

    private void OnDestroy()
    {
        if (buttons == null)
            return;

        foreach (Button button in buttons)
        {
            if (button != null)
                button.onClick.RemoveListener(PlayClickSound);
        }
    }
}
