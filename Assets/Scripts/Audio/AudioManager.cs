using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Configuração de Áudio")]
    [SerializeField]
    private AudioSource bgmAudioSource;

    [SerializeField]
    private AudioSource sfxAudioSource;

    [Header("Músicas de Fundo")]
    [SerializeField]
    private AudioClip mainMenuMusic;

    [Tooltip("Lista de músicas tocadas em sequência na cena Desktop.")]
    [SerializeField]
    private AudioClip[] desktopPlaylist;

    [SerializeField]
    private bool shufflePlaylist = false;

    [Header("Playlist")]
    [Tooltip("Tempo de silêncio entre uma música e outra (em segundos).")]
    [Range(0f, 30f)]
    [SerializeField]
    private float silenceBetweenTracks = 5f;

    [Tooltip("Duração do fade out no final de cada música (em segundos).")]
    [Range(0f, 5f)]
    [SerializeField]
    private float fadeOutDuration = 2f;

    [Header("Volume")]
    [Tooltip("Volume padrão da música de fundo (0 a 1).")]
    [Range(0f, 1f)]
    [SerializeField]
    private float defaultMusicVolume = 0.3f;

    [Tooltip("Volume padrão dos efeitos sonoros (0 a 1).")]
    [Range(0f, 1f)]
    [SerializeField]
    private float defaultSFXVolume = 1f;

    [Header("Efeitos Sonoros - Cliques e UI")]
    [SerializeField]
    private AudioClip cliqueFecharJanela;

    [SerializeField]
    private AudioClip cliqueAbrirJanela;

    [SerializeField]
    private AudioClip somMinimizarJanela;

    [SerializeField]
    private AudioClip cliqueBotaoPadrao;

    [SerializeField]
    private AudioClip somGameOver;

    [SerializeField]
    private AudioClip somGameWin;

    [SerializeField]
    private AudioClip cliqueBotaoAprovacao;

    // Controle da playlist
    private int _playlistIndex = -1;
    private bool _playlistActive = false;
    private Coroutine _playlistCoroutine;
    private float _bgmVolume;
    private float _sfxVolume;

    private const string PREF_MUSIC_VOLUME = "MusicVolume";
    private const string PREF_SFX_VOLUME = "SFXVolume";

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        GarantirAudioSources();
        CarregarVolumes();

        Debug.Log(
            $"[AudioManager] Awake: BGM Volume inicial={bgmAudioSource.volume}, " +
            $"SFX Volume inicial={sfxAudioSource.volume}");
    }

    private void Start()
    {
        string cenaAtual = SceneManager.GetActiveScene().name;

        if (EhCenaDeEncerramento(cenaAtual))
            return;

        if (cenaAtual == "MainMenu")
            TocarMusica(mainMenuMusic);
        else
            IniciarPlaylist();
    }

    private void Update()
    {
        // Clique do mouse (SFX global)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TocarSFX(cliqueBotaoPadrao);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AoCarregarCena;
        ActionController.SyncSignal("OnWindowClosed", TocarSomFecharJanela);
        ActionController.SyncSignal("OnWindowOpened", TocarSomAbrirJanela);
        ActionController.SyncSignal("OnWindowMinimized", TocarSomMinimizarJanela);
        ActionController.SyncSignal("OnButtonClicked", TocarSomBotao);
        ActionController.SyncSignal("OnApprovalButtonClicked", TocarSomAprovacao);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
        ActionController.UnsyncSignal("OnWindowClosed", TocarSomFecharJanela);
        ActionController.UnsyncSignal("OnWindowOpened", TocarSomAbrirJanela);
        ActionController.UnsyncSignal("OnWindowMinimized", TocarSomMinimizarJanela);
        ActionController.UnsyncSignal("OnButtonClicked", TocarSomBotao);
        ActionController.UnsyncSignal("OnApprovalButtonClicked", TocarSomAprovacao);
    }

    private void AoCarregarCena(Scene scene, LoadSceneMode mode)
    {
        if (EhCenaDeEncerramento(scene.name))
        {
            PararPlaylist();
            PararMusicaDeFundo();

            if (scene.name == "GameOver")
                TocarSFX(somGameOver);
            else if (scene.name == "GameWin")
                TocarSFX(somGameWin);
            return;
        }

        if (scene.name == "Desktop")
            IniciarPlaylist();
        else if (scene.name == "MainMenu")
        {
            PararPlaylist();
            TocarMusica(mainMenuMusic);
        }
    }

    private bool EhCenaDeEncerramento(string nomeCena) =>
        nomeCena == "GameOver" || nomeCena == "GameWin";

    // ============================================================
    // AudioSources e volume
    // ============================================================
    private void GarantirAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (bgmAudioSource == null)
            bgmAudioSource =
                sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        if (sfxAudioSource == null)
            sfxAudioSource =
                sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();

        bgmAudioSource.loop = false;
        bgmAudioSource.playOnAwake = false;

        sfxAudioSource.loop = false;
        sfxAudioSource.playOnAwake = false;
    }

    private void CarregarVolumes()
    {
        float musicVolume = PlayerPrefs.HasKey(PREF_MUSIC_VOLUME)
            ? PlayerPrefs.GetFloat(PREF_MUSIC_VOLUME)
            : defaultMusicVolume;
        float sfxVolume = PlayerPrefs.HasKey(PREF_SFX_VOLUME)
            ? PlayerPrefs.GetFloat(PREF_SFX_VOLUME)
            : defaultSFXVolume;

        musicVolume = ValidarVolume(musicVolume, defaultMusicVolume, PREF_MUSIC_VOLUME);
        sfxVolume = ValidarVolume(sfxVolume, defaultSFXVolume, PREF_SFX_VOLUME);

        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);

        Debug.Log(
            $"[AudioManager] Volumes carregados: BGM={_bgmVolume}, SFX={_sfxVolume}; " +
            $"AudioSources: BGM={bgmAudioSource.volume}, SFX={sfxAudioSource.volume}");
    }

    private static float ValidarVolume(float volume, float fallback, string prefKey)
    {
        if (float.IsNaN(volume) || float.IsInfinity(volume) || volume < 0f || volume > 1f)
        {
            Debug.LogWarning(
                $"[AudioManager] Valor invalido em {prefKey} ({volume}). Usando fallback {fallback}.");
            return Mathf.Clamp01(fallback);
        }

        return Mathf.Clamp01(volume);
    }

    public void SetMusicVolume(float volume)
    {
        volume = ValidarVolume(volume, defaultMusicVolume, PREF_MUSIC_VOLUME);
        _bgmVolume = volume;

        if (bgmAudioSource != null)
            bgmAudioSource.volume = volume;

        PlayerPrefs.SetFloat(PREF_MUSIC_VOLUME, volume);
        PlayerPrefs.Save();

        Debug.Log($"[AudioManager] SetMusicVolume({volume}) -> _bgmVolume={_bgmVolume}");
    }

    public void SetSFXVolume(float volume)
    {
        volume = ValidarVolume(volume, defaultSFXVolume, PREF_SFX_VOLUME);
        _sfxVolume = volume;

        if (sfxAudioSource != null)
            sfxAudioSource.volume = volume;

        PlayerPrefs.SetFloat(PREF_SFX_VOLUME, volume);
        PlayerPrefs.Save();

        Debug.Log($"[AudioManager] SetSFXVolume({volume}) -> _sfxVolume={_sfxVolume}");
    }

    // ============================================================
    // Playlist
    // ============================================================
    private void IniciarPlaylist()
    {
        if (desktopPlaylist == null || desktopPlaylist.Length == 0)
        {
            Debug.LogWarning("[AudioManager] Playlist da Desktop está vazia.");
            return;
        }

        if (_playlistActive && bgmAudioSource.isPlaying)
            return;

        _playlistActive = true;
        _playlistIndex = shufflePlaylist ? Random.Range(0, desktopPlaylist.Length) : 0;

        if (_playlistCoroutine != null)
            StopCoroutine(_playlistCoroutine);
        _playlistCoroutine = StartCoroutine(TocarPlaylistLoop());
    }

    private IEnumerator TocarPlaylistLoop()
    {
        while (_playlistActive)
        {
            if (desktopPlaylist.Length == 0)
                yield break;

            AudioClip clip = desktopPlaylist[_playlistIndex];
            if (clip == null)
            {
                _playlistIndex = ProximoIndice();
                continue;
            }

            // Garante o volume correto antes de tocar
            bgmAudioSource.volume = _bgmVolume;
            bgmAudioSource.clip = clip;
            bgmAudioSource.loop = false;
            bgmAudioSource.Play();
            Debug.Log(
                $"[AudioManager] Tocando clip: {clip.name}, volume={bgmAudioSource.volume}");

            // Tempo antes do fade out começar
            float tempoSemFade = Mathf.Max(0f, clip.length - fadeOutDuration);
            yield return new WaitForSeconds(tempoSemFade);

            // Fade out (usa unscaledDeltaTime para não ser afetado por Time.timeScale)
            float tempoPassado = 0f;
            while (tempoPassado < fadeOutDuration)
            {
                tempoPassado += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(tempoPassado / fadeOutDuration);
                bgmAudioSource.volume = Mathf.Lerp(_bgmVolume, 0f, t);
                yield return null;
            }

            bgmAudioSource.Stop();
            bgmAudioSource.volume = _bgmVolume;
            Debug.Log(
                $"[AudioManager] Fade concluido e volume restaurado: {_bgmVolume}");

            // Silêncio entre músicas
            if (silenceBetweenTracks > 0f)
                yield return new WaitForSeconds(silenceBetweenTracks);

            _playlistIndex = ProximoIndice();
        }
    }

    private int ProximoIndice()
    {
        if (shufflePlaylist)
        {
            if (desktopPlaylist.Length == 1)
                return 0;
            int novo;
            do
            {
                novo = Random.Range(0, desktopPlaylist.Length);
            } while (novo == _playlistIndex);
            return novo;
        }
        return (_playlistIndex + 1) % desktopPlaylist.Length;
    }

    private void PararPlaylist()
    {
        _playlistActive = false;
        if (_playlistCoroutine != null)
        {
            StopCoroutine(_playlistCoroutine);
            _playlistCoroutine = null;
        }

        if (bgmAudioSource != null)
            bgmAudioSource.volume = _bgmVolume;
    }

    // ============================================================
    // Música avulsa (menu principal, etc.)
    // ============================================================
    public void TocarMusica(AudioClip musica)
    {
        if (musica == null || bgmAudioSource == null)
            return;

        PararPlaylist();

        if (bgmAudioSource.isPlaying && bgmAudioSource.clip == musica)
            return;

        bgmAudioSource.volume = _bgmVolume;
        bgmAudioSource.clip = musica;
        bgmAudioSource.loop = true;
        bgmAudioSource.Play();
        Debug.Log(
            $"[AudioManager] Tocando musica avulsa: {musica.name}, volume={bgmAudioSource.volume}");
    }

    public void PararMusicaDeFundo()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
            bgmAudioSource.Stop();
    }

    // ============================================================
    // SFX
    // ============================================================
    public void TocarSFX(AudioClip clip)
    {
        if (clip != null && sfxAudioSource != null)
        {
            sfxAudioSource.volume = _sfxVolume;
            sfxAudioSource.PlayOneShot(clip, 1f);
            Debug.Log(
                $"[AudioManager] TocarSFX({clip.name}), volume={sfxAudioSource.volume}");
        }
    }

    private void TocarSomFecharJanela() => TocarSFX(cliqueFecharJanela);

    private void TocarSomAbrirJanela() => TocarSFX(cliqueAbrirJanela);

    private void TocarSomMinimizarJanela() => TocarSFX(somMinimizarJanela);

    private void TocarSomBotao() => TocarSFX(cliqueBotaoPadrao);

    private void TocarSomAprovacao() => TocarSFX(cliqueBotaoAprovacao);
}
