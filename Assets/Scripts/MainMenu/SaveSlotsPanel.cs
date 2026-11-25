using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Painel com os 3 slots de save do menu
// Montado na cena MainMenu; botoes e textos arrastados no Inspector, na ordem slot 1, 2, 3.
// Novo Jogo: qualquer slot (ocupado e sobrescrito direto). Carregar: slot vazio fica desabilitado.
public class SaveSlotsPanel : MonoBehaviour
{
    public enum Mode { NewGame, Load }

    [Header("Slots (arrastar na ordem 1, 2, 3)")]
    [SerializeField] private Button[] slotButtons = new Button[DataController.SaveSlotCount];
    [SerializeField] private TextMeshProUGUI[] slotTexts = new TextMeshProUGUI[DataController.SaveSlotCount];
    [SerializeField] private Image[] slotIcons = new Image[DataController.SaveSlotCount];

    [Header("Opcional")]
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private string newGameTitle = "Novo Jogo - escolha um slot";
    [SerializeField] private string loadTitle = "Carregar - escolha um slot";

    [Header("Texto do slot")]
    [SerializeField] private string emptyText = "Slot {0}\nVazio";
    // {0} = slot, {1} = dia, {2} = horario do jogo, {3} = data/hora real do ultimo save
    [SerializeField] private string usedText = "Slot {0}\nDia {1} - {2}\nSalvo em {3}";
    [SerializeField] private string dayStartText = "inicio do expediente";
    [SerializeField] private string savedAtFormat = "dd/MM/yyyy HH:mm";

    private Action<int> onSlotChosen;

    private void Awake()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] == null)
            {
                Debug.LogError($"[SaveSlotsPanel] Botao do slot {i + 1} nao foi arrastado no Inspector.", this);
                continue;
            }

            int slot = i + 1;
            slotButtons[i].onClick.AddListener(() => Choose(slot));
        }

        for (int i = 0; i < slotIcons.Length; i++)
            if (slotIcons[i] == null)
                Debug.LogWarning($"[SaveSlotsPanel] Icone do slot {i + 1} nao foi arrastado no Inspector.", this);
    }

    public void Open(Mode mode, Action<int> onChosen)
    {
        onSlotChosen = onChosen;
        if (title != null)
            title.text = mode == Mode.NewGame ? newGameTitle : loadTitle;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            SaveSlotInfo info = DataController.GetSlotInfo(i + 1);

            if (slotButtons[i] != null)
                slotButtons[i].interactable = mode == Mode.NewGame || info.exists;

            if (i < slotTexts.Length && slotTexts[i] != null)
                slotTexts[i].text = Describe(info);

            if (i < slotIcons.Length && slotIcons[i] != null)
            {
                if (info.currentCharacter != null)
                    slotIcons[i].sprite = Resources.Load<Sprite>($"Images/NPCs/{info.currentCharacter}");
                else
                    slotIcons[i].sprite = Resources.Load<Sprite>($"Images/NPCs/Diskette");

                if (!info.exists)
                    slotIcons[i].sprite = Resources.Load<Sprite>($"System/Plus_Signal");
            }
        }

        gameObject.SetActive(true);
    }

    // Ligado no OnClick do botao Voltar.
    public void Close()
    {
        onSlotChosen = null;
        gameObject.SetActive(false);
    }

    private void Choose(int slot)
    {
        Action<int> callback = onSlotChosen;
        Close();
        callback?.Invoke(slot);
    }

    private string Describe(SaveSlotInfo info)
    {
        if (!info.exists)
            return string.Format(emptyText, info.slot);

        string time = info.hour < 0 ? dayStartText : $"{info.hour:00}:{info.minute:00}";
        return string.Format(usedText, info.slot, info.day, time, info.lastSaved.ToString(savedAtFormat));
    }
}
