// Sinais (ActionController) do companheiro. Os textos ficam so no Resources/Companion/CompanionTips.json;
// o codigo do jogo so manda o id da dica.
public static class CompanionSignals
{
    // Payload string (id de uma dica do CompanionTips.json).
    public const string ShowTip = "CompanionShowTip";

    // Payload string (id da dica). A dica terminou: ultimo balao fechado e companheiro saiu.
    public const string TipFinished = "CompanionTipFinished";

    // Mostra uma dica do JSON pelo id. Se ja foi vista neste save, nada acontece.
    public static void Show(string tipId)
    {
        ActionController.SendAction(ShowTip, tipId);
    }
}
