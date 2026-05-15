using UnityEngine;

public class InventoryScreen : BasePopup
{
    public InventoryItemUI inventoryItemUIPrefab;

    [SerializeField] private InventoryDataContainer inventoryDataContainer;

    void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        for (int i = 0; i < inventoryDataContainer.items.Count; i++)
        {
            InventoryItemData inventoryItemData = inventoryDataContainer.items[i];

            InventoryItemUI inventoryItemUI = Instantiate(inventoryItemUIPrefab);

            inventoryItemUI.Setup(inventoryItemData);
        }
    }
}
