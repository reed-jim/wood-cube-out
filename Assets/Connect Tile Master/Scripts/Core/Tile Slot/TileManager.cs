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
    public List<ItemCategory> itemCategoryPool;
    private int totalSet;
    private int remainingSet;
    private int remainingSlotContainer;
    private float highestYGridPosition;
    public LevelItem levelItem;

    public float currentLevelTime;
    public bool isLevelActive;
    [HideInInspector] public bool isTimeFrozen;
    private Tween freezeTimeTween;

    private Dictionary<ItemCategory, List<int>> availableItemIdsPerCategory = new Dictionary<ItemCategory, List<int>>();

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

    private void Update()
    {
        if (isLevelActive && !isTimeFrozen && currentLevelTime > 0)
        {
            currentLevelTime -= Time.deltaTime;

            if (currentLevelTime <= 0)
            {
                currentLevelTime = 0;
                isLevelActive = false;
                loseLevelEvent?.Invoke();
            }
        }
    }

    protected virtual void MoreActionOnDestroy()
    {
        RevivePopup.reviveEvent -= OnRevive;
    }
    #endregion

    #region Level
    public void SetupLevel(LevelItem levelItem)
    {
        this.levelItem = levelItem;

        if (levelItem.timeLimit > 0)
        {
            currentLevelTime = levelItem.timeLimit;
            isLevelActive = true;
        }

        isTimeFrozen = false;
        if (freezeTimeTween.isAlive)
        {
            freezeTimeTween.Stop();
        }

        availableItemIdsPerCategory.Clear();

        foreach (var theme in levelItem.themeDataList)
        {
            var iconSet = tileIconContainer.categoryIconSets.FirstOrDefault(x => x.itemCategory == theme.category);
            if (iconSet != null)
            {
                int spriteCount = iconSet.icons != null ? iconSet.icons.Length : 0;
                int textCount = iconSet.text != null ? iconSet.text.Length : 0;
                int totalItems = spriteCount + textCount;

                List<int> itemIds = new List<int>();
                for (int i = 0; i < totalItems; i++)
                {
                    itemIds.Add(i);
                }
                availableItemIdsPerCategory[theme.category] = itemIds;
            }
        }

        ArrangeSlotsInShelf();

        RemoveOldTiles();

        int totalSet = levelItem.themeDataList.Count;

        SpawnTiles(totalSet);

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

        ItemCategory nextCategory = GetNextCategoryForSlotBasedOnDifficulty(difficultyLevel, tileSlotContainer);

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
                tileSlotContainer.Init(categories[0], GetTargetCardAmount(categories[0]));
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

    private ItemCategory GetNextCategoryForSlotBasedOnDifficulty(int difficultyLevel, TileSlotContainer requestingContainer)
    {
        Dictionary<ItemCategory, int> numTilePerCategory = new Dictionary<ItemCategory, int>();

        for (int i = 0; i < tiles.Count; i++)
        {
            BaseTile tile = tiles[i];

            // Skip tiles already done or already committed to a slot
            if (tile.IsDone || tile.IsOnSlot)
            {
                continue;
            }

            CommonUtil.SumValueOfDictionary(numTilePerCategory, tile.ItemCategory, 1);
        }

        for (int i = 0; i < itemCategoryPool.Count; i++)
        {
            CommonUtil.SumValueOfDictionary(numTilePerCategory, itemCategoryPool[i], 1);
        }

        // Count active tiles of each category currently on the board
        Dictionary<ItemCategory, int> boardCounts = new Dictionary<ItemCategory, int>();
        for (int i = 0; i < tiles.Count; i++)
        {
            BaseTile tile = tiles[i];
            if (tile.IsDone || tile.IsOnSlot)
            {
                continue;
            }
            CommonUtil.SumValueOfDictionary(boardCounts, tile.ItemCategory, 1);
        }

        bool isHardMode = levelItem != null && levelItem.isHard;

        // Find candidates that have enough tiles remaining
        List<ItemCategory> candidates = new List<ItemCategory>();
        foreach (var category in numTilePerCategory.Keys)
        {
            int activeContainersForCategory = tileSlotContainers.Count(item =>
                item != requestingContainer && !item.isLocked && !item.isHidden && item.ItemCategory == category);
            int totalNotDone = numTilePerCategory[category];
            int targetCardAmount = GetTargetCardAmount(category);

            if (totalNotDone >= (activeContainersForCategory + 1) * targetCardAmount)
            {
                candidates.Add(category);
            }
        }

        // Split candidates into Group A (not currently ordered elsewhere) and Group B (already ordered elsewhere)
        List<ItemCategory> groupA = new List<ItemCategory>();
        List<ItemCategory> groupB = new List<ItemCategory>();

        foreach (var cat in candidates)
        {
            int activeContainersForCategory = tileSlotContainers.Count(item =>
                item != requestingContainer && !item.isLocked && !item.isHidden && item.ItemCategory == cat);
            if (activeContainersForCategory == 0)
                groupA.Add(cat);
            else
                groupB.Add(cat);
        }

        Func<ItemCategory, int> getBoardCount = (cat) => boardCounts.ContainsKey(cat) ? boardCounts[cat] : 0;

        if (isHardMode)
        {
            groupA = groupA.OrderBy(getBoardCount).ToList();
            groupB = groupB.OrderBy(getBoardCount).ToList();
        }
        else
        {
            groupA = groupA.OrderByDescending(getBoardCount).ToList();
            groupB = groupB.OrderByDescending(getBoardCount).ToList();
        }

        if (groupA.Count > 0)
        {
            return groupA[0];
        }
        if (groupB.Count > 0)
        {
            return groupB[0];
        }

        return ItemCategory.None;
    }

    public int GetTargetCardAmount(ItemCategory category)
    {
        var theme = levelItem.themeDataList.FirstOrDefault(t => t.category == category);
        if (theme != null)
        {
            return theme.numSprite + theme.numString;
        }
        return 4; // fallback
    }

    private Dictionary<ItemCategory, int> remainingStringPerCategory = new Dictionary<ItemCategory, int>();
    private Dictionary<ItemCategory, int> remainingSpritePerCategory = new Dictionary<ItemCategory, int>();

    private List<ItemCategory> GetItemCategoryPool(int totalSet)
    {
        List<ItemCategory> itemCategoryPool = new List<ItemCategory>();
        remainingStringPerCategory.Clear();
        remainingSpritePerCategory.Clear();

        foreach (var theme in levelItem.themeDataList)
        {
            if (!remainingSpritePerCategory.ContainsKey(theme.category))
                remainingSpritePerCategory[theme.category] = 0;
            if (!remainingStringPerCategory.ContainsKey(theme.category))
                remainingStringPerCategory[theme.category] = 0;

            remainingSpritePerCategory[theme.category] += theme.numSprite;
            remainingStringPerCategory[theme.category] += theme.numString;

            int count = theme.numSprite + theme.numString;
            for (int i = 0; i < count; i++)
            {
                itemCategoryPool.Add(theme.category);
            }
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
                        tile.IsMoving = false;

                        // Hide tile and mark as done so it returns to pool and updates win condition properly
                        tile.IsDone = true;
                        tiles.Remove(tile);
                        tile.gameObject.SetActive(false);
                        tile.transform.SetParent(tileSlot.transform);

                        tileSlotContainer.SwitchItem(tile);

                        if (tileSlotContainer.IsLastSlot(tileSlot) && !tileSlotContainer.isCompleted)
                        {
                            tileSlotContainer.isCompleted = true;
                            bool isAnyRemainingTile = tiles.Any(item => !item.IsOnSlot);

                            if (remainingSlotContainer > 0 && isAnyRemainingTile)
                            {
                                SpawnSlot(index);
                            }
                            else
                            {
                                HideSlot(index);
                            }
                        }

                        onCompleteAction?.Invoke();

                        // DropTileDown(tile);
                        EmptyTile(tile);
                        SpawnTilesForEmptyGrid();
                    });

                    tileSlot.AddTile(tile);
                    tileSlotContainer.AddTile(tile);

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

        tile.IsMoving = true;
        Tween.Position(tile.transform, slot.transform.position, duration: 0.3f).OnComplete(() =>
        {
            tile.IsMoving = false;

            tile.IsDone = true;
            tiles.Remove(tile);
            tile.gameObject.SetActive(false);
            tile.transform.SetParent(slot.transform);

            tileSlotContainer.SwitchItem(tile);

            if (tileSlotContainer.IsLastSlot(slot) && !tileSlotContainer.isCompleted)
            {
                tileSlotContainer.isCompleted = true;
                bool isAnyRemainingTile = tiles.Any(item => !item.IsOnSlot);

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

    public bool HasOpenContainerOfCategory(ItemCategory category)
    {
        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            TileSlotContainer tileSlotContainer = tileSlotContainers[i];

            if (tileSlotContainer.isLocked || tileSlotContainer.isHidden || tileSlotContainer.isMoving)
            {
                continue;
            }

            if (tileSlotContainer.ItemCategory == category && tileSlotContainer.GetAvailableTileSlot() != null)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsHoveringValidContainer(ItemCategory category, Vector2 screenPoint, Camera camera)
    {
        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            TileSlotContainer tileSlotContainer = tileSlotContainers[i];

            if (tileSlotContainer.isLocked || tileSlotContainer.isHidden || tileSlotContainer.isMoving)
            {
                continue;
            }

            if (tileSlotContainer.ItemCategory == category && tileSlotContainer.GetAvailableTileSlot() != null)
            {
                RectTransform rectTransform = tileSlotContainer.GetComponent<RectTransform>();
                if (rectTransform != null && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, camera))
                {
                    return true;
                }
            }
        }

        return false;
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
                    onCompleteAction?.Invoke();

                    EmptyTile(tile);
                    SpawnTilesForEmptyGrid();
                    // DropTileDown(tile);

                    if (IsAllShelfFilled())
                    {
                        loseLevelEvent?.Invoke();
                    }

                    isFound = true;
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

    private void EmptyTile(BaseTile movedTile)
    {
        if (!grids.Any(item => item.tile == movedTile))
        {
            return;
        }

        Grid selectedTileGrid = grids.First(item => item.tile == movedTile);

        selectedTileGrid.tile = null;
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
                    BaseTile movingTile = affectedGrids[i + 1].tile;
                    movingTile.IsMoving = true;
                    Tween.LocalPositionY(movingTile.transform, affectedGrids[i].position.y, duration: 0.3f).OnComplete(() =>
                    {
                        movingTile.IsMoving = false;
                    });

                    affectedGrids[i].tile = movingTile;
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
            if (itemCategoryPool.Count <= 0)
            {
                break;
            }

            if (grids[i].tile == null)
            {
                BaseTile tile = ObjectPoolingEverything.GetFromPool<BaseTile>(GameConstants.tile);
                if (tile != null)
                {
                    tile.gameObject.SetActive(true);

                    tiles.Add(tile);

                    // Vector3 spawnPos = grids[i].position.ChangeY(highestYGridPosition + 200);

                    tile.transform.SetParent(tileContainer);
                    tile.transform.position = CardStockPile.Ins.transform.position;

                    ItemCategory nextCategory = GetNextCategoryBasedOnDifficulty(difficultyLevel);

                    InitTileWithSpriteOrString(tile, nextCategory);

                    tile.IsMoving = true;
                    Tween.LocalPosition(tile.transform, grids[i].position, duration: 0.3f).OnComplete(() =>
                    {
                        tile.IsMoving = false;
                    });

                    grids[i].tile = tile;

                    CardStockPile.Ins.DrawNewCard(tile);
                }

            }
        }
    }

    private void SpawnTiles(int totalSet)
    {
        int col = levelItem.initColumn;
        int row = levelItem.initRow;
        float tileSpacing = 1.05f * 195;

        float totalWidth = (col - 1) * tileSpacing;
        float totalHeight = (row - 1) * tileSpacing;

        int currentCategoryIndex;

        int totalCards = levelItem.themeDataList.Sum(t => t.numSprite + t.numString);
        int slotsToFill = Mathf.Min(col * row, totalCards);
        grids = new Grid[slotsToFill];

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
                if (itemCategoryPool.Count == 0) break;

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

        CardStockPile.Ins.UpdateText();

        BaseTile SpawnTile(int xIndex, int yIndex)
        {
            ItemCategory currentCategory = itemCategoryPool.First();
            itemCategoryPool.RemoveAt(0);

            CategoryIconSet categoryIconSet = tileIconContainer.categoryIconSets.First(item => item.itemCategory == currentCategory);

            float xPos = xIndex * tileSpacing - totalWidth / 2f;
            float yPos = yIndex * tileSpacing - totalHeight / 2f;

            Vector3 spawnPos = new Vector3(xPos, yPos, 0);

            BaseTile tile = ObjectPoolingEverything.GetFromPool<BaseTile>(GameConstants.tile);

            tile.gameObject.SetActive(true);
            tile.transform.SetParent(tileContainer);
            tile.transform.localPosition = spawnPos;

            InitTileWithSpriteOrString(tile, categoryIconSet.itemCategory);

            tiles.Add(tile);

            return tile;
        }
    }
    #endregion

    private void InitTileWithSpriteOrString(BaseTile tile, ItemCategory nextCategory)
    {
        CategoryIconSet iconSet = tileIconContainer.categoryIconSets.First(x => x.itemCategory == nextCategory);

        int itemId = GetUniqueItemId(nextCategory, iconSet);

        int spriteCount = iconSet.icons != null ? iconSet.icons.Length : 0;
        string textData = null;
        Sprite spriteData = null;

        if (itemId < spriteCount)
        {
            spriteData = iconSet.icons[itemId];
        }
        else
        {
            textData = iconSet.text[itemId - spriteCount];
        }

        tile.Init(nextCategory, itemId, spriteData, textData);
    }

    private int GetUniqueItemId(ItemCategory category, CategoryIconSet iconSet)
    {
        if (availableItemIdsPerCategory.ContainsKey(category) && availableItemIdsPerCategory[category].Count > 0)
        {
            int index = UnityEngine.Random.Range(0, availableItemIdsPerCategory[category].Count);
            int itemId = availableItemIdsPerCategory[category][index];
            availableItemIdsPerCategory[category].RemoveAt(index);
            return itemId;
        }

        // Fallback: return a random valid index
        int spriteCount = iconSet.icons != null ? iconSet.icons.Length : 0;
        int textCount = iconSet.text != null ? iconSet.text.Length : 0;
        int totalItems = spriteCount + textCount;
        return UnityEngine.Random.Range(0, totalItems);
    }


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



    #region Booster
    public void UseFreezeTimeBooster(float duration)
    {
        isTimeFrozen = true;

        if (freezeTimeTween.isAlive)
        {
            freezeTimeTween.Stop();
        }

        freezeTimeTween = Tween.Delay(duration).OnComplete(() =>
        {
            isTimeFrozen = false;
        });
    }

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
        for (int i = 0; i < additionalTileSlots.Count; i++)
        {
            additionalTileSlots[i].gameObject.SetActive(false);
        }
    }

    public void UseHintBooster()
    {
        TileSlotContainer activeContainer = null;
        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            if (!tileSlotContainers[i].isLocked && !tileSlotContainers[i].isHidden && !tileSlotContainers[i].isMoving && !tileSlotContainers[i].isCompleted && tileSlotContainers[i].GetAvailableTileSlot() != null)
            {
                activeContainer = tileSlotContainers[i];
                break;
            }
        }

        if (activeContainer != null)
        {
            activeContainer.SetHighlight(true);

            BaseTile highlightedTile = null;
            foreach (var tile in tiles)
            {
                if (!tile.IsOnSlot && !tile.IsOutside && !tile.IsDone && tile.ItemCategory == activeContainer.ItemCategory)
                {
                    tile.SetHighlight(true);
                    highlightedTile = tile;
                    break;
                }
            }

            if (highlightedTile != null)
            {
                highlightedTile.onInteract = () =>
                {
                    if (activeContainer != null)
                    {
                        activeContainer.SetHighlight(false);
                    }
                };
            }
            else
            {
                Tween.Delay(2f).OnComplete(() =>
                {
                    if (activeContainer != null)
                    {
                        activeContainer.SetHighlight(false);
                    }
                });
            }
        }
    }

    public bool CanUseShuffleBooster()
    {
        return tiles.Any(tile => !tile.IsOnSlot && !tile.IsOutside);
    }

    public void UseShuffleBooster()
    {
        // ── STEP 1: Identify tiles to shuffle (on grid, not on slot, not outside) ──
        // Note: tiles list only contains active (non-done) tiles since completed tiles are removed immediately.
        List<BaseTile> tilesToShuffle = tiles.Where(t => !t.IsOnSlot && !t.IsOutside && !t.IsMoving).ToList();
        int gridSize = tilesToShuffle.Count;
        if (gridSize == 0) return;

        // ── STEP 2 & 3: Build per-category item ID pools from TWO sources: ──
        //   (a) ThemeItemId of active grid tiles
        //   (b) Unspawned content in availableItemIdsPerCategory
        Dictionary<ItemCategory, List<int>> itemPools = new Dictionary<ItemCategory, List<int>>();

        // (a) Collect actual item IDs from active tiles
        foreach (BaseTile tile in tilesToShuffle)
        {
            ItemCategory cat = tile.ItemCategory;
            if (!itemPools.ContainsKey(cat)) itemPools[cat] = new List<int>();
            itemPools[cat].Add(tile.ThemeItemId);
        }

        // (b) Add unspawned content from existing available pools
        foreach (var theme in levelItem.themeDataList)
        {
            ItemCategory cat = theme.category;
            if (!itemPools.ContainsKey(cat)) itemPools[cat] = new List<int>();

            if (availableItemIdsPerCategory.ContainsKey(cat))
            {
                itemPools[cat].AddRange(availableItemIdsPerCategory[cat]);
            }
        }

        // Shuffle each pool and convert to queues for sequential unique dequeue
        Dictionary<ItemCategory, Queue<int>> itemQueues = new Dictionary<ItemCategory, Queue<int>>();
        foreach (var kv in itemPools)
        {
            kv.Value.Shuffle();
            itemQueues[kv.Key] = new Queue<int>(kv.Value);
        }

        // ── STEP 4: Build combined category pool: active grid tiles + stock ──
        //    Shuffle determines new category assignments for all grid positions.
        List<ItemCategory> allCategoriesInPlay = tilesToShuffle.Select(t => t.ItemCategory).ToList();
        allCategoriesInPlay.AddRange(itemCategoryPool);

        // Priority: categories currently showing in active (non-locked) TileSlotContainers
        List<ItemCategory> priorityCategories = tileSlotContainers
            .Where(c => !c.isLocked && !c.isHidden)
            .Select(c => c.ItemCategory)
            .Distinct()
            .ToList();

        // Guarantee exactly 1 tile on the grid for each active unlocked theme,
        // so the player always has at least 1 playable tile per open slot.
        List<ItemCategory> guaranteedCategories = new List<ItemCategory>();
        foreach (ItemCategory cat in priorityCategories)
        {
            if (guaranteedCategories.Count >= gridSize) break;
            if (allCategoriesInPlay.Contains(cat))
            {
                guaranteedCategories.Add(cat);
                allCategoriesInPlay.Remove(cat); // consume exactly one occurrence
            }
        }

        // Fill remaining slots with weighted random:
        //  - If priority-theme cards are still available, 50% chance to draw from them first
        //    (encourages more playable tiles appearing on grid)
        //  - Otherwise draw freely from any remaining category
        List<ItemCategory> priorityPool = allCategoriesInPlay
            .Where(c => priorityCategories.Contains(c))
            .ToList();
        List<ItemCategory> otherPool = allCategoriesInPlay
            .Where(c => !priorityCategories.Contains(c))
            .ToList();

        priorityPool.Shuffle();
        otherPool.Shuffle();

        int slotsLeft = gridSize - guaranteedCategories.Count;

        List<ItemCategory> shuffledGridCategories = new List<ItemCategory>(guaranteedCategories);

        for (int slot = 0; slot < slotsLeft; slot++)
        {
            bool hasPriority = priorityPool.Count > 0;
            bool hasOther = otherPool.Count > 0;

            if (!hasPriority && !hasOther) break;

            ItemCategory chosen;
            // priorityCardRatio% → pick from priority pool (same theme as active orders), else from other
            float ratio = levelItem != null ? levelItem.priorityCardRatio : 0.5f;
            if (hasPriority && (!hasOther || UnityEngine.Random.value < ratio))
            {
                chosen = priorityPool[0];
                priorityPool.RemoveAt(0);
            }
            else
            {
                // No priority cards left OR lost the 50/50 coin flip
                if (hasOther)
                {
                    chosen = otherPool[0];
                    otherPool.RemoveAt(0);
                }
                else
                {
                    // otherPool empty, fall back to remaining priorityPool
                    chosen = priorityPool[0];
                    priorityPool.RemoveAt(0);
                }
            }

            shuffledGridCategories.Add(chosen);
        }

        // Shuffle the final list so guaranteed tiles land at random positions, not always first
        shuffledGridCategories.Shuffle();

        // ── STEP 5: Assign unique content to each grid tile from the queues ──
        // Shuffle tilesToShuffle itself so different tile objects (grid positions) receive
        // different categories each time, preventing identical consecutive shuffles.
        tilesToShuffle.Shuffle();

        Vector3 deckLocalPos = Vector3.zero;
        if (CardStockPile.Ins != null)
            deckLocalPos = tilesToShuffle[0].transform.parent.InverseTransformPoint(CardStockPile.Ins.transform.position);

        for (int i = 0; i < gridSize; i++)
        {
            ItemCategory newCat = shuffledGridCategories[i];
            CategoryIconSet iconSet = tileIconContainer.categoryIconSets.First(x => x.itemCategory == newCat);

            int itemId = 0;
            if (itemQueues.ContainsKey(newCat) && itemQueues[newCat].Count > 0)
            {
                itemId = itemQueues[newCat].Dequeue();
            }
            else
            {
                // Fallback if queue is empty
                int spriteCount = iconSet.icons != null ? iconSet.icons.Length : 0;
                int textCount = iconSet.text != null ? iconSet.text.Length : 0;
                itemId = UnityEngine.Random.Range(0, spriteCount + textCount);
            }

            int currentSpriteCount = iconSet.icons != null ? iconSet.icons.Length : 0;
            string textData = null;
            Sprite spriteData = null;

            if (itemId < currentSpriteCount)
            {
                spriteData = iconSet.icons[itemId];
            }
            else
            {
                textData = iconSet.text[itemId - currentSpriteCount];
            }

            tilesToShuffle[i].Shuffle(newCat, itemId, spriteData, textData, deckLocalPos);
        }

        // ── STEP 6: Rebuild available pools from queue remnants ──
        //    Whatever remains in the queues after grid assignment = content for future spawns.
        foreach (var theme in levelItem.themeDataList)
        {
            ItemCategory cat = theme.category;
            availableItemIdsPerCategory[cat] = itemQueues.ContainsKey(cat)
                ? new List<int>(itemQueues[cat])
                : new List<int>();
        }

        // ── STEP 6b: Safety dedup pass ──
        // Solution: remove every currently-visible tile content from the pool right now.
        foreach (BaseTile tile in tiles)
        {
            if (tile.IsDone) continue;
            ItemCategory tileCat = tile.ItemCategory;
            if (availableItemIdsPerCategory.ContainsKey(tileCat))
            {
                availableItemIdsPerCategory[tileCat].Remove(tile.ThemeItemId);
            }
        }

        // ── STEP 7: Rebuild itemCategoryPool from leftover categories not assigned to the grid ──
        // Leftover = what remains in priorityPool + otherPool after the fill loop consumed from them.
        itemCategoryPool = priorityPool;
        itemCategoryPool.AddRange(otherPool);
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
                        tile.IsMoving = true;
                        Tween.Alpha(tile.icon, 1, duration: 0.3f);
                        Tween.Position(tile.transform, tileSlot.transform.position, duration: 0.3f).OnComplete(() =>
                        {
                            tile.IsMoving = false;

                            tile.IsDone = true;
                            tiles.Remove(tile);
                            tile.gameObject.SetActive(false);
                            tile.transform.SetParent(tileSlot.transform);

                            tileSlotContainer.SwitchItem(tile);

                            if (tileSlotContainer.IsLastSlot(tileSlot) && !tileSlotContainer.isCompleted)
                            {
                                tileSlotContainer.isCompleted = true;
                                bool isAnyRemainingTile = tiles.Any(item => !item.IsOnSlot);

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
