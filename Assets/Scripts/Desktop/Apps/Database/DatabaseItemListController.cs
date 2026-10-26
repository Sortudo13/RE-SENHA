using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DatabaseItemListController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtNome;
    [SerializeField] private TextMeshProUGUI txtSubtitulo;
    [SerializeField] private Button button;
    [SerializeField] private Image bgImage;

    private EmployeeData currentEmployee;
    private Action<EmployeeData> onSelectCallback;

    public void Setup(EmployeeData employee, Action<EmployeeData> onSelect, bool isSelected = false)
    {
        currentEmployee = employee;
        onSelectCallback = onSelect;

        if (txtNome == null)
            txtNome = GetComponentInChildren<TextMeshProUGUI>();

        if (button == null)
            button = GetComponent<Button>() ?? GetComponentInChildren<Button>();

        if (bgImage == null)
            bgImage = GetComponent<Image>();

        if (txtNome != null)
        {
            if (txtSubtitulo != null)
            {
                txtNome.text = employee.nome;
                txtSubtitulo.text = $"{employee.matricula} | {employee.departamento}";
            }
            else
            {
                txtNome.text = $"{employee.nome}\n<size=70%><color=#AAAAAA>{employee.matricula} | {employee.departamento}</color></size>";
            }
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onSelectCallback?.Invoke(currentEmployee));
        }

        SetSelected(isSelected);
    }

    public void SetSelected(bool isSelected)
    {
        if (bgImage != null)
        {
            bgImage.color = isSelected ? new Color(0.2f, 0.4f, 0.6f, 1f) : new Color(0.12f, 0.12f, 0.14f, 0.9f);
        }
    }
}
