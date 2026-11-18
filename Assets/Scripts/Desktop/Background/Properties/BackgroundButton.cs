using UnityEngine;
using UnityEngine.UI;

public class BackgroundButton : MonoBehaviour
{
    private void Awake()
    {
        if (transform.TryGetComponent(out Button button))
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ActionController.SendAction<Sprite>("SetWallpaper", transform.GetComponentInChildren<Image>().sprite));
        }
    }

    private void OnDestroy()
    {
        if (transform.TryGetComponent(out Button button))
            button.onClick.RemoveAllListeners();
    }

}
