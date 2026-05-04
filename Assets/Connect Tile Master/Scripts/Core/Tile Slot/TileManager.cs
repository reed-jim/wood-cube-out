using System;
using System.Collections.Generic;
using System.Linq;
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
                tile.Init(ItemCategory.Ball, tileIconContainer.categoryIconSets[0].icons[0]);
                // tile.Init(categoryIconSet.itemCategory, categoryIconSet.icons[categoryIconIndex]);

                Tween.LocalPositionY(tile.transform, grids[i].position.y, duration: 0.3f);

                grids[i].tile = tile;
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

        int currentCategoryIndex = 0;
        int categoryIconIndex = tileIconContainer.categoryIconSets[currentCategoryIndex].icons.Length;

        grids = new Grid[col * row];

        float leftMost = 0;
        float rightMost = 0;
        float topMost = 0;
        float bottomMost = 0;

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
            if (categoryIconIndex >= tileIconContainer.categoryIconSets[currentCategoryIndex].icons.Length)
            {
                currentCategoryIndex = UnityEngine.Random.Range(0, tileIconContainer.categoryIconSets.Length);
                categoryIconIndex = 0;
            }

            CategoryIconSet categoryIconSet = tileIconContainer.categoryIconSets[currentCategoryIndex];

            float xPos = xIndex * tileSpacing - totalWidth / 2f;
            float yPos = yIndex * tileSpacing - totalHeight / 2f;

            Vector3 spawnPos = new Vector3(xPos, yPos, 0);

            BaseTile tile = ObjectPoolingEverything.GetFromPool<BaseTile>(GameConstants.tile);

            tile.transform.SetParent(tileContainer);
            tile.transform.localPosition = spawnPos;
            tile.Init(categoryIconSet.itemCategory, categoryIconSet.icons[categoryIconIndex]);

            categoryIconIndex++;

            tiles.Add(tile);

            return tile;
        }
    }
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
