using UnityEngine;
using UnityEngine.UI;

public class BackgroundButton : MonoBehaviour
{
    private Button button = null;
    private Image image = null;
    private Sprite currentSprite = null;

    private void Awake()
    {
        image = GetComponentInChildren<Image>();
        if (image != null && currentSprite == null)
            currentSprite = image.sprite;

        button = GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ActionController.SendAction<Sprite>("SetWallpaper", currentSprite));
        }
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();
    }

    public void Setup(Sprite sprite)
    {
        currentSprite = sprite;

        if (image == null)
            image = GetComponentInChildren<Image>();

        if (image != null)
            image.sprite = currentSprite;
    }

    public Sprite Read() => currentSprite;

}
