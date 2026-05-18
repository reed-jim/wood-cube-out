using System.Linq;
using TMPro;
using UnityEngine;

public class InventoryScreen : BasePopup
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text armorText;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        BaseCharacter.equipInventoryItemEvent += UpdateUI;
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        BaseCharacter.equipInventoryItemEvent -= UpdateUI;
    }

    public void UpdateUI()
    {
        float totalHealth = CharacterManager.Ins.mainCharacter.inventoryItems.Sum(i => i.health.currentValue);
        float totalArmor = CharacterManager.Ins.mainCharacter.inventoryItems.Sum(i => i.armor.currentValue);

        healthText.text = $"{totalHealth}";
        armorText.text = $"{totalArmor}";
    }
}
