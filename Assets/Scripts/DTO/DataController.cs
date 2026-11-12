using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DataController
{
    private static SaveData currentSave;
    public static SaveData CurrentSave => currentSave ??= LoadingGame();
    public static int CurrentDay
    {
        get => PlayerPrefs.GetInt("CurrentDay", 1);
        set => PlayerPrefs.SetInt("CurrentDay", value);
    }

    public static int MaxDay
    {
        get => PlayerPrefs.GetInt("MaxDay", 5);
        set => PlayerPrefs.SetInt("MaxDay", value);
    }

    public static int CurrentErrorCount
    {
        get => PlayerPrefs.GetInt("CurrentErrorCount", 0);
        set => PlayerPrefs.SetInt("CurrentErrorCount", value);
    }

    public static int MaxErrors
    {
        get => PlayerPrefs.GetInt("MaxErrors", 3);
        set => PlayerPrefs.SetInt("MaxErrors", value);
    }

    public static string SavePath
    {
        get => Path.Combine(Application.persistentDataPath, "save.json");
    }

    public static Boolean PersistMailBox
    {
        get => PlayerPrefs.GetInt("PersistMailBox", 0) == 1;
        set => PlayerPrefs.SetInt("PersistMailBox", value ? 1 : 0);
    }

    public static string ConvertExtension(string extension)
    {
        return extension switch
        {
            "mail" => "Email",
            "manual" => "Manual",
            "png" => "Image",
            "exe" => "RG",
            _ => "RG",
        };
    }

    public static void AdvanceDay()
    {
        CurrentDay++;

        if (CurrentDay >= MaxDay)
        {
            ResetGame();
            SceneManager.LoadScene("GameWin");
        }

        LoadDay();
        ActionController.SendAction("DayChanged");
    }

    public static void LoadDay()
    {
        SaveData save = GetDailyDataJson(CurrentDay);
        if (PersistMailBox && currentSave != null)
        {
            MailBox userBox = currentSave.currentMailBoxes.Find(box => box.mailBox == "User");
            List<EmailData> oldMails = userBox?.emails ?? new List<EmailData>();
            save.currentMailBoxes.Find(box => box.mailBox == "User").emails = oldMails;
        }
        currentSave = save;
        SavingGame(save);
    }

    public static void ResetGame()
    {
        DeleteSaveFile();
        currentSave = null;
        LoadingGame();
    }

    public static void SavingGame(SaveData save = null)
    {
        save ??= currentSave;
        save.current_day = CurrentDay;
        save.error_count = CurrentErrorCount;
        string json = JsonConvert.SerializeObject(save, Formatting.Indented);
        File.WriteAllText(SavePath, json);
    }

    public static SaveData LoadingGame()
    {
        SaveData save = null;
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            if (!string.IsNullOrEmpty(json))
                save = JsonConvert.DeserializeObject<SaveData>(json);
        }
        if (save == null)
        {
            CurrentDay = 1;
            CurrentErrorCount = 0;
            save = GetDailyDataJson(CurrentDay);
        }
        currentSave = save;

        CurrentDay = currentSave.current_day;
        CurrentErrorCount = currentSave.error_count;
        SavingGame();

        ActionController.SendAction("DayChanged");
        return save;
    }

    public static SaveData GetDailyDataJson(int day)
    {
        return new SaveData
        {
            current_day = day,
            error_count = 0,
            availableMails = LoadJsonList<EmailData>($"Emails/Dia_{day}"),
            currentMailBoxes = new()
            {
                new(){ mailBox = "User", emails = new () },
                new(){ mailBox = "Company", emails = new () }
            },
            tickets = LoadJsonList<TicketData>($"Tickets/Dia_{day}")
        };
    }

    private static List<T> LoadJsonList<T>(string resourcePath)
    {
        TextAsset file = Resources.Load<TextAsset>(resourcePath);
        if (file == null)
            return new List<T>();

        return JsonConvert.DeserializeObject<List<T>>(file.text) ?? new List<T>();
    }

    public static void DeleteSaveFile()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}

// Estruturas de dados

public class WindowData
{
    public string ID { get; }
    public Sprite Icon { get; }
    public string Name { get; }

    public WindowData(string id, Sprite icon, string name)
    {
        ID = id;
        Icon = icon;
        Name = name;
    }
}

[System.Serializable]
public class SaveData
{
    public int current_day;
    public int error_count;
    public List<EmailData> availableMails;
    public List<MailBox> currentMailBoxes;
    public List<TicketData> tickets;
}

[System.Serializable]
public class EmailData
{
    public string mailBox;
    public Boolean isRead = false;
    public float timeToReceive = 0f;
    public int hourToReceive = -1;
    public int minuteToReceive = -1;
    public string sender;
    public string subject;
    public string body;
    public List<AttachmentData> attachments = new();
    public List<EvidenceData> evidences = new();
}

[System.Serializable]
public class MailBox
{
    public int newEmails = 0;
    public string mailBox;
    public List<EmailData> emails;
}

[System.Serializable]
public class EvidenceData
{
    // Generic fields support email, chat and database-backed inspections.
    public string findingId;
    public string sourceType;
    public string sourceItemId;
    public string fieldId;
    public string evaluatorId;
    public string ruleId;

    // Legacy fields remain while existing email saves are migrated.
    public string subruleId;
    public string field;
    public string rule;
    public string evidence;
    public string explanation;
    public string reason;
}

[System.Serializable]
public class TicketData
{
    public string id; // novo — usado como label na lista lateral
    public string requester;
    public string body;
    public string reason;
    public TicketDecision decision;
}

public enum TicketDecision
{
    Approve,
    Reject,
}

[System.Serializable]
public class FeedbackData
{
    public bool isPositive;
    public string message;
    public FeedbackData(bool isPositive, string message)
    {
        this.isPositive = isPositive;
        this.message = message;
    }
}

[System.Serializable]
public class AttachmentData
{
    public DocumentType documentType;
    public string name;
    public string extension;
    public string linkedName;
    public AttachmentContent data;
}

[System.Serializable]
public class AttachmentContent { }

// Fazer classes herdadas de AttachmentContent para cada tipo de documento que contém dados do usuário.
// Exemplos abaixo:
[System.Serializable]
public class DadosTexto : AttachmentContent
{
    public string texto;
}

[System.Serializable]
public class DadosFoto : AttachmentContent
{
    public string url;
}

// Enumerações
// Esses devem ser nomes equivalentes aos das funções no Window Manager
public enum DocumentType
{
    Manual,
    Email,
    RG,
}
