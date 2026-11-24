using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class InspectionModeController : MonoBehaviour
{

    // O overlay apenas escurece a tela durante a inspecao; ele nao bloqueia cliques
    // (raycastTarget desligado no proprio objeto, na cena Desktop).
    [SerializeField]
    private GameObject inspectionOverlay;

    private readonly SignalSubscriptions signals = new();

    public bool IsActive { get; private set; }

    private void Awake()
    {
        signals
            .On(InspectionSignals.ToggleMode, ToggleMode)
            .On(InspectionSignals.DeactivateMode, DeactivateMode)
            .On(InspectionSignals.StateRequested, PublishState);
        IsActive = false;
        if (inspectionOverlay != null)
            inspectionOverlay.SetActive(false);
    }

    private void OnDestroy()
    {
        signals.Clear();
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

        ActionController.SendAction(InspectionSignals.ModeChanged, active);
        if (active)
            ActionController.SendAction(TutorialSignals.InspectionModeEntered, "inspection");
    }

    private void PublishState()
    {
        ActionController.SendAction(InspectionSignals.ModeChanged, IsActive);
    }

    public void ToggleMode()
    {
        SetActive(!IsActive);
    }

    public void DeactivateMode()
    {
        SetActive(false);
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
