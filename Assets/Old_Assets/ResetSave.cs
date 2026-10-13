using UnityEngine;
using UnityEngine.UI;

public class ResetSave : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (this.TryGetComponent(out Button button))
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(DataController.ResetDays);
        }
    }
}
