using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public RectTransform rectTransform;
    public Image containerImage;

    public bool isShelf;
    public bool isLocked;

    public BaseTile Tile
    {
        get => tile;
        set => tile = value;
    }

    public Vector2 Size => rectTransform.sizeDelta;

    public void AddTile(BaseTile tile)
    {
        this.tile = tile;

        if (!isShelf)
        {
            tile.IsOnValidSlot = true;
        }

        tile.IsOnSlot = true;
    }

    public void ClearTile()
    {
        if (tile != null)
        {
            if (!isShelf)
            {
                tile.gameObject.SetActive(false);
                tile.IsDone = true;
            }

            tile = null;
        }
    }
}
