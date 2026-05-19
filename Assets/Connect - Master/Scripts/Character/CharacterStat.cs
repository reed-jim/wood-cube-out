using System.Collections.Generic;
using UnityEngine;

public class CharacterStat : MonoBehaviour
{
    public CharacterServiceLocator characterServiceLocator;

    public BaseStat health;
    public BaseStat armor;
    public BaseStat damageMultiplier;
    public BaseStat attackSpeed;
    public BaseStat reloadSpeed;
    public BaseStat moveSpeed;

    public void CalculateFinalStat()
    {
        for (int i = 0; i < characterServiceLocator.character.inventoryItems.Count; i++)
        {
            InventoryItem inventoryItem = characterServiceLocator.character.inventoryItems[i];

            health.currentValue += inventoryItem.health.currentValue;
            armor.currentValue += inventoryItem.armor.currentValue;
        }
    }

    public static CharacterStat operator +(CharacterStat stat1, CharacterStat stat2)
    {
        CharacterStat result = new CharacterStat();

        result.health = stat1.health + stat2.health;

        return result;
    }
}
