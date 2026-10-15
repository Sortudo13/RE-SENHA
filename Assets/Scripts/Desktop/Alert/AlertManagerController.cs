using UnityEngine;

public class AlertManagerController : MonoBehaviour
{
    public static Transform AlertsLayer { get; private set; }
    private void Awake()
    {
        AlertsLayer = transform;
    }
}
