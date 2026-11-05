using System.Collections.Generic;
using UnityEngine;

public static class InspectionSelectableRegistry
{
    private static readonly HashSet<RectTransform> selectables = new();

    public static void Register(RectTransform selectable)
    {
        if (selectable != null)
            selectables.Add(selectable);
    }

    public static void Unregister(RectTransform selectable)
    {
        if (selectable != null)
            selectables.Remove(selectable);
    }

    public static bool Contains(Vector2 screenPosition, Camera eventCamera)
    {
        foreach (RectTransform selectable in selectables)
        {
            if (selectable != null
                && selectable.gameObject.activeInHierarchy
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
