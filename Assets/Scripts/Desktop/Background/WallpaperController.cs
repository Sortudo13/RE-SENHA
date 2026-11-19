using UnityEngine;
using UnityEngine.UI;

public class WallpaperController : MonoBehaviour
{
    private Image wallpaperImage = null;
    private void Awake()
    {
        wallpaperImage = GetComponentInChildren<Image>();
        if (wallpaperImage != null)
            wallpaperImage.sprite = DataController.CurrentMetaData.currentBackground != null ? Resources.Load<Sprite>($"Background/{DataController.CurrentMetaData.currentBackground}") : wallpaperImage.sprite;
        ActionController.SyncSignal<Sprite>("SetWallpaper", SetWallpaper);
    }
    private void OnDestroy() => ActionController.UnsyncSignal<Sprite>("SetWallpaper", SetWallpaper);

    private void SetWallpaper(Sprite sprite)
    {
        if (sprite != null && wallpaperImage != null)
        {
            wallpaperImage.sprite = sprite;
            DataController.CurrentMetaData.currentBackground = sprite.name;
            DataController.SaveMetaData();
        }
    }
}
