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
    public static readonly Color[] tierColors =
    {
        new Color32(180, 180, 180, 255), // Common - Gray
        new Color32( 80, 200, 120, 255), // Uncommon - Green
        new Color32( 70, 140, 255, 255), // Rare - Blue
        new Color32(180,  90, 255, 255), // Epic - Purple
        new Color32(255, 170,  40, 255), // Legendary - Orange
        new Color32(255,  60, 120, 255), // Mythic - Crimson/Pink
        new Color32(  0, 255, 220, 255), // Exotic - Cyan/Turquoise
        new Color32(255, 255, 120, 255)  // Unique - Gold-Yellow
    };

    public InventoryDataContainer inventoryDataContainer;

    private InventoryItemData _inventoryItemData;

    public static event Action<ScreenRoute> switchRouteEvent;

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
        switchRouteEvent?.Invoke(ScreenRoute.Inventory);
    }

    public void Setup(InventoryItemData inventoryItemData)
    {
        nameText.text = inventoryItemData.name;

        icon.sprite = inventoryItemData.icon;
        // tierImage.sprite = tierSprites[(int)inventoryItemData.tier];
        outline.effectColor = tierColors[(int)inventoryItemData.tier];

        _inventoryItemData = inventoryItemData;
    }

    private void HideImmediately()
    {
        gameObject.SetActive(false);
    }
}
