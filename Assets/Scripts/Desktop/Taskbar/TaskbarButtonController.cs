using UnityEngine;
using UnityEngine.UI;

public class TaskbarButtonController : MonoBehaviour
{
    private string targetWindowID;
    private WindowController windowMenuInstance;

    public void Init(WindowData data)
    {
        Image iconImage = gameObject.GetComponentInChildren<Image>();
        Button button = gameObject.GetComponentInChildren<Button>();

        targetWindowID = data.ID;

        if (iconImage != null && data.Icon != null)
            iconImage.sprite = data.Icon;

        button.onClick.AddListener(() => ActionController.SendAction($"Swap_{targetWindowID}"));
        button.onClick.AddListener(() => ActionController.SendAction("HideNotificationBadge", data.Name));

        ActionController.SyncSignal($"WindowDestroyed_{targetWindowID}", DestroyTaskbarButton);
        ActionController.SyncSignal($"ToFront_{targetWindowID}", ToFront);
    }

    public void OpenWindowMenu()
    {
        ActionController.SendAction("OpenWindowMenu");
    }

    private void ClearWindowMenuReference()
    {
        if (windowMenuInstance != null)
            ActionController.UnsyncSignal(
                $"WindowDestroyed_{windowMenuInstance.AppID}",
                ClearWindowMenuReference
            );

        windowMenuInstance = null;
    }

    private void ToFront()
    {
        transform.SetAsFirstSibling();
    }

    private void DestroyTaskbarButton()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        ActionController.UnsyncSignal($"WindowDestroyed_{targetWindowID}", DestroyTaskbarButton);
        ActionController.UnsyncSignal($"ToFront_{targetWindowID}", ToFront);
    }
}
