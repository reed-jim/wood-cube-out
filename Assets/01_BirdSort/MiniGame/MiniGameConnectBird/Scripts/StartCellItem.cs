using UnityEngine;

[System.Serializable]
public class StartCellItem
{
    public Vector2Int cellPosition;
    public CellItem cellItem;

    public StartCellItem(Vector2Int cellPosition, CellItem cellItem)
    {
        this.cellPosition = cellPosition;
        this.cellItem = cellItem;
    }
}

