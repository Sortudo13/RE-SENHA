using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class InspectionModeController : MonoBehaviour
{
    public const string ToggleAction = "ToggleInspectionMode";
    public const string DeactivateAction = "DeactivateInspectionMode";
    public const string ModeChangedAction = "InspectionModeChanged";

    // O overlay apenas escurece a tela durante a inspecao; ele nao bloqueia cliques.
    // A cena Desktop ainda guarda o overlay com raycast ligado (e os componentes legados de bloqueio),
    // entao o Awake desliga o raycast por codigo. Quando a cena puder ser editada, configurar
    // raycastTarget = false no proprio objeto e remover DisableOverlayRaycasts.
    [SerializeField]
    private GameObject inspectionOverlay;

    public bool IsActive { get; private set; }

    private void Awake()
    {
        ActionController.SyncSignal(ToggleAction, ToggleMode);
        ActionController.SyncSignal(DeactivateAction, DeactivateMode);
        IsActive = false;
        if (inspectionOverlay != null)
        {
            DisableOverlayRaycasts();
            inspectionOverlay.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal(ToggleAction, ToggleMode);
        ActionController.UnsyncSignal(DeactivateAction, DeactivateMode);
    }

    private void Update()
    {
        if (Keyboard.current != null
            && Keyboard.current.spaceKey.wasPressedThisFrame
            && !IsTypingInTextField())
            ToggleMode();
    }

    public void SetActive(bool active)
    {
        if (IsActive == active)
            return;

        IsActive = active;
        if (inspectionOverlay != null)
            inspectionOverlay.SetActive(active);

        ActionController.SendAction(ModeChangedAction, active);
    }

    public void ToggleMode()
    {
        SetActive(!IsActive);
    }

    public void DeactivateMode()
    {
        SetActive(false);
    }

    private void DisableOverlayRaycasts()
    {
        foreach (Graphic graphic in inspectionOverlay.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = false;

        foreach (CanvasGroup group in inspectionOverlay.GetComponentsInChildren<CanvasGroup>(true))
            group.blocksRaycasts = false;
    }

    // Como a interface continua clicavel durante a inspecao, o Espaco digitado em um campo
    // de texto (ex.: pesquisa do Database) nao deve alternar o modo.
    private static bool IsTypingInTextField()
    {
        GameObject selected = EventSystem.current != null
            ? EventSystem.current.currentSelectedGameObject
            : null;
        if (selected == null)
            return false;

        if (selected.TryGetComponent(out TMP_InputField tmpInput))
            return tmpInput.isFocused;

        return selected.TryGetComponent(out InputField legacyInput) && legacyInput.isFocused;
    }
}
