using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Utilities;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static GameEnum;

public class TileManager : Singleton<TileManager>
{
    [SerializeField] private List<TileSlotContainer> tileSlotContainers;
    [SerializeField] private List<TileSlot> additionalTileSlots;
    [SerializeField] public List<BaseTile> tiles;
    [SerializeField] private Grid[] grids;
    [SerializeField] private TileIconContainer tileIconContainer;
    [SerializeField] private Image upToAll;

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
    public int remainingSlotContainer;
    private float highestYGridPosition;
    public LevelItem levelItem;

    public float currentLevelTime;
    public bool isLevelActive;
    [HideInInspector] public bool isTimeFrozen;
    private Tween freezeTimeTween;

    public bool IsShuffling { get; private set; }
    public bool IsDealingCards { get; private set; }

    private Dictionary<ItemCategory, List<int>> availableItemIdsPerCategory = new Dictionary<ItemCategory, List<int>>();

    public static event Action updateActiveSlotsEvent;

    public int NumLockedTileSlotContainer => tileSlotContainers.Count(item => item.isLocked);

    public bool CanUseUnlockThemeBooster()
    {
        int activeContainers = tileSlotContainers.Count(item => !item.isHidden && !item.isCompleted);
        int remainingThemes = activeContainers + remainingSlotContainer;
        return remainingSlotContainer > 0;
    }

