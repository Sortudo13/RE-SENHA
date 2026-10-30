using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DatabaseItemListController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtNome;
    [SerializeField] private TextMeshProUGUI txtSetor;
    [SerializeField] private Button button;

    private EmployeeData currentEmployee;
    private Action<EmployeeData> onSelectCallback;

    public void Setup(EmployeeData employee, Action<EmployeeData> onSelect)
    {
        currentEmployee = employee;
        onSelectCallback = onSelect;

        if (txtNome == null)
            txtNome = transform.Find("TxtNome")?.GetComponent<TextMeshProUGUI>();

        if (txtSetor == null)
            txtSetor = transform.Find("TxtSetor")?.GetComponent<TextMeshProUGUI>();

        if (button == null)
            button = GetComponent<Button>() ?? GetComponentInChildren<Button>();

        if (txtNome != null)
            txtNome.text = employee.nome;

        if (txtSetor != null)
            txtSetor.text = $"{employee.matricula} | {employee.departamento}";

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onSelectCallback?.Invoke(currentEmployee));
        }

    }
}
