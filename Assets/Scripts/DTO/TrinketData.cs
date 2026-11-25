using UnityEngine;

[CreateAssetMenu(fileName = "NewTrinket", menuName = "Data/Trinket Data")]
public class TrinketData : ScriptableObject
{
    [SerializeField] private int id;
    [SerializeField] private string iconName;
    [SerializeField] private string trinketName;

    public int TrinketId => id;
    public string IconName => iconName;
    public string Name => trinketName;

    public Sprite GetIcon() => string.IsNullOrEmpty(iconName) ? null : Resources.Load<Sprite>($"Trinkets/{iconName}");

}