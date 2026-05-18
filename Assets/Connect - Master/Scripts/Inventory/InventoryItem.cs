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

public class InventoryItem : MonoBehaviour
{
    public BaseStat health;
    public BaseStat armor;
    public float price;
    public Sprite icon;
    public InventoryItemTier tier;

    public virtual void SpecialEffect()
    {

    }
}
