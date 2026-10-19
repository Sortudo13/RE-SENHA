using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Configuração de Áudio")]
    [SerializeField] private AudioSource bgmAudioSource; // Áudio para música de fundo
    [SerializeField] private AudioSource sfxAudioSource; // Áudio para efeitos sonoros (SFX)

    [Header("Música de Fundo (BGM)")]
    [SerializeField] private AudioClip musicaDeFundo;

    [Header("Efeitos Sonoros - Cliques e UI")]
    [SerializeField] private AudioClip cliqueFecharJanela;
    [SerializeField] private AudioClip cliqueAbrirJanela;
    [SerializeField] private AudioClip cliqueBotaoPadrao;

    private void Awake()
    {
        // Padrão Singleton para ter apenas um AudioManager na cena
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

        // Garante que existam componentes AudioSource configurados
        GarantirAudioSources();
    }

    private void Start()
    {
        // Toca a música de fundo ao iniciar o jogo
        TocarMusicaDeFundo();
    }

    private void Update()
    {
        // 0 representa o Botão Esquerdo do Mouse
        if (Input.GetMouseButtonDown(0))
        {
            TocarSFX(cliqueBotaoPadrao);
        }
    }

    private void OnEnable()
    {
        // Sincroniza com os eventos do ActionController
        ActionController.SyncSignal("OnWindowClosed", TocarSomFecharJanela);
        ActionController.SyncSignal("OnWindowOpened", TocarSomAbrirJanela);
        ActionController.SyncSignal("OnButtonClicked", TocarSomBotao);
    }

    private void OnDisable()
    {
        // Desfaz a inscrição nos eventos do ActionController ao desativar
        ActionController.UnsyncSignal("OnWindowClosed", TocarSomFecharJanela);
        ActionController.UnsyncSignal("OnWindowOpened", TocarSomAbrirJanela);
        ActionController.UnsyncSignal("OnButtonClicked", TocarSomBotao);
    }

    private void GarantirAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        
        if (bgmAudioSource == null)
        {
            bgmAudioSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        }

        if (sfxAudioSource == null)
        {
            sfxAudioSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();
        }

        // Configurações padrão para BGM
        bgmAudioSource.loop = true;
        bgmAudioSource.playOnAwake = false;

        // Configurações padrão para SFX
        sfxAudioSource.loop = false;
        sfxAudioSource.playOnAwake = false;
    }

    public void TocarMusicaDeFundo()
    {
        if (musicaDeFundo != null && bgmAudioSource != null)
        {
            bgmAudioSource.clip = musicaDeFundo;
            bgmAudioSource.Play();
        }
    }

    public void TocarSFX(AudioClip clip)
    {
        if (clip != null && sfxAudioSource != null)
        {
            // PlayOneShot permite sobrepor múltiplos efeitos sem cortar o som anterior
            sfxAudioSource.PlayOneShot(clip);
        }
    }

    // Handlers para os eventos acionados pelo ActionController
    private void TocarSomFecharJanela() => TocarSFX(cliqueFecharJanela);
    private void TocarSomAbrirJanela() => TocarSFX(cliqueAbrirJanela);
    private void TocarSomBotao() => TocarSFX(cliqueBotaoPadrao);
}