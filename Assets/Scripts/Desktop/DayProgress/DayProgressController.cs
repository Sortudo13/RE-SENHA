using UnityEngine;
using UnityEngine.SceneManagement;

// Conta do dia dos apps com decisao
/* O expediente acaba pelo relogio (ClockController)
Depois disso, quando nao houver mais emails da empresa na caixa, aparece o alerta "Avancar Dia" (ShowNextDay)
Uso: RegisterAnswer antes do feedback (o alerta mostra o contador de erros) e
CheckDayEnd depois de remover o item respondido.*/
public static class DayProgressController
{
    [RuntimeInitializeOnLoadMethod]
    private static void SubscribeWorkEnded()
    {
        // Se tudo ja foi respondido quando o relogio acaba nenhuma decisao chamaria CheckDayEnd.
        ActionController.UnsyncSignal("WorkEnded", CheckDayEnd);
        ActionController.SyncSignal("WorkEnded", CheckDayEnd);
    }

    public static void RegisterAnswer(bool correct)
    {
        if (!correct)
            DataController.CurrentErrorCount++;
    }

    public static void CheckDayEnd()
    {
        // O GameOver vem primeiro, o terceiro erro no ultimo item do dia e GameOver
        if (DataController.CurrentErrorCount >= DataController.MaxErrors)
        {
            DataController.ResetGame();
            SceneManager.LoadScene("GameOver");
            return;
        }

        if (ClockController.IsStillWorking || HasPendingItems())
            return;

        ActionController.SendAction("ShowNextDay");
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

        // PENDENTE: tickets e chat entram aqui
        // O TicketController ainda tem a propria conta de erros
        return false;
    }
}
