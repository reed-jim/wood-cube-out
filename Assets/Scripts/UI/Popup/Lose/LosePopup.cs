using System;
using Firebase.RemoteConfig;
using NamPhuThuy.GamePlay;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static GameEnum;

public class LosePopup : BasePopup
{
    [SerializeField] private Button replayButton;
    //[SerializeField] private Button skipLevelButton;
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private Image levelProgressFill;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Button buyTimeByCoinButton;
    [SerializeField] private Button buyTimeByAdsButton;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelObserver levelObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private float transitionDuration;
    [Header("ANOTHERTIMES")]
    [SerializeField] private GameObject objFirstTimes;
    [SerializeField] private GameObject objAnotherTimes;
    [SerializeField] private TMP_Text txPrice;
    [SerializeField] private Button buyTimeByCoinButtonAnother;
    [SerializeField] private Button buyTimeByAdsButtonAnother;
    [SerializeField] private ItemBuyElement itemReviveTime;
    [SerializeField] private ItemBuyElement itemHint;
    [SerializeField] private ItemBuyElement itemFreeze;
    [SerializeField] private ItemBuyElement itemAddSlot;

    #region PRIVATE FIELD
    private bool _isAllowedSkipLevel;
    private int _revivePurchaseCount;
    #endregion

    public static event Action replayLevelEvent;
    public static event Action skipLevelEvent;
    public static event Action<int> changeLivesNumberEvent;
    public static event Action buyTimeReviveEvent;
    public static event Action<int> updateBoosterQuantityEvent;
    public static event Action<ScreenRoute> openIAPShopPopupEvent;

    protected override void RegisterMoreEvent()
    {
        // GameStateLose.loseLevelEvent += OnLevelLose;
        GameStateLose.loseLevelEvent += OnLevelLose;
        //RevivePopup.reviveRefusedEvent += OnLevelLose;
        //GameplayScreen.skipLevelEvent += SkipLevel;
        Application.quitting += OnAppClosed;
        LevelLoader.startLevelEvent += ResetRevivePurchaseCount;

        replayButton.onClick.AddListener(Replay);
        //skipLevelButton.onClick.AddListener(SkipLevel);
        returnHomeButton.onClick.AddListener(ReturnHome);
        buyTimeByCoinButton.onClick.AddListener(BuyTimeByCoin);
        buyTimeByAdsButton.onClick.AddListener(BuyTimeByAds);
        if (buyTimeByCoinButtonAnother != null) buyTimeByCoinButtonAnother.onClick.AddListener(BuyTimeByCoin);
        if (buyTimeByAdsButtonAnother != null) buyTimeByAdsButtonAnother.onClick.AddListener(BuyTimeByAds);

        _isAllowedSkipLevel = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_SHOW_SKIP_LEVEL_BUTTON, true);

