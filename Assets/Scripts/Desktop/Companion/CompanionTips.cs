using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

// Dicas do companheiro (Resources/Companion/CompanionTips.json) + controle de "ja vista" por save slot.
public static class CompanionTips
{
    public const string ResourcePath = "Companion/CompanionTips";

    private static Dictionary<string, CompanionTip> cached;

    // Estaticos sobrevivem entre Plays (domain reload desligado). Cada Play rele o JSON atual.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache()
    {
        cached = null;
    }

    // Chave desconhecida (ex.: "pos" em vez de "pose") falha no carregamento, em vez de sumir em silencio.
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        MissingMemberHandling = MissingMemberHandling.Error,
    };

    public static CompanionTip Find(string tipId)
    {
        if (string.IsNullOrWhiteSpace(tipId))
            return null;

        cached ??= Load();
        return cached.TryGetValue(tipId.Trim(), out CompanionTip tip) ? tip : null;
    }

    public static bool WasSeen(string tipId)
    {
        if (string.IsNullOrWhiteSpace(tipId))
            return false;

        List<string> seen = DataController.CurrentSave?.seenTips;
        return seen != null && seen.Contains(tipId);
    }

    // Grava no save em memoria; vai para o disco no proximo DataController.SavingGame().
    public static void MarkSeen(string tipId)
    {
        SaveData save = DataController.CurrentSave;
        if (save == null || string.IsNullOrWhiteSpace(tipId))
            return;

        save.seenTips ??= new List<string>();
        if (!save.seenTips.Contains(tipId))
            save.seenTips.Add(tipId);
    }

    private static Dictionary<string, CompanionTip> Load()
    {
        Dictionary<string, CompanionTip> tips = new(StringComparer.Ordinal);

        TextAsset file = Resources.Load<TextAsset>(ResourcePath);
        if (file == null)
        {
            Debug.LogWarning($"[CompanionTips] Resources/{ResourcePath}.json nao encontrado.");
            return tips;
        }

        CompanionTipBook book;
        try
        {
            book = JsonConvert.DeserializeObject<CompanionTipBook>(file.text, JsonSettings);
        }
        catch (JsonException e)
        {
            Debug.LogError($"[CompanionTips] JSON invalido: {e.Message}");
            return tips;
        }

        if (book?.tips == null)
            return tips;

        foreach (CompanionTip tip in book.tips)
        {
            if (tip == null || string.IsNullOrWhiteSpace(tip.id))
            {
                Debug.LogWarning("[CompanionTips] Dica sem id ignorada.");
                continue;
            }
            if (!tip.HasLines)
            {
                Debug.LogWarning($"[CompanionTips] Dica '{tip.id}' sem lines ignorada.");
                continue;
            }
            if (!tips.TryAdd(tip.id.Trim(), tip))
                Debug.LogWarning($"[CompanionTips] Id repetido: '{tip.id}'. Valeu a primeira.");
        }

        return tips;
    }
}
