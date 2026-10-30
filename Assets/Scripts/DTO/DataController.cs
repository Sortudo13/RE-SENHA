using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DataController
{
    public static int CurrentDay
    {
        get => PlayerPrefs.GetInt("CurrentDay", 1);
        set => PlayerPrefs.SetInt("CurrentDay", value);
    }

    public static int MaxDay
    {
        get => PlayerPrefs.GetInt("MaxDay", 1);
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
        ActionController.SendAction("DayChanged");
        SaveEmails(LoadDailyJson(CurrentDay));
    }

    public static void ReduceDay()
    {
        CurrentDay--;
        ActionController.SendAction("DayChanged");
        SaveEmails(LoadDailyJson(CurrentDay));
    }

    public static void ResetDay()
    {
        SaveEmails(LoadDailyJson(CurrentDay));
    }

    public static void ResetDays()
    {
        CurrentDay = 1;
        CurrentErrorCount = 0;
        ActionController.SendAction("DayChanged");
        SaveEmails(LoadDailyJson(CurrentDay));
    }

    public static void SaveEmails(SaveData save)
    {
        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(SavePath, json);
    }

    public static void SaveEmails(List<EmailData> inbox, int error_count, int current_day)
    {
        SaveData save = new() { emails = inbox, error_count = error_count, current_day = current_day };
        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(SavePath, json);
    }

    public static SaveData LoadEmails()
    {
        SaveData save = null;

        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);

            if (!string.IsNullOrEmpty(json))
            {
                save = JsonUtility.FromJson<SaveData>(json);
            }
        }
        if (save == null)
        {
            save = LoadDailyJson(CurrentDay);
            SaveEmails(save);
        }
        return save;
    }

    public static SaveData LoadDailyJson(int day)
    {
        // Lê o arquivo "Dia_1.json", "Dia_2.json", etc. da pasta Assets/Resources/Emails/
        TextAsset jsonText = Resources.Load<TextAsset>($"Emails/Dia_{day}");
        SaveData save = null;

        if (jsonText != null)
        {
            save = JsonUtility.FromJson<SaveData>(jsonText.text);
        }
        save ??= new SaveData
        {
            current_day = CurrentDay,
            error_count = 0,
            emails = new List<EmailData>(),
            tickets = new List<TicketData>()
        };

        TextAsset ticketJson = Resources.Load<TextAsset>($"Tickets/Dia_{day}");
        if (ticketJson != null)
        {
            SaveData ticketSave = JsonUtility.FromJson<SaveData>(ticketJson.text);
            save.tickets = ticketSave?.tickets ?? new List<TicketData>();
        }

        save.tickets ??= new List<TicketData>();
        return save;
    }

    public static void DeleteSaveFile()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
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
    public List<EmailData> emails;
    public List<TicketData> tickets;
}

[System.Serializable]
public class EmailData
{
    public string sender;
    public string subject;
    public string body;
    public List<AttachmentData> attachments;
    public EmailError error;
    public string reason; //TODO: Adicionar opção para coletar o local da discrepância a fim de permitir a seleção durante a análise do email.
}

[System.Serializable]
public class TicketData
{
    public string requester;
    public string subject;
    public string body;
    public string reason;
    public TicketDecision decision;
}

public enum TicketDecision
{
    Approve,
    Reject
}

[System.Serializable]
public class AttachmentData
{
    public DocumentType documentType;
    public string name;
    public string extension;
    public string linkedName;
    public AttachmentContent data;

    public AttachmentData(DocumentType type, string fullname, AttachmentContent data)
    {
        documentType = type;
        string[] temp = fullname.Split('.', 2);
        name = temp[0];
        extension = (temp.Length > 1) ? temp[1] : "exe";
        linkedName = fullname;
        this.data = data;
    }
}

[System.Serializable]
public class AttachmentContent { }

// Fazer classes herdadas de AttachmentContent para cada tipo de documento que contém dados do usuário.
// Exemplos abaixo:
[System.Serializable]
public class DadosTexto : AttachmentContent { public string texto; }

[System.Serializable]
public class DadosFoto : AttachmentContent { public string url; }

// Enumerações
public enum EmailError
{
    None,
    SenderMismatch,
    SubjectMismatch,
    BodyMismatch,
    AttachmentMismatch
}
// Esses devem ser nomes equivalentes aos das funções no Window Manager
public enum DocumentType
{
    Manual,
    Email,
    RG
}