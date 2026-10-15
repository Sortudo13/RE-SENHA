using System;
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
        save ??= new SaveData { current_day = CurrentDay, error_count = 0, emails = new List<EmailData>() };
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

[System.Serializable]
public class SaveData
{
    public int current_day;
    public int error_count;
    public List<EmailData> emails;
}

[System.Serializable]
public class EmailData
{
    public string sender;
    public string subject;
    public string body; //TODO: Permitir a formatação do corpo do email, incluindo negrito, itálico, sublinhado, cores, links e imagens. (uso de tags HTML ou Markdown)
    public List<string> attachments; //TODO: Permitir anexo de documentos, imagens ou links. Se não houver anexo, deixar em branco.
    public EmailError error;
    public string reason; //TODO: Adicionar opção para coletar o local da discrepância a fim de permitir a seleção durante a análise do email.
}
public enum EmailError
{
    None,
    SenderMismatch,
    SubjectMismatch,
    BodyMismatch,
    AttachmentMismatch
}