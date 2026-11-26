using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChatInbox", menuName = "Re:Senha/Chat/Chat Inbox")]
public class ChatInbox : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        [Tooltip("Conversa agendada.")]
        public ChatConversation conversation;

        [Header("Chegada (dia e horário de jogo)")]
        [Range(1, 5)] public int arrivalDay = 1;
        [Range(8, 16)] public int arrivalHour = 8;
        [Range(0, 59)] public int arrivalMinute = 0;

        [Tooltip("Desempate quando duas conversas chegam no mesmo dia/minuto.")]
        public int sortOrder;

        public int ArrivalTotalMinutes => arrivalHour * 60 + arrivalMinute;
    }

    [Tooltip("Todas as conversas agendadas do jogo.")]
    public List<Entry> entries = new List<Entry>();

    public IEnumerable<Entry> GetSortedEntries()
    {
        var copy = new List<Entry>(entries);
        copy.Sort((a, b) =>
        {
            int cmpDay = a.arrivalDay.CompareTo(b.arrivalDay);
            if (cmpDay != 0) return cmpDay;

            int cmpMin = a.ArrivalTotalMinutes.CompareTo(b.ArrivalTotalMinutes);
            return cmpMin != 0 ? cmpMin : a.sortOrder.CompareTo(b.sortOrder);
        });
        return copy;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var seen = new HashSet<string>();
        foreach (var entry in entries)
        {
            if (entry?.conversation == null) continue;

            string id = entry.conversation.conversationId;
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("[ChatInbox] Entry com conversationId vazio.", this);
                continue;
            }
            if (!seen.Add(id))
                Debug.LogError($"[ChatInbox] conversationId duplicado: '{id}'.", this);

            bool outOfDay = entry.arrivalDay < 1 || entry.arrivalDay > 5;
            bool outOfHour = entry.arrivalHour < 8 || entry.arrivalHour > 16
                             || (entry.arrivalHour == 16 && entry.arrivalMinute > 0);

            if (outOfDay || outOfHour)
            {
                Debug.LogWarning(
                    $"[ChatInbox] '{id}' agendada fora do expediente: "
                    + $"Dia {entry.arrivalDay} às {entry.arrivalHour:D2}:{entry.arrivalMinute:D2}.",
                    this
                );
            }
        }
    }
#endif
}