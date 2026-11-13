using UnityEngine;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    public static CursorController Instance { get; private set; }

    public CursorStyle CurrentStyle { get; private set; } = CursorStyle.Default;

    [Header("Texture config")]
    [SerializeField] private Texture2D cursorTextureDefault;
    [SerializeField] private Texture2D cursorTexturePoint;
    [SerializeField] private Texture2D cursorTextureHand;

    private Vector2 clickPosition = Vector2.zero;

    private GameObject cursorVirtual;
    private RectTransform cursorVirtualRect;
    private Image cursorVirtualImage;
    private Sprite virtualSpriteDefault;
    private Sprite virtualSpritePoint;
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

        if (cursorTexturePoint != null)
        {
            virtualSpritePoint = Sprite.Create(
                cursorTexturePoint,
                new Rect(0, 0, cursorTexturePoint.width, cursorTexturePoint.height),
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

        GameObject cursorGO = new("VirtualCursorImage");
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
        CurrentStyle = modeOfCursor;

        switch (modeOfCursor)
        {
            case CursorStyle.Default:
                Cursor.SetCursor(cursorTextureDefault, clickPosition, CursorMode.Auto);
                if (cursorVirtualImage != null && virtualSpriteDefault != null)
                    cursorVirtualImage.sprite = virtualSpriteDefault;
                break;

            case CursorStyle.Point:
                Cursor.SetCursor(cursorTexturePoint, clickPosition, CursorMode.Auto);
                if (cursorVirtualImage != null && virtualSpritePoint != null)
                    cursorVirtualImage.sprite = virtualSpritePoint;
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

    public void GetVirtualCursorExtents(out Vector2 minExtent, out Vector2 maxExtent)
    {
        if (cursorVirtualRect == null)
        {
            minExtent = Vector2.zero;
            maxExtent = Vector2.zero;
            return;
        }

        Vector2 size = cursorVirtualRect.sizeDelta;
        Vector2 pivot = cursorVirtualRect.pivot;

        // Quanto o sprite se estende para a esquerda (x) e para baixo (y) do ponto de clique
        minExtent = new Vector2(size.x * pivot.x, size.y * pivot.y);

        // Quanto o sprite se estende para a direita (x) e para cima (y) do ponto de clique
        maxExtent = new Vector2(size.x * (1f - pivot.x), size.y * (1f - pivot.y));
    }
}

public enum CursorStyle
{
    Keep,
    Default,
    Point,
    Hand
}