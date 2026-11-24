using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Companheiro de dicas (estilo Clippy). Prefab: Prefabs/Companion/Companion.prefab, na raiz da cena Desktop.
// Tudo o que aparece e configurado no prefab; os textos vem do Resources/Companion/CompanionTips.json.
//
// Ao receber CompanionSignals.ShowTip(id): entra na tela (Animator), mostra os baloes da dica e sai.
// Cada dica aparece uma vez por save slot (ids vistos ficam em SaveData.seenTips).
// A cada balao:
//   pose -> sprite do Portrait trocada pela da lista poseSprites (sem entrada: sprite padrao do prefab).
//           Se o Animator tiver um trigger ou estado com o nome da pose, ele tambem e tocado.
//   anim -> trigger do Animator com esse nome (ex.: "Jump", "Shake").
//
// Parametros do Animator usados pelo script:
//   bool "Visible"  -> true entra, false sai. O script espera um estado com tag "Shown" / "Hidden"
//                      na camada 0 antes de seguir.
//   bool "Talking"  -> true enquanto o texto do balao esta sendo digitado (opcional).
public class CompanionController : MonoBehaviour
{
    [Serializable]
    public sealed class PoseSprite
    {
        public string pose;
        public Sprite sprite;
    }

    public const string VisibleParam = "Visible";
    public const string TalkingParam = "Talking";
    public const string ShownTag = "Shown";
    public const string HiddenTag = "Hidden";

    private const float AnimatorWaitTimeout = 3f;

    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [Tooltip("O que entra/sai da tela. Fica desativado (e o Animator desligado) entre as dicas. " +
             "Vazio = pai do Portrait.")]
    [SerializeField] private RectTransform body;
    [Tooltip("Retrato do companheiro; a sprite que estiver aqui e a expressao padrao.")]
    [SerializeField] private Image portrait;
    [SerializeField] private GameObject bubble;
    [SerializeField] private Button bubbleButton;
    [SerializeField] private TextMeshProUGUI bubbleText;
    [Tooltip("\"clique para continuar\". Com CanvasGroup so some (o balao nao muda de altura).")]
    [SerializeField] private GameObject continueHint;

    [Header("Expressoes (nome da pose no JSON -> sprite)")]
    [SerializeField] private List<PoseSprite> poseSprites = new();

    [Header("Ritmo")]
    [SerializeField] private float charsPerSecond = 45f;
    [SerializeField] private float delayBetweenTips = 0.4f;

    private readonly Queue<CompanionTip> queue = new();
    private readonly HashSet<string> warned = new(StringComparer.OrdinalIgnoreCase);
    // O JSON nao precisa acertar maiusculas: "jump" acha o trigger "Jump".
    private readonly Dictionary<string, AnimatorControllerParameter> animatorParams = new(StringComparer.OrdinalIgnoreCase);

    private Sprite defaultSprite;
    private CompanionTip currentTip;
    private Coroutine runner;
    private bool clickRequested;

    // ---- Ciclo de vida ----

    private void Awake()
    {
        if (animator == null || animator.runtimeAnimatorController == null
            || portrait == null || bubble == null || bubbleText == null)
        {
            Debug.LogError("[Companion] Referencias do prefab faltando (Animator com controller, Portrait, Bubble, Text).", this);
            enabled = false;
            return;
        }

        defaultSprite = portrait.sprite;
        if (body == null)
            body = portrait.transform.parent as RectTransform;

        if (bubbleButton != null)
            bubbleButton.onClick.AddListener(OnBubbleClicked);

        bubble.SetActive(false);
        SetHintVisible(false);

        ActionController.SyncSignal<string>(CompanionSignals.ShowTip, OnShowTip);
    }

    private void Start()
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
            animatorParams[parameter.name] = parameter;

        if (!HasParam(VisibleParam, AnimatorControllerParameterType.Bool))
            Debug.LogError($"[Companion] O Animator precisa de um bool '{VisibleParam}'.", this);

