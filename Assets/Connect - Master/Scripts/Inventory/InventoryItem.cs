using UnityEngine;

public enum InventoryItemTier
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic,
    Exotic,
    Unique
}


public enum InventoryItemType
{
    Weapon,
    Armor,
    Helmet,
    Gloves,
    Boots,
    Accessories
}

public class InventoryItem : MonoBehaviour
{
    public int id;
    public BaseStat health;
    public BaseStat armor;
    public float price;
    public Sprite icon;
    public InventoryItemType itemType;
    public InventoryItemTier tier;

    public virtual void SpecialEffect()
    {

    }

    public virtual string GetSpecialEffectDescription()
    {
        return "No more effect";
    }
}
