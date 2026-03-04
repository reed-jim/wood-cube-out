using System.Collections.Generic;
using UnityEngine;

public enum ItemCategory
{
    Ball,
    Food,
    Animal
}

public class TileSlot : MonoBehaviour
{
    [SerializeField] private BaseTile tile;

    public BaseTile Tile
    {
        get => tile;
        set => tile = value;
    }

    public void AddTile(BaseTile tile)
    {
        this.tile = tile;
    }

    public void ClearTile()
    {
        if (tile != null)
        {
            tile.gameObject.SetActive(false);

            tile = null;
        }
    }
}
