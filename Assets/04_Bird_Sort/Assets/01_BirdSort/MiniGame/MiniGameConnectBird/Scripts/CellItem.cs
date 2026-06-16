using UnityEngine;

public class CellItem
{
    public CELL_ITEM_TYPE cellItemType;
    public string itemString;

    public CellItem(CELL_ITEM_TYPE cellItemType, string itemString)
    {
        this.cellItemType = cellItemType;
        this.itemString = itemString;
    }
}

