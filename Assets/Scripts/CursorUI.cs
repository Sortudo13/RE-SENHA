using UnityEngine;

public class CursorUI : MonoBehaviour
    {
        public static CursorUI Instance { get; private set; }

        [SerializeField] private Texture2D cursorTextureDefault;
        [SerializeField] private Texture2D cursorTextureHand;

        [SerializeField] private Vector2 clickPosition = Vector2.zero;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            Cursor.SetCursor(cursorTextureDefault, clickPosition, CursorMode.Auto);
        }

        public void SetToMode(ModeOfCursor modeOfCursor)
        {
            switch (modeOfCursor)
            {
                case ModeOfCursor.Default:
                    Cursor.SetCursor(cursorTextureDefault, clickPosition, CursorMode.Auto);
                    break;
                case ModeOfCursor.Hand:
                    Cursor.SetCursor(cursorTextureHand, clickPosition, CursorMode.Auto);
                    break;
                default:
                    Cursor.SetCursor(cursorTextureDefault, clickPosition, CursorMode.Auto);
                    break;
            }
        }
    }


    public enum ModeOfCursor
    {
        Default,
        Hand
    }
