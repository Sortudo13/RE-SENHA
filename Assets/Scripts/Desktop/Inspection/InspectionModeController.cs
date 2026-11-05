using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InspectionModeController : MonoBehaviour
{
    public const string ToggleAction = "ToggleInspectionMode";
    public const string DeactivateAction = "DeactivateInspectionMode";
    public const string ModeChangedAction = "InspectionModeChanged";

    [SerializeField]
    private GameObject inspectionOverlay;

    public bool IsActive { get; private set; }

    private void Awake()
    {
        ActionController.SyncSignal(ToggleAction, ToggleMode);
        ActionController.SyncSignal(DeactivateAction, DeactivateMode);
        IsActive = false;
        if (inspectionOverlay != null)
            inspectionOverlay.SetActive(false);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal(ToggleAction, ToggleMode);
        ActionController.UnsyncSignal(DeactivateAction, DeactivateMode);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
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
}
