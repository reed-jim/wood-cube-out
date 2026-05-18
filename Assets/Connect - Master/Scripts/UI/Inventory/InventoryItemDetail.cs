using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemDetail : BasePopup
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text tierText;

    [SerializeField] private Button equipButton;

    private InventoryItem _inventoryItem;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        InventoryItemUI.showInventoryItemDetailEvent += ShowItemDetail;

        equipButton.onClick.AddListener(Equip);
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

        Show();

        _inventoryItem = inventoryItem;
    }

    private void Equip()
    {
        CharacterManager.Ins.mainCharacter.EquipInventoryItem(_inventoryItem);

        Hide();
    }
}
