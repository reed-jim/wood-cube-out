using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItemData", menuName = "Scriptable Objects/InventoryItemData")]
public class InventoryItemData : ScriptableObject
{
    public BaseStat health;
    public BaseStat armor;
    public InventoryItemTier tier;
}
