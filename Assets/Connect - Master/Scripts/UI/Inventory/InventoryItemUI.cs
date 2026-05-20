using System;
using Coffee.UIEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class InventoryItemUI : PageViewItem
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image icon;
    [SerializeField] private UIEffect effectUI;

    public InventoryDataContainer inventoryDataContainer;

    private InventoryItem _inventoryItem;

    public static event Action<InventoryItem> showInventoryItemDetailEvent;

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
        if (_inventoryItem != null)
        {
            showInventoryItemDetailEvent?.Invoke(_inventoryItem);
        }
    }

    public void Setup(InventoryItem inventoryItem)
    {
        nameText.text = inventoryItem.name;

        icon.sprite = inventoryItem.icon;
        effectUI.edgeColor = InventoryDataContainer.GetTierColor(inventoryItem.tier);

        _inventoryItem = inventoryItem;
    }

    private void HideImmediately()
    {
        gameObject.SetActive(false);
    }
}
