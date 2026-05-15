using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : PageViewItem
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image tierImage;

    public Sprite[] tierSprites;

    public InventoryItemData inventoryItemData;

    public void Setup(InventoryItemData inventoryItemData)
    {
        nameText.text = inventoryItemData.name;

        tierImage.sprite = tierSprites[(int)inventoryItemData.tier];

        this.inventoryItemData = inventoryItemData;
    }
}
