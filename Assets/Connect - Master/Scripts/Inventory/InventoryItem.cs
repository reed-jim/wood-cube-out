using UnityEngine;

public enum InventoryItemTier
{
    
}

public class InventoryItem : MonoBehaviour
{
    public BaseStat health;
    public BaseStat armor;
    public float price;

    public virtual void SpecialEffect()
    {

    }
}
