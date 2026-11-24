using UnityEngine;
using UnityEngine.UI;

public class CharacterList : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;

    private void Awake() => InitCharacterList();

    private void InitCharacterList()
    {
        Sprite[] imagens = Resources.LoadAll<Sprite>("Images/NPCs");

        foreach (Sprite img in imagens)
        {
            GameObject newCharacter = Instantiate(itemPrefab, transform);
            newCharacter.GetComponentInChildren<CharacterButton>().Setup(img);
        }
    }
}