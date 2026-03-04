using System;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : Singleton<TileManager>
{
    [SerializeField] private List<TileSlot> tileSlots;

    [SerializeField] private List<BaseTile> tiles;

    public void AddSlot(TileSlot slot)
    {
        tileSlots.Add(slot);
    }

    public void AddTile(BaseTile tile)
    {
        tiles.Add(tile);
    }

    public void SpawnSlot()
    {
        List<ItemCategory> itemCategoryPool = new List<ItemCategory>();

        foreach (BaseTile tile in tiles)
        {
            itemCategoryPool.Add(tile.ItemCategory);
        }

        TileSlot tileSlot = ObjectPoolingEverything.GetFromPool<TileSlot>(GameConstants.tileSlot);

        tileSlot.ItemCategory = ItemCategory.Ball;
    }

    public void MoveTileToSlot(BaseTile tile)
    {
        for (int i = 0; i < tileSlots.Count; i++)
        {
            if (tileSlots[i].ItemCategory == tile.ItemCategory)
            {
                tile.gameObject.SetActive(false);

                break;
            }
        }
    }
}
