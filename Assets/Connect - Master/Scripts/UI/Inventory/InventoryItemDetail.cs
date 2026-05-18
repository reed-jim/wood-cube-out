using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemDetail : BasePopup
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text tierText;

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

    private void ShowItemDetail(InventoryItemData inventoryItemData)
    {
        nameText.text = inventoryItemData.name;
        icon.sprite = inventoryItemData.icon;
        tierText.text = $"{inventoryItemData.tier}";
        tierText.color = InventoryDataContainer.GetTierColor(inventoryItemData.tier);

        Show();
    }
}
