using UnityEngine;

public class DocumentFlipController : MonoBehaviour
{
    private GameObject front;
    private GameObject back;
    private bool showFront;

    public void Awake()
    {
        front = transform.GetChild(0).gameObject;
        back = transform.GetChild(1).gameObject;
        showFront = true;
    }

    public void Start()
    {
        UpdateVisibility();
    }

    public void ToggleVisibility()
    {
        showFront = !showFront;

        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (showFront)
        {
            front.SetActive(true);
            back.SetActive(false);
        }
        else
        {
            front.SetActive(false);
            back.SetActive(true);
        }
    }
}
