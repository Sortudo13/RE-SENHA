using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Tela de loading (fade + blocos andando no trilho + mensagem) para troca de cena ou qualquer trabalho.
//   LoadingScreen.LoadScene("Desktop");                     troca de cena
//   LoadingScreen.Run(DataController.AdvanceDay, "Dia 02");  roda um codigo com a tela na frente
//   LoadingScreen.Run(MinhaCoroutine(), "Gerando...");       trabalho que leva varios frames
// Em botao: crie um metodo no seu controller que chame uma das funcoes acima e ligue no OnClick.
// Se o codigo rodado pelo Run pedir uma cena (ex.: AdvanceDay no ultimo dia -> GameWin),
// a troca acontece na mesma tela, sem piscar.
//
// Nao precisa colocar nada na cena: na primeira chamada o prefab Resources/LoadingScreen e
// instanciado e sobrevive as trocas de cena. O visual e editado direto nesse prefab.
// "marquee" e o grupo de blocos; ele anda dentro do pai (o trilho), que recorta com RectMask2D.
// O fundo cobrindo a tela (Raycast Target ligado) bloqueia cliques durante o loading.
[RequireComponent(typeof(CanvasGroup))]
public class LoadingScreen : MonoBehaviour
{
    private const string PrefabPath = "LoadingScreen";

    private static LoadingScreen instance;

    // Enquanto true, novos pedidos sao ignorados (clique duplo, GameOver chamado duas vezes...).
    public static bool IsLoading { get; private set; }

    [SerializeField]
    private CanvasGroup group;

    [SerializeField]
    private TMP_Text messageText;

    // Mensagem usada quando a chamada nao passa uma.
    [SerializeField]
    private string defaultMessage = "Carregando...";

    [Header("Blocos")]
    [SerializeField]
    private RectTransform marquee;

    // Pixels por segundo que os blocos andam no trilho.
    [SerializeField, Min(1f)]
    private float marqueeSpeed = 180f;

    // Anda em saltos deste tamanho (pixels), como no XP. 0 = movimento suave.
    [SerializeField, Min(0f)]
    private float marqueeStep = 0f;

    [Header("Tempos")]
    [SerializeField, Min(0f)]
    private float fadeSeconds = 0.25f;

    // Tempo minimo com a tela visivel; trocas leves (Desktop, avancar dia) acabam em um frame
    // e a tela so piscaria.
    [SerializeField, Min(0f)]
    private float minSeconds = 1.5f;

    private bool workRunning;
    private string pendingScene;
    private float marqueeOffset;
    private float progress;
    private float elapsed;

