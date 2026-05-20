using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InventoryScreen : BasePopup
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text armorText;

    [SerializeField] private EquippedInventoryItemUI[] equippedInventoryItemUIs;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        BaseCharacter.equipInventoryItemEvent += UpdateUI;
        BaseCharacter.updateEquippedInventoryItemEvent += UpdateEquippedInventoryItems;
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        BaseCharacter.equipInventoryItemEvent -= UpdateUI;
        BaseCharacter.updateEquippedInventoryItemEvent -= UpdateEquippedInventoryItems;
    }

    public void UpdateUI()
    {
        float totalHealth = CharacterManager.Ins.mainCharacter.inventoryItems.Sum(i => i.health.currentValue);
        float totalArmor = CharacterManager.Ins.mainCharacter.inventoryItems.Sum(i => i.armor.currentValue);

        healthText.text = $"{totalHealth}";
        armorText.text = $"{totalArmor}";
    }

    public void UpdateEquippedInventoryItems(List<InventoryItem> inventoryItems)
    {
        List<EquippedInventoryItemUI> remainingEquippedInventoryItemUIs = new List<EquippedInventoryItemUI>();

        remainingEquippedInventoryItemUIs.AddRange(equippedInventoryItemUIs);

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            InventoryItem inventoryItem = inventoryItems[i];

            EquippedInventoryItemUI equippedInventoryItemUI = equippedInventoryItemUIs.First(i => i.inventoryItemType == inventoryItem.itemType);

            equippedInventoryItemUI.Equip(inventoryItem);

            remainingEquippedInventoryItemUIs.Remove(equippedInventoryItemUI);
        }

        for (int i = 0; i < remainingEquippedInventoryItemUIs.Count; i++)
        {
            EquippedInventoryItemUI equippedInventoryItemUI = remainingEquippedInventoryItemUIs[i];

            equippedInventoryItemUI.Unequip();
        }
    }
}
