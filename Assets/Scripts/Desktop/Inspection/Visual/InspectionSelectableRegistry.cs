using System.Collections.Generic;
using UnityEngine;

// Sem uso desde 2026-11-09: o Inspector nao bloqueia mais a interface.
// Mantido apenas porque o componente ainda esta no objeto InspectionOverlay da cena Desktop;
// remover este script junto com esse objeto quando a cena puder ser editada.

public sealed class InspectionSelectableRegistry : MonoBehaviour
{
    public const string RegistryAvailableAction = "InspectionRegistryAvailable";
    public const string RegistryRequestedAction = "InspectionRegistryRequested";

    private readonly HashSet<RectTransform> selectables = new();

    private void Awake()
    {
        ActionController.SyncSignal(
            RegistryRequestedAction,
            PublishRegistry
        );
    }

    private void Start()
    {
        PublishRegistry();
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal(
            RegistryRequestedAction,
            PublishRegistry
        );
        selectables.Clear();
    }

    private void PublishRegistry()
    {
        ActionController.SendAction(RegistryAvailableAction, this);
    }

    public void Register(RectTransform selectable)
    {
        if (selectable != null)
            selectables.Add(selectable);
    }

    public void Unregister(RectTransform selectable)
    {
        if (selectable != null)
            selectables.Remove(selectable);
    }

    public bool Contains(Vector2 screenPosition, Camera eventCamera)
    {
        selectables.RemoveWhere(selectable => selectable == null);
        foreach (RectTransform selectable in selectables)
        {
            if (selectable.gameObject.activeInHierarchy
                && RectTransformUtility.RectangleContainsScreenPoint(
                    selectable,
                    screenPosition,
                    eventCamera
                ))
                return true;
        }

        return false;
    }
}