    public TileSlotContainer GetNextLockedTileSlotContainer()
    {
        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            if (tileSlotContainers[i].isLocked)
            {
                Debug.LogFormat("CountLock = {0}, Title = {1}", tileSlotContainers.Count(item => item.isLocked), tileSlotContainers[i].ItemCategory);
                return tileSlotContainers[i];
            }
        }
        return null;
    }

    public static event Action winLevelEvent;
    public static event Action loseLevelEvent;

    private float valueRotateWhenShuffling = 60f;
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

        IsShuffling = false;
        IsDealingCards = false;

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
            //TutorialLevelOne.Ins.HideTutorial();
        }

        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            tileSlotContainers[i].SetOriginalLockState(i >= 2);
        }

        SpawnSlots();
    }

    private void RemoveOldTiles()
    {
        foreach (var tileToRemove in tiles)
        {
            if (tileToRemove != null && tileToRemove.gameObject != null)
                Destroy(tileToRemove.gameObject);
        }

        tiles = new List<BaseTile>();
    }
    #endregion

    #region Slot
    public void SpawnSlot(int slotIndex)
    {
        TileSlotContainer tileSlotContainer = tileSlotContainers[slotIndex];

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
            updateActiveSlotsEvent?.Invoke();
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
                tileSlotContainer.gameObject.SetActive(true);
                tileSlotContainer.Init(categories[0], GetTargetCardAmount(categories[0]));
                categories.RemoveAt(0);
            }
            else
            {
                tileSlotContainer.gameObject.SetActive(true);
                tileSlotContainer.Init();
            }

            float initialPositionY = tileSlotContainer.originalPositionY;

            Tween.LocalPositionY(tileSlotContainer.transform, initialPositionY + 1600, duration: 0.7f)
            .OnComplete(() =>
            {
                tileSlotContainer.ClearTiles();
                tileSlotContainer.UpdateCountText();

                StartCoroutine(WaitAndDropContainer(tileSlotContainer, initialPositionY));
            });
        }
    }

    public void UseUnlockThemeBooster()
    {
        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            if (tileSlotContainers[i].isLocked)
            {
                tileSlotContainers[i].Unlock(onCompleteAction: () =>
                {
                    remainingSlotContainer--;
                    updateActiveSlotsEvent?.Invoke();
                });
                break;
            }
        }
    }
    #endregion

    #region Category
    private List<ItemCategory> GetNextCategories(int n)
    {
        List<ItemCategory> result = tiles
            .GroupBy(t => t.ItemCategory)
            .OrderByDescending(g => g.Count())
            .Take(n)
            .Select(g => g.Key)
            .ToList();

        if (result.Count < n && levelItem != null && levelItem.themeDataList != null)
        {
            List<ItemCategory> allCategories = levelItem.themeDataList.Select(t => t.category).ToList();
            allCategories.Shuffle();
            foreach (var cat in allCategories)
            {
                if (!result.Contains(cat))
                {
                    result.Add(cat);
                    if (result.Count >= n) break;
                }
            }
        }

        result.Shuffle();
        return result;
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
                        tile.transform.SetParent(tileSlot.transform);

                        tileSlotContainer.SwitchItem(tile);

                        if (tileSlotContainer.IsLastSlot(tileSlot) && !tileSlotContainer.isCompleted)
                        {
                            tileSlotContainer.isCompleted = true;

                            Tween.Delay(1.2f).OnComplete(() =>
                            {
                                if (tileSlotContainer == null) return;
                                bool isAnyRemainingTile = tiles.Any(item => !item.IsOnSlot);

                                if (remainingSlotContainer > 0 && isAnyRemainingTile)
                                {
                                    SpawnSlot(index);
                                }
                                else
                                {
                                    HideSlot(index);
                                }
                            });
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
            tile.transform.SetParent(slot.transform);

            tileSlotContainer.SwitchItem(tile);

            if (tileSlotContainer.IsLastSlot(slot) && !tileSlotContainer.isCompleted)
            {
                tileSlotContainer.isCompleted = true;

                Tween.Delay(1.2f).OnComplete(() =>
                {
                    if (tileSlotContainer == null) return;
                    bool isAnyRemainingTile = tiles.Any(item => !item.IsOnSlot);

                    if (remainingSlotContainer > 0 && isAnyRemainingTile)
                    {
                        SpawnSlot(tileSlotContainerIndex);
                    }
                    else
                    {
                        HideSlot(tileSlotContainerIndex);
                    }
                });
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
                if (tileSlotContainers[i].isLocked || tileSlotContainers[i].isMoving || tileSlotContainers[i].isHidden)
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

                    tile.SetFaceUp(false);
                    tile.transform.localEulerAngles = Vector3.zero;
                    tile.transform.localScale = Vector3.one;

                    tile.IsMoving = true;

                    float distance = Vector3.Distance(tile.transform.localPosition, grids[i].position);
                    float dealSpeed = 1500f;
                    float dealDuration = distance / dealSpeed;

                    tile.transform.SetAsLastSibling();

                    Tween.LocalPosition(tile.transform, grids[i].position, dealDuration);
                    Tween.Scale(tile.transform, Vector3.one, dealDuration, ease: Ease.OutQuad);
                    Tween.Scale(tile.ObjHide.transform, 0.91f, dealDuration, ease: Ease.OutQuad);

                    float halfDuration = dealDuration / 2f;
                    Tween.LocalEulerAngles(tile.transform, Vector3.zero, new Vector3(0, 70f, -70f), halfDuration, ease: Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        if (tile == null) return;
                        Tween.LocalEulerAngles(tile.transform, new Vector3(0, 70f, -70f), Vector3.zero, halfDuration, ease: Ease.InQuad)
                        .OnComplete(() =>
                        {
                            if (tile == null) return;
                            float flipDuration = 0.15f;
                            Tween.LocalEulerAngles(tile.transform, Vector3.zero, new Vector3(0, 90f, 0), flipDuration).OnComplete(() =>
                            {
                                if (tile != null)
                                {
                                    tile.SetFaceUp(true);
                                    Tween.LocalEulerAngles(tile.transform, new Vector3(0, 90f, 0), Vector3.zero, flipDuration)
                                    .OnComplete(() =>
                                    {
                                        if (tile != null) tile.IsMoving = false;
                                    });
                                }
                            });
                        });
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
        float tileSpacingWidth = 1f * 210;
        float tileSpacingHeight = 1f * 266;

        float totalWidth = (col - 1) * tileSpacingWidth;
        float totalHeight = (row - 1) * tileSpacingHeight;

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

        StartCoroutine(DealCardsAnimCoroutine());

        BaseTile SpawnTile(int xIndex, int yIndex)
        {
            ItemCategory currentCategory = itemCategoryPool.First();
            itemCategoryPool.RemoveAt(0);

            CategoryIconSet categoryIconSet = tileIconContainer.categoryIconSets.First(item => item.itemCategory == currentCategory);

            float xPos = xIndex * tileSpacingWidth - totalWidth / 2f;
            float yPos = yIndex * tileSpacingHeight - totalHeight / 2f;

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

    private IEnumerator WaitForTransitionCompleted()
    {
        bool isTransitionDone = false;
        Action onTransitionDone = () => isTransitionDone = true;
        SceneTransition.transitionCompletedEvent += onTransitionDone;

        float timeout = 10f; // Phòng hờ lỗi kẹt loading
        while ((!isTransitionDone || !GamePersistentVariable.isLoadingComplete) && timeout > 0)
        {
            timeout -= UnityEngine.Time.deltaTime;
            yield return null;
        }

        SceneTransition.transitionCompletedEvent -= onTransitionDone;
    }

    private IEnumerator WaitAndDropContainer(TileSlotContainer container, float targetY)
    {
        yield return StartCoroutine(WaitForTransitionCompleted());
        Tween.LocalPositionY(container.transform, targetY, duration: 0.3f);
    }

    private IEnumerator DealCardsAnimCoroutine()
    {
        IsDealingCards = true;
        // Prepare
        foreach (var grid in grids)
        {
            if (grid == null || grid.tile == null) continue;

            BaseTile tile = grid.tile;
            tile.SetFaceUp(false);
            tile.transform.position = CardStockPile.Ins.transform.position;
            tile.transform.localEulerAngles = Vector3.zero;
            tile.transform.localScale = Vector3.one;
        }

        yield return StartCoroutine(WaitForTransitionCompleted());

        var sortedGrids = grids.Where(g => g != null && g.tile != null)
                               .OrderByDescending(g => g.position.y)
                               .ThenBy(g => g.position.x)
                               .ToList();

        foreach (var grid in sortedGrids)
        {
            BaseTile tile = grid.tile;

            float distance = Vector3.Distance(tile.transform.localPosition, grid.position);
            float dealSpeed = 1500f;
            float dealDuration = distance / dealSpeed;

            tile.transform.SetAsLastSibling();
            Tween.LocalPosition(tile.transform, grid.position, dealDuration);
            Tween.Scale(tile.transform, Vector3.one, dealDuration, ease: Ease.OutQuad);
            Tween.Scale(tile.ObjHide.transform, 0.91f, dealDuration, ease: Ease.OutQuad);

            float halfDuration = dealDuration / 2f;
            Tween.LocalEulerAngles(tile.transform, Vector3.zero, new Vector3(0, 70f, -70f), halfDuration, ease: Ease.OutQuad)
                .OnComplete(() =>
                {
                    if (tile == null) return;
                    Tween.LocalEulerAngles(tile.transform, new Vector3(0, 70f, -70f), Vector3.zero, halfDuration, ease: Ease.InQuad);
                });

            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(0.3f);

        foreach (var grid in sortedGrids)
        {
            BaseTile tile = grid.tile;

            float flipDuration = 0.15f;
            Tween.LocalEulerAngles(tile.transform, Vector3.zero, new Vector3(0, 90f, 0), flipDuration).OnComplete(() =>
            {
                if (tile != null)
                {
                    tile.SetFaceUp(true);
                    Tween.LocalEulerAngles(tile.transform, new Vector3(0, 90f, 0), Vector3.zero, flipDuration);
                }
            });

            yield return new WaitForSeconds(0.15f);
        }

        IsDealingCards = false;
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
    public void UseFreezeTimeBooster(float duration, UnityAction callback = null)
    {
        isTimeFrozen = true;

        if (freezeTimeTween.isAlive)
        {
            freezeTimeTween.Stop();
        }

        freezeTimeTween = Tween.Delay(duration).OnComplete(() =>
        {
            callback?.Invoke();
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
    #region Booster Shuffle
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

            Transform originalContainerParent = null;
            Transform originalTileParent = null;
            int originalContainerSiblingIndex = -1;
            int originalTileSiblingIndex = -1;

            if (upToAll != null)
            {
                upToAll.gameObject.SetActive(true);
                Tween.Alpha(upToAll, 0.7f, duration: 0.25f);
                originalContainerParent = activeContainer.transform.parent;
                originalContainerSiblingIndex = activeContainer.transform.GetSiblingIndex();
                activeContainer.transform.SetParent(upToAll.transform, true);

                if (highlightedTile != null)
                {
                    originalTileParent = highlightedTile.transform.parent;
                    originalTileSiblingIndex = highlightedTile.transform.GetSiblingIndex();
                    highlightedTile.transform.SetParent(upToAll.transform, true);
                }
            }

            Action restoreParents = () =>
            {
                if (upToAll != null)
                {
                    Tween.Alpha(upToAll, 0, duration: 0.3f).OnComplete(() =>
                    {
                        upToAll.gameObject.SetActive(false);
                    });

                    if (originalContainerParent != null && activeContainer != null)
                    {
                        activeContainer.transform.SetParent(originalContainerParent, true);
                        activeContainer.transform.SetSiblingIndex(originalContainerSiblingIndex);
                    }

                    if (originalTileParent != null && highlightedTile != null)
                    {
                        highlightedTile.transform.SetParent(originalTileParent, true);
                        highlightedTile.transform.SetSiblingIndex(originalTileSiblingIndex);
                    }
                }
            };

            if (highlightedTile != null)
            {
                highlightedTile.onInteract = () =>
                {
                    if (activeContainer != null)
                    {
                        activeContainer.SetHighlight(false);
                    }
                    restoreParents();
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
                    restoreParents();
                });
            }
        }
    }

    public bool CanUseHintBooster()
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
            foreach (var tile in tiles)
            {
                if (!tile.IsOnSlot && !tile.IsOutside && !tile.IsDone && tile.ItemCategory == activeContainer.ItemCategory)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool CanUseShuffleBooster()
    {
        return tiles.Any(tile => !tile.IsOnSlot && !tile.IsOutside);
    }

    private struct ShuffleTargetInfo
    {
        public BaseTile tile;
        public ItemCategory category;
        public int itemId;
        public Sprite sprite;
        public string text;
        public Vector3 targetLocalPos;
    }

    public void UseShuffleBooster()
    {

        List<BaseTile> tilesToShuffle = tiles.Where(t => !t.IsOnSlot && !t.IsOutside && !t.IsMoving).ToList();
        int gridSize = tilesToShuffle.Count;
        if (gridSize == 0) return;

        foreach (BaseTile tile in tilesToShuffle)
        {
            tile.IsMoving = true;
        }
        Dictionary<ItemCategory, List<int>> freedIds = new Dictionary<ItemCategory, List<int>>();
        foreach (BaseTile tile in tilesToShuffle)
        {
            ItemCategory cat = tile.ItemCategory;
            if (!freedIds.ContainsKey(cat)) freedIds[cat] = new List<int>();
            freedIds[cat].Add(tile.ThemeItemId);
        }

        Dictionary<ItemCategory, List<int>> itemPools = new Dictionary<ItemCategory, List<int>>();
        foreach (var theme in levelItem.themeDataList)
        {
            ItemCategory cat = theme.category;
            itemPools[cat] = new List<int>();

            if (availableItemIdsPerCategory.ContainsKey(cat))
                itemPools[cat].AddRange(availableItemIdsPerCategory[cat]);

            if (freedIds.ContainsKey(cat))
                itemPools[cat].AddRange(freedIds[cat]);
        }

        Dictionary<ItemCategory, Queue<int>> itemQueues = new Dictionary<ItemCategory, Queue<int>>();
        foreach (var kv in itemPools)
        {
            kv.Value.Shuffle();
            itemQueues[kv.Key] = new Queue<int>(kv.Value);
        }


        List<ItemCategory> allCategoriesInPlay = tilesToShuffle.Select(t => t.ItemCategory).ToList();
        allCategoriesInPlay.AddRange(itemCategoryPool);

        List<ItemCategory> priorityCategories = tileSlotContainers
            .Where(c => !c.isLocked && !c.isHidden)
            .Select(c => c.ItemCategory)
            .Distinct()
            .ToList();

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

        // Fill remaining slots with weighted random
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
            float ratio = levelItem != null ? levelItem.priorityCardRatio : 0.5f;
            if (hasPriority && (!hasOther || UnityEngine.Random.value < ratio))
            {
                chosen = priorityPool[0];
                priorityPool.RemoveAt(0);
            }
            else
            {
                if (hasOther)
                {
                    chosen = otherPool[0];
                    otherPool.RemoveAt(0);
                }
                else
                {
                    chosen = priorityPool[0];
                    priorityPool.RemoveAt(0);
                }
            }

            shuffledGridCategories.Add(chosen);
        }

        shuffledGridCategories.Shuffle();
        tilesToShuffle.Shuffle();

        List<ShuffleTargetInfo> shuffleTargets = new List<ShuffleTargetInfo>();
        Dictionary<ItemCategory, List<int>> assignedGridIds = new Dictionary<ItemCategory, List<int>>();

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
                int spriteCount = iconSet.icons != null ? iconSet.icons.Length : 0;
                int textCount = iconSet.text != null ? iconSet.text.Length : 0;
                int totalItems = spriteCount + textCount;
                List<int> alreadyUsed = assignedGridIds.ContainsKey(newCat) ? assignedGridIds[newCat] : new List<int>();
                List<int> candidates = new List<int>();
                for (int id = 0; id < totalItems; id++)
                    if (!alreadyUsed.Contains(id)) candidates.Add(id);
                itemId = candidates.Count > 0
                    ? candidates[UnityEngine.Random.Range(0, candidates.Count)]
                    : UnityEngine.Random.Range(0, totalItems);
            }

            // Track the assigned ID
            if (!assignedGridIds.ContainsKey(newCat)) assignedGridIds[newCat] = new List<int>();
            assignedGridIds[newCat].Add(itemId);

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

            shuffleTargets.Add(new ShuffleTargetInfo
            {
                tile = tilesToShuffle[i],
                category = newCat,
                itemId = itemId,
                sprite = spriteData,
                text = textData,
                targetLocalPos = tilesToShuffle[i].transform.localPosition
            });
        }

        // ── STEP 6: Rebuild available pools from queue remnants ──
        foreach (var theme in levelItem.themeDataList)
        {
            ItemCategory cat = theme.category;
            availableItemIdsPerCategory[cat] = itemQueues.ContainsKey(cat)
                ? new List<int>(itemQueues[cat])
                : new List<int>();
        }

        // ── STEP 6b: Dedup pass using ASSIGNED IDs ──
        foreach (var kv in assignedGridIds)
        {
            ItemCategory cat = kv.Key;
            if (availableItemIdsPerCategory.ContainsKey(cat))
            {
                foreach (int id in kv.Value)
                    availableItemIdsPerCategory[cat].Remove(id);
            }
        }

        foreach (BaseTile tile in tiles)
        {
            if (tile.IsDone || tile.IsMoving) continue;
            if (tilesToShuffle.Contains(tile)) continue;
            ItemCategory tileCat = tile.ItemCategory;
            if (availableItemIdsPerCategory.ContainsKey(tileCat))
                availableItemIdsPerCategory[tileCat].Remove(tile.ThemeItemId);
        }

        IsShuffling = true;
        // Start the shuffle animation coroutine
        StartCoroutine(ShuffleBoosterCoroutine(shuffleTargets, priorityPool, otherPool));
    }

    private IEnumerator ShuffleBoosterCoroutine(List<ShuffleTargetInfo> shuffleTargets, List<ItemCategory> priorityPool, List<ItemCategory> otherPool)
    {
        // 1. Xác định vị trí bộ bài
        Vector3 deckLocalPos = Vector3.zero;
        if (CardStockPile.Ins != null && shuffleTargets.Count > 0)
        {
            deckLocalPos = shuffleTargets[0].tile.transform.parent.InverseTransformPoint(CardStockPile.Ins.transform.position);
        }

        // Sắp xếp thứ tự các lá bài từ trái qua phải, từ trên xuống dưới
        var sortedTargets = shuffleTargets
            .OrderByDescending(t => t.tile.transform.parent.TransformPoint(t.targetLocalPos).y)
            .ThenBy(t => t.tile.transform.parent.TransformPoint(t.targetLocalPos).x)
            .ToList();

        // 2a. Lần lượt úp các lá bài trên grid
        int flipsCompleted = 0;
        for (int i = 0; i < sortedTargets.Count; i++)
        {
            var target = sortedTargets[i];
            float delay = i * 0.08f;
            target.tile.FlipFaceDown(delay, () =>
            {
                flipsCompleted++;
            });
        }

        // Đợi tất cả úp xong
        while (flipsCompleted < sortedTargets.Count)
        {
            yield return null;
        }

        // 2b. Khi tất cả đã úp xong mới bắt đầu di chuyển để shuffle về bộ bài (staggered)
        int flyCompleted = 0;
        for (int i = 0; i < sortedTargets.Count; i++)
        {
            var target = sortedTargets[i];
            float flyDelay = i * 0.15f; // Di chuyển xen kẽ nhanh
            StartCoroutine(FlyToDeckWithDelayCoroutine(target.tile, deckLocalPos, flyDelay, () =>
            {
                flyCompleted++;
            }));
        }

        // Đợi tất cả bay về bộ bài hoàn tất
        while (flyCompleted < sortedTargets.Count)
        {
            yield return null;
        }

        // 3. Cập nhật dữ liệu mới khi các lá bài đang nằm ẩn trong bộ bài
        foreach (var target in sortedTargets)
        {
            if (target.tile == null) continue;
            target.tile.Init(target.category, target.itemId, target.sprite, target.text);
            target.tile.SetFaceUp(false);
            target.tile.transform.localScale = Vector3.one;
            target.tile.transform.localEulerAngles = Vector3.zero;
        }

        // 4. Cập nhật lại pool chung của màn chơi
        itemCategoryPool = new List<ItemCategory>(priorityPool);
        itemCategoryPool.AddRange(otherPool);
        CardStockPile.Ins.UpdateText();

        yield return new WaitForSeconds(0.2f);

        // 5. Chia bài ra
        foreach (var target in sortedTargets)
        {
            BaseTile tile = target.tile;
            if (tile == null) continue;

            float distance = Vector3.Distance(tile.transform.localPosition, target.targetLocalPos);
            float dealSpeed = 1500f;
            float dealDuration = distance / dealSpeed;

            tile.transform.SetAsLastSibling();
            Tween.LocalPosition(tile.transform, target.targetLocalPos, dealDuration);
            Tween.Scale(tile.transform, Vector3.one, dealDuration, ease: Ease.OutQuad);
            Tween.Scale(tile.ObjHide.transform, 0.91f, dealDuration, ease: Ease.OutQuad);

            float halfDuration = dealDuration / 3f;
            Tween.LocalEulerAngles(tile.transform, Vector3.zero, new Vector3(0, valueRotateWhenShuffling, -valueRotateWhenShuffling), halfDuration)
                .OnComplete(() =>
                {
                    if (tile == null) return;
                    Tween.LocalEulerAngles(tile.transform, new Vector3(0, valueRotateWhenShuffling, -valueRotateWhenShuffling), Vector3.zero, halfDuration * 2, ease: Ease.InQuad);
                });

            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(0.3f);

        // 5b. Lật bài ngửa lên lần lượt
        foreach (var target in sortedTargets)
        {
            BaseTile tile = target.tile;
            if (tile == null) continue;

            float flipDuration = 0.15f;
            Tween.LocalEulerAngles(tile.transform, Vector3.zero, new Vector3(0, 90f, 0), flipDuration).OnComplete(() =>
            {
                if (tile != null)
                {
                    tile.SetFaceUp(true);
                    Tween.LocalEulerAngles(tile.transform, new Vector3(0, 90f, 0), Vector3.zero, flipDuration).OnComplete(() =>
                    {
                        if (tile != null) tile.IsMoving = false;
                    });
                }
            });

            yield return new WaitForSeconds(0.15f);
        }

        IsShuffling = false;
    }

    private IEnumerator FlyToDeckWithDelayCoroutine(BaseTile tile, Vector3 deckLocalPos, float delay, Action onComplete)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);

        if (tile != null)
        {
            tile.FlyToDeck(deckLocalPos, onComplete);
        }
        else
        {
            onComplete?.Invoke();
        }
    }
    #endregion
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
                        Tween.Position(tile.transform, tileSlot.transform.position, duration: 0.15f).OnComplete(() =>
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

                                Tween.Delay(1.2f).OnComplete(() =>
                                {
                                    if (tileSlotContainer == null) return;
                                    bool isAnyRemainingTile = tiles.Any(item => !item.IsOnSlot);

                                    if (remainingSlotContainer > 0 && isAnyRemainingTile)
                                    {
                                        SpawnSlot(index);
                                    }
                                    else
                                    {
                                        HideSlot(index);
                                    }
                                });
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
            isLevelActive = false;
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
