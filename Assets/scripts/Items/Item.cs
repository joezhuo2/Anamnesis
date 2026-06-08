using UnityEngine;

public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary, Unique }

public class Item : ScriptableObject
{
    public Sprite sprite;
    public ItemRarity rarity;
    public string itemName;
    [TextArea] public string itemDesc;
}