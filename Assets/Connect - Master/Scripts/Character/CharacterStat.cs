using System.Collections.Generic;
using UnityEngine;

public class CharacterStat : MonoBehaviour
{
    public CharacterServiceLocator characterServiceLocator;

    public BaseStat health;
    public BaseStat armor;

    public void CalculateFinalStat()
    {
        for (int i = 0; i < characterServiceLocator.character.inventoryItems.Count; i++)
        {
            InventoryItem inventoryItem = characterServiceLocator.character.inventoryItems[i];

            health.currentValue += inventoryItem.health.currentValue;
            armor.currentValue += inventoryItem.armor.currentValue;
        }
    }
}
