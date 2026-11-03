using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class InspectionModeController : MonoBehaviour
{
    public static bool IsActive { get; private set; }
    public static InspectionModeController Instance { get; private set; }
    public static event Action<bool> ActiveStateChanged;

    private readonly Dictionary<Selectable, bool> selectableStates = new();
    private InspectionField? selectedField;
    private RegraData selectedRule;
    private SubregraData selectedSubrule;
    private GameObject dimmer;
    private TMP_Text feedbackText;
    private RectTransform hoverTarget;
    private RectTransform selectedTarget;
    private RectTransform selectedRuleTarget;
    private readonly Image[] dimmerSections = new Image[24];
    private readonly List<Rect> holes = new();
    private readonly List<float> yCoordinates = new();
    private readonly List<Vector2> openIntervals = new();

    private void Awake()
    {
        Instance = this;
        CreateDimmer();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            SetActive(!IsActive);
    }

    public void SetActive(bool active)
    {
        if (IsActive == active)
            return;

        IsActive = active;
        selectedField = null;
        selectedRule = null;
        selectedSubrule = null;
        hoverTarget = null;
        selectedTarget = null;
        selectedRuleTarget = null;

        if (IsActive)
        {
            LockSelectables();
            dimmer.SetActive(true);
            UpdateDimmerSections();
            feedbackText.text = "Selecione um campo do email e uma regra do manual.";
        }
        else
        {
            RestoreSelectables();
            dimmer.SetActive(false);
        }

        ActiveStateChanged?.Invoke(IsActive);
    }

    public static void SelectField(InspectionField field, RectTransform target)
    {
        if (!IsActive || Instance == null)
            return;

        Instance.hoverTarget = target;
        Instance.selectedTarget = target;
        Instance.UpdateDimmerSections();
        Instance.selectedField = field;
        Instance.TryMatch();
    }

    public static void HighlightField(RectTransform target)
    {
        if (Instance == null || !IsActive)
            return;

        Instance.hoverTarget = target;
        Instance.UpdateDimmerSections();
    }

    public static void ClearHighlight(RectTransform target)
    {
        if (Instance == null || Instance.hoverTarget != target)
            return;

        Instance.hoverTarget = null;
        Instance.UpdateDimmerSections();
    }

    public static void SelectRule(RegraData rule, SubregraData subrule, RectTransform target)
    {
        if (!IsActive || Instance == null)
            return;

        Instance.selectedRule = rule;
        Instance.selectedSubrule = subrule;
        if (target != null)
            Instance.selectedRuleTarget = target;
        Instance.UpdateDimmerSections();
        Instance.TryMatch();
    }

    private void TryMatch()
    {
        if (!selectedField.HasValue || selectedRule == null)
            return;

        if (selectedSubrule == null || selectedField.Value != selectedRule.campo)
        {
            feedbackText.text = "O campo selecionado não corresponde à regra.";
            return;
        }

        if (!TryRegisterMatchOnTarget())
        {
            feedbackText.text = "Nenhuma quebra dessa regra foi encontrada neste item.";
            return;
        }

        selectedField = null;
        selectedRule = null;
        selectedSubrule = null;
        SetActive(false);
    }

    private bool TryRegisterMatchOnTarget()
    {
        foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include
        ))
        {
            if (behaviour is IInspectionMatchTarget target)
            {
                if (target.TryRegisterInspectionMatch(
                    selectedField.Value,
                    selectedRule,
                    selectedSubrule
                ))
                    return true;
            }
        }

        return false;
    }

    private void LockSelectables()
    {
        selectableStates.Clear();
        foreach (Selectable selectable in FindObjectsByType<Selectable>(FindObjectsInactive.Include))
        {
            selectableStates[selectable] = selectable.interactable;
            selectable.interactable = false;
        }
    }

    private void RestoreSelectables()
    {
        foreach (KeyValuePair<Selectable, bool> state in selectableStates)
        {
            if (state.Key != null)
                state.Key.interactable = state.Value;
        }

        selectableStates.Clear();
    }

    private void CreateDimmer()
    {
        dimmer = new GameObject("InspectionModeDimmer", typeof(RectTransform), typeof(Image));
        dimmer.transform.SetParent(transform, false);
        dimmer.transform.SetAsFirstSibling();
        RectTransform rect = dimmer.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = dimmer.GetComponent<Image>();
        image.color = Color.clear;
        image.raycastTarget = false;
        for (int index = 0; index < dimmerSections.Length; index++)
            CreateDimmerSection(index);

        GameObject feedbackObject = new("Feedback", typeof(RectTransform), typeof(TextMeshProUGUI));
        feedbackObject.transform.SetParent(dimmer.transform, false);
        RectTransform feedbackRect = feedbackObject.GetComponent<RectTransform>();
        feedbackRect.anchorMin = new Vector2(0.5f, 1f);
        feedbackRect.anchorMax = new Vector2(0.5f, 1f);
        feedbackRect.pivot = new Vector2(0.5f, 1f);
        feedbackRect.anchoredPosition = new Vector2(0f, -30f);
        feedbackRect.sizeDelta = new Vector2(700f, 60f);
        feedbackText = feedbackObject.GetComponent<TMP_Text>();
        feedbackText.alignment = TextAlignmentOptions.Center;
        feedbackText.fontSize = 24f;
        feedbackText.color = Color.white;
        feedbackText.raycastTarget = false;
        dimmer.SetActive(false);
    }

    private void CreateDimmerSection(int index)
    {
        GameObject section = new($"Section{index}", typeof(RectTransform), typeof(Image));
        section.transform.SetParent(dimmer.transform, false);
        Image sectionImage = section.GetComponent<Image>();
        sectionImage.color = new Color(0f, 0f, 0f, 0.55f);
        sectionImage.raycastTarget = false;
        dimmerSections[index] = sectionImage;
    }

    private void UpdateDimmerSections()
    {
        if (dimmer == null)
            return;

        RectTransform canvasRect = transform as RectTransform;
        Rect canvasBounds = canvasRect.rect;
        holes.Clear();
        AddHole(holes, canvasRect, selectedTarget, canvasBounds);
        AddHole(holes, canvasRect, selectedRuleTarget, canvasBounds);
        if (hoverTarget != null && hoverTarget != selectedTarget)
            AddHole(holes, canvasRect, hoverTarget, canvasBounds);

        yCoordinates.Clear();
        yCoordinates.Add(canvasBounds.yMin);
        yCoordinates.Add(canvasBounds.yMax);
        foreach (Rect hole in holes)
        {
            yCoordinates.Add(hole.yMin);
            yCoordinates.Add(hole.yMax);
        }
        yCoordinates.Sort();

        int sectionIndex = 0;
        for (int bandIndex = 0; bandIndex < yCoordinates.Count - 1; bandIndex++)
        {
            float bandMin = yCoordinates[bandIndex];
            float bandMax = yCoordinates[bandIndex + 1];
            if (bandMax <= bandMin)
                continue;

            openIntervals.Clear();
            float midpoint = (bandMin + bandMax) / 2f;
            foreach (Rect hole in holes)
            {
                if (midpoint >= hole.yMin && midpoint <= hole.yMax)
                    openIntervals.Add(new Vector2(hole.xMin, hole.xMax));
            }
            openIntervals.Sort((left, right) => left.x.CompareTo(right.x));

            float darkStart = canvasBounds.xMin;
            foreach (Vector2 interval in openIntervals)
            {
                if (interval.x > darkStart)
                    SetNextSection(
                        ref sectionIndex,
                        darkStart,
                        bandMin,
                        interval.x - darkStart,
                        bandMax - bandMin
                    );
                darkStart = Mathf.Max(darkStart, interval.y);
            }
            if (darkStart < canvasBounds.xMax)
                SetNextSection(
                    ref sectionIndex,
                    darkStart,
                    bandMin,
                    canvasBounds.xMax - darkStart,
                    bandMax - bandMin
                );
        }

        while (sectionIndex < dimmerSections.Length)
            SetSection(dimmerSections[sectionIndex++], 0f, 0f, 0f, 0f);
    }

    private static void AddHole(List<Rect> holes, RectTransform canvasRect, RectTransform target, Rect canvasBounds)
    {
        if (target == null)
            return;

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, min, null, out Vector2 localMin);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, max, null, out Vector2 localMax);
        holes.Add(Rect.MinMaxRect(
            Mathf.Max(canvasBounds.xMin, localMin.x),
            Mathf.Max(canvasBounds.yMin, localMin.y),
            Mathf.Min(canvasBounds.xMax, localMax.x),
            Mathf.Min(canvasBounds.yMax, localMax.y)
        ));
    }

    private static void SetSection(Image section, float x, float y, float width, float height)
    {
        if (section == null)
            return;

        RectTransform rect = section.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(Mathf.Max(0f, width), Mathf.Max(0f, height));
        section.gameObject.SetActive(width > 0f && height > 0f);
    }

    private void SetNextSection(
        ref int sectionIndex,
        float x,
        float y,
        float width,
        float height
    )
    {
        if (sectionIndex >= dimmerSections.Length)
            return;

        SetSection(dimmerSections[sectionIndex++], x, y, width, height);
    }

}
