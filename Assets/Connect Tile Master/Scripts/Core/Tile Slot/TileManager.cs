using System;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Utilities;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class TileManager : Singleton<TileManager>
{
    public RectTransform tileContainer;

    [SerializeField] private List<TileSlotContainer> tileSlotContainers;
    [SerializeField] private List<TileSlot> additionalTileSlots;
    [SerializeField] private List<BaseTile> tiles;
    [SerializeField] private Grid[] grids;

    [SerializeField] private TileIconContainer tileIconContainer;

    private float highestYGridPosition;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        SpawnTiles();
        SpawnSlots();
    }

    public void AddTile(BaseTile tile)
    {
        tiles.Add(tile);
    }

    public void SpawnSlot(int slotIndex)
    {
        ItemCategory nextCategory = GetNextCategory();

        TileSlotContainer tileSlotContainer = tileSlotContainers[slotIndex];

        if (!tileSlotContainer.IsActive)
        {
            tileSlotContainer.Init(nextCategory);

            float initialPositionY = tileSlotContainer.transform.localPosition.y;

            Tween.LocalPositionY(tileSlotContainer.transform, initialPositionY + 1600, duration: 0.7f)
            .OnComplete(() =>
            {
                tileSlotContainer.ClearTiles();

                Tween.LocalPositionY(tileSlotContainer.transform, initialPositionY, duration: 0.3f);
            });
        }
    }

    public void SpawnSlots()
    {
        List<ItemCategory> categories = GetNextCategories(tileSlotContainers.Count);

        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            TileSlotContainer tileSlotContainer = tileSlotContainers[i];

            if (!tileSlotContainer.IsActive)
            {
                tileSlotContainer.Init(categories[0]);

                float initialPositionY = tileSlotContainer.transform.localPosition.y;

                Tween.LocalPositionY(tileSlotContainer.transform, initialPositionY + 1600, duration: 0.7f)
                .OnComplete(() =>
                {
                    tileSlotContainer.ClearTiles();

                    Tween.LocalPositionY(tileSlotContainer.transform, initialPositionY, duration: 0.3f);
                });
            }

            if (categories.Count > 0)
            {
                categories.RemoveAt(0);
            }
        }
    }

    private ItemCategory GetNextCategory()
    {
        return tiles
            .Where(item => !item.IsDone)
            .GroupBy(t => t.ItemCategory)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
    }

    private List<ItemCategory> GetNextCategories(int n)
    {
        return tiles
            .Where(item => !item.IsDone)
            .GroupBy(t => t.ItemCategory)
            .OrderByDescending(g => g.Count())
            .Take(n)
            .Select(g => g.Key)
            .ToList();
    }

    public void MoveTileToSlot(BaseTile tile)
    {
        bool isFound = false;

        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            int index = i;

            TileSlotContainer tileSlotContainer = tileSlotContainers[i];

            if (tileSlotContainer.isLocked)
            {
                continue;
            }

            if (tileSlotContainer.ItemCategory == tile.ItemCategory)
            {
                TileSlot tileSlot = tileSlotContainer.GetAvailableTileSlot();

                if (tileSlot != null)
                {
                    tile.transform.SetParent(tileSlot.transform);

                    Tween.Position(tile.transform, tileSlot.transform.position, duration: 0.3f).OnComplete(() =>
                    {
                        if (tileSlotContainer.IsLastSlot(tileSlot))
                        {
                            SpawnSlot(index);
                        }

                        DropTileDown(tile);
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
                tile.transform.SetParent(tileSlot.transform);

                Tween.Position(tile.transform, tileSlot.transform.position, duration: 0.3f).OnComplete(() =>
                {
                    DropTileDown(tile);
                });

                additionalTileSlots[i].AddTile(tile);

                break;
            }
        }
    }

    private void DropTileDown(BaseTile movedTile)
    {
        Grid selectedTileGrid = grids.First(item => item.tile == movedTile);
        List<Grid> affectedGrids =
            grids.Where(item => item.IsSameColumn(selectedTileGrid) && item.topBound > selectedTileGrid.topBound).ToList();

        affectedGrids.Add(selectedTileGrid);
        selectedTileGrid.tile = null;
        affectedGrids = affectedGrids.OrderBy(item => item.bottomBound).ToList();

        for (int i = 0; i < affectedGrids.Count; i++)
        {
            if (affectedGrids[i].tile == null && i + 1 < affectedGrids.Count)
            {
                Tween.LocalPositionY(affectedGrids[i + 1].tile.transform, affectedGrids[i].position.y, duration: 0.3f);

                affectedGrids[i].tile = affectedGrids[i + 1].tile;
                affectedGrids[i + 1].tile = null;
            }
        }

        SpawnTilesForEmptyGrid();
    }

    private void SpawnTilesForEmptyGrid()
    {
        for (int i = 0; i < grids.Length; i++)
        {
            if (grids[i].tile == null)
            {
                BaseTile tile = ObjectPoolingEverything.GetFromPool<BaseTile>(GameConstants.tile);

                Vector3 spawnPos = grids[i].position.ChangeY(highestYGridPosition + 200);

                tile.transform.SetParent(tileContainer);
                tile.transform.localPosition = spawnPos;
                tile.Init(ItemCategory.Vegetable, tileIconContainer.categoryIconSets[0].icons[0]);
                // tile.Init(categoryIconSet.itemCategory, categoryIconSet.icons[categoryIconIndex]);

                Tween.LocalPositionY(tile.transform, grids[i].position.y, duration: 0.3f);

                grids[i].tile = tile;
            }
        }
    }

    private List<ItemCategory> GetItemCategoryPool(int col, int row)
    {
        List<ItemCategory> itemCategoryPool = new List<ItemCategory>();

        ItemCategory currentItemCategory = ItemCategory.Vegetable;

        for (int i = 0; i < col * row; i++)
        {
            if (i % 4 == 0)
            {
                currentItemCategory = (ItemCategory)UnityEngine.Random.Range(0, tileIconContainer.categoryIconSets.Length);
            }

            itemCategoryPool.Add(currentItemCategory);
        }

        itemCategoryPool.Shuffle();

        return itemCategoryPool;
    }

    private void SpawnTiles()
    {
        int col = 5;
        int row = GetRandomRow(col, 4, 8);
        float tileSpacing = 1.1f * 200;

        float totalWidth = (col - 1) * tileSpacing;
        float totalHeight = (row - 1) * tileSpacing;

        int currentCategoryIndex = 0;
        int categoryIconIndex = 0;

        grids = new Grid[col * row];

        float leftMost = 0;
        float rightMost = 0;
        float topMost = 0;
        float bottomMost = 0;

        List<ItemCategory> itemCategoryPool = GetItemCategoryPool(col, row);

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                int index = j + i * col;

                BaseTile spawnedTile = SpawnTile(j, i);

                grids[index] = new Grid()
                {
                    leftBound = spawnedTile.LeftBound,
                    rightBound = spawnedTile.RightBound,
                    topBound = spawnedTile.TopBound,
                    bottomBound = spawnedTile.BottomBound,
                    position = spawnedTile.transform.localPosition,
                    tile = spawnedTile
                };

                if (spawnedTile.TopBound > highestYGridPosition)
                {
                    highestYGridPosition = spawnedTile.TopBound;
                }

                if (spawnedTile.LeftBound < leftMost)
                {
                    leftMost = spawnedTile.LeftBound;
                }

                if (spawnedTile.RightBound > rightMost)
                {
                    rightMost = spawnedTile.RightBound;
                }

                if (spawnedTile.TopBound > topMost)
                {
                    topMost = spawnedTile.TopBound;
                }

                if (spawnedTile.BottomBound < bottomMost)
                {
                    bottomMost = spawnedTile.BottomBound;
                }

                tileContainer.sizeDelta = new Vector2(rightMost - leftMost, topMost - bottomMost);
            }
        }

        BaseTile SpawnTile(int xIndex, int yIndex)
        {
            currentCategoryIndex = (int)itemCategoryPool.First();
            itemCategoryPool.RemoveAt(0);

            CategoryIconSet categoryIconSet = tileIconContainer.categoryIconSets[currentCategoryIndex];

            categoryIconIndex = UnityEngine.Random.Range(0, categoryIconSet.icons.Length);

            float xPos = xIndex * tileSpacing - totalWidth / 2f;
            float yPos = yIndex * tileSpacing - totalHeight / 2f;

            Vector3 spawnPos = new Vector3(xPos, yPos, 0);

            BaseTile tile = ObjectPoolingEverything.GetFromPool<BaseTile>(GameConstants.tile);

            tile.transform.SetParent(tileContainer);
            tile.transform.localPosition = spawnPos;
            tile.Init(categoryIconSet.itemCategory, categoryIconSet.icons[categoryIconIndex]);

            tiles.Add(tile);

            return tile;
        }
    }

    #region Util
    private int GetRandomRow(int col, int minRow, int maxRow, int maxTries = 50)
    {
        // Fast random attempts
        for (int i = 0; i < maxTries; i++)
        {
            int row = UnityEngine.Random.Range(minRow, maxRow + 1);
            if ((col * row) % 4 == 0)
                return row;
        }

        // Guaranteed fallback
        int step;

        if (col % 4 == 0)
            step = 1;
        else if (col % 2 == 0)
            step = 2;
        else
            step = 4;

        // Find first valid row >= minRow
        int start = ((minRow + step - 1) / step) * step;

        if (start <= maxRow)
            return start;

        // Absolute guarantee fallback (if range somehow invalid)
        // Clamp to nearest valid value (even outside range if needed)
        return ((minRow + step - 1) / step) * step;
    }
    #endregion
}

[Serializable]
public class Grid
{
    public float leftBound;
    public float rightBound;
    public float topBound;
    public float bottomBound;
    public Vector3 position;
    public BaseTile tile;

    public bool IsSameColumn(Grid other)
    {
        return Mathf.Approximately(leftBound, other.leftBound) && Mathf.Approximately(rightBound, other.rightBound);
    }
}
