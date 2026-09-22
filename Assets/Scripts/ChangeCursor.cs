using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private ModeOfCursor modeOfCursor;


        public void OnPointerEnter(PointerEventData eventData)
        {
            CursorUI.Instance.SetToMode(modeOfCursor);
        }

        public void OnPointerExit(PointerEventData  eventData)
        {
            CursorUI.Instance.SetToMode(ModeOfCursor.Default);
        }
    }