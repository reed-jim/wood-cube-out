using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.UI;


public class EquippedInventoryItemUI : InventoryItemUI
{
    [SerializeField] private Image emptyIcon;

    public InventoryItemType inventoryItemType;

    public void Equip(InventoryItem inventoryItem)
    {
        Setup(inventoryItem);

        SaferioTween.AlphaHide(emptyIcon);
    }

    public void Unequip()
    {
        SaferioTween.AlphaShow(emptyIcon);
    }
}
