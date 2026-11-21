using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Preenche o RG (Prefabs/Documents/RG.prefab) com os dados do anexo (RGData, resolvido com o Database).
// Fica na raiz do RG; os campos sao arrastados no Inspector (filhos de "Frente").
// Como no Database, o rotulo ("Nome", "Registro Geral"...) e um texto fixo no prefab;
// aqui so entra o valor, nos objetos "... Campo" / "... Data" / "... Numero".
// O cabecalho "Nome Estado" vem de estadoEmissao (nome por extenso); o verso (brasao federal) e fixo no prefab.
public class RGController : MonoBehaviour
{
    [Header("Campos (arrastar do RG/Frente)")]
    [SerializeField] private TextMeshProUGUI nome;
    [SerializeField] private TextMeshProUGUI numero;
    [SerializeField] private TextMeshProUGUI dataExpedicao;
    [SerializeField] private TextMeshProUGUI dataNascimento;
    [SerializeField] private TextMeshProUGUI naturalidade;
    [SerializeField] private TextMeshProUGUI assinatura;
    [Tooltip("Cabecalho (Informacoes Estado/Nome Estado). Sem estadoEmissao nos dados, fica o texto do prefab.")]
    [SerializeField] private TextMeshProUGUI estadoEmissao;
    [SerializeField] private Image foto;

    private void Awake()
    {
        // Avisa uma vez, ao abrir, qual campo ficou sem arrastar.
        if (nome == null) LogMissing(nameof(nome));
        if (numero == null) LogMissing(nameof(numero));
        if (dataExpedicao == null) LogMissing(nameof(dataExpedicao));
        if (dataNascimento == null) LogMissing(nameof(dataNascimento));
        if (naturalidade == null) LogMissing(nameof(naturalidade));
        if (assinatura == null) LogMissing(nameof(assinatura));
        if (estadoEmissao == null) LogMissing(nameof(estadoEmissao));
        if (foto == null) LogMissing(nameof(foto));
    }

    public void Show(RGData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[RGController] Anexo de RG sem dados.", this);
            return;
        }

        RGDocument rg = data.Resolve();
        SetText(nome, rg.nome);
        SetText(numero, rg.numero);
        SetText(dataExpedicao, rg.dataExpedicao);
        SetText(dataNascimento, rg.dataNascimento);
        SetText(naturalidade, $"{rg.naturalidade}/{rg.estadoEmissor}");
        SetText(assinatura, rg.nome);
        if (!string.IsNullOrEmpty(rg.estadoEmissao))
            SetText(estadoEmissao, rg.estadoEmissao);

        if (foto != null)
        {
            Sprite sprite = rg.GetPhoto();
            if (sprite != null)
                foto.sprite = sprite;
            else if (!string.IsNullOrEmpty(rg.foto))
                Debug.LogWarning($"[RGController] Foto 'Images/{rg.foto}' nao encontrada.", this);
        }
    }

    private static void SetText(TextMeshProUGUI target, string value)
    {
        if (target != null)
            target.text = value;
    }

    private void LogMissing(string field)
    {
        Debug.LogError($"[RGController] Campo '{field}' nao foi arrastado no Inspector.", this);
    }
}
