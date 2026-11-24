using System;
using System.Collections.Generic;
using UnityEngine;

// Conta do dia dos apps com decisao
/* O expediente acaba pelo relogio (ClockController)
Depois disso, quando nao houver mais emails da empresa na caixa, aparece o alerta "Avancar Dia" (ShowNextDay)
Uso: RegisterAnswer antes do feedback (o alerta mostra o contador de erros) e
CheckDayEnd depois de remover o item respondido.
Outros apps (chat; ticket depois) dizem se ainda tem item pendente com AddPendingCheck.*/
public static class DayProgressController
{
    private static readonly List<Func<bool>> pendingChecks = new();

    // Roda antes de carregar a cena: limpar depois (padrao AfterSceneLoad) apagaria o que os
    // objetos da primeira cena ja registraram no OnEnable (ex.: o chat ao dar Play no Desktop).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetPendingChecks()
    {
        pendingChecks.Clear();
    }

    [RuntimeInitializeOnLoadMethod]
    private static void SubscribeWorkEnded()
    {
        // Se tudo ja foi respondido quando o relogio acaba nenhuma decisao chamaria CheckDayEnd.
        ActionController.UnsyncSignal("WorkEnded", CheckDayEndOnWorkEnded);
        ActionController.SyncSignal("WorkEnded", CheckDayEndOnWorkEnded);
    }

    private static void CheckDayEndOnWorkEnded() => CheckDayEnd();

    // hasPending devolve true enquanto o app tiver item do dia sem resposta.
    public static void AddPendingCheck(Func<bool> hasPending)
    {
        if (hasPending != null && !pendingChecks.Contains(hasPending))
            pendingChecks.Add(hasPending);
    }

    public static void RemovePendingCheck(Func<bool> hasPending)
    {
        pendingChecks.Remove(hasPending);
    }

    public static void RegisterAnswer(bool correct)
    {
        if (!correct)
            DataController.CurrentErrorCount++;
    }

    // Devolve true se deu GameOver: o save foi apagado, e quem chamou nao deve salvar mais nada.
    public static bool CheckDayEnd()
    {
        // O GameOver vem primeiro, o terceiro erro no ultimo item do dia e GameOver
        if (DataController.CurrentErrorCount >= DataController.MaxErrors)
        {
            DataController.ResetGame();
            LoadingScreen.LoadScene("GameOver");
            return true;
        }

        if (ClockController.IsStillWorking || HasPendingItems())
            return false;

        ActionController.SendAction("ShowNextDay");
        return false;
    }

    /*So eh consultado depois do fim do expediente. Os emails da empresa que nao chegaram 
    ate la nunca chegam (o SenderController descarta), entao availableMails nao conta.
    A caixa User tambem nao conta*/
    private static bool HasPendingItems()
    {
        SaveData save = DataController.CurrentSave;
        MailBox companyBox = save.currentMailBoxes?.Find(box => box.mailBox == "Company");
        if (companyBox?.emails != null && companyBox.emails.Count > 0)
            return true;

        foreach (Func<bool> hasPending in pendingChecks)
            if (hasPending())
                return true;

        // PENDENTE: tickets entram aqui (AddPendingCheck)
        // O TicketController ainda tem a propria conta de erros
        return false;
    }
}
