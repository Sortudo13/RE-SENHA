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
    // Pedido de um seletor criado depois da ultima mudanca de modo (ex.: subregras do Manual);
    // a resposta e republicar o estado atual em ModeChangedAction e nos highlights.
    public const string StateRequestedAction = "InspectionStateRequested";

    // O overlay apenas escurece a tela durante a inspecao; ele nao bloqueia cliques
    // (raycastTarget desligado no proprio objeto, na cena Desktop).
    [SerializeField]
    private GameObject inspectionOverlay;

    public bool IsActive { get; private set; }

    private void Awake()
    {
        ActionController.SyncSignal(ToggleAction, ToggleMode);
        ActionController.SyncSignal(DeactivateAction, DeactivateMode);
        ActionController.SyncSignal(StateRequestedAction, PublishState);
        IsActive = false;
        if (inspectionOverlay != null)
            inspectionOverlay.SetActive(false);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal(ToggleAction, ToggleMode);
        ActionController.UnsyncSignal(DeactivateAction, DeactivateMode);
        ActionController.UnsyncSignal(StateRequestedAction, PublishState);
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

    private void PublishState()
    {
        ActionController.SendAction(ModeChangedAction, IsActive);
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
