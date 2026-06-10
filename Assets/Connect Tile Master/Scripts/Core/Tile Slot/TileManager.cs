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

    private Dictionary<ItemCategory, List<Sprite>> availableSpritesPerCategory = new Dictionary<ItemCategory, List<Sprite>>();
    private Dictionary<ItemCategory, List<string>> availableTextsPerCategory = new Dictionary<ItemCategory, List<string>>();

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
        if (isLevelActive && currentLevelTime > 0)
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

        availableSpritesPerCategory.Clear();
        availableTextsPerCategory.Clear();

        foreach (var theme in levelItem.themeDataList)
        {
            var iconSet = tileIconContainer.categoryIconSets.FirstOrDefault(x => x.itemCategory == theme.category);
            if (iconSet != null)
            {
                if (iconSet.icons != null && iconSet.icons.Length > 0)
                {
                    availableSpritesPerCategory[theme.category] = new List<Sprite>(iconSet.icons);
                }
                else
                {
                    availableSpritesPerCategory[theme.category] = new List<Sprite>();
                }

                if (iconSet.text != null && iconSet.text.Length > 0)
                {
                    availableTextsPerCategory[theme.category] = new List<string>(iconSet.text);
                }
                else
                {
                    availableTextsPerCategory[theme.category] = new List<string>();
                }
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

    private ItemCategory GetNextCategoryForSlotBasedOnDifficulty(int difficultyLevel, TileSlotContainer requestingContainer)
    {
        // The higher the score, the easier
        Dictionary<ItemCategory, int> numTilePerCategory = new Dictionary<ItemCategory, int>();

        for (int i = 0; i < tiles.Count; i++)
        {
            BaseTile tile = tiles[i];

            if (tile.IsDone)
            {
                continue;
            }

            CommonUtil.SumValueOfDictionary(numTilePerCategory, tile.ItemCategory, 1);
        }

        for (int i = 0; i < itemCategoryPool.Count; i++)
        {
            CommonUtil.SumValueOfDictionary(numTilePerCategory, itemCategoryPool[i], 1);
        }

        Dictionary<ItemCategory, int> scoreOfEachCategory = new Dictionary<ItemCategory, int>();

        foreach (var item in numTilePerCategory)
        {
            int addedScore = item.Value;
            int targetCardAmount = GetTargetCardAmount(item.Key);

            if (item.Value >= targetCardAmount)
            {
                addedScore *= 10;
            }

            CommonUtil.SumValueOfDictionary(scoreOfEachCategory, item.Key, addedScore);
        }

        scoreOfEachCategory = scoreOfEachCategory.OrderByDescending(item => item.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        ItemCategory[] sortedCategory = scoreOfEachCategory.OrderByDescending(item => item.Value)
            .Select(kvp => kvp.Key)
            .ToArray();

        for (int i = difficultyLevel; i < scoreOfEachCategory.Count; i++)
        {
            ItemCategory category = sortedCategory[i];

            int activeContainersForCategory = tileSlotContainers.Count(item => item != requestingContainer && !item.isLocked && !item.isHidden && item.ItemCategory == category);
            int totalNotDone = numTilePerCategory.ContainsKey(category) ? numTilePerCategory[category] : 0;
            int targetCardAmount = GetTargetCardAmount(category);

            if (totalNotDone >= (activeContainersForCategory + 1) * targetCardAmount)
            {
                if (activeContainersForCategory == 0)
                {
                    return category;
                }
            }
        }

        for (int i = 0; i < scoreOfEachCategory.Count; i++)
        {
            ItemCategory category = sortedCategory[i];

            int activeContainersForCategory = tileSlotContainers.Count(item => item != requestingContainer && !item.isLocked && !item.isHidden && item.ItemCategory == category);
            int totalNotDone = numTilePerCategory.ContainsKey(category) ? numTilePerCategory[category] : 0;
            int targetCardAmount = GetTargetCardAmount(category);

            if (totalNotDone >= (activeContainersForCategory + 1) * targetCardAmount)
            {
                return category;
            }
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
                        tile.gameObject.SetActive(false);
                        tile.transform.SetParent(tileSlot.transform);

                        tileSlotContainer.SwitchItem(tile);

                        if (tileSlotContainer.IsLastSlot(tileSlot) && !tileSlotContainer.isCompleted)
                        {
                            tileSlotContainer.isCompleted = true;
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
            tile.gameObject.SetActive(false);
            tile.transform.SetParent(slot.transform);

            tileSlotContainer.SwitchItem(tile);

            if (tileSlotContainer.IsLastSlot(slot) && !tileSlotContainer.isCompleted)
            {
                tileSlotContainer.isCompleted = true;
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
        bool isString = false;
        int remainingString = remainingStringPerCategory.ContainsKey(nextCategory) ? remainingStringPerCategory[nextCategory] : 0;
        int remainingSprite = remainingSpritePerCategory.ContainsKey(nextCategory) ? remainingSpritePerCategory[nextCategory] : 0;

        int totalRemaining = remainingString + remainingSprite;
        if (totalRemaining > 0)
        {
            if (UnityEngine.Random.Range(0, totalRemaining) < remainingString)
            {
                isString = true;
                remainingStringPerCategory[nextCategory]--;
            }
            else
            {
                remainingSpritePerCategory[nextCategory]--;
            }
        }

        string textData = null;
        Sprite spriteData = null;

        if (isString)
        {
            textData = GetUniqueText(nextCategory);
        }
        else
        {
            spriteData = GetUniqueSprite(nextCategory);
        }

        tile.Init(nextCategory, spriteData, textData);
    }

    private string GetUniqueText(ItemCategory category)
    {
        if (availableTextsPerCategory.ContainsKey(category) && availableTextsPerCategory[category].Count > 0)
        {
            int index = UnityEngine.Random.Range(0, availableTextsPerCategory[category].Count);
            string text = availableTextsPerCategory[category][index];
            availableTextsPerCategory[category].RemoveAt(index);
            return text;
        }

        // Fallback if we run out
        return tileIconContainer.GetRandomTextOfCategory(category);
    }

    private Sprite GetUniqueSprite(ItemCategory category)
    {
        if (availableSpritesPerCategory.ContainsKey(category) && availableSpritesPerCategory[category].Count > 0)
        {
            int index = UnityEngine.Random.Range(0, availableSpritesPerCategory[category].Count);
            Sprite sprite = availableSpritesPerCategory[category][index];
            availableSpritesPerCategory[category].RemoveAt(index);
            return sprite;
        }

        // Fallback if we run out
        return tileIconContainer.GetRandomIconOfCategory(category);
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

    public bool CanUseShuffleBooster()
    {
        return tiles.Any(tile => !tile.IsOnSlot && !tile.IsDone && !tile.IsOutside);
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

            if (tile.IsText)
            {
                if (!availableTextsPerCategory.ContainsKey(tile.ItemCategory))
                    availableTextsPerCategory[tile.ItemCategory] = new List<string>();
                availableTextsPerCategory[tile.ItemCategory].Add(tile.category.text);
            }
            else
            {
                if (!availableSpritesPerCategory.ContainsKey(tile.ItemCategory))
                    availableSpritesPerCategory[tile.ItemCategory] = new List<Sprite>();
                if (tile.icon.sprite != null)
                    availableSpritesPerCategory[tile.ItemCategory].Add(tile.icon.sprite);
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
            int targetCardAmount = GetTargetCardAmount(categoryPoolForAllTileLeft[i]);
            if (priorityCategory.Contains(categoryPoolForAllTileLeft[i]) && shuffledCategory.Count(item => item == categoryPoolForAllTileLeft[i]) < targetCardAmount)
            {
                shuffledCategory.Add(categoryPoolForAllTileLeft[i]);
                toRemoveCategory.Add(categoryPoolForAllTileLeft[i]);
            }
        }

        foreach (var category in toRemoveCategory)
        {
            categoryPoolForAllTileLeft.Remove(category);
        }

        categoryPoolForAllTileLeft.Shuffle();

        int currentCategoryAdded = shuffledCategory.Count;

        shuffledCategory.AddRange(categoryPoolForAllTileLeft.Take(totalActiveTiles - currentCategoryAdded));

        for (int i = 0; i < tilesToShuffle.Count; i++)
        {
            ItemCategory newCategory = shuffledCategory[i];

            bool useText = false;
            int textsLeft = availableTextsPerCategory.ContainsKey(newCategory) ? availableTextsPerCategory[newCategory].Count : 0;
            int spritesLeft = availableSpritesPerCategory.ContainsKey(newCategory) ? availableSpritesPerCategory[newCategory].Count : 0;

            if (textsLeft > 0 && spritesLeft > 0)
            {
                useText = UnityEngine.Random.Range(0, textsLeft + spritesLeft) < textsLeft;
            }
            else if (textsLeft > 0)
            {
                useText = true;
            }
            else if (spritesLeft > 0)
            {
                useText = false;
            }
            else
            {
                CategoryIconSet iconSet = tileIconContainer.categoryIconSets.First(item => item.itemCategory == newCategory);
                bool hasText = iconSet.text != null && iconSet.text.Length > 0;
                bool hasIcon = iconSet.icons != null && iconSet.icons.Length > 0;
                if (hasText && hasIcon) useText = UnityEngine.Random.value > 0.5f;
                else if (hasText) useText = true;
                else useText = false;
            }

            string textData = null;
            Sprite spriteData = null;

            if (useText)
            {
                textData = GetUniqueText(newCategory);
            }
            else
            {
                spriteData = GetUniqueSprite(newCategory);
            }

            tilesToShuffle[i].Shuffle(newCategory, spriteData, textData);
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
                        tile.IsMoving = true;
                        Tween.Alpha(tile.icon, 1, duration: 0.3f);
                        Tween.Position(tile.transform, tileSlot.transform.position, duration: 0.3f).OnComplete(() =>
                        {
                            tile.IsMoving = false;

                            tile.IsDone = true;
                            tile.gameObject.SetActive(false);
                            tile.transform.SetParent(tileSlot.transform);

                            tileSlotContainer.SwitchItem(tile);

                            if (tileSlotContainer.IsLastSlot(tileSlot) && !tileSlotContainer.isCompleted)
                            {
                                tileSlotContainer.isCompleted = true;
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
