using UnityEngine;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    public static CursorController Instance { get; private set; }

    [Header("Texture config")]
    [SerializeField] private Texture2D cursorTextureDefault;
    [SerializeField] private Texture2D cursorTextureHand;

    private Vector2 clickPosition = Vector2.zero;

    private GameObject cursorVirtual;
    private RectTransform cursorVirtualRect;
    private Image cursorVirtualImage;
    private Sprite virtualSpriteDefault;
    private Sprite virtualSpriteHand;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitVirtualCursorUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetToMode(CursorStyle.Default);
    }

    private void InitVirtualCursorUI()
    {
        if (cursorTextureDefault != null)
        {
            virtualSpriteDefault = Sprite.Create(
                cursorTextureDefault,
                new Rect(0, 0, cursorTextureDefault.width, cursorTextureDefault.height),
                new Vector2(0f, 1f)
            );
        }

        if (cursorTextureHand != null)
        {
            virtualSpriteHand = Sprite.Create(
                cursorTextureHand,
                new Rect(0, 0, cursorTextureHand.width, cursorTextureHand.height),
                new Vector2(0f, 1f)
            );
        }

        cursorVirtual = new GameObject("VirtualCursorCanvas");
        cursorVirtual.transform.SetParent(transform);

        Canvas canvas = cursorVirtual.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
        cursorVirtual.AddComponent<CanvasScaler>();

        GameObject cursorGO = new GameObject("VirtualCursorImage");
        cursorGO.transform.SetParent(cursorVirtual.transform);

        cursorVirtualImage = cursorGO.AddComponent<Image>();
        cursorVirtualImage.raycastTarget = false;

        cursorVirtualRect = cursorGO.GetComponent<RectTransform>();
        cursorVirtualRect.pivot = new Vector2(0f, 1f);

        if (cursorTextureDefault != null)
        {
            cursorVirtualRect.sizeDelta = new Vector2(cursorTextureDefault.width, cursorTextureDefault.height);
        }

        cursorVirtual.SetActive(false);
    }

    public void SetToMode(CursorStyle modeOfCursor)
    {
        switch (modeOfCursor)
        {
            case CursorStyle.Default:
                Cursor.SetCursor(cursorTextureDefault, clickPosition, CursorMode.Auto);
                if (cursorVirtualImage != null && virtualSpriteDefault != null)
                    cursorVirtualImage.sprite = virtualSpriteDefault;
                break;

            case CursorStyle.Hand:
                Cursor.SetCursor(cursorTextureHand, clickPosition, CursorMode.Auto);
                if (cursorVirtualImage != null && virtualSpriteHand != null)
                    cursorVirtualImage.sprite = virtualSpriteHand;
                break;
        }
    }

    public void ShowVirtualCursor(bool show)
    {
        Cursor.visible = !show;
        if (cursorVirtual != null){
            cursorVirtual.SetActive(show);
        }
    }

    public void UpdateVirtualCursorPosition(Vector2 screenPosition)
    {
        if (cursorVirtualRect != null)
            cursorVirtualRect.position = screenPosition;
    }
}

public enum CursorStyle
{
    Keep,
    Default,
    Hand
}