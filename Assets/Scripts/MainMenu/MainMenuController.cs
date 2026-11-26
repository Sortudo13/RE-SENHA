using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Tooltip("Painel dos 3 slots (SaveSlotCanvas).")]
    [SerializeField]
    private SaveSlotsPanel slotsPanel;

    private void Awake()
    {
        if (slotsPanel == null)
            Debug.LogError("[MainMenuController] Slots Panel nao foi arrastado no Inspector.", this);
    }

    public void NovoJogo() => slotsPanel.Open(SaveSlotsPanel.Mode.NewGame, ComecarNoSlot);

    public void Carregar() => slotsPanel.Open(SaveSlotsPanel.Mode.Load, CarregarSlot);

    // Slot ocupado e sobrescrito direto, sem confirmacao (decidido em 2026-11-21).
    private void ComecarNoSlot(int slot)
    {
        DataController.SelectSlot(slot);
        // Limpa o save do disco
        DataController.ResetGame();
        AbrirDesktop();
    }

    private void CarregarSlot(int slot)
    {
        DataController.SelectSlot(slot);
        DataController.LoadingGame();
        AbrirDesktop();
    }

    private void AbrirDesktop()
    {
        LoadingScreen.LoadScene("Desktop");
    }

    public void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        UnityEngine.Application.Quit();
#endif
    }
}
