using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class InventoryItemUI : PageViewItem
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image icon;
    [SerializeField] private Image tierImage;
    [SerializeField] private UnityEngine.UI.Outline outline;

    public Sprite[] tierSprites;

    public InventoryDataContainer inventoryDataContainer;

    private InventoryItemData _inventoryItemData;

    public static event Action<InventoryItemData> showInventoryItemDetailEvent;

    public override void Setup(int index)
    {
        if (index < inventoryDataContainer.items.Count)
        {
            Setup(inventoryDataContainer.items[index]);
        }
        else
        {
            HideImmediately();
        }
    }

    protected override void Select()
    {
        showInventoryItemDetailEvent?.Invoke(_inventoryItemData);
    }

    public void Setup(InventoryItemData inventoryItemData)
    {
        nameText.text = inventoryItemData.name;

        icon.sprite = inventoryItemData.icon;
        // tierImage.sprite = tierSprites[(int)inventoryItemData.tier];
        outline.effectColor = InventoryDataContainer.GetTierColor(inventoryItemData.tier);

        _inventoryItemData = inventoryItemData;
    }

    private void HideImmediately()
    {
        gameObject.SetActive(false);
    }
}
