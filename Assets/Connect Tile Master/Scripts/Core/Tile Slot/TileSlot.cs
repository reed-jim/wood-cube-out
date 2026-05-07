using System.Collections.Generic;
using UnityEngine;

public enum ItemCategory
{
    Vegetable,
    Food,
    Animal,
    Drink,
    Vacation,
    School,
    Fruit,
    Fastfood,
    None
}

public class TileSlot : MonoBehaviour
{
    [SerializeField] private BaseTile tile;
    public TileSlotContainer tileSlotContainer;

    public bool isShelf;

    public BaseTile Tile
    {
        get => tile;
        set => tile = value;
    }

    public void AddTile(BaseTile tile)
    {
        this.tile = tile;

        if (!isShelf)
        {
            tile.IsOnValidSlot = true;
        }
    }

    public void ClearTile()
    {
        if (tile != null)
        {
            if (!isShelf)
            {
                tile.gameObject.SetActive(false);
            }

            tile.IsDone = true;
            tile = null;
        }
    }
}
