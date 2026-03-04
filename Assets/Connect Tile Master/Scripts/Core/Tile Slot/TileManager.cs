using System;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : Singleton<TileManager>
{
    public Transform tileContainer;

    [SerializeField] private List<TileSlot> tileSlots;
    [SerializeField] private List<BaseTile> tiles;


    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        SpawnTiles();
    }

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
            TileSlot tileSlot = tileSlots[i];

            if (tileSlot.ItemCategory == tile.ItemCategory)
            {
                tile.transform.position = tileSlot.transform.position;

                tileSlot.AddTile(tile);

                break;
            }
        }
    }

    private void SpawnTiles()
    {
        int col = 5;
        int row = 5;
        float tileSpacing = 1.1f * 200;

        float totalWidth = (col - 1) * tileSpacing;
        float totalHeight = (row - 1) * tileSpacing;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                float xPos = j * tileSpacing - totalWidth / 2f;
                float zPos = i * tileSpacing - totalHeight / 2f;

                Vector3 spawnPos = new Vector3(xPos, 0f, zPos);

                BaseTile tile = ObjectPoolingEverything.GetFromPool<BaseTile>(GameConstants.tile);

                tile.transform.SetParent(tileContainer);
                tile.transform.localPosition = spawnPos;
                tile.ItemCategory = CommonUtil.GetRandomEnumValue<ItemCategory>();

                tiles.Add(tile);
            }
        }
    }
}
