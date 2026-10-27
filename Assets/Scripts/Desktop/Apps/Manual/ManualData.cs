using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NovoManual", menuName = "Manual/Manual de Regras")]
public class ManualData : ScriptableObject
{
    public List<RegraData> todasAsRegras;

    // Retorna apenas as regras ativas até o dia/nível atual
    public List<RegraData> ObterRegrasAtivas(int diaAtual)
    {
        return todasAsRegras.FindAll(regra => regra.nivelIntroducao <= diaAtual);
    }
}
