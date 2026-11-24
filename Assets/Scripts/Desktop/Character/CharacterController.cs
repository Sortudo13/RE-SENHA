using UnityEngine;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour
{
    private Image characterImage = null;
    private void Awake()
    {
        characterImage = GetComponentInChildren<Image>();
        if (characterImage != null)
            characterImage.sprite = DataController.CurrentSave.currentCharacter != null ? Resources.Load<Sprite>($"Images/NPCs/{DataController.CurrentSave.currentCharacter}") : characterImage.sprite;
        ActionController.SyncSignal<Sprite>("SetCharacter", SetCharacter);
    }

    private void OnDestroy() => ActionController.UnsyncSignal<Sprite>("SetCharacter", SetCharacter);

    private void SetCharacter(Sprite sprite)
    {
        if (sprite != null && characterImage != null)
        {
            characterImage.sprite = sprite;
            DataController.CurrentSave.currentCharacter = sprite.name;
            DataController.SavingGame();
        }
    }
}