        // Os parametros so podem ser lidos com o Animator ligado; depois disso ele dorme ate a 1a dica.
        SetIdle(true);
    }

    // Parado: Body desativado e Animator desligado. Nao desenha nada (nem na Scene view) e o
    // Animator nao reescreve a posicao do Body todo frame (o que refaria o Canvas a cada frame).
    // Ao religar, o Animator volta para o estado padrao (Hidden) com Visible = false.
    private void SetIdle(bool idle)
    {
        if (body != null)
            body.gameObject.SetActive(!idle);
        animator.enabled = !idle;
    }

    private void OnDestroy()
    {
        if (bubbleButton != null)
            bubbleButton.onClick.RemoveListener(OnBubbleClicked);

        ActionController.UnsyncSignal<string>(CompanionSignals.ShowTip, OnShowTip);
    }

    // ---- Sinal ----

    private void OnShowTip(string tipId)
    {
        if (!isActiveAndEnabled)
            return;

        CompanionTip tip = CompanionTips.Find(tipId);
        if (tip == null)
        {
            Debug.LogWarning($"[Companion] Dica '{tipId}' nao existe no {CompanionTips.ResourcePath}.json.");
            return;
        }
        if (ShouldSkip(tip))
            return;

        // A mesma dica pedida duas vezes seguidas (ex.: duas janelas do Manual) entra uma vez so.
        if (currentTip?.id == tip.id)
            return;
        foreach (CompanionTip queued in queue)
        {
            if (queued.id == tip.id)
                return;
        }

        queue.Enqueue(tip);
        runner ??= StartCoroutine(RunQueue());
    }

    private static bool ShouldSkip(CompanionTip tip)
    {
        return CompanionTips.WasSeen(tip.id);
    }

    private void OnBubbleClicked()
    {
        clickRequested = true;
    }

    // ---- Sequencia ----

    private IEnumerator RunQueue()
    {
        // Sinal enviado no mesmo frame do carregamento: espera o Start ler o Animator.
        yield return null;

        while (queue.Count > 0)
        {
            CompanionTip tip = queue.Dequeue();
            if (ShouldSkip(tip))
                continue;

            currentTip = tip;
            CompanionTips.MarkSeen(tip.id);

            yield return ShowTip(tip);

            currentTip = null;
            ActionController.SendAction(CompanionSignals.TipFinished, tip.id);

            if (queue.Count > 0)
                yield return Wait(delayBetweenTips);
        }

        runner = null;
    }

    private IEnumerator ShowTip(CompanionTip tip)
    {
        bubble.SetActive(false);
        SetIdle(false);
        SetPose(tip.lines[0]?.pose);

        animator.SetBool(VisibleParam, true);
        yield return WaitAnimatorTag(ShownTag);

        bubble.SetActive(true);
        foreach (CompanionLine line in tip.lines)
        {
            if (line != null)
                yield return ShowLine(line);
        }
        bubble.SetActive(false);

        ResetTriggers();
        animator.SetBool(VisibleParam, false);
        yield return WaitAnimatorTag(HiddenTag);
        SetPose(null);
        SetIdle(true);
    }

    private IEnumerator ShowLine(CompanionLine line)
    {
        SetPose(line.pose);
        if (!string.IsNullOrWhiteSpace(line.anim) && !TryPlayAnimator(line.anim.Trim()))
            WarnOnce($"anim:{line.anim}", $"[Companion] anim '{line.anim}' sem trigger/estado no Animator.");

        clickRequested = false;
        SetHintVisible(false);
        bubbleText.text = line.text ?? "";
        bubbleText.maxVisibleCharacters = 0;
        bubbleText.ForceMeshUpdate();
        int total = bubbleText.textInfo.characterCount;

        // Digitando; clique completa o texto de uma vez.
        SetTalking(true);
        float visible = 0f;
        while (visible < total)
        {
            if (clickRequested)
            {
                clickRequested = false;
                break;
            }
            visible += Time.deltaTime * Mathf.Max(1f, charsPerSecond);
            bubbleText.maxVisibleCharacters = Mathf.FloorToInt(visible);
            yield return null;
        }
        bubbleText.maxVisibleCharacters = total;
        SetTalking(false);

        // Espera o clique para o proximo balao.
        SetHintVisible(true);
        while (!clickRequested)
            yield return null;
        clickRequested = false;
        SetHintVisible(false);
    }

    private void SetHintVisible(bool visible)
    {
        if (continueHint == null)
            return;
        if (continueHint.TryGetComponent(out CanvasGroup group))
            group.alpha = visible ? 1f : 0f;
        else
            continueHint.SetActive(visible);
    }

    // ---- Expressao ----

    // Troca a sprite do Portrait. Pose vazia ou sem entrada em poseSprites -> sprite padrao do prefab.
    private void SetPose(string pose)
    {
        pose = pose?.Trim();
        Sprite sprite = defaultSprite;
        bool found = string.IsNullOrEmpty(pose);

        if (!found)
        {
            foreach (PoseSprite entry in poseSprites)
            {
                if (entry?.sprite != null && string.Equals(entry.pose, pose, StringComparison.OrdinalIgnoreCase))
                {
                    sprite = entry.sprite;
                    found = true;
                    break;
                }
            }

            // Expressao animada: estado/trigger com o nome da pose (pode trocar a sprite nos keyframes).
            if (TryPlayAnimator(pose))
                found = true;
        }

        if (!found)
            WarnOnce($"pose:{pose}", $"[Companion] Pose '{pose}' sem sprite em poseSprites nem estado no Animator; usando a padrao.");

        portrait.sprite = sprite;
    }

    // ---- Animator ----

    private bool HasParam(string name, AnimatorControllerParameterType type)
    {
        return animatorParams.TryGetValue(name, out AnimatorControllerParameter parameter) && parameter.type == type;
    }

    // Trigger com esse nome, ou estado com esse nome em alguma camada. false se nao existe.
    private bool TryPlayAnimator(string name)
    {
        if (animatorParams.TryGetValue(name, out AnimatorControllerParameter parameter)
            && parameter.type == AnimatorControllerParameterType.Trigger)
        {
            animator.SetTrigger(parameter.name);
            return true;
        }

        for (int layer = 0; layer < animator.layerCount; layer++)
        {
            int hash = Animator.StringToHash($"{animator.GetLayerName(layer)}.{name}");
            if (animator.HasState(layer, hash))
            {
                animator.Play(hash, layer, 0f);
                return true;
            }
        }
        return false;
    }

    // Trigger disparado no ultimo balao e ainda nao consumido nao pode sobrar para a proxima dica.
    private void ResetTriggers()
    {
        foreach (AnimatorControllerParameter parameter in animatorParams.Values)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger)
                animator.ResetTrigger(parameter.name);
        }
    }

    private void SetTalking(bool talking)
    {
        if (HasParam(TalkingParam, AnimatorControllerParameterType.Bool))
            animator.SetBool(TalkingParam, talking);
    }

    private IEnumerator WaitAnimatorTag(string tag)
    {
        // Um frame para o Animator ler o parametro novo.
        yield return null;

        float elapsed = 0f;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsTag(tag) || animator.IsInTransition(0))
        {
            elapsed += Time.deltaTime;
            if (elapsed > AnimatorWaitTimeout)
            {
                WarnOnce($"tag:{tag}", $"[Companion] Animator nao chegou num estado com tag '{tag}' na camada 0.");
                yield break;
            }
            yield return null;
        }
    }

    // Tempo do jogo: o menu de pausa (timeScale 0) congela o companheiro junto.
    private static IEnumerator Wait(float seconds)
    {
        for (float t = 0f; t < seconds; t += Time.deltaTime)
            yield return null;
    }

    private void WarnOnce(string key, string message)
    {
        if (warned.Add(key))
            Debug.LogWarning(message, this);
    }
}
