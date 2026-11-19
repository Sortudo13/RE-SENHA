using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Botao do inspetor na taskbar: faz o mesmo que o Espaco (InspectionModeController).
// Tira a selecao depois do clique para o Espaco nao "apertar" o botao de novo via Submit da UI,
// o que desfaria o toggle. Pinta o icone enquanto o modo estiver ligado.
[RequireComponent(typeof(Button))]
public sealed class InspectionModeButton : MonoBehaviour
{
    [SerializeField]
    private Color activeColor = new(1f, 0.85f, 0.3f, 1f);

    private readonly SignalSubscriptions signals = new();
    private Button button;
    private Graphic icon;
    private Color normalColor;

    private void Awake()
    {
        button = GetComponent<Button>();
        icon = button.targetGraphic;
        if (icon != null)
            normalColor = icon.color;

        button.onClick.AddListener(OnClick);
        signals.On<bool>(InspectionSignals.ModeChanged, OnModeChanged);
    }

    private void Start()
    {
        ActionController.SendAction(InspectionSignals.StateRequested);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClick);
        signals.Clear();
    }

    private void OnClick()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        ActionController.SendAction(InspectionSignals.ToggleMode);
    }

    private void OnModeChanged(bool active)
    {
        if (icon != null)
            icon.color = active ? activeColor : normalColor;
    }
}
