using UnityEngine;
using UnityEngine.UI;

public class WallpaperController : MonoBehaviour
{
    private void Awake() => ActionController.SyncSignal<Sprite>("SetWallpaper", SetWallpaper);
    private void OnDestroy() => ActionController.UnsyncSignal<Sprite>("SetWallpaper", SetWallpaper);

    private void SetWallpaper(Sprite sprite)
    {
        gameObject.GetComponent<Image>().sprite = sprite;
    }
}
