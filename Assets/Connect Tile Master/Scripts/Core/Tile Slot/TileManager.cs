using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Utilities;
using GplayCdn;
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
    public List<TileSlotContainer> TileSlotContainers => tileSlotContainers;
    [SerializeField] private List<TileSlot> additionalTileSlots;
    [SerializeField] public List<BaseTile> tiles;
    [SerializeField] private Grid[] grids;
    [SerializeField] private TileIconContainer tileIconContainer;
    [SerializeField] private Image upToAll;
    public Image UpToAll => upToAll;
    [SerializeField] private RectTransform head;
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
    private int remainingSetToComplete;
    public int remainingSlotContainer;
    private float highestYGridPosition;
    public LevelItem levelItem;

    public float currentLevelTime;
    public bool isLevelActive;
    private bool _hasTriggeredWin;
    [HideInInspector] public bool isTimeFrozen;
    [HideInInspector] public bool isCurrentLevelTutorialActive;
    private Tween freezeTimeTween;
    private Tween gridPositionTween;

    public static ItemCategory targetThemeLevel10 = ItemCategory.None;

    public bool IsShuffling { get; private set; }
    private bool _isDealingStartCards;
    public bool IsDealingCards
    {
        get
        {
            if (_isDealingStartCards) return true;
            if (tiles != null)
            {
                for (int i = 0; i < tiles.Count; i++)
                {
                    if (tiles[i] != null && tiles[i].IsMoving)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public BaseTile CurrentlyDraggedTile { get; set; }

    public bool IsAnyTileDragging()
    {
        return CurrentlyDraggedTile != null;
    }

    private Dictionary<ItemCategory, List<int>> availableItemIdsPerCategory = new Dictionary<ItemCategory, List<int>>();

    public static event Action updateActiveSlotsEvent;

    public int NumLockedTileSlotContainer => tileSlotContainers.Count(item => item.isLocked);

    public bool IsAnyThemeCompleting()
    {
        if (tileSlotContainers == null) return false;
        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            var container = tileSlotContainers[i];
            if (container != null && !container.isLocked && !container.isHidden)
            {
                if (container.isCompleted || container.isMoving)
                {
                    return true;
                }
            }
        }
        return false;
    }

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
    public static event Action startLevelEvent;

    private float valueRotateWhenShuffling = 60f;
    private float originalHeadY;

    #region Life Cycle
    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        if (head != null)
        {
            originalHeadY = head.anchoredPosition.y;
        }

        Input.multiTouchEnabled = false;

        RevivePopup.reviveEvent += OnRevive;
        BasePopup.popupShowEvent += OnPopupShow;
        BasePopup.popupHideEvent += OnPopupHide;

        totalSlotInShelf = 5;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.W))
        {
            InstantWin();
        }
#endif

        if (isLevelActive && !isTimeFrozen && !BasePopup.IsAnyPopupOpen && currentLevelTime > 0)
        {
            currentLevelTime -= Time.deltaTime;

            if (currentLevelTime <= 15f && currentLevelTime > 0)
            {
                SoundManager.Instance?.PlaySoundCountingTime();
            }

            if (currentLevelTime <= 0)
            {
                currentLevelTime = 0;
                isLevelActive = false;
                SoundManager.Instance?.StopSoundCountingTime();
                loseLevelEvent?.Invoke();
            }
        }
    }

    private void OnDestroy()
    {
        MoreActionOnDestroy();
    }

    protected virtual void MoreActionOnDestroy()
    {
        RevivePopup.reviveEvent -= OnRevive;
        BasePopup.popupShowEvent -= OnPopupShow;
        BasePopup.popupHideEvent -= OnPopupHide;
    }

    private void OnPopupShow()
    {
        SoundManager.Instance?.StopSoundCountingTime();
    }

    private void OnPopupHide()
    {
        if (isLevelActive && !isTimeFrozen && !BasePopup.IsAnyPopupOpen && currentLevelTime <= 15f && currentLevelTime > 0)
        {
            SoundManager.Instance?.PlaySoundCountingTime();
        }
    }
    #endregion

    #region Level
    public void SetupLevel(LevelItem levelItem)
    {
        this.levelItem = levelItem;

        // Check if any categories need CDN sprites downloaded
        StartCoroutine(SetupLevelWithCdnDownload(levelItem));
    }

    private IEnumerator SetupLevelWithCdnDownload(LevelItem levelItem)
    {
        // Collect CDN categories that need downloading for this level
        List<CategoryIconSet> cdnSetsToDownload = new List<CategoryIconSet>();

        foreach (var theme in levelItem.themeDataList)
        {
            var iconSet = tileIconContainer.categoryIconSets.FirstOrDefault(x => x.itemCategory == theme.category);
            if (iconSet != null && iconSet.isOnServer)
            {
                // Check if icons need downloading (null or empty)
                bool needsDownload = (iconSet.icons == null || iconSet.icons.Length == 0 || iconSet.icons.Any(s => s == null));
                bool needsSpriteOrder = iconSet.spriteOrder == null && !string.IsNullOrEmpty(iconSet.spriteOrderCdnKey);

                if (needsDownload || needsSpriteOrder)
                {
                    cdnSetsToDownload.Add(iconSet);
                }
            }
        }

        // Download CDN sprites if needed
        if (cdnSetsToDownload.Count > 0 && CdnDownloader.Instance != null)
        {
            Debug.Log($"[TileManager] Downloading CDN sprites for {cdnSetsToDownload.Count} categories...");

            bool allDone = false;
            StartCoroutine(DownloadCdnSpritesForLevel(cdnSetsToDownload, () => allDone = true));
            yield return new WaitUntil(() => allDone);

            Debug.Log("[TileManager] CDN sprites download complete.");
        }

        // Notify that the level (including assets/sprites download) is fully loaded
        NamPhuThuy.GamePlay.LevelLoader.NotifyLevelLoaded();

        // Continue with normal setup
        SetupLevelInternal(levelItem);
        startLevelEvent?.Invoke();
    }

    private void ShowNoInternetPopup()
    {
        if (!NoAdsLoadedPopup.IsShown)
        {
            NoAdsLoadedPopup.IsNoInternetMode = true;
            BasePopup.TriggerShowAdNotLoadedPopupEvent();
        }
    }

    private IEnumerator DownloadCdnSpritesForLevel(List<CategoryIconSet> cdnSets, Action onComplete)
    {
        bool allCached = true;
        foreach (var iconSet in cdnSets)
        {
            if (iconSet.iconCdnKeys != null)
            {
                foreach (var key in iconSet.iconCdnKeys)
                {
                    if (!string.IsNullOrEmpty(key) && !CdnDownloader.Instance.IsResourceCached(key))
                    {
                        allCached = false;
                        break;
                    }
                }
            }
            if (!allCached) break;
            if (!string.IsNullOrEmpty(iconSet.spriteOrderCdnKey) && !CdnDownloader.Instance.IsResourceCached(iconSet.spriteOrderCdnKey))
            {
                allCached = false;
                break;
            }
        }

        if (!allCached && Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("[TileManager] No internet and assets not cached. Showing popup.");
            ShowNoInternetPopup();

            while (Application.internetReachability == NetworkReachability.NotReachable)
            {
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        float startTime = Time.unscaledTime;

        foreach (var iconSet in cdnSets)
        {
            // Download icons
            if (iconSet.iconCdnKeys != null && iconSet.iconCdnKeys.Length > 0)
            {
                // Initialize icons array if needed
                if (iconSet.icons == null || iconSet.icons.Length != iconSet.iconCdnKeys.Length)
                {
                    iconSet.icons = new Sprite[iconSet.iconCdnKeys.Length];
                }

                for (int i = 0; i < iconSet.iconCdnKeys.Length; i++)
                {
                    string key = iconSet.iconCdnKeys[i];
                    if (string.IsNullOrEmpty(key)) continue;
                    if (iconSet.icons[i] != null) continue; // Already loaded

                    int idx = i;
                    bool done = false;
                    bool success = false;

                    while (!success)
                    {
                        if (Application.internetReachability == NetworkReachability.NotReachable)
                        {
                            ShowNoInternetPopup();
                            yield return new WaitUntil(() => Application.internetReachability != NetworkReachability.NotReachable);
                            startTime = Time.unscaledTime;
                        }

                        done = false;
                        CdnDownloader.Instance.LoadSprite(key, (sprite) =>
                        {
                            iconSet.icons[idx] = sprite;
                            success = true;
                            done = true;
                        }, () =>
                        {
                            Debug.LogError($"[TileManager] Failed to download icon {idx} for {iconSet.itemCategory}, key: {key}");
                            done = true;
                        });

                        while (!done)
                        {
                            if (Application.internetReachability == NetworkReachability.NotReachable)
                            {
                                Debug.LogWarning("[TileManager] Internet disconnected during download. Showing popup.");
                                ShowNoInternetPopup();
                            }
                            else if (!success && (Time.unscaledTime - startTime > 5f))
                            {
                                if (!NoAdsLoadedPopup.IsShown)
                                {
                                    Debug.LogWarning("[TileManager] Download taking too long (>5s). Showing popup.");
                                    NoAdsLoadedPopup.IsWeakInternetMode = true;
                                    ShowNoInternetPopup();
                                    startTime = Time.unscaledTime;
                                }
                            }

                            if (NoAdsLoadedPopup.IsShown)
                            {
                                startTime = Time.unscaledTime;
                            }

                            yield return null;
                        }

                        if (!success)
                        {
                            yield return new WaitForSecondsRealtime(1f);
                        }
                    }
                }
            }

            // Download spriteOrder
            if (!string.IsNullOrEmpty(iconSet.spriteOrderCdnKey) && iconSet.spriteOrder == null)
            {
                bool done = false;
                bool success = false;

                while (!success)
                {
                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        ShowNoInternetPopup();
                        yield return new WaitUntil(() => Application.internetReachability != NetworkReachability.NotReachable);
                        startTime = Time.unscaledTime;
                    }

                    done = false;
                    CdnDownloader.Instance.LoadSprite(iconSet.spriteOrderCdnKey, (sprite) =>
                    {
                        iconSet.spriteOrder = sprite;
                        success = true;
                        done = true;
                    }, () =>
                    {
                        Debug.LogError($"[TileManager] Failed to download spriteOrder for {iconSet.itemCategory}");
                        done = true;
                    });

                    while (!done)
                    {
                        if (Application.internetReachability == NetworkReachability.NotReachable)
                        {
                            Debug.LogWarning("[TileManager] Internet disconnected during download. Showing popup.");
                            ShowNoInternetPopup();
                        }
                        else if (!success && (Time.unscaledTime - startTime > 5f))
                        {
                            if (!NoAdsLoadedPopup.IsShown)
                            {
                                Debug.LogWarning("[TileManager] Download taking too long (>5s). Showing popup.");
                                NoAdsLoadedPopup.IsWeakInternetMode = true;
                                ShowNoInternetPopup();
                                startTime = Time.unscaledTime;
                            }
                        }

                        if (NoAdsLoadedPopup.IsShown)
                        {
                            startTime = Time.unscaledTime;
                        }

                        yield return null;
                    }

                    if (!success)
                    {
                        yield return new WaitForSecondsRealtime(1f);
                    }
                }
            }
        }

        if (NoAdsLoadedPopup.IsShown)
        {
            NoAdsLoadedPopup.Instance?.HidePopup();
        }

        onComplete?.Invoke();
    }

    private void SetupLevelInternal(LevelItem levelItem)
    {
        if (levelItem != null && levelItem.themeDataList != null && levelItem.tileIconContainer != null)
        {
            foreach (var theme in levelItem.themeDataList)
            {
                if (theme.numSprite == 0 && theme.numString == 0)
                {
                    foreach (var iconSet in levelItem.tileIconContainer.categoryIconSets)
                    {
                        if (iconSet.itemCategory == theme.category)
                        {
                            theme.numSprite = iconSet.icons != null ? iconSet.icons.Length : 0;
                            theme.numString = iconSet.text != null ? iconSet.text.Length : 0;
                            break;
                        }
                    }
                }
            }
        }

        if (levelItem != null && levelItem.isTutorial)
        {
            string prefKey = "Tutorial_Level_" + GamePersistentVariable.currentLevel;
            isCurrentLevelTutorialActive = PlayerPrefs.GetInt(prefKey, 0) == 0;
            if (isCurrentLevelTutorialActive)
            {
                PlayerPrefs.SetInt(prefKey, 1);
                PlayerPrefs.Save();
            }
        }
        else
        {
            isCurrentLevelTutorialActive = false;
        }

        isLevelActive = false;
        SoundManager.Instance?.StopSoundCountingTime();
        _hasTriggeredWin = false;
        if (levelItem.timeLimit > 0)
        {
            currentLevelTime = levelItem.timeLimit;
        }
        else
        {
            currentLevelTime = 0;
        }

        isTimeFrozen = false;
        if (freezeTimeTween.isAlive)
        {
            freezeTimeTween.Stop();
        }

        IsShuffling = false;
        _isDealingStartCards = false;

        // Cập nhật vị trí Y của tileContainer từ levelItem
        if (tileContainer != null && levelItem != null)
        {
            Vector3 pos = tileContainer.localPosition;
            pos.y = levelItem.ActualTileContainerYOffset;
            tileContainer.localPosition = pos;
        }

        // Cập nhật vị trí Y của head nếu grid nhỏ hơn 3x4 (tổng < 12)
        if (head != null && levelItem != null)
        {
            Vector2 headPos = head.anchoredPosition;
            if (levelItem.initRow * levelItem.initColumn < 12)
            {
                headPos.y = originalHeadY - 30f; // đẩy head xuống 1 khoảng Y
            }
            else
            {
                headPos.y = originalHeadY;
            }
            head.anchoredPosition = headPos;
        }

        availableItemIdsPerCategory.Clear();

        foreach (var theme in levelItem.themeDataList)
        {
            var iconSet = tileIconContainer.categoryIconSets.FirstOrDefault(x => x.itemCategory == theme.category);
            if (iconSet != null)
            {
                int spriteCount;
                if (iconSet.isOnServer)
                {
                    // CDN category (index 21+): icons may have been cleared after upload.
                    // CDN download already ran above, so icons[] should now be populated.
                    // Use iconCdnKeys.Length as authoritative count (in case icons are still null).
                    spriteCount = (iconSet.icons != null && iconSet.icons.Length > 0)
                        ? iconSet.icons.Length
                        : (iconSet.iconCdnKeys != null ? iconSet.iconCdnKeys.Length : 0);
                }
                else
                {
                    // Local category (index 0-20): always use icons array directly.
                    spriteCount = iconSet.icons != null ? iconSet.icons.Length : 0;
                }

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

        if (isCurrentLevelTutorialActive)
        {
            if (GamePersistentVariable.currentLevel == 1 || GamePersistentVariable.currentLevel == 13)
            {
                TutorialLevelOne tut1 = TutorialLevelOne.Ins != null ? TutorialLevelOne.Ins : FindObjectOfType<TutorialLevelOne>(true);
                if (tut1 != null)
                {
                    tut1.gameObject.SetActive(true);
                    tut1.StartTutorial();
                }
            }
            else if (GamePersistentVariable.currentLevel == 2)
            {
                TutorialLevelTwo tut2 = TutorialLevelTwo.Ins != null ? TutorialLevelTwo.Ins : FindObjectOfType<TutorialLevelTwo>(true);
                if (tut2 != null)
                {
                    tut2.gameObject.SetActive(true);
                    tut2.StartTutorial();
                }
            }
            else if (GamePersistentVariable.currentLevel == 5)
            {
                TutorialLevelFive tut5 = TutorialLevelFive.Ins != null ? TutorialLevelFive.Ins : FindObjectOfType<TutorialLevelFive>(true);
                if (tut5 != null)
                {
                    tut5.gameObject.SetActive(true);
                    tut5.StartTutorial();
                }
            }
            else if (GamePersistentVariable.currentLevel == 7)
            {
                TutorialLevelSeven tut7 = TutorialLevelSeven.Ins != null ? TutorialLevelSeven.Ins : FindObjectOfType<TutorialLevelSeven>(true);
                if (tut7 != null)
                {
                    tut7.gameObject.SetActive(true);
                    tut7.StartTutorial();
                }
            }
            else if (GamePersistentVariable.currentLevel == 10)
            {
                TutorialLevelTen tut10 = TutorialLevelTen.Ins != null ? TutorialLevelTen.Ins : FindObjectOfType<TutorialLevelTen>(true);
                if (tut10 != null)
                {
                    tut10.gameObject.SetActive(true);
                    tut10.StartTutorial();
                }
            }
            else if (GamePersistentVariable.currentLevel == 15)
            {
                TutorialLevelSixteen tut16 = TutorialLevelSixteen.Ins != null ? TutorialLevelSixteen.Ins : FindObjectOfType<TutorialLevelSixteen>(true);
                if (tut16 != null)
                {
                    tut16.gameObject.SetActive(true);
                    tut16.StartTutorial();
                }
            }
        }
        else
        {
            if (TutorialLevelOne.Ins != null)
            {
                TutorialLevelOne.Ins.HideTutorial();
            }

            if (TutorialLevelTwo.Ins != null)
            {
                TutorialLevelTwo.Ins.HideTutorial();
            }

            if (TutorialLevelFive.Ins != null)
            {
                TutorialLevelFive.Ins.HideTutorial();
            }

            if (TutorialLevelSeven.Ins != null)
            {
                TutorialLevelSeven.Ins.HideTutorial();
            }

            if (TutorialLevelTen.Ins != null)
            {
                TutorialLevelTen.Ins.HideTutorial();
            }

            if (TutorialLevelSixteen.Ins != null)
            {
                TutorialLevelSixteen.Ins.HideTutorial();
            }
        }

        SpawnTiles(totalSet);

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

        if (isCurrentLevelTutorialActive && GamePersistentVariable.currentLevel == 2)
        {
            result.Remove(ItemCategory.Fast_Food);
            result.Remove(ItemCategory.Gaming);

            result.Insert(0, ItemCategory.Gaming);
            result.Insert(0, ItemCategory.Fast_Food);
        }
        else if (isCurrentLevelTutorialActive && GamePersistentVariable.currentLevel == 5)
        {
            result.Remove(ItemCategory.Number_5);
            result.Insert(0, ItemCategory.Number_5);
        }

        else if (isCurrentLevelTutorialActive && GamePersistentVariable.currentLevel == 10)
        {
            if (targetThemeLevel10 != ItemCategory.None)
            {
                result.Remove(targetThemeLevel10);
                result.Insert(0, targetThemeLevel10);
            }
        }
        else if (isCurrentLevelTutorialActive && GamePersistentVariable.currentLevel == 15)
        {
            if (levelItem.themeDataList.Count > 2)
            {
                ItemCategory lockedTheme = levelItem.themeDataList[2].category;
                result.Remove(lockedTheme);
                if (result.Count >= 2)
                {
                    result.Insert(2, lockedTheme);
                }
                else
                {
                    result.Add(lockedTheme);
                }
            }
        }

        return result;
    }

    private ItemCategory GetNextCategoryBasedOnDifficulty(int difficultyLevel)
    {
        // 1. Collect currently active ordered categories (unlocked, not completed, not hidden)
        List<ItemCategory> orderedCategories = tileSlotContainers
            .Where(c => !c.isLocked && !c.isHidden && !c.isCompleted)
            .Select(c => c.ItemCategory)
            .Distinct()
            .ToList();

        ItemCategory chosenCategory;

        if (orderedCategories.Count > 0)
        {
            List<ItemCategory> matchingInPool = itemCategoryPool
                .Where(c => orderedCategories.Contains(c))
                .ToList();

            List<ItemCategory> nonMatchingInPool = itemCategoryPool
                .Where(c => !orderedCategories.Contains(c))
                .ToList();

            if (matchingInPool.Count > 0)
            {
                int countOnGrid = tiles.Count(tile => !tile.IsDone && !tile.IsOnSlot && orderedCategories.Contains(tile.ItemCategory));
                float ratio = 0f;
                if (countOnGrid <= 1) ratio = 1.0f;
                else if (countOnGrid == 2) ratio = 0.85f;
                else if (countOnGrid == 3) ratio = 0.60f;
                else if (countOnGrid == 4) ratio = 0.40f;
                else if (countOnGrid == 5) ratio = 0.35f;
                else if (countOnGrid == 6) ratio = 0.20f;
                else if (countOnGrid == 7) ratio = 0.10f;
                else ratio = 0.0f;

                float randVal = UnityEngine.Random.value;

                if (randVal <= ratio || nonMatchingInPool.Count == 0)
                {
                    chosenCategory = matchingInPool[UnityEngine.Random.Range(0, matchingInPool.Count)];
                    itemCategoryPool.Remove(chosenCategory);
                    return chosenCategory;
                }
                else
                {
                    chosenCategory = nonMatchingInPool[UnityEngine.Random.Range(0, nonMatchingInPool.Count)];
                    itemCategoryPool.Remove(chosenCategory);
                    return chosenCategory;
                }
            }
            else if (nonMatchingInPool.Count > 0)
            {
                chosenCategory = nonMatchingInPool[UnityEngine.Random.Range(0, nonMatchingInPool.Count)];
                itemCategoryPool.Remove(chosenCategory);
                return chosenCategory;
            }
        }

        // Fallback: If no ordered categories or target pools are empty, use score difficulty sorting
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

        for (int i = difficultyLevel; i < scoreOfEachCategory.Count; i++)
        {
            if (itemCategoryPool.Contains(sortedCategory[i]))
            {
                itemCategoryPool.Remove(sortedCategory[i]);

                return sortedCategory[i];
            }
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

        if (isCurrentLevelTutorialActive && GamePersistentVariable.currentLevel == 2)
        {
            if (itemCategoryPool.Remove(ItemCategory.Gaming))
            {
                itemCategoryPool.Insert(0, ItemCategory.Gaming);
            }

            int fastFoodCount = 0;
            if (itemCategoryPool.Remove(ItemCategory.Fast_Food)) fastFoodCount++;
            if (itemCategoryPool.Remove(ItemCategory.Fast_Food)) fastFoodCount++;

            for (int i = 0; i < fastFoodCount; i++)
            {
                itemCategoryPool.Insert(0, ItemCategory.Fast_Food);
            }
        }
        else if (isCurrentLevelTutorialActive && GamePersistentVariable.currentLevel == 5)
        {
            int number5Count = 0;
            if (itemCategoryPool.Remove(ItemCategory.Number_5)) number5Count++;
            if (itemCategoryPool.Remove(ItemCategory.Number_5)) number5Count++;

            for (int i = 0; i < number5Count; i++)
            {
                itemCategoryPool.Insert(0, ItemCategory.Number_5);
            }
        }
        else if (isCurrentLevelTutorialActive && GamePersistentVariable.currentLevel == 10)
        {
            if (itemCategoryPool.Count > 0)
            {
                ItemCategory targetTheme = itemCategoryPool[0];
                targetThemeLevel10 = targetTheme;

                int gridSize = (levelItem.initRow > 0 && levelItem.initColumn > 0) ? levelItem.initRow * levelItem.initColumn : 16;
                int removedCount = itemCategoryPool.RemoveAll(c => c == targetTheme);

                int insertPos = Mathf.Min(gridSize - 2, itemCategoryPool.Count);
                if (insertPos < 0) insertPos = 0;

                int numInGrid = Mathf.Min(2, removedCount);
                for (int i = 0; i < numInGrid; i++)
                {
                    itemCategoryPool.Insert(insertPos, targetTheme);
                }

                for (int i = numInGrid; i < removedCount; i++)
                {
                    itemCategoryPool.Add(targetTheme);
                }
            }
        }
        else if (isCurrentLevelTutorialActive && GamePersistentVariable.currentLevel == 15)
        {
            if (levelItem.themeDataList.Count > 2)
            {
                ItemCategory lockedTheme = levelItem.themeDataList[2].category;
                int lockedThemeCount = itemCategoryPool.Count(c => c == lockedTheme);
                itemCategoryPool.RemoveAll(c => c == lockedTheme);

                int numInGrid = Mathf.Min(3, lockedThemeCount);
                for (int i = 0; i < numInGrid; i++)
                {
                    itemCategoryPool.Insert(0, lockedTheme);
                }

                for (int i = numInGrid; i < lockedThemeCount; i++)
                {
                    itemCategoryPool.Add(lockedTheme);
                }
            }
        }

        return itemCategoryPool;
    }
    #endregion

    private void OnSlotCompleted(TileSlotContainer tileSlotContainer, int index)
    {
        tileSlotContainer.isCompleted = true;

        remainingSetToComplete--;
        if (remainingSetToComplete <= 0)
        {
            isLevelActive = false;
        }


        if (tileSlotContainer == null) return;

        bool isAnyRemainingTileAfterDelay = tiles.Any(item => !item.IsOnSlot);

        if (remainingSlotContainer > 0 && isAnyRemainingTileAfterDelay)
        {
            SpawnSlot(index);
        }
        else
        {
            HideSlot(index);
        }
    }

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
                    //SoundManager.Instance.PlaySoundCardPutIntoTheme();
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
                            OnSlotCompleted(tileSlotContainer, index);
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
        //SoundManager.Instance.PlaySoundCardPutIntoTheme();
        Tween.Position(tile.transform, slot.transform.position, duration: 0.3f).OnComplete(() =>
        {
            tile.IsMoving = false;

            tile.IsDone = true;
            tiles.Remove(tile);
            tile.transform.SetParent(slot.transform);

            tileSlotContainer.SwitchItem(tile);

            if (tileSlotContainer.IsLastSlot(slot) && !tileSlotContainer.isCompleted)
            {
                OnSlotCompleted(tileSlotContainer, tileSlotContainerIndex);
            }

            // DropTileDown(tile);
        });

        slot.AddTile(tile);
        tileSlotContainer.AddTile(tile);
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

    public TileSlotContainer GetHoveredContainer(Vector2 screenPoint, Camera camera)
    {
        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            TileSlotContainer tileSlotContainer = tileSlotContainers[i];

            if (tileSlotContainer.isLocked || tileSlotContainer.isHidden || tileSlotContainer.isMoving)
            {
                continue;
            }

            RectTransform rectTransform = tileSlotContainer.GetComponent<RectTransform>();
            if (rectTransform != null && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, camera))
            {
                return tileSlotContainer;
            }
        }

        return null;
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
        //SoundManager.Instance.PlaySoundDealCards();
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
                    tile.txCount.gameObject.SetActive(false);

                    tiles.Add(tile);

                    // Vector3 spawnPos = grids[i].position.ChangeY(highestYGridPosition + 200);

                    tile.transform.SetParent(tileContainer);
                    tile.transform.position = CardStockPile.Ins.transform.position;

                    ItemCategory nextCategory = GetNextCategoryBasedOnDifficulty(difficultyLevel);
                    if (isCurrentLevelTutorialActive)
                    {
                        if (GamePersistentVariable.currentLevel == 2)
                        {
                            TutorialLevelTwo tut2 = TutorialLevelTwo.Ins != null ? TutorialLevelTwo.Ins : FindObjectOfType<TutorialLevelTwo>(true);
                            if (tut2 != null && tut2.gameObject.activeSelf)
                            {
                                nextCategory = tut2.GetTutorialForcedCategory(nextCategory);
                            }
                        }
                        else
                        {
                            TutorialLevelOne tut1 = TutorialLevelOne.Ins != null ? TutorialLevelOne.Ins : FindObjectOfType<TutorialLevelOne>(true);
                            if (tut1 != null && tut1.gameObject.activeSelf)
                            {
                                nextCategory = tut1.GetTutorialForcedCategory(nextCategory);
                            }
                        }
                    }

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
                                    SoundManager.Instance?.PlaySoundFlipCard();
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
        //SoundManager.Instance.PlaySoundDealCards();
        int col = levelItem.initColumn;
        int row = levelItem.initRow;
        // Use custom spacing from LevelItem if set (> 0), otherwise fall back to defaults.
        // Useful when row < 4 or col < 5 and tiles need more spread.
        float tileSpacingWidth = levelItem.ActualSpacingWidth;   // default 210
        float tileSpacingHeight = levelItem.ActualSpacingHeight;  // default 266

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
        remainingSetToComplete = totalSet;
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
            int actualYIndex = yIndex;
            if (isCurrentLevelTutorialActive)
            {
                actualYIndex = row - 1 - yIndex;
            }
            float yPos = actualYIndex * tileSpacingHeight - totalHeight / 2f;

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
        _isDealingStartCards = true;
        // Prepare
        foreach (var grid in grids)
        {
            if (grid == null || grid.tile == null) continue;

            BaseTile tile = grid.tile;
            tile.SetFaceUp(false);
            tile.transform.position = CardStockPile.Ins.transform.position;
            tile.transform.localEulerAngles = Vector3.zero;
            tile.transform.localScale = Vector3.one;
            if (tile.ObjHide != null)
            {
                tile.ObjHide.transform.localScale = Vector3.one;
            }
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
            SoundManager.Instance?.PlaySoundDealCards();
            Tween.LocalPosition(tile.transform, grid.position, dealDuration);

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
                    SoundManager.Instance?.PlaySoundFlipCard();
                    tile.SetFaceUp(true);
                    Tween.LocalEulerAngles(tile.transform, new Vector3(0, 90f, 0), Vector3.zero, flipDuration)
                        .OnComplete(() =>
                        {
                            if (tile != null) tile.IsMoving = false;
                        });
                }
            });

            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(0.15f);

        _isDealingStartCards = false;
        isLevelActive = true;
    }
    #endregion

    private void InitTileWithSpriteOrString(BaseTile tile, ItemCategory nextCategory)
    {
        CategoryIconSet iconSet = tileIconContainer.categoryIconSets.First(x => x.itemCategory == nextCategory);

        int itemId = GetUniqueItemId(nextCategory, iconSet);

        int spriteCount;
        string textData = null;
        Sprite spriteData = null;

        if (iconSet.isOnServer)
        {

            spriteCount = (iconSet.icons != null && iconSet.icons.Length > 0)
                ? iconSet.icons.Length
                : (iconSet.iconCdnKeys != null ? iconSet.iconCdnKeys.Length : 0);

            if (itemId < spriteCount)
            {
                if (iconSet.icons != null && iconSet.icons.Length > itemId && iconSet.icons[itemId] != null)
                {
                    spriteData = iconSet.icons[itemId];
                }
                // Fallback: read directly from CDN memory cache
                if (spriteData == null && iconSet.iconCdnKeys != null
                    && iconSet.iconCdnKeys.Length > itemId && GplayCdn.CdnDownloader.Instance != null)
                {
                    string key = iconSet.iconCdnKeys[itemId];
                    if (!string.IsNullOrEmpty(key))
                        spriteData = GplayCdn.CdnDownloader.Instance.GetCachedSprite(key);
                }
            }
            else
            {
                if (iconSet.text != null && iconSet.text.Length > (itemId - spriteCount))
                    textData = iconSet.text[itemId - spriteCount];
            }
        }
        else
        {
            // ── Local path (index 0-20): use icons[] directly, no CDN ────────────
            spriteCount = iconSet.icons != null ? iconSet.icons.Length : 0;

            if (itemId < spriteCount)
            {
                spriteData = iconSet.icons[itemId];
            }
            else
            {
                if (iconSet.text != null && iconSet.text.Length > (itemId - spriteCount))
                    textData = iconSet.text[itemId - spriteCount];
            }
        }

        tile.Init(nextCategory, itemId, spriteData, textData);
    }

    private int GetUniqueItemId(ItemCategory category, CategoryIconSet iconSet)
    {
        if (isCurrentLevelTutorialActive && GamePersistentVariable.currentLevel == 2 && category == ItemCategory.Gaming)
        {
            int spCount;
            if (iconSet.isOnServer)
                spCount = (iconSet.icons != null && iconSet.icons.Length > 0)
                    ? iconSet.icons.Length
                    : (iconSet.iconCdnKeys != null ? iconSet.iconCdnKeys.Length : 0);
            else
                spCount = iconSet.icons != null ? iconSet.icons.Length : 0;

            if (availableItemIdsPerCategory.ContainsKey(category))
            {
                int targetIdIndex = availableItemIdsPerCategory[category].FindIndex(id => id < spCount);
                if (targetIdIndex != -1)
                {
                    int itemId = availableItemIdsPerCategory[category][targetIdIndex];
                    availableItemIdsPerCategory[category].RemoveAt(targetIdIndex);
                    return itemId;
                }
            }
        }
        else if (isCurrentLevelTutorialActive && GamePersistentVariable.currentLevel == 5 && category == ItemCategory.Number_5)
        {
            int spCount;
            if (iconSet.isOnServer)
                spCount = (iconSet.icons != null && iconSet.icons.Length > 0)
                    ? iconSet.icons.Length
                    : (iconSet.iconCdnKeys != null ? iconSet.iconCdnKeys.Length : 0);
            else
                spCount = iconSet.icons != null ? iconSet.icons.Length : 0;

            if (availableItemIdsPerCategory.ContainsKey(category))
            {
                if (TutorialLevelFive.Ins != null && !TutorialLevelFive.Ins.hasDealtFirstTextCard)
                {
                    int textIdIndex = availableItemIdsPerCategory[category].FindIndex(id => id >= spCount);
                    if (textIdIndex != -1)
                    {
                        TutorialLevelFive.Ins.hasDealtFirstTextCard = true;
                        int itemId = availableItemIdsPerCategory[category][textIdIndex];
                        availableItemIdsPerCategory[category].RemoveAt(textIdIndex);
                        return itemId;
                    }
                }
                else if (TutorialLevelFive.Ins != null && !TutorialLevelFive.Ins.hasDealtFirstSpriteCard)
                {
                    int spriteIdIndex = availableItemIdsPerCategory[category].FindIndex(id => id < spCount);
                    if (spriteIdIndex != -1)
                    {
                        TutorialLevelFive.Ins.hasDealtFirstSpriteCard = true;
                        int itemId = availableItemIdsPerCategory[category][spriteIdIndex];
                        availableItemIdsPerCategory[category].RemoveAt(spriteIdIndex);
                        return itemId;
                    }
                }
            }
        }

        if (availableItemIdsPerCategory.ContainsKey(category) && availableItemIdsPerCategory[category].Count > 0)
        {
            int index = UnityEngine.Random.Range(0, availableItemIdsPerCategory[category].Count);
            int itemId = availableItemIdsPerCategory[category][index];
            availableItemIdsPerCategory[category].RemoveAt(index);
            return itemId;
        }

        // Fallback: return a random valid index
        int spriteCount;
        if (iconSet.isOnServer)
            spriteCount = (iconSet.icons != null && iconSet.icons.Length > 0)
                ? iconSet.icons.Length
                : (iconSet.iconCdnKeys != null ? iconSet.iconCdnKeys.Length : 0);
        else
            spriteCount = iconSet.icons != null ? iconSet.icons.Length : 0;

        int textCount = iconSet.text != null ? iconSet.text.Length : 0;
        int totalItems = spriteCount + textCount;
        return UnityEngine.Random.Range(0, Mathf.Max(1, totalItems));
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
        SoundManager.Instance?.StopSoundCountingTime();

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
        BaseTile highlightedTile = null;

        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            var container = tileSlotContainers[i];
            if (!container.isLocked && !container.isHidden && !container.isMoving && !container.isCompleted && container.GetAvailableTileSlot() != null)
            {
                foreach (var tile in tiles)
                {
                    if (!tile.IsOnSlot && !tile.IsOutside && !tile.IsDone && tile.ItemCategory == container.ItemCategory)
                    {
                        activeContainer = container;
                        highlightedTile = tile;
                        break;
                    }
                }

                if (activeContainer != null)
                {
                    break;
                }
            }
        }

        if (activeContainer != null)
        {
            if (upToAll != null && activeContainer.transform.parent == upToAll.transform) return;

            activeContainer.SetHighlight(true);

            if (highlightedTile != null)
            {
                highlightedTile.SetHighlight(true);
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
        for (int i = 0; i < tileSlotContainers.Count; i++)
        {
            var container = tileSlotContainers[i];
            if (!container.isLocked && !container.isHidden && !container.isMoving && !container.isCompleted && container.GetAvailableTileSlot() != null)
            {
                foreach (var tile in tiles)
                {
                    if (!tile.IsOnSlot && !tile.IsOutside && !tile.IsDone && tile.ItemCategory == container.ItemCategory)
                    {
                        return true;
                    }
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
            float ratio = 0.5f;
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

        if (isCurrentLevelTutorialActive && GamePersistentVariable.currentLevel == 10)
        {
            if (targetThemeLevel10 != ItemCategory.None)
            {
                // Move targetTheme to front of shuffledGridCategories
                List<ItemCategory> targetCats = shuffledGridCategories.Where(c => c == targetThemeLevel10).ToList();
                shuffledGridCategories.RemoveAll(c => c == targetThemeLevel10);
                shuffledGridCategories.InsertRange(0, targetCats);

                // Sort tilesToShuffle by y position descending so first elements are at the top row
                tilesToShuffle = tilesToShuffle.OrderByDescending(t => t.transform.localPosition.y).ToList();
            }
        }

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
            Tween.Delay(delay).OnComplete(() => { if (SoundManager.Instance != null) SoundManager.Instance.PlaySoundFlipCard(); });
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

        // Calculate arrival times for all tiles to determine correct rendering order
        var arrivalTimes = new List<System.Tuple<BaseTile, float>>();
        for (int i = 0; i < sortedTargets.Count; i++)
        {
            var target = sortedTargets[i];
            if (target.tile == null) continue;

            float flyDelay = i * 0.15f;
            float distance = Vector3.Distance(target.tile.transform.localPosition, deckLocalPos);
            float moveDuration = Mathf.Max(0.2f, distance / 1500f);
            float arrivalTime = flyDelay + moveDuration;

            arrivalTimes.Add(new System.Tuple<BaseTile, float>(target.tile, arrivalTime));
        }

        // Sort tiles by arrival time in ascending order (earlier arrival -> lower layer/sibling index)
        arrivalTimes.Sort((a, b) => a.Item2.CompareTo(b.Item2));

        // Set sibling index before starting the fly animation
        for (int k = 0; k < arrivalTimes.Count; k++)
        {
            if (arrivalTimes[k].Item1 != null)
            {
                arrivalTimes[k].Item1.transform.SetSiblingIndex(k);
            }
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
            SoundManager.Instance?.PlaySoundDealCards();
            Tween.LocalPosition(tile.transform, target.targetLocalPos, dealDuration);

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

            if (SoundManager.Instance != null) SoundManager.Instance.PlaySoundFlipCard();

            float flipDuration = 0.15f;
            Tween.LocalEulerAngles(tile.transform, Vector3.zero, new Vector3(0, 90f, 0), flipDuration).OnComplete(() =>
            {
                if (tile != null)
                {
                    SoundManager.Instance?.PlaySoundFlipCard();
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
                        //Tween.Alpha(tile.icon, 1, duration: 0.3f);
                        Tween.Position(tile.transform, tileSlot.transform.position, duration: 0.15f).OnComplete(() =>
                        {
                            tile.IsMoving = false;

                            tile.IsDone = true;
                            tiles.Remove(tile);
                            //tile.gameObject.SetActive(false);
                            tile.transform.SetParent(tileSlot.transform);

                            tileSlotContainer.SwitchItem(tile);

                            if (tileSlotContainer.IsLastSlot(tileSlot) && !tileSlotContainer.isCompleted)
                            {
                                OnSlotCompleted(tileSlotContainer, index);
                            }

                            tile.IsOutside = false;

                            UpdateOutsideTileCountText();
                        });

                        tileSlot.AddTile(tile);
                        tileSlotContainer.AddTile(tile);

                        return true;
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region Win Level
    public void CheckWinLevel()
    {
        if (remainingSet <= 0)
        {
            bool anySlotMoving = false;
            if (tileSlotContainers != null)
            {
                for (int i = 0; i < tileSlotContainers.Count; i++)
                {
                    if (tileSlotContainers[i] != null && tileSlotContainers[i].isMoving)
                    {
                        anySlotMoving = true;
                        break;
                    }
                }
            }

            if (anySlotMoving)
            {
                return;
            }

            if (!_hasTriggeredWin)
            {
                InstantWin();
            }
        }
    }

    public void InstantWin()
    {
        SoundManager.Instance?.StopSoundCountingTime();
        if (!_hasTriggeredWin)
        {
            _hasTriggeredWin = true;
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
