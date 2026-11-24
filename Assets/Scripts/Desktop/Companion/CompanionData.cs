using System;
using System.Collections.Generic;

// Formato do Resources/Companion/CompanionTips.json.

[Serializable]
public class CompanionTipBook
{
    public int version = 1;
    public List<CompanionTip> tips = new();
}

[Serializable]
public class CompanionTip
{
    // Identifica a dica no JSON e na lista de dicas ja vistas do save (SaveData.seenTips).
    // Toda dica aparece uma vez so por save slot.
    public string id;

    // Baloes em sequencia; o jogador clica no balao para passar.
    public List<CompanionLine> lines = new();

    public bool HasLines => lines != null && lines.Count > 0;
}

[Serializable]
public class CompanionLine
{
    // Expressao neste balao: nome de uma entrada em CompanionController.poseSprites (no prefab)
    // ou de um estado/trigger do Animator. Vazio = sprite padrao.
    public string pose = "";

    public string text = "";

    // Trigger do Animator tocado ao abrir o balao (ex.: "Jump", "Shake"). Vazio = nenhum.
    public string anim = "";
}
