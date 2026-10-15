using UnityEngine;

public class TaskbarManagerController : MonoBehaviour
{
    [SerializeField] private TaskbarButtonController taskbarButtonPrefab;

    private void Awake()
    {
        ActionController.SyncSignal<WindowData>("WindowCreated", OnWindowCreated);
    }

    private void OnWindowCreated(WindowData data)
    {
        TaskbarButtonController newButton = Instantiate(taskbarButtonPrefab, this.transform);
        newButton.Init(data);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal<WindowData>("WindowCreated", OnWindowCreated);
    }
}
