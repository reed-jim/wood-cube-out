using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.RemoteConfig;
using Lean.Localization;
using NamPhuThuy.AnimateWithScripts;
using NamPhuThuy.Data;
using NamPhuThuy.UI;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;
using static GameEnum;

public class WinPopup : BasePopup
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private Button x5CoinButton;
    [SerializeField] private RectTransform coinContainer;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text starText;
    [SerializeField] private TMP_Text goldPassKeyText;
    [SerializeField] private RectTransform coinGroup;
    [SerializeField] private RectTransform starGroup;
    [SerializeField] private RectTransform eventItemGroup;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedMultiplyCoinText;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;
    [SerializeField] private LevelDataContainer levelDataContainer;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private GoldPassDataContainer goldPassDataContainer;
    [SerializeField] private CoinChallengeConfigContainer coinChallengeConfigContainer;

    [Header("CUSTOMIZE")]
    [SerializeField] private float timeCollectCoin;

    private bool _isCollectCoinRemoteConfig;
    private int _multiplierCoinWinLevel;
    private int _currentCollectedCoin;
    private bool _isAlreadyWatchRewardedAd;

    [Header("WIN ANIM")]
    [SerializeField] private bool interactable = true;
    [SerializeField] private SegmentedProgressBar picRewardProgressBar;
    [SerializeField] private SegmentedProgressBar coinRewardProgressBar;
    private bool[] isRewardProgressCompleted;

    #region EVENT
    private bool _isCoinChallengeClaimable;
    #endregion

    public static event Action<int> goLevelEvent;
    public static event Action nextLevelEvent;
    public static event Action<int> collectCoinEvent;
    public static event Action showSceneTransitionEvent;

    #region Override Methods

    protected override void MoreActionInAwake()
    {
        base.MoreActionInAwake();

        picRewardProgressBar.InitRewardProgressBar();
        coinRewardProgressBar.InitRewardProgressBar();
    }

    protected override void Show(Action onCompletedAction = null)
    {
        picRewardProgressBar.Reset();
        coinRewardProgressBar.Reset();

        // Coin Reward Progress
        float progressPerLevel = 1f / DataManager.Ins.PlayerData.RequiredLevelForCoinRewardGift;

        float prevCoinRewardProgress = DataManager.Ins.PlayerData.CurrentCoinRewardProgress;
        float currentCoinRewardProgress = DataManager.Ins.PlayerData.CurrentCoinRewardProgress + progressPerLevel;

        if (currentCoinRewardProgress > 1)
        {
            currentCoinRewardProgress = progressPerLevel;
        }

        picRewardProgressBar.SetupRewardProgressBar();
        coinRewardProgressBar.SetupRewardProgressBar(currentCoinRewardProgress, prevCoinRewardProgress);

        DataManager.Ins.PlayerData.CurrentCoinRewardProgress = currentCoinRewardProgress;

        base.Show(onCompletedAction);

        DisableInteract();
    }

    protected override void Hide(Action onCompletedAction = null)
    {
        base.Hide(onCompletedAction);
    }

    protected override void RegisterMoreEvent()
    {
        GameStateWin.winLevelEvent += OnLevelWinAsync;
        DebugPopup.winLevelEvent += OnLevelWinAsync;

        continueButton.onClick.AddListener(OnClickContinue);
        returnHomeButton.onClick.AddListener(OnClickHome);
        x5CoinButton.onClick.AddListener(X5Coin);
    }

    protected override void UnregisterMoreEvent()
    {
        GameStateWin.winLevelEvent -= OnLevelWinAsync;
        DebugPopup.winLevelEvent -= OnLevelWinAsync;
    }

    #endregion

    private async void OnLevelWinAsync()
    {
        AdmobAdsMax.Instance.ShowBannerCollap();

        coinText.text = $"{DataConst.COIN_REWARDED_WIN_LEVEL}";
        starText.text = $"{0}";
        goldPassKeyText.text = $"{0}";

        continueButton.interactable = false;
        returnHomeButton.interactable = true;
        x5CoinButton.interactable = true;

        continueButton.transform.localScale = Vector3.zero;
        x5CoinButton.transform.localScale = Vector3.one;

        TweenHelper.DoPunch(continueButton.transform, 1, onCompleteAction: () =>
        {
            continueButton.interactable = true;
        });

        // REMOTE CONFIG IS COLLECT COIN
        _isCollectCoinRemoteConfig = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_COLLECT_COIN_WIN_LEVEL, true);
        _multiplierCoinWinLevel = RemoteConfigController.GetIntConfig(FirebaseConfig.MULTIPLIER_COIN_WIN_LEVEL, 5);

        // coinContainer.gameObject.SetActive(_isCollectCoinRemoteConfig);
        x5CoinButton.gameObject.SetActive(_isCollectCoinRemoteConfig);

        if (!_isCollectCoinRemoteConfig)
        {
            continueButton.GetComponent<RectTransform>().localPosition += new Vector3(0, 80, 0);
        }
        else
        {
            localizedMultiplyCoinText.transform.localScale = Vector3.zero;

            _tweens.Add(Tween.Delay(0.2f).OnComplete(() =>
            {
                localizedMultiplyCoinText.UpdateTranslationWithParameter(GameConstants.VALUE_PARAMETER, $"{_multiplierCoinWinLevel}");

                _tweens.Add(Tween.Scale(localizedMultiplyCoinText.transform, 1, duration: 0.3f));
            }));
        }

        // CheckEvent();

        Show();

        SoundManager.Instance.PlaySoundWin();

        await Task.Delay(500);

        #region NEW

        RunRewardsProgress();

        #endregion

        _currentCollectedCoin = 0;

        int coinQuantity = GamePersistentVariable.isHardMode ? 30 : 10;

        // uncomment to grant free coin win level
        // CollectCoin(coinQuantity);

        TrackLevelWin();

        if (currentLevel.Value - UserData.NumLevelLoop >= levelDataContainer.MaxLevel)
        {
            await UserData.SetNumLevelLoop(UserData.NumLevelLoop + 1);
        }

        currentLevel.Value++;
        currentLevel.Save();
        DataUtility.Save(GameConstants.CURRENT_LEVEL_LOOP, -1);

        GamePersistentVariable.currentLevel = currentLevel.Value;

        _isAlreadyWatchRewardedAd = false;

        #region GOLD PASS
        // HandleGoldPass();
        HandleCoinChallenge();
        HandleStarChest();
        HandleSpaceRace();
        #endregion

        DataUtility.Save("num_level_play_in_day", DataUtility.Load("num_level_play_in_day", 0) + 1);

        // GameLeaderboardManager.Instance.AddPlayerScoreToLeaderboard();
    }

    private void CollectCoin(int numCoin)
    {
        userResourcesObserver.UserResources.CoinQuantity += numCoin - _currentCollectedCoin;

        userResourcesObserver.Save();

        _currentCollectedCoin += numCoin;

        SaferioTracking.TrackResourceSource
        (
            currentLevel.Value, "Coin", numCoin - _currentCollectedCoin, (int)userResourcesObserver.UserResources.CoinQuantity, "win_level"
        );
    }

    #region Button Events

    private async void OnClickContinue()
    {
        DebugLogger.Log();
        // collectCoinEvent?.Invoke(coinContainer.localPosition - new Vector3(0, coinContainer.sizeDelta.y), 500);

        if (!interactable) return;

        if (_isAlreadyWatchRewardedAd)
        {
            DebugLogger.Log(message: $"Already watch rewarded ad, skip interstitial ad");
            OnInterstitialAdCompleted();
        }
        else
        {
            continueButton.interactable = false;

            DoubleInterAdsConfig doubleInterAdsConfig = GamePersistentVariable.doubleInterAdsConfig;

            if (doubleInterAdsConfig.IsValidDoubleInter() && currentLevel.Value >= doubleInterAdsConfig.minLevel)
            {
                DebugLogger.Log(message: $"Call double interstitial ad, 1");
                AdmobAdsMax.Instance.ShowDoubleInterstitialAds
                (
                    OnInterstitialAdCompleted, isShowBreakAds: true
                );
            }
            else
            {
                DebugLogger.Log(message: $"Call interstitial ad, 2");
                AdmobAdsMax.Instance.ShowInterstitial(actionIniterClose: OnInterstitialAdCompleted);
            }
        }
    }
    private void OnClickHome()
    {
        returnHomeButton.interactable = false;

        Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
    }

    #endregion

    private void OnInterstitialAdCompleted()
    {
        PlayCollectCoinAnimationAndNextLevel();

        SaferioTracking.TrackInterstitialAdCompleted(currentLevel.Value, "next_level");
    }

    private void PlayCollectCoinAnimationAndNextLevel()
    {
        _tweens.Add(Tween.Delay(3f).OnComplete(() =>
        {
            HideImmediately();
        }));

        bool isBackHome = HandleSuggestFeature();

        float delayTime = 2;

        if (_currentCollectedCoin > 0)
        {
            collectCoinEvent?.Invoke(_currentCollectedCoin);
        }
        else
        {
            delayTime = 0;
        }

        // wait for collect coin animation
        if (isBackHome)
        {
            Tween.Delay(delayTime).OnComplete(() =>
            {
                OnClickHome();
            });
        }
        else
        {
            Tween.Delay(delayTime).OnComplete(() =>
        {
            showSceneTransitionEvent?.Invoke();
        });

            Tween.Delay(delayTime + 0.5f).OnComplete(() =>
            {
                goLevelEvent?.Invoke(currentLevel.Value);
            });
        }

        AdsDurationObserver.duration = 0;
        GamePersistentVariable.isAutoTestingLevel = false;
    }

    #region X5 Coin
    private void X5Coin()
    {
        if (!interactable)
        {
            return;
        }

        AdmobAdsMax.Instance.ShowVideoReward(
            OnRewaredAdCompleted, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: ActionWatchVideo.MultiplyCoinWinLevel);
    }

    private void OnRewaredAdCompleted()
    {
        x5CoinButton.interactable = false;
        continueButton.interactable = false;

        CollectCoin(DataConst.COIN_REWARDED_WIN_LEVEL);

        PlayCollectCoinAnimationAndNextLevel();

        SaferioTracking.TrackRewardedAdCompleted(currentLevel.Value, "multiply_coin_win_level");

        _isAlreadyWatchRewardedAd = true;
    }

    private void ShowAdsNotLoadedPopup()
    {
        InvokeShowAdNotLoadedPopupEvent();
    }
    #endregion

    private async void TrackLevelWin()
    {
        bool isFirstWinLevel = DataUtility.LoadAsync($"{GameConstants.IS_FIRST_WIN_LEVEL}_{currentLevel.Value}", true);

        if (isFirstWinLevel)
        {
            SaferioTracking.TrackLevelFirstWin(currentLevel.Value, GamePersistentVariable.levelPrefab, levelBoosterObserver);

            DataUtility.SaveAsync($"{GameConstants.IS_FIRST_WIN_LEVEL}_{currentLevel.Value}", false);
        }

        SaferioTracking.TrackLevelWin(currentLevel.Value, levelBoosterObserver);

        // don't track loop level CostCenter
        if (currentLevel.Value <= levelDataContainer.MaxLevel)
        {
            SaferioTracking.TrackLevelEnd(currentLevel.Value, isSuccess: true, EndLevelReason.Win.ToString());
        }

        SaferioTracking.TrackNumLevelPassed(currentLevel.Value);
    }

    #region GOLD PASS
    private void HandleGoldPass()
    {
        GamePersistentVariable.goldPassData.keyCollected++;

        if (GamePersistentVariable.goldPassData.keyCollected >= goldPassDataContainer.CurrentMilestone.KeyRequired)
        {
            goldPassDataContainer.CurrentMilestone.IsPassed = true;

            GamePersistentVariable.goldPassData.passLevel++;
            GamePersistentVariable.goldPassData.keyCollected = 0;

            GamePersistentVariable.goldPassData.goldPassMilestones = goldPassDataContainer.goldPassMilestones;
        }

        DataUtility.Save(GameConstants.GOLD_PASS_DATA_CONTAINER, GamePersistentVariable.goldPassData);
    }
    #endregion

    #region COIN CHALLENGE
    private void HandleCoinChallenge()
    {
        CoinChallengeData coinChallengeData = DataUtility.Load<CoinChallengeData>(GameConstants.COIN_CHALLENGE_DATA, null);

        if (coinChallengeData == null)
        {
            coinChallengeData = new CoinChallengeData();
            coinChallengeData.startTimestamp = CommonUtil.GetCurrentTimestamp();
        }

        DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;

        bool isAvailable = dayOfWeek == DayOfWeek.Monday || dayOfWeek == DayOfWeek.Tuesday;

        if (currentLevel.ValueUsingPureJson < 30 || !isAvailable)
        {
            return;
        }

        if (coinChallengeData.IsCompleted())
        {
            return;
        }

        int currentMilestone = coinChallengeData.CurrentMilestone;

        coinChallengeData.EventItemCollectedOfMilstones[currentMilestone] += GamePersistentVariable.coinChallengeItemGainedInLevel;
        coinChallengeData.newGainedItem += GamePersistentVariable.coinChallengeItemGainedInLevel;

        _isCoinChallengeClaimable = false;

        if (coinChallengeData.EventItemCollectedOfMilstones[currentMilestone] >= coinChallengeConfigContainer.RequiredItems[currentMilestone])
        {
            int overflowItem = coinChallengeData.EventItemCollectedOfMilstones[currentMilestone] - coinChallengeConfigContainer.RequiredItems[currentMilestone];

            if (coinChallengeData.CurrentMilestone < coinChallengeData.EventItemCollectedOfMilstones.Length)
            {
                coinChallengeData.CurrentMilestone++;
                coinChallengeData.EventItemCollectedOfMilstones[coinChallengeData.CurrentMilestone] += overflowItem;
            }
            else
            {
                coinChallengeData.isCompleted = true;
            }

            _isCoinChallengeClaimable = true;
        }

        for (int i = 0; i < coinChallengeData.TotalMilestone; i++)
        {
            if (i < currentMilestone)
            {
                if (!coinChallengeData.IsMilestonesClaimed[i])
                {
                    _isCoinChallengeClaimable = true;

                    break;
                }
            }
        }

        DataUtility.Save(GameConstants.COIN_CHALLENGE_DATA, coinChallengeData);
    }

    private void HandleWeeklyChallenge()
    {
        WeeklyChallengeData challengeData = DataUtility.Load(GameConstants.WEEKLY_CHALLENGE_DATA, new WeeklyChallengeData());

        challengeData.AddItemCurrentMilestone();

        DataUtility.Save(GameConstants.WEEKLY_CHALLENGE_DATA, challengeData);
    }

    private void HandleStarChest()
    {
        StarLevelData data = DataUtility.Load(GameConstants.STAR_LEVEL_DATA, new StarLevelData());

        data.starCollected += 3;
        data.newGainedItem += 3;

        DataUtility.Save(GameConstants.STAR_LEVEL_DATA, data);
    }

    private void HandleSpaceRace()
    {
        SpaceRaceData data = DataUtility.Load(GameConstants.SPACE_RACE_DATA, new SpaceRaceData());

        if (!data.isStarted)
        {
            return;
        }

        for (int i = 0; i < data.spaceRacePlayerGroup.Length; i++)
        {
            if (data.spaceRacePlayerGroup[i].isSelf)
            {
                data.spaceRacePlayerGroup[i].scoreSinceRaceBegin++;
            }
        }

        DataUtility.Save(GameConstants.SPACE_RACE_DATA, data);
    }

    private bool HandleSuggestFeature()
    {
        if (currentLevel.Value == 30 && !DataUtility.Load("Is Coin Challenge Suggested", false))
        {
            return true;
        }

        StarLevelData starLevelData = DataUtility.Load(GameConstants.STAR_LEVEL_DATA, new StarLevelData());

        if (starLevelData.starCollected >= GameConstants.REQUIRE_STAR)
        {
            return true;
        }

        if (currentLevel.Value % 10 == 0 && currentLevel.Value > 0)
        {
            return true;
        }

        if (_isCoinChallengeClaimable)
        {
            return true;
        }

        return false;
    }

    private void CheckEvent()
    {
        CoinChallengeData coinChallengeData = DataUtility.Load(GameConstants.COIN_CHALLENGE_DATA, new CoinChallengeData());

        bool isUnlocked = currentLevel.Value >= 30;

        DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;

        bool isAvailable = dayOfWeek == DayOfWeek.Monday || dayOfWeek == DayOfWeek.Tuesday;

        isAvailable = false;

        if (isUnlocked && isAvailable && !coinChallengeData.IsCompleted())
        {
            eventItemGroup.gameObject.SetActive(true);

            coinGroup.localPosition = coinGroup.localPosition.ChangeX(-280);
            starGroup.localPosition = coinGroup.localPosition.ChangeX(0);
            eventItemGroup.localPosition = coinGroup.localPosition.ChangeX(280);
        }
        else
        {
            eventItemGroup.gameObject.SetActive(false);

            coinGroup.localPosition = coinGroup.localPosition.ChangeX(-140);
            starGroup.localPosition = coinGroup.localPosition.ChangeX(140);
        }
    }

    #endregion

    #region Progress

    private void EnableInteract()
    {
        interactable = true;
    }

    private void DisableInteract()
    {
        interactable = false;
    }

    private void RunRewardsProgress()
    {
        isRewardProgressCompleted = new bool[2];

        bool willCoinProgressComplete = coinRewardProgressBar.WillProgressCompleted();

        coinRewardProgressBar.Progress(() =>
        {
            SetRewardProgressComplete(1);

            if (willCoinProgressComplete)
            {
                picRewardProgressBar.Progress(() =>
                {
                    SetRewardProgressComplete(0);
                });
            }
        });

        if (!willCoinProgressComplete)
        {
            picRewardProgressBar.Progress(() =>
            {
                SetRewardProgressComplete(0);
            });
        }
    }

    private void SetRewardProgressComplete(int i)
    {
        isRewardProgressCompleted[i] = true;

        bool isDone = true;

        for (int j = 0; j < isRewardProgressCompleted.Length; j++)
        {
            if (!isRewardProgressCompleted[j])
            {
                isDone = false;

                break;
            }
        }

        if (isDone)
        {
            EnableInteract();
        }
    }

    #endregion
}
