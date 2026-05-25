using System;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Utilities;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using static GameEnum;

public class TileManager : Singleton<TileManager>
{
    [SerializeField] private List<TileSlotContainer> tileSlotContainers;
    [SerializeField] private List<TileSlot> additionalTileSlots;
    [SerializeField] public List<BaseTile> tiles;
    [SerializeField] private Grid[] grids;
    [SerializeField] private TileIconContainer tileIconContainer;

    public RectTransform tileContainer;
    public TMP_Text setCountText;
    public RectTransform outsideBox;
    public CanvasGroup outsideBoxCanvasGroup;
    public TMP_Text outsideTileCountText;

    public int difficultyLevel;
    public int totalSlotInShelf;
    private List<ItemCategory> itemCategoryPool;
    private int totalSet;
    private int remainingSet;
    private int remainingSlotContainer;
    private float highestYGridPosition;
    public LevelItem levelItem;

    public int NumLockedTileSlotContainer => tileSlotContainers.Count(item => item.isLocked);

    public static event Action winLevelEvent;
    public static event Action loseLevelEvent;

    #region Life Cycle
    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        RevivePopup.reviveEvent += OnRevive;

        totalSlotInShelf = 5;
    }

    void OnDestroy()
    {
        RevivePopup.reviveEvent -= OnRevive;
    }
    #endregion

    #region Level
    public void SetupLevel(LevelItem levelItem)
    {
        this.levelItem = levelItem;

        ArrangeSlotsInShelf();

        RemoveOldTiles();

        SpawnTiles(levelItem.poolCategory.Length);

        if (levelItem.isTutorial)
        {
            TutorialLevelOne.Ins.StartTutorial();
        }
        else
        {
            TutorialLevelOne.Ins.HideTutorial();
        }

        SpawnSlots();
    }

    private void RemoveOldTiles()
    {
        foreach (var tileToRemove in tiles)
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.tile, tileToRemove.gameObject);
        }

        tiles = new List<BaseTile>();
    }
    #endregion

    #region Slot
    public void SpawnSlot(int slotIndex)
    {
        TileSlotContainer tileSlotContainer = tileSlotContainers[slotIndex];

        tileSlotContainer.ClearTiles();

        ItemCategory nextCategory = GetNextCategoryForSlotBasedOnDifficulty(difficultyLevel);

        if (nextCategory == ItemCategory.None)
        {
            HideSlot(slotIndex);

            return;
        }

        tileSlotContainer.Complete(nextCategory, onCompleteAction: () =>
        {
            MoveTilesFromShelfToBox();
            MoveTilesFromOutsideBoxToSlots();
        });

        remainingSet--;
        remainingSlotContainer--;

        UpdateSetCountText();
    }

    public void HideSlot(int slotIndex)
    {
        TileSlotContainer tileSlotContainer = tileSlotContainers[slotIndex];

        tileSlotContainer.Hide(() =>
        {
            CheckWinLevel();
        });

        remainingSet--;

        UpdateSetCountText();
    }

    public void SpawnSlots()
    {
        List<ItemCategory> categories = GetNextCategories(tileSlotContainers.Count);

        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            TileSlotContainer tileSlotContainer = tileSlotContainers[i];

            if (categories.Count > 0)
            {
                tileSlotContainer.Init(categories[0]);
            }
            else
            {
                tileSlotContainer.Init();
            }

            float initialPositionY = tileSlotContainer.transform.localPosition.y;

            Tween.LocalPositionY(tileSlotContainer.transform, initialPositionY + 1600, duration: 0.7f)
            .OnComplete(() =>
            {
                tileSlotContainer.ClearTiles();

                Tween.LocalPositionY(tileSlotContainer.transform, initialPositionY, duration: 0.3f);
            });

            if (categories.Count > 0)
            {
                categories.RemoveAt(0);
            }
        }
    }
    #endregion

    #region Category
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

    private ItemCategory GetNextCategoryBasedOnDifficulty(int difficultyLevel)
    {
        // The higher the score, the easier
        List<ItemCategory> categoriesOfTileSlotContainers = new List<ItemCategory>();

        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            TileSlotContainer tileSlotContainer = tileSlotContainers[i];

            if (tileSlotContainer.isLocked)
            {
                continue;
            }

            if (!categoriesOfTileSlotContainers.Contains(tileSlotContainer.ItemCategory))
            {
                categoriesOfTileSlotContainers.Add(tileSlotContainer.ItemCategory);
            }
        }

        Dictionary<ItemCategory, int> scoreOfEachCategory = new Dictionary<ItemCategory, int>();

        for (int i = 0; i < categoriesOfTileSlotContainers.Count; i++)
        {
            CommonUtil.SumValueOfDictionary(scoreOfEachCategory, categoriesOfTileSlotContainers[i], 1000);
        }

        for (int i = 0; i < itemCategoryPool.Count; i++)
        {
            CommonUtil.SumValueOfDictionary(scoreOfEachCategory, itemCategoryPool[i], 1);
        }

        scoreOfEachCategory = scoreOfEachCategory.OrderByDescending(item => item.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        ItemCategory[] sortedCategory = scoreOfEachCategory.OrderByDescending(item => item.Value)
            .Select(kvp => kvp.Key)
            .ToArray();

        // Debug
        // foreach (var item in scoreOfEachCategory)
        // {
        //     Debug.Log($"scoreOfEachCategory {item.Key} {item.Value}");
        // }

        for (int i = difficultyLevel; i < scoreOfEachCategory.Count; i++)
        {
            if (itemCategoryPool.Contains(sortedCategory[i]))
            {
                itemCategoryPool.Remove(sortedCategory[i]);

                return sortedCategory[i];
            }
        }

        foreach (var item in scoreOfEachCategory)
        {
            ItemCategory category = item.Key;
            int score = item.Value;
        }

        // fallback return first value of itemCategoryPool
        ItemCategory nextCategory = itemCategoryPool.First();

        itemCategoryPool.RemoveAt(0);

        return nextCategory;
    }

    private ItemCategory GetNextCategoryForSlotBasedOnDifficulty(int difficultyLevel)
    {
        // The higher the score, the easier
        Dictionary<ItemCategory, int> numTilePerCategory = new Dictionary<ItemCategory, int>();

        for (int i = 0; i < tiles.Count; i++)
        {
            BaseTile tile = tiles[i];

            CommonUtil.SumValueOfDictionary(numTilePerCategory, tile.ItemCategory, 1);
        }

        Dictionary<ItemCategory, int> scoreOfEachCategory = new Dictionary<ItemCategory, int>();

        foreach (var item in numTilePerCategory)
        {
            int addedScore = item.Value;

            if (item.Value >= 4)
            {
                addedScore *= 10;
            }

            CommonUtil.SumValueOfDictionary(scoreOfEachCategory, item.Key, addedScore);
        }

        for (int i = 0; i < itemCategoryPool.Count; i++)
        {
            CommonUtil.SumValueOfDictionary(scoreOfEachCategory, itemCategoryPool[i], 1);
        }

        scoreOfEachCategory = scoreOfEachCategory.OrderByDescending(item => item.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        ItemCategory[] sortedCategory = scoreOfEachCategory.OrderByDescending(item => item.Value)
            .Select(kvp => kvp.Key)
            .ToArray();

        // Debug
        // foreach (var item in scoreOfEachCategory)
        // {
        //     Debug.Log($"scoreOfEachCategory {item.Key} {item.Value}");
        // }

        for (int i = difficultyLevel; i < scoreOfEachCategory.Count; i++)
        {
            if (!tileSlotContainers.Any(item => item.ItemCategory == sortedCategory[i]))
            {
                int remaningTileOfCategory = tiles.Where(i => !i.IsOnValidSlot).Count(item => item.ItemCategory == sortedCategory[i]) +
                    itemCategoryPool.Count(item => item == sortedCategory[i]);

                if (remaningTileOfCategory >= 4)
                {
                    return sortedCategory[i];
                }
            }
        }

        for (int i = difficultyLevel; i < scoreOfEachCategory.Count; i++)
        {
            int remaningTileOfCategory = tiles.Where(i => !i.IsOnValidSlot).Count(item => item.ItemCategory == sortedCategory[i]) +
                    itemCategoryPool.Count(item => item == sortedCategory[i]);

            if (remaningTileOfCategory >= 4)
            {
                return sortedCategory[i];
            }
        }

        return ItemCategory.None;
    }

    private List<ItemCategory> GetItemCategoryPool(int totalSet)
    {
        List<ItemCategory> itemCategoryPool = new List<ItemCategory>();

        ItemCategory currentItemCategory = ItemCategory.Vegetable;

        for (int i = 0; i < totalSet * 4; i++)
        {
            if (i % 4 == 0)
            {
                currentItemCategory = levelItem.poolCategory[i / 4];
            }

            itemCategoryPool.Add(currentItemCategory);
        }

        itemCategoryPool.Shuffle();

        return itemCategoryPool;
    }
    #endregion

    #region Move Tiles  
    public void MoveTileToSlot(BaseTile tile, Action onCompleteAction)
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
                    Tween.Position(tile.transform, tileSlot.transform.position, duration: 0.3f).OnComplete(() =>
                    {
                        tile.transform.SetParent(tileSlot.transform);

                        if (tileSlotContainer.IsLastSlot(tileSlot))
                        {
                            bool isAnyRemainingTile = tiles.Any(item => !item.IsDone);

                            if (remainingSlotContainer > 0 && isAnyRemainingTile)
                            {
                                SpawnSlot(index);
                            }
                            else
                            {
                                HideSlot(index);
                            }
                        }

                        DropTileDown(tile);

                        onCompleteAction?.Invoke();
                    });

                    tileSlot.AddTile(tile);

                    isFound = true;

                    break;
                }
            }
        }

        if (!isFound)
        {
            MoveTileToAdditionalSlot(tile, onCompleteAction);
        }
    }

    public void MoveTileToSlotFromShelf(BaseTile tile, TileSlot slot, int tileSlotContainerIndex)
    {
        TileSlotContainer tileSlotContainer = tileSlotContainers[tileSlotContainerIndex];

        Tween.Position(tile.transform, slot.transform.position, duration: 0.3f).OnComplete(() =>
        {
            tile.transform.SetParent(slot.transform);

            if (tileSlotContainer.IsLastSlot(slot))
            {
                bool isAnyRemainingTile = tiles.Any(item => !item.IsDone);

                if (remainingSlotContainer > 0 && isAnyRemainingTile)
                {
                    SpawnSlot(tileSlotContainerIndex);
                }
                else
                {
                    HideSlot(tileSlotContainerIndex);
                }
            }

            // DropTileDown(tile);
        });

        slot.AddTile(tile);
    }

    private void MoveTileToAdditionalSlot(BaseTile tile, Action onCompleteAction)
    {
        bool isFound = false;

        for (int i = 0; i < additionalTileSlots.Count; i++)
        {
            TileSlot tileSlot = additionalTileSlots[i];

            if (!additionalTileSlots[i].isLocked && additionalTileSlots[i].Tile == null)
            {
                tile.transform.SetParent(tileSlot.transform);

                Tween.Position(tile.transform, tileSlot.transform.position, duration: 0.3f).OnComplete(() =>
                {
                    DropTileDown(tile);

                    if (IsAllShelfFilled())
                    {
                        loseLevelEvent?.Invoke();
                    }

                    isFound = true;

                    onCompleteAction?.Invoke();
                });

                additionalTileSlots[i].AddTile(tile);

                break;
            }
        }

        if (!isFound)
        {
            onCompleteAction?.Invoke();
        }

        bool IsAllShelfFilled()
        {
            for (int i = 0; i < additionalTileSlots.Count; i++)
            {
                if (!additionalTileSlots[i].isLocked && additionalTileSlots[i].Tile == null)
                {
                    return false;
                }
            }

            return true;
        }
    }

    private void MoveTilesFromShelfToBox()
    {
        for (int i = 0; i < additionalTileSlots.Count; i++)
        {
            TileSlot tileSlot = additionalTileSlots[i];

            if (tileSlot.Tile != null)
            {
                TileSlot validTileSlot = GetEmptyTileSlot(tileSlot.Tile, out int tileSlotContainerIndex);

                if (validTileSlot != null)
                {
                    MoveTileToSlotFromShelf(tileSlot.Tile, validTileSlot, tileSlotContainerIndex);

                    tileSlot.ClearTile();
                }
            }
        }

        TileSlot GetEmptyTileSlot(BaseTile tile, out int tileSlotContainerIndex)
        {
            tileSlotContainerIndex = 0;

            for (int i = 0; i < tileSlotContainers.Count; i++)
            {
                if (tileSlotContainers[i].isLocked)
                {
                    continue;
                }

                for (int j = 0; j < tileSlotContainers[i].TileSlots.Length; j++)
                {
                    if (tileSlotContainers[i].ItemCategory == tile.ItemCategory && tileSlotContainers[i].TileSlots[j].Tile == null)
                    {
                        tileSlotContainerIndex = i;

                        return tileSlotContainers[i].TileSlots[j];
                    }
                }
            }

            return null;
        }
    }

    private void DropTileDown(BaseTile movedTile)
    {
        if (!grids.Any(item => item.tile == movedTile))
        {
            return;
        }

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
                if (affectedGrids[i + 1].tile != null)
                {
                    Tween.LocalPositionY(affectedGrids[i + 1].tile.transform, affectedGrids[i].position.y, duration: 0.3f);

                    affectedGrids[i].tile = affectedGrids[i + 1].tile;
                    affectedGrids[i + 1].tile = null;
                }
            }
        }

        SpawnTilesForEmptyGrid();
    }
    #endregion

    #region Spawn Tiles
    private void SpawnTilesForEmptyGrid()
    {
        if (itemCategoryPool.Count <= 0)
        {
            return;
        }

        for (int i = 0; i < grids.Length; i++)
        {
            if (grids[i].tile == null)
            {
                BaseTile tile = ObjectPoolingEverything.GetFromPool<BaseTile>(GameConstants.tile);

                tiles.Add(tile);

                Vector3 spawnPos = grids[i].position.ChangeY(highestYGridPosition + 200);

                tile.transform.SetParent(tileContainer);
                tile.transform.localPosition = spawnPos;

                ItemCategory nextCategory = GetNextCategoryBasedOnDifficulty(difficultyLevel);

                tile.Init(nextCategory, tileIconContainer.GetRandomIconOfCategory(nextCategory));

                Tween.LocalPositionY(tile.transform, grids[i].position.y, duration: 0.3f);

                grids[i].tile = tile;
            }
        }
    }

    private void SpawnTiles(int totalSet)
    {
        int col = 5;
        int row = GetRandomRow(col, 4, 6);
        float tileSpacing = 1.1f * 200;

        float totalWidth = (col - 1) * tileSpacing;
        float totalHeight = (row - 1) * tileSpacing;

        int currentCategoryIndex;
        int categoryIconIndex;

        grids = new Grid[col * row];

        float leftMost = 0;
        float rightMost = 0;
        float topMost = 0;
        float bottomMost = 0;

        this.totalSet = totalSet;
        remainingSet = totalSet;
        remainingSlotContainer = totalSet - 2;

        UpdateSetCountText();

        itemCategoryPool = GetItemCategoryPool(totalSet);

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

            tile.gameObject.SetActive(true);
            tile.transform.SetParent(tileContainer);
            tile.transform.localPosition = spawnPos;
            tile.Init(categoryIconSet.itemCategory, categoryIconSet.icons[categoryIconIndex]);

            tiles.Add(tile);

            return tile;
        }
    }
    #endregion

    #region Text
    private void UpdateSetCountText()
    {
        setCountText.text = $"{remainingSet}/{totalSet}";

        SaferioTween.Punch(setCountText.transform, 0.1f);
    }

    private void UpdateOutsideTileCountText()
    {
        int numOutsideTile = tiles.Count(item => item.IsOutside);

        outsideTileCountText.text = $"{numOutsideTile}";

        SaferioTween.Punch(outsideTileCountText.transform, 0.1f);
    }
    #endregion

    #region Event
    private void OnRevive(BoosterType boosterType)
    {
        if (boosterType == BoosterType.UnlockScrewBox)
        {
            for (int i = 0; i < tileSlotContainers.Count; i++)
            {
                TileSlotContainer tileSlotContainer = tileSlotContainers[i];

                if (tileSlotContainer.isLocked)
                {
                    tileSlotContainer.Unlock(() =>
                    {
                        MoveTilesFromShelfToBox();
                        MoveTilesFromOutsideBoxToSlots();
                    });
                }
            }
        }
        else
        {
            UseSweeperBooster();
        }
    }
    #endregion

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

    #region Booster
    public void UseAddHoleBooster()
    {
        if (totalSlotInShelf < 7)
        {
            totalSlotInShelf++;

            ArrangeSlotsInShelf();
        }
    }

    private void ArrangeSlotsInShelf()
    {
        float distance = 1.15f * additionalTileSlots[0].Size.x;
        float totalWidth = (totalSlotInShelf - 1) * distance;

        Vector3 position = additionalTileSlots[0].transform.localPosition;

        for (int i = 0; i < additionalTileSlots.Count; i++)
        {
            if (!additionalTileSlots[i].gameObject.activeInHierarchy)
            {
                additionalTileSlots[i].containerImage.color = additionalTileSlots[i].containerImage.color.ChangeAlpha(0);
                Tween.Alpha(additionalTileSlots[i].containerImage, 1, duration: 0.3f);
            }

            additionalTileSlots[i].gameObject.SetActive(i < totalSlotInShelf);
            additionalTileSlots[i].isLocked = i >= totalSlotInShelf;

            position.x = -totalWidth / 2f + i * distance;

            Tween.LocalPositionX(additionalTileSlots[i].transform, position.x, duration: 0.3f);
        }
    }

    public void UseShuffleBooster()
    {
        List<ItemCategory> categoryPoolForAllTileLeft = new List<ItemCategory>();
        List<BaseTile> tilesToShuffle = new List<BaseTile>();

        for (int i = 0; i < tiles.Count; i++)
        {
            BaseTile tile = tiles[i];

            if (tile.IsOnSlot)
            {
                continue;
            }

            categoryPoolForAllTileLeft.Add(tile.ItemCategory);
            tilesToShuffle.Add(tile);
        }

        for (int i = 0; i < itemCategoryPool.Count; i++)
        {
            categoryPoolForAllTileLeft.Add(itemCategoryPool[i]);
        }

        List<ItemCategory> priorityCategory = new List<ItemCategory>();

        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            if (!tileSlotContainers[i].isLocked && !priorityCategory.Contains(tileSlotContainers[i].ItemCategory))
            {
                priorityCategory.Add(tileSlotContainers[i].ItemCategory);
            }
        }

        int totalActiveTiles = tilesToShuffle.Count();

        List<ItemCategory> shuffledCategory = new List<ItemCategory>();

        List<ItemCategory> toRemoveCategory = new List<ItemCategory>();

        for (int i = 0; i < categoryPoolForAllTileLeft.Count; i++)
        {
            if (priorityCategory.Contains(categoryPoolForAllTileLeft[i]) && shuffledCategory.Count(item => item == categoryPoolForAllTileLeft[i]) < 4)
            {
                shuffledCategory.Add(categoryPoolForAllTileLeft[i]);
                toRemoveCategory.Remove(categoryPoolForAllTileLeft[i]);
            }
        }

        categoryPoolForAllTileLeft.RemoveAll(item => toRemoveCategory.Contains(item));

        categoryPoolForAllTileLeft.Shuffle();

        int currentCategoryAdded = shuffledCategory.Count;

        shuffledCategory.AddRange(categoryPoolForAllTileLeft.Take(totalActiveTiles - currentCategoryAdded));

        for (int i = 0; i < tilesToShuffle.Count; i++)
        {
            tilesToShuffle[i].Shuffle(shuffledCategory[i], tileIconContainer.GetRandomIconOfCategory(shuffledCategory[i]));
        }
    }

    public bool IsAnyTileOnShelf()
    {
        return additionalTileSlots.Any(item => item.Tile != null);
    }

    public void UseSweeperBooster()
    {
        if (!outsideBox.gameObject.activeInHierarchy)
        {
            outsideBox.gameObject.SetActive(true);

            outsideBoxCanvasGroup.alpha = 0;
            Tween.Alpha(outsideBoxCanvasGroup, 1, duration: 0.3f);
        }

        for (int i = 0; i < additionalTileSlots.Count; i++)
        {
            TileSlot tileSlot = additionalTileSlots[i];

            if (tileSlot.Tile != null)
            {
                tileSlot.Tile.MoveToOutsideBox();
                tileSlot.ClearTile();
            }
        }

        UpdateOutsideTileCountText();
    }

    public void MoveTilesFromOutsideBoxToSlots()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            BaseTile tile = tiles[i];

            if (tile.IsOutside)
            {
                bool isFound = MoveTileToSlot(tile);

                if (isFound)
                {
                    tile.IsOutside = false;
                }
            }
        }

        bool MoveTileToSlot(BaseTile tile)
        {
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
                        Tween.Alpha(tile.icon, 1, duration: 0.3f);
                        Tween.Position(tile.transform, tileSlot.transform.position, duration: 0.3f).OnComplete(() =>
                        {
                            tile.transform.SetParent(tileSlot.transform);

                            if (tileSlotContainer.IsLastSlot(tileSlot))
                            {
                                bool isAnyRemainingTile = tiles.Any(item => !item.IsDone);

                                if (remainingSlotContainer > 0 && isAnyRemainingTile)
                                {
                                    SpawnSlot(index);
                                }
                                else
                                {
                                    HideSlot(index);
                                }
                            }

                            tile.IsOutside = false;

                            UpdateOutsideTileCountText();
                        });

                        tileSlot.AddTile(tile);

                        return true;
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region Win Level
    private void CheckWinLevel()
    {
        if (remainingSet <= 0)
        {
            winLevelEvent?.Invoke();
        }
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
