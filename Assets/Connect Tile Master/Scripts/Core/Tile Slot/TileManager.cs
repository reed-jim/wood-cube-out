using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class TileManager : Singleton<TileManager>
{
    public Transform tileContainer;

    [SerializeField] private List<TileSlotContainer> tileSlotContainers;
    [SerializeField] private List<TileSlot> additionalTileSlots;
    [SerializeField] private List<BaseTile> tiles;


    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        SpawnTiles();
    }

    public void AddTile(BaseTile tile)
    {
        tiles.Add(tile);
    }

    // public void SpawnSlot()
    // {
    //     List<ItemCategory> itemCategoryPool = new List<ItemCategory>();

    //     foreach (BaseTile tile in tiles)
    //     {
    //         itemCategoryPool.Add(tile.ItemCategory);
    //     }

    //     TileSlot tileSlot = ObjectPoolingEverything.GetFromPool<TileSlot>(GameConstants.tileSlot);

    //     tileSlot.ItemCategory = ItemCategory.Ball;
    // }

    public void MoveTileToSlot(BaseTile tile)
    {
        bool isFound = false;

        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            TileSlotContainer tileSlotContainer = tileSlotContainers[i];

            if (tileSlotContainer.ItemCategory == tile.ItemCategory)
            {
                TileSlot tileSlot = tileSlotContainer.GetAvailableTileSlot();

                if (tileSlot != null)
                {
                    Tween.Position(tile.transform, tileSlot.transform.position, duration: 0.3f).OnComplete(() =>
                    {
                        if (tileSlotContainer.IsLastSlot(tileSlot))
                        {
                            tileSlotContainer.ClearTiles();
                            tileSlotContainer.Init();
                        }
                    });

                    tileSlot.AddTile(tile);

                    isFound = true;

                    break;
                }
            }
        }

        if (!isFound)
        {
            MoveTileToAdditionalSlot(tile);
        }
    }

    private void MoveTileToAdditionalSlot(BaseTile tile)
    {
        for (int i = 0; i < additionalTileSlots.Count; i++)
        {
            TileSlot tileSlot = additionalTileSlots[i];

            if (additionalTileSlots[i].Tile == null)
            {
                Tween.Position(tile.transform, tileSlot.transform.position, duration: 0.3f).OnComplete(() =>
                {

                });

                additionalTileSlots[i].AddTile(tile);

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
                float yPos = i * tileSpacing - totalHeight / 2f;

                Vector3 spawnPos = new Vector3(xPos, yPos, 0);

                BaseTile tile = ObjectPoolingEverything.GetFromPool<BaseTile>(GameConstants.tile);

                tile.transform.SetParent(tileContainer);
                tile.transform.localPosition = spawnPos;
                tile.Init();

                tiles.Add(tile);
            }
        }
    }
}
