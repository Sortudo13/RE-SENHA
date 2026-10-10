using UnityEngine;
using UnityEngine.UI;

public class DesktopIconController : MonoBehaviour
{
    [Header("Icon Settings")]
    //TODO: Automatizar a inserção de sprite, nome, etc (tendo em mente que o script do ícone é genérico e pode ser usado para qualquer app)

    [Header("Window Settings")]
    [SerializeField] private GameObject window;
    [SerializeField] private Transform layer;

    [Header("Taskbar Settings")]
    [SerializeField] private GameObject taskbarButton;
    [SerializeField] private Transform taskbarLayer;

    private GameObject connectedWindow;
    private GameObject connectedButton;

    public void OpenApp()
    {
        if (connectedWindow == null)
        {
            connectedWindow = Instantiate(window, layer);
            connectedButton = Instantiate(taskbarButton, taskbarLayer);
            if (connectedWindow.TryGetComponent(out WindowController windowController))
            {
                if (connectedButton.TryGetComponent(out Button button))
                {
                    button.onClick.AddListener(windowController.SwapWindow);
                }
                windowController.OnDestroyedObject += DestroyButton;
            }
        }
        else
        {
            if (connectedWindow.TryGetComponent(out WindowController windowController))
            {
                windowController.OpenWindow();
            }
            connectedButton.transform.SetAsFirstSibling();
        }
    }

    // Destrói o botão da taskbar. O procedimento de troca da atividade da janela vai ser feito no onclick do botão da taskbar, que vai chamar o método SwapWindow() do WindowController.
    private void DestroyButton()
    {
        Destroy(connectedButton);
    }
}