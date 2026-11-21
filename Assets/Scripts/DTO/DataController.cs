using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public static int MaxWindows
    {
        get => PlayerPrefs.GetInt("MaxWindows", 2);
        set => PlayerPrefs.SetInt("MaxWindows", value);
    }

    // Recebe o numero do slot (ex.: SavePath = "2"), nao o caminho.
    public static string SavePath
    {
        get => Path.Combine(Application.persistentDataPath, $"save_{saveNumber}.json");
        set
        {
            if (saveNumber == value)
                return;

            saveNumber = value;
            // O save em memoria era do slot anterior; o proximo CurrentSave le o slot novo
            // (LoadingGame tambem acerta CurrentDay e CurrentErrorCount, que ficam no PlayerPrefs).
            currentSave = null;
        }
    }

    // Copia do save como estava no comeco do dia atual; o "Reiniciar Dia" volta para ela.
    // Um por slot. Nome fora do padrao "save_*.json" para nao aparecer numa lista de saves.
    // Quem copiar, renomear ou apagar um slot leva este arquivo junto (DeleteSaveFile ja apaga).
    public static string DayStartPath
    {
        get => Path.Combine(Application.persistentDataPath, $"daystart_{saveNumber}.json");
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
        SaveData save = BuildDayStart(CurrentDay);
        currentSave = save;
        SavingGame(save);
        SaveDayStart(save);
        ActionController.SendAction("DayChanged");
    }

    // Save do comeco do dia: dados do dia (Resources) + o que vem dos dias anteriores
    // (caixa User, se PersistMailBox estiver ligado).
    private static SaveData BuildDayStart(int day)
    {
        SaveData save = GetDailyDataJson(day);
        if (PersistMailBox && currentSave != null)
        {
            MailBox userBox = currentSave.currentMailBoxes.Find(box => box.mailBox == "User");
            List<EmailData> oldMails = userBox?.emails ?? new List<EmailData>();
            save.currentMailBoxes.Find(box => box.mailBox == "User").emails = oldMails;
        }
        return save;
    }

    private static void SaveDayStart(SaveData save)
    {
        string json = JsonConvert.SerializeObject(save, Formatting.Indented);
        File.WriteAllText(DayStartPath, json);
    }

    // Devolve null se nao ha copia ou se ela e de outro dia.
    private static SaveData LoadDayStart(int day)
    {
        if (!File.Exists(DayStartPath))
            return null;

        string json = File.ReadAllText(DayStartPath);
        if (string.IsNullOrEmpty(json))
            return null;

        SaveData save = JsonConvert.DeserializeObject<SaveData>(json);
        return save != null && save.current_day == day ? save : null;
    }

    public static void ResetGame()
    {
        DeleteSaveFile();
        currentSave = null;
        LoadingGame();
    }

    // Desfaz tudo o que foi feito no dia: volta para o save do comeco do dia (emails, achados,
    // advertencias, chat, horario) e recarrega o Desktop. So o que vem de dias anteriores fica.
    public static void ResetDayAndReload()
    {
        // Um loading em andamento ignoraria o LoadScene, e o save ja teria sido reiniciado.
        if (LoadingScreen.IsLoading)
        {
            Debug.LogWarning("[DataController] ResetDayAndReload ignorado: ja existe um loading.");
            return;
        }

        if (currentSave == null)
            LoadingGame();

        SaveData save = LoadDayStart(CurrentDay);
        if (save == null)
        {
            // Save antigo, de antes da copia do comeco do dia: remonta o dia a partir dos Resources.
            Debug.LogWarning(
                "[DataController] ResetDayAndReload: sem copia do comeco do dia; remontando o dia."
            );
            save = BuildDayStart(CurrentDay);
            SaveDayStart(save);
        }

        currentSave = save;
        SavingGame(save); // tambem volta CurrentDay e CurrentErrorCount para os do comeco do dia

        Debug.Log(
            $"[DataController] ResetDayAndReload: dia {save.current_day} reiniciado (erros={save.error_count})."
        );

        // A cena antiga continua rodando durante o loading (minimo 1,5 s). Pausado, o relogio, o
        // envio de emails e o "digitando..." do chat nao mexem no save reiniciado nem o salvam
        // com o horario antigo. A tela de loading anima com unscaledDeltaTime.
        Time.timeScale = 0f;
        SceneManager.sceneLoaded += OnDayResetSceneLoaded;
        LoadingScreen.LoadScene("Desktop");

        // Pedido recusado (ex.: cena fora do Build Settings): nao deixar o jogo pausado.
        if (!LoadingScreen.IsLoading)
            OnDayResetSceneLoaded(default, LoadSceneMode.Single);
    }

    private static void OnDayResetSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnDayResetSceneLoaded;
        Time.timeScale = 1f;
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
            SaveDayStart(save);
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
        if (File.Exists(DayStartPath))
            File.Delete(DayStartPath);
    }

    public static void SaveMetaData(MetaData metaData = null)
    {
        currentMetaData = metaData ?? CurrentMetaData;
        string json = JsonConvert.SerializeObject(currentMetaData, Formatting.Indented);
        File.WriteAllText(DataPath, json);
    }

    public static MetaData LoadMetaData()
    {
        if (!File.Exists(DataPath))
        {
            currentMetaData = new MetaData();
            SaveMetaData();
            return currentMetaData;

        }
        string json = File.ReadAllText(DataPath);
        currentMetaData = JsonConvert.DeserializeObject<MetaData>(json) ?? new MetaData();
        return currentMetaData;
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
public class PNGData : AttachmentContent
{
    public string imagePath;

    public Sprite GetImage() => Resources.Load<Sprite>($"Images/{imagePath}");
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
            _ when obj.Property("imagePath") != null => new PNGData(),
            _ when obj.Property("text") != null => new PDFData(),
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
    PNG,
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