    // Com o "Enter Play Mode" sem domain reload, os campos estaticos sobrevivem entre um Play e outro.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        IsLoading = false;
    }

    public static void LoadScene(string sceneName, string message = null)
    {
        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"LoadingScreen: a cena '{sceneName}' nao existe ou nao esta no Build Settings."
            );
            return;
        }

        // Pedido feito de dentro de um Run: a cena carrega depois do trabalho, na mesma tela.
        if (IsLoading && instance != null && instance.workRunning)
        {
            instance.pendingScene = sceneName;
            return;
        }

        Begin(null, sceneName, message);
    }

    public static void Run(Action work, string message = null)
    {
        if (work != null)
            Begin(FromAction(work), null, message);
    }

    public static void Run(IEnumerator work, string message = null)
    {
        if (work != null)
            Begin(work, null, message);
    }

    private static void Begin(IEnumerator work, string sceneName, string message)
    {
        if (IsLoading)
        {
            Debug.LogWarning("LoadingScreen: pedido ignorado, ja existe um loading em andamento.");
            return;
        }

        if (instance == null)
            instance = CreateInstance();

        IsLoading = true;
        instance.StartCoroutine(instance.Sequence(work, sceneName, message));
    }

    private static LoadingScreen CreateInstance()
    {
        LoadingScreen prefab = Resources.Load<LoadingScreen>(PrefabPath);
        LoadingScreen created;
        if (prefab != null)
        {
            created = Instantiate(prefab);
            created.name = prefab.name;
        }
        else
        {
            // Sem o prefab o jogo nao trava: tudo funciona, so que sem nada na tela.
            Debug.LogError(
                $"LoadingScreen: prefab Resources/{PrefabPath} (com o componente LoadingScreen) nao encontrado."
            );
            created = new GameObject("LoadingScreen (sem prefab)").AddComponent<LoadingScreen>();
        }

        DontDestroyOnLoad(created.gameObject);
        return created;
    }

    private void Awake()
    {
        if (group == null)
            group = GetComponent<CanvasGroup>();

        SetVisible(0f);
    }

    // unscaledDeltaTime: anima mesmo com o jogo pausado (Time.timeScale = 0).
    private void Update()
    {
        if (marquee == null || group.alpha <= 0f || !(marquee.parent is RectTransform track))
            return;

        float blocksWidth = marquee.rect.width;
        float span = track.rect.width + blocksWidth;
        if (span <= 0f)
            return;

        // Entra pela esquerda, sai pela direita e recomeca.
        marqueeOffset = (marqueeOffset + marqueeSpeed * Time.unscaledDeltaTime) % span;
        float x = marqueeOffset - blocksWidth;
        if (marqueeStep > 0f)
            x = Mathf.Floor(x / marqueeStep) * marqueeStep;

        marquee.anchoredPosition = new Vector2(x, marquee.anchoredPosition.y);
    }

    private IEnumerator Sequence(IEnumerator work, string sceneName, string message)
    {
        try
        {
            if (messageText != null)
                messageText.text = string.IsNullOrWhiteSpace(message) ? defaultMessage : message;
            progress = 0f;
            elapsed = 0f;
            marqueeOffset = 0f;
            yield return Fade(0f, 1f);

            if (work != null)
            {
                workRunning = true;
                StartCoroutine(RunGuarded(work));
                while (workRunning)
                {
                    StepProgress(0.9f);
                    yield return null;
                }
            }

            if (pendingScene != null)
            {
                sceneName = pendingScene;
                pendingScene = null;
            }

            if (sceneName != null)
                yield return LoadSceneSteps(sceneName);
            else
                while (progress < 1f)
                {
                    StepProgress(1f);
                    yield return null;
                }

            yield return Fade(1f, 0f);
        }
        finally
        {
            IsLoading = false;
            workRunning = false;
            pendingScene = null;
        }
    }

    // Uma excecao no trabalho e logada e o loading termina normalmente, em vez de travar a tela.
    private IEnumerator RunGuarded(IEnumerator work)
    {
        try
        {
            while (true)
            {
                object current;
                bool hasNext;
                try
                {
                    hasNext = work.MoveNext();
                    current = hasNext ? work.Current : null;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    hasNext = false;
                    current = null;
                }

                if (!hasNext)
                    break;
                yield return current;
            }
        }
        finally
        {
            workRunning = false;
        }
    }

    private IEnumerator LoadSceneSteps(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null)
        {
            Debug.LogError($"LoadingScreen: nao foi possivel carregar a cena '{sceneName}'.");
            yield break;
        }

        // Segura a ativacao: o progress para em 0.9 ate liberarmos.
        op.allowSceneActivation = false;

        // Se ja houve um trabalho antes, a cena ocupa o resto do progresso.
        float start = progress;
        while (progress < 1f)
        {
            StepProgress(start + (1f - start) * Mathf.Clamp01(op.progress / 0.9f));
            yield return null;
        }

        op.allowSceneActivation = true;
        yield return new WaitUntil(() => op.isDone);
        Debug.Log($"LoadingScreen: cena '{sceneName}' carregada.");
    }

    // Progresso interno (nao aparece na tela): segura a tela ate o trabalho acabar
    // e ate passar minSeconds. Nunca volta e nunca passa do alvo.
    private void StepProgress(float target)
    {
        elapsed += Time.unscaledDeltaTime;
        float timeCap = minSeconds > 0f ? elapsed / minSeconds : 1f;
        progress = Mathf.Max(progress, Mathf.Min(target, timeCap));
    }

    private IEnumerator Fade(float from, float to)
    {
        // Bloqueia cliques durante todo o loading, inclusive no fade.
        group.blocksRaycasts = true;
        for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, t / fadeSeconds);
            yield return null;
        }
        SetVisible(to);
    }

    private void SetVisible(float alpha)
    {
        group.alpha = alpha;
        group.blocksRaycasts = alpha > 0f;
        group.interactable = false;
    }

    private static IEnumerator FromAction(Action work)
    {
        work();
        yield break;
    }
}
