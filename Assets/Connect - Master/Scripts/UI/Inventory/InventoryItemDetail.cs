using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemDetail : BasePopup
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text tierText;
    [SerializeField] private TMP_Text specialEffectText;

    [SerializeField] private Button equipButton;
    [SerializeField] private TMP_Text equipButtonText;

    private InventoryItem _inventoryItem;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        InventoryItemUI.showInventoryItemDetailEvent += ShowItemDetail;
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        InventoryItemUI.showInventoryItemDetailEvent -= ShowItemDetail;
    }

    private void ShowItemDetail(InventoryItem inventoryItem)
    {
        nameText.text = inventoryItem.name;
        icon.sprite = inventoryItem.icon;
        tierText.text = $"{inventoryItem.tier}";
        tierText.color = InventoryDataContainer.GetTierColor(inventoryItem.tier);
        specialEffectText.text = inventoryItem.GetSpecialEffectDescription();

        bool isItemEquipped = CharacterManager.Ins.mainCharacter.IsInventoryItemEquipped(inventoryItem);

        equipButtonText.text = isItemEquipped ? "Unequip" : "Equip";

        equipButton.onClick.RemoveAllListeners();

        if (isItemEquipped)
        {
            equipButton.onClick.AddListener(Unequip);
        }
        else
        {
            equipButton.onClick.AddListener(Equip);
        }

        Show();

        _inventoryItem = inventoryItem;
    }

    private void Equip()
    {
        CharacterManager.Ins.mainCharacter.EquipInventoryItem(_inventoryItem);

        Hide();
    }

    private void Unequip()
    {
        CharacterManager.Ins.mainCharacter.UnequipInventoryItem(_inventoryItem);

        Hide();
    }
}
