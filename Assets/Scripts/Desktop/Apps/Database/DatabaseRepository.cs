using System.Collections.Generic;
using UnityEngine;

// Fonte unica dos colaboradores do dia, compartilhada pela janela do Database e pelo Inspector.
// Os dados ficam disponiveis mesmo com a janela do Database fechada.
public static class DatabaseRepository
{
    private static List<EmployeeData> employees;
    private static int loadedDay = -1;

    // Estaticos sobrevivem entre Plays; limpar aqui faz cada Play reler o JSON atual.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache()
    {
        employees = null;
        loadedDay = -1;
    }

    // Le Resources/Database/Database_Dia_<dia>.json; sem ele, usa Database_Base.json.
    public static IReadOnlyList<EmployeeData> GetEmployees()
    {
        int day = DataController.CurrentDay;
        if (employees != null && loadedDay == day)
            return employees;

        TextAsset jsonAsset = Resources.Load<TextAsset>($"Database/Database_Dia_{day}");
        if (jsonAsset == null)
            jsonAsset = Resources.Load<TextAsset>("Database/Database_Base");

        if (jsonAsset == null || string.IsNullOrEmpty(jsonAsset.text))
        {
            Debug.LogError(
                "DatabaseRepository: no database JSON was found in Resources/Database."
            );
            employees = new List<EmployeeData>();
        }
        else
        {
            EmployeeDatabaseSave data = JsonUtility.FromJson<EmployeeDatabaseSave>(jsonAsset.text);
            employees = data?.employees ?? new List<EmployeeData>();
        }

        loadedDay = day;
        return employees;
    }
}
