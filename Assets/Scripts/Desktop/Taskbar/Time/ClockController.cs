using System;
using TMPro;
using UnityEngine;

public class ClockController : MonoBehaviour
{
    [Header("Work Settings")]
    [SerializeField][Range(0, 23)] private int startHour = 8;
    [SerializeField][Range(0, 23)] private int endHour = 16;

    [Header("Real Life Duration (in minutes)")]
    [Tooltip("How long the work should last in real life")]
    [SerializeField] private float workDurationInMinutes = 5f;

    // Static properties for other scripts to query
    public static int StartHour { get; private set; }
    public static int CurrentHour { get; private set; }
    public static int CurrentMinute { get; private set; }
    public static bool IsStillWorking { get; private set; }
    public static bool IsClockActive { get; private set; }
    public static float RealSecondsPerGameMinute { get; private set; }

    private float realTimeElapsed;
    private float totalWorkHours;
    private float realSecondsPerGameHour;
    private TextMeshProUGUI textTMPro;
    private void Awake()
    {
        InitializeClockSettings();
    }

    private void OnEnable()
    {
        if (TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI component))
            textTMPro = component;

        IsClockActive = true;
        ActionController.SyncSignal("TimeUpdated", UpdateText);
        ActionController.SyncSignal("DayChanged", StartWork);
    }

    private void OnDisable()
    {
        IsClockActive = false;
        ActionController.UnsyncSignal("TimeUpdated", UpdateText);
        ActionController.UnsyncSignal("DayChanged", StartWork);
    }

    void Start()
    {
        StartWork();
    }

    void Update()
    {
        if (!IsStillWorking) return;

        // Accumulates elapsed real-life time
        realTimeElapsed += Time.deltaTime;

        // Calculates total game hours elapsed since the start
        float gameHoursElapsed = realTimeElapsed / realSecondsPerGameHour;

        // If time runs out, end the shift
        if (gameHoursElapsed >= totalWorkHours)
        {
            EndWork();
            return;
        }

        // Calculates current game hour and minute
        float exactGameHour = (startHour + gameHoursElapsed) % 24;

        CurrentHour = Mathf.FloorToInt(exactGameHour);
        CurrentMinute = Mathf.FloorToInt((exactGameHour - CurrentHour) * 60f);

        // Notifies texts to update themselves
        ActionController.SendAction("TimeUpdated");
    }

    private void InitializeClockSettings()
    {
        StartHour = startHour;
        totalWorkHours = endHour - startHour;

        if (totalWorkHours <= 0) totalWorkHours += 24;

        float realDurationInSeconds = workDurationInMinutes * 60f;
        realSecondsPerGameHour = realDurationInSeconds / totalWorkHours;
        RealSecondsPerGameMinute = realSecondsPerGameHour / 60f;
    }

    private void StartWork()
    {
        if (realSecondsPerGameHour <= 0f)
            InitializeClockSettings();

        SaveData save = DataController.CurrentSave;

        // Se houver um horário válido salvo, calcula quanto tempo real já passou
        if (save != null && save.current_hour >= 0)
        {
            int hoursPassed = save.current_hour - startHour;
            if (hoursPassed < 0) hoursPassed += 24;

            float totalGameHoursPassed = hoursPassed + (save.current_minute / 60f) + 0.0001f;

            if (totalGameHoursPassed >= totalWorkHours)
            {
                EndWork();
                return;
            }

            IsStillWorking = true;
            realTimeElapsed = totalGameHoursPassed * realSecondsPerGameHour;
            CurrentHour = save.current_hour;
            CurrentMinute = save.current_minute;
        }
        else
        {
            // Novo save ou novo dia: começa do início do expediente
            IsStillWorking = true;
            realTimeElapsed = 0f;
            CurrentHour = startHour;
            CurrentMinute = 0;

            if (save != null)
            {
                save.current_hour = CurrentHour;
                save.current_minute = CurrentMinute;
            }
        }

        ActionController.SendAction("TimeUpdated");
        ActionController.SendAction("WorkStarted");
    }

    private void UpdateText()
    {
        if (textTMPro != null)
            textTMPro.text = $"{CurrentHour:D2}:{CurrentMinute:D2}";
    }

    private void EndWork() //Fazer alerta de fim de expediente
    {
        IsStillWorking = false;
        CurrentHour = endHour;
        CurrentMinute = 0;

        ActionController.SendAction("TimeUpdated");
        ActionController.SendAction("WorkEnded");
    }
}
