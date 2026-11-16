using UnityEngine;
using UnityEngine.UI;

public class TaskbarButtonController : MonoBehaviour
{
    private string targetWindowID;

    public void Init(WindowData data)
    {
        Image iconImage = gameObject.GetComponentInChildren<Image>();
        Button button = gameObject.GetComponentInChildren<Button>();

        targetWindowID = data.ID;

        if (iconImage != null && data.Icon != null)
            iconImage.sprite = data.Icon;

        button.onClick.AddListener(() => ActionController.SendAction($"Swap_{targetWindowID}"));

        ActionController.SyncSignal($"WindowDestroyed_{targetWindowID}", DestroyTaskbarButton);
        ActionController.SyncSignal($"ToFront_{targetWindowID}", ToFront);
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
