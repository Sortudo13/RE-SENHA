using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DataController
{
    private static SaveData currentSave;
    private static MetaData currentMetaData;
    private static string saveNumber = "1";
    public static SaveData CurrentSave => currentSave ??= LoadingGame();
    public static MetaData CurrentMetaData => currentMetaData ??= LoadMetaData();
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
        get => Path.Combine(Application.persistentDataPath, $"save_{saveNumber}.json");
        set => saveNumber = value;
    }

    public static Boolean PersistMailBox
    {
        get => PlayerPrefs.GetInt("PersistMailBox", 0) == 1;
        set => PlayerPrefs.SetInt("PersistMailBox", value ? 1 : 0);
    }

    public static string DataPath
    {
        get => Path.Combine(Application.persistentDataPath, $"data.json");
    }

    public static void AdvanceDay()
    {
        CurrentDay++;

        if (CurrentDay > MaxDay)
        {
            ResetGame();
            LoadingScreen.LoadScene("GameWin");
            return;
        }

        LoadDay();
    }

    public static void RetrocedeDay()
    {
        CurrentDay--;

        if (CurrentDay < 1)
            CurrentDay = 1;

        LoadDay();
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
        ActionController.SendAction("DayChanged");
    }

    public static void ResetGame()
    {
        DeleteSaveFile();
        currentSave = null;
        LoadingGame();
    }

    public static void SavingGame(SaveData save = null)
    {
        if (save == null)
        {
            save = CurrentSave;
            save.current_day = CurrentDay;
            save.error_count = CurrentErrorCount;
            if (ClockController.IsClockActive)
            {
                save.current_hour = ClockController.CurrentHour;
                save.current_minute = ClockController.CurrentMinute;
            }
            string json = JsonConvert.SerializeObject(save, Formatting.Indented);
            File.WriteAllText(SavePath, json);
        }
        else
        {
            CurrentDay = save.current_day;
            CurrentErrorCount = save.error_count;

            string json = JsonConvert.SerializeObject(save, Formatting.Indented);
            File.WriteAllText(SavePath, json);
        }
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
        ActionController.SendAction("DayChanged");
        SavingGame();

        return save;
    }

    public static SaveData GetDailyDataJson(int day)
    {
        return new SaveData
        {
            current_day = day,
            error_count = 0,
            current_hour = -1,
            current_minute = -1,
            availableMails = LoadJsonList<EmailData>($"Emails/Dia_{day}"),
            currentMailBoxes = new()
            {
                new() { mailBox = "User", emails = new() },
                new() { mailBox = "Company", emails = new() },
            },
            tickets = LoadJsonList<TicketData>($"Tickets/Dia_{day}"),
            chatStates = new(),
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

    public static void SaveMetaData(MetaData metaData = null)
    {
        currentMetaData = metaData ?? CurrentMetaData;
        string json = JsonConvert.SerializeObject(CurrentMetaData, Formatting.Indented);
        File.WriteAllText(DataPath, json);
    }

    public static MetaData LoadMetaData()
    {
        if (!File.Exists(DataPath))
        {
            currentMetaData = new MetaData();
            SaveMetaData();
            return CurrentMetaData;

        }
        string json = File.ReadAllText(DataPath);
        currentMetaData = JsonConvert.DeserializeObject<MetaData>(json) ?? new MetaData();
        return CurrentMetaData;
    }
}

// Estruturas de dados

[System.Serializable]
public class MetaData
{
    public string currentBackground = null;
    public List<AchievementData> achievements = new();
    public List<TrinketData> trinkets = new();
    public StatisticsData statistics = new();
}

[System.Serializable]
public class AchievementData
{
    public string achievementId;
    public string title;
    public string description;
    public bool isUnlocked;
}

[System.Serializable]
public class TrinketData
{
    public string TrinketId { get; set; }
    public string IconName { get; set; }
    public string Name { get; set; }
}

[System.Serializable]
public class StatisticsData
{

}

[System.Serializable]
public class SaveData
{
    public int current_day;
    public int error_count;
    public int current_hour = -1;
    public int current_minute = 0;
    public List<EmailData> availableMails;
    public List<MailBox> currentMailBoxes;
    public List<ChatSaveState> chatStates;
    public List<TicketData> availableTickets;
    public List<TicketData> tickets;
}

[System.Serializable]
public class EmailData : AttachmentContent
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

    [JsonConverter(typeof(AttachmentContentConverter))]
    public AttachmentContent data;
}

[System.Serializable]
public class AttachmentContent { }

// Fazer classes herdadas de AttachmentContent para cada tipo de documento que contém dados do usuário.
// Exemplos abaixo:
[System.Serializable]
public class PDFData : AttachmentContent
{
    public string text;
}

// Anexo que abre o Manual numa aba e, se ruleId vier preenchido, direto nessa regra.
[System.Serializable]
public class ManualData : AttachmentContent
{
    public string app = "email";
    public string ruleId;
}

public class RGData : AttachmentContent
{
    public string rgNumber;
    public string name;
}

[System.Serializable]
public class PhotoData : AttachmentContent
{
    public string title;
    public string description;
    public string photoPath;
}

public class AttachmentContentConverter : JsonConverter<AttachmentContent>
{
    // Ao salvar (SavingGame), grava a subclasse real automaticamente sem interferência
    public override bool CanWrite => false;

    public override AttachmentContent ReadJson(JsonReader reader, Type objectType, AttachmentContent existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        JObject obj = JObject.Load(reader);

        if (!obj.HasValues)
            return new AttachmentContent();

        // Escolhe a subclasse de acordo com as chaves encontradas no JSON
        AttachmentContent content = obj switch
        {
            _ when obj.Property("sender") != null || obj.Property("subject") != null => new EmailData(),
            _ when obj.Property("ruleId") != null => new ManualData(),
            _ when obj.Property("text") != null => new PDFData(),
            _ when obj.Property("url") != null => new PhotoData(),
            _ => new AttachmentContent()
        };

        serializer.Populate(obj.CreateReader(), content);
        return content;
    }

    public override void WriteJson(JsonWriter writer, AttachmentContent value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

// Enumerações
// Esses devem ser nomes equivalentes aos das funções no Window Manager
public enum DocumentType
{
    Manual,
    Email,
    PDF,
    RG,
}

[System.Serializable]
public class ChatSaveState
{
    public string conversationId;
    public string currentNodeId;
    public bool isFinished;
    public bool hasArrived;
    public bool hasUnread;

    public List<ChatMessageEntry> messageHistory = new();
}

[System.Serializable]
public class ChatMessageEntry
{
    public bool isFromNpc; // true = NPC, false = jogador
    public string text;
}
