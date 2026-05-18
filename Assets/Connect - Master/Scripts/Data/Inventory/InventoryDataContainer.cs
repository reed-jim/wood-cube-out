using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryDataContainer", menuName = "Scriptable Objects/InventoryDataContainer")]
public class InventoryDataContainer : ScriptableObject
{
    public List<InventoryItem> items;

    public static readonly Color[] tierColors =
    {
        new Color32(180, 180, 180, 255), // Common - Gray
        new Color32( 80, 200, 120, 255), // Uncommon - Green
        new Color32( 70, 140, 255, 255), // Rare - Blue
        new Color32(180,  90, 255, 255), // Epic - Purple
        new Color32(255, 170,  40, 255), // Legendary - Orange
        new Color32(255,  60, 120, 255), // Mythic - Crimson/Pink
        new Color32(  0, 255, 220, 255), // Exotic - Cyan/Turquoise
        new Color32(255, 255, 120, 255)  // Unique - Gold-Yellow
    };

    public static Color GetTierColor(InventoryItemTier tier)
    {
        return tierColors[(int)tier];
    }
}
