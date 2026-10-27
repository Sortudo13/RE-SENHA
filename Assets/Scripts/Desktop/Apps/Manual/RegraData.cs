using UnityEngine;

[CreateAssetMenu(fileName = "NovaRegra", menuName = "Manual/Regra")]
public class RegraData : ScriptableObject
{
    public string idRegra; // Ex: "REG_001"
    public string titulo; // Ex: "Verificação de Domínio"

    [TextArea(3, 6)]
    public string descricao; // Ex: "E-mails corporativos válidos devem terminar estritamente em @corporacao.com."

    public int nivelIntroducao; // Em qual dia/nível essa regra entra em vigor
}