        //if (!_isAllowedSkipLevel)
        //{
        //    skipLevelButton.gameObject.SetActive(false);
        //}
    }

    protected override void UnregisterMoreEvent()
    {
        // GameStateLose.loseLevelEvent -= OnLevelLose;
        GameStateLose.loseLevelEvent -= OnLevelLose;
        //RevivePopup.reviveRefusedEvent -= OnLevelLose;
        //GameplayScreen.skipLevelEvent -= SkipLevel;
        Application.quitting -= OnAppClosed;
        LevelLoader.startLevelEvent -= ResetRevivePurchaseCount;
    }

    private void OnAppClosed()
    {
        SaferioTracking.TrackLevelLose(currentLevel.Value, GetLevelProgress(), levelBoosterObserver, EndLevelReason.Quit.ToString());
    }

    // private void OnApplicationQuit()
    // {
    //     SaferioTracking.TrackLevelLose(currentLevel.Value, GetLevelProgress(), levelBoosterObserver, EndLevelReason.Quit.ToString());
    // }

    private void OnLevelLose()
    {
        if (TileManager.Ins != null && TileManager.Ins.CurrentlyDraggedTile != null)
        {
            TileManager.Ins.CurrentlyDraggedTile.CancelDrag();
        }

        levelProgressFill.fillAmount = 0;

        progressText.text = $"{0}%";

        changeLivesNumberEvent?.Invoke(-1);

        float progress = GetLevelProgress();

        SaferioTracking.TrackLevelLose(currentLevel.Value, progress, levelBoosterObserver, EndLevelReason.Lose.ToString());

        // don't track loop level CostCenter
        if (currentLevel.Value <= LevelManager.Ins.LevelDataContainer.MaxLevel)
        {
            SaferioTracking.TrackLevelEnd(currentLevel.Value, isSuccess: false, EndLevelReason.Lose.ToString());
        }

        UpdatePurchaseUI();

        Show(onCompletedAction: () =>
        {
            Tween.Custom(0, progress, duration: transitionDuration, onValueChange: newVal =>
            {
                // +0.1f for better progress bar visual
                levelProgressFill.fillAmount = newVal;

                progressText.text = $"{(int)(newVal * 100)}%";
            });
        });

        SoundManager.Instance.PlaySoundLose();

        // COUNT LOSE LEVEL
        int timeLoseLevel = DataUtility.Load($"Time Lose Level {currentLevel.Value}", 0);

        // -Avoid cheating
        if (progress > 0.2f)
        {
            timeLoseLevel++;

            DataUtility.Save($"Time Lose Level {currentLevel.Value}", timeLoseLevel);
        }

        #region SKIP LEVEL
        //GamePersistentVariable.isAllowedSkipLevel = false;

        //if (!_isAllowedSkipLevel)
        //{
        //    return;
        //}

        //if (timeLoseLevel >= RemoteConfigController.GetIntConfig(FirebaseConfig.MIN_TIME_LOSE_LEVEL_TO_ALLOW_SKIP, 2))
        //{
        //    //skipLevelButton.gameObject.SetActive(true);

        //    GamePersistentVariable.isAllowedSkipLevel = true;
        //}
        //else
        //{
        //    //skipLevelButton.gameObject.SetActive(false);
        //}
        #endregion

        GamePersistentVariable.isLevelLost = true;
    }

    private float GetLevelProgress()
    {
        if (TileManager.Ins == null || TileManager.Ins.levelItem == null)
        {
            return 0f;
        }

        int totalCards = 0;
        var themeDataList = TileManager.Ins.levelItem.themeDataList;
        if (themeDataList != null)
        {
            for (int i = 0; i < themeDataList.Count; i++)
            {
                var theme = themeDataList[i];
                if (theme != null)
                {
                    totalCards += theme.numSprite + theme.numString;
                }
            }
        }

        if (totalCards == 0)
        {
            return 0f;
        }

        int remainingCards = 0;
        if (TileManager.Ins.tiles != null)
        {
            remainingCards += TileManager.Ins.tiles.Count;
        }
        if (TileManager.Ins.itemCategoryPool != null)
        {
            remainingCards += TileManager.Ins.itemCategoryPool.Count;
        }

        int completedCards = totalCards - remainingCards;
        return Mathf.Clamp01((float)completedCards / totalCards);
    }

    private void Replay()
    {
        AdmobAdsMax.Instance.ShowInterstitial(actionIniterClose: OnInterstitialAdCompleted);
    }

    private void OnInterstitialAdCompleted()
    {
        replayLevelEvent?.Invoke();

        SaferioTracking.TrackReplayLevel(currentLevel.Value);
        SaferioTracking.TrackInterstitialAdCompleted(currentLevel.Value, "replay");

        Hide();
    }

    //private void SkipLevel()
    //{
    //    AdmobAdsMax.Instance.ShowVideoReward(
    //        OnRewaredAdCompletedToSkipLevel, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: ActionWatchVideo.SkipLevel);
    //}

    private void OnRewaredAdCompletedToSkipLevel()
    {
        skipLevelEvent?.Invoke();

        SaferioTracking.TrackSkipLevel(currentLevel.Value);
        SaferioTracking.TrackRewardedAdCompleted(currentLevel.Value, "SkipLevel");
        TrackLevelWin();

        Hide();
    }

    private void ShowAdsNotLoadedPopup()
    {
        InvokeShowAdNotLoadedPopupEvent();
    }

    private void BuyTimeByCoin()
    {
        userResourcesObserver.Load();

        int currentPurchaseNumber = _revivePurchaseCount + 1;
        int currentPrice = GetCurrentPrice();
        float reviveTime = GetReviveTime(currentPurchaseNumber);

        if (userResourcesObserver.UserResources.CoinQuantity >= currentPrice)
        {
            userResourcesObserver.ChangeCoin(-currentPrice);

            if (UserResourcesManager.Instance != null)
            {
                UserResourcesManager.Instance.RefreshCoinText();
            }

            SaferioTracking.TrackResourceSink(
                currentLevel.Value, "Coin",
                currentPrice, (int)userResourcesObserver.UserResources.CoinQuantity, "BuyTimeLosePopup");

            GrantPurchaseRewards(currentPurchaseNumber);
            _revivePurchaseCount++;

            Hide(onCompletedAction: () =>
            {
                if (TileManager.Ins != null)
                {
                    TileManager.Ins.currentLevelTime += reviveTime;
                    TileManager.Ins.isLevelActive = true;
                }

                buyTimeReviveEvent?.Invoke();
            });
        }
        else
        {
            Hide(onCompletedAction: () =>
            {
                openIAPShopPopupEvent?.Invoke(ScreenRoute.Lose);
            });
        }
    }

    private void BuyTimeByAds()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            NoAdsLoadedPopup.OnOkClickedNoInternet = OnRewaredAdCompletedToBuyTime;
            SwitchRoute(ScreenRoute.NoInternet);
        }
        else
        {
            AdmobAdsMax.Instance.ShowVideoReward(
                OnRewaredAdCompletedToBuyTime, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: ActionWatchVideo.BuyTime);
        }
    }

    private void OnRewaredAdCompletedToBuyTime()
    {
        SaferioTracking.TrackRewardedAdCompleted(currentLevel.Value, "BuyTimeLosePopup");

        int currentPurchaseNumber = _revivePurchaseCount + 1;
        float reviveTime = GetReviveTime(currentPurchaseNumber);

        GrantPurchaseRewards(currentPurchaseNumber);
        _revivePurchaseCount++;

        Action reviveAction = () =>
        {
            if (TileManager.Ins != null)
            {
                TileManager.Ins.currentLevelTime += reviveTime;
                TileManager.Ins.isLevelActive = true;
            }

            buyTimeReviveEvent?.Invoke();
        };

        if (_isShown)
        {
            Hide(onCompletedAction: reviveAction);
        }
        else
        {
            reviveAction();
        }
    }

    private void ReturnHome()
    {
        //Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
    }

    #region SKIP LEVEL
    private async void TrackLevelWin()
    {
        bool isFirstWinLevel = DataUtility.LoadAsync($"{GameConstants.IS_FIRST_WIN_LEVEL}_{currentLevel.Value}", true);

        if (isFirstWinLevel)
        {
            SaferioTracking.TrackLevelFirstWin(currentLevel.Value, GamePersistentVariable.levelPrefab, levelBoosterObserver);

            DataUtility.SaveAsync($"{GameConstants.IS_FIRST_WIN_LEVEL}_{currentLevel.Value}", false);
        }

        SaferioTracking.TrackLevelWin(currentLevel.Value, levelBoosterObserver);
    }
    #endregion

    #region REVIVE PURCHASE SYSTEM
    private void ResetRevivePurchaseCount()
    {
        _revivePurchaseCount = 0;
    }

    private void UpdatePurchaseUI()
    {
        int n = _revivePurchaseCount + 1;

        if (n == 1)
        {
            objFirstTimes.SetActive(true);
            objAnotherTimes.SetActive(false);
        }
        else
        {
            objFirstTimes.SetActive(false);
            objAnotherTimes.SetActive(true);

            txPrice.text = $"{900 * n}";

            // Update revive time display
            int reviveTime = GetReviveTime(n);
            if (itemReviveTime != null) itemReviveTime.SetData(true, reviveTime);

            // Update booster rewards display
            UpdateRewardElements(n);
        }
    }

    private void UpdateRewardElements(int n)
    {
        // Hide all reward elements first
        if (itemHint != null) itemHint.Hide();
        if (itemFreeze != null) itemFreeze.Hide();
        if (itemAddSlot != null) itemAddSlot.Hide();

        if (n == 2)
        {
            // Lần 2: 1 Hint
            if (itemHint != null) itemHint.SetData(false, 1);
        }
        else if (n == 3)
        {
            // Lần 3: 1 Freeze + 1 AddSlot
            if (itemFreeze != null) itemFreeze.SetData(false, 1);
            if (itemAddSlot != null) itemAddSlot.SetData(false, 1);
        }
        else if (n > 3)
        {
            // Lần n>3: (n-2) Freeze + (n-2) AddSlot
            int boosterCount = n - 2;
            if (itemFreeze != null) itemFreeze.SetData(false, boosterCount);
            if (itemAddSlot != null) itemAddSlot.SetData(false, boosterCount);
        }
    }

    private int GetReviveTime(int purchaseNumber)
    {
        switch (purchaseNumber)
        {
            case 1: return 30;
            case 2: return 40;
            case 3: return 45;
            default: return 50;
        }
    }

    private int GetCurrentPrice()
    {
        return 900 * (_revivePurchaseCount + 1);
    }

    private void GrantPurchaseRewards(int purchaseNumber)
    {
        if (purchaseNumber == 2)
        {
            // 1 Hint
            userResourcesObserver.ChangeBoosterQuantity((int)BoosterType.Hint, 1);
            updateBoosterQuantityEvent?.Invoke((int)BoosterType.Hint);
        }
        else if (purchaseNumber == 3)
        {
            // 1 Freeze + 1 AddSlot (UnlockTheme)
            userResourcesObserver.ChangeBoosterQuantity((int)BoosterType.FreezeTime, 1);
            userResourcesObserver.ChangeBoosterQuantity((int)BoosterType.UnlockTheme, 1);
            updateBoosterQuantityEvent?.Invoke((int)BoosterType.FreezeTime);
            updateBoosterQuantityEvent?.Invoke((int)BoosterType.UnlockTheme);
        }
        else if (purchaseNumber > 3)
        {
            // (n-2) Freeze + (n-2) AddSlot (UnlockTheme)
            int boosterCount = purchaseNumber - 2;
            userResourcesObserver.ChangeBoosterQuantity((int)BoosterType.FreezeTime, boosterCount);
            userResourcesObserver.ChangeBoosterQuantity((int)BoosterType.UnlockTheme, boosterCount);
            updateBoosterQuantityEvent?.Invoke((int)BoosterType.FreezeTime);
            updateBoosterQuantityEvent?.Invoke((int)BoosterType.UnlockTheme);
        }
    }
    #endregion
}
