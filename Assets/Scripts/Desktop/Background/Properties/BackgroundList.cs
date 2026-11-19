using UnityEngine;
using UnityEngine.UI;

public class BackgroundList : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;

    private void Awake() => InitBackgroundList();

    private void InitBackgroundList()
    {
        Sprite[] imagens = Resources.LoadAll<Sprite>("Background");

        foreach (Sprite img in imagens)
        {
            GameObject newBackground = Instantiate(itemPrefab, transform);
            newBackground.GetComponentInChildren<BackgroundButton>().Setup(img);
        }
    }
}