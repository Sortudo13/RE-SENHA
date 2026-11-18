using UnityEngine;
using UnityEngine.UI;

public class BackgroundList : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;

    private void Awake() => InitBackgroundList();

    private void OnDestroy()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    private void InitBackgroundList()
    {
        Sprite[] imagens = Resources.LoadAll<Sprite>("Background");

        foreach (Sprite img in imagens)
        {
            GameObject newBackground = Instantiate(itemPrefab, transform);
            if (newBackground.TryGetComponent(out Image imageComponent))
                imageComponent.sprite = img;
        }
    }
}