using UnityEngine;

public class AlertManagerController : MonoBehaviour
{
    public static Transform AlertsLayer { get; private set; }

    private void Awake()
    {
        AlertsLayer = transform;
    }

    public static GameObject ShowAlert(GameObject alertPrefab)
    {
        if (alertPrefab == null)
        {
            Debug.LogError("Cannot show an alert without an alert prefab.");
            return null;
        }

        if (AlertsLayer == null)
        {
            Debug.LogError("CanvasAlerts is not available in the active scene.");
            return null;
        }

        GameObject alert = Object.Instantiate(alertPrefab, AlertsLayer);
        alert.transform.SetAsLastSibling();
        alert.SetActive(true);
        return alert;
    }

    public static void CloseAlert(GameObject alert)
    {
        if (alert != null)
            Object.Destroy(alert);
    }
}
