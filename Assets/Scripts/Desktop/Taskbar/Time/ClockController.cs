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
    public static int CurrentHour { get; private set; }
    public static int CurrentMinute { get; private set; }
    public static bool IsStillWorking { get; private set; }

    private float realTimeElapsed;
    private float totalWorkHours;
    private float realSecondsPerGameHour;
    private TextMeshProUGUI textTMPro;

    private void OnEnable()
    {
        if (TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI component))
        {
            textTMPro = component;
        }
        ActionController.SyncSignal("TimeUpdated", UpdateText);
    }

    private void OnDisable()
    {
         ActionController.UnsyncSignal("TimeUpdated", UpdateText);
    }

    void Start()
    {
        // Calculates how many hours the shift lasts in the game (e.g., 16 - 8 = 8 hours)
        totalWorkHours = endHour - startHour;

        // If the end hour is earlier than the start hour (passing midnight)
        if (totalWorkHours <= 0) totalWorkHours += 24;

        // Converts real time from minutes to seconds
        float realDurationInSeconds = workDurationInMinutes * 60f;

        // Finds how many real seconds equal 1 game hour
        realSecondsPerGameHour = realDurationInSeconds / totalWorkHours;

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

    private void StartWork()
    {
        IsStillWorking = true;
        realTimeElapsed = 0f;
        CurrentHour = startHour;
        CurrentMinute = 0;

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
