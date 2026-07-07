using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.RemoteConfig;
using Lean.Localization;
using NamPhuThuy.GamePlay;
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
    [SerializeField] private TMP_Text coinTextAds;
    [SerializeField] private TMP_Text coinTextNormal;
    [SerializeField] private TMP_Text txCoinResource;
    [SerializeField] private Image imgCoin;
    [SerializeField] private Image imgCoinContinue;
    [SerializeField] private Image imgCoinX5;
    [SerializeField] private ParticleSystem fxGainCoin;


    [SerializeField] private TMP_Text starText;
    [SerializeField] private TMP_Text goldPassKeyText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI txLevel;
    [SerializeField] private RectTransform coinGroup;
    [SerializeField] private RectTransform starGroup;
    [SerializeField] private RectTransform eventItemGroup;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedMultiplyCoinText;
    [SerializeField] private Image coinStartGain;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;
    [SerializeField] private ParticleSystem fxFallingWin;
    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private GoldPassDataContainer goldPassDataContainer;
    [SerializeField] private CoinChallengeConfigContainer coinChallengeConfigContainer;

    [Header("CUSTOMIZE")]
    [SerializeField] private float timeCollectCoin;

    private bool _isCollectCoinRemoteConfig;
    private int _multiplierCoinWinLevel;
    private int _currentCollectedCoin;
    private bool _isAlreadyWatchRewardedAd;
    private int coinClaimDefault = 40;
    private bool _isAtLastLevel;
    #region EVENT
    private bool _isCoinChallengeClaimable;
    #endregion

    public static event Action<int> goLevelEvent;
    public static event Action nextLevelEvent;
    public static event Action<Vector3, int> collectCoinEvent;
    public static event Action<Vector3, Vector3, int, Action<int>> collectCoinWithCallbackEvent;

    protected override void RegisterMoreEvent()
    {
        GameStateWin.winLevelEvent += OnLevelWinAsync;
        DebugPopup.winLevelEvent += OnLevelWinAsync;

        continueButton.onClick.AddListener(Continue);
        //returnHomeButton.onClick.AddListener(ReturnHome);
        x5CoinButton.onClick.AddListener(X5Coin);
    }

    protected override void UnregisterMoreEvent()
    {
        GameStateWin.winLevelEvent -= OnLevelWinAsync;
        DebugPopup.winLevelEvent -= OnLevelWinAsync;
    }

    public void PlayFxWin()
    {
        fxFallingWin.gameObject.SetActive(true);
        fxFallingWin.Play();
    }
    private async void OnLevelWinAsync()
    {
        if (GameplayScreen.Ins != null)
        {
            GameplayScreen.Ins.StopTimerWarning();
        }

        AdmobAdsMax.Instance.ShowBannerCollap();

        coinTextAds.text = "x0";
        coinTextNormal.text = "x0";
        starText.text = "0";
        goldPassKeyText.text = "0";
        txLevel.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{currentLevel.Value}");
        //continueButton.transform.localScale = Vector3.zero;

        if (txCoinResource != null)
        {
            userResourcesObserver.Load();
            txCoinResource.text = $"{userResourcesObserver.UserResources.CoinQuantity}";
        }

        continueButton.interactable = false;
        returnHomeButton.interactable = false;
        x5CoinButton.interactable = false;

        x5CoinButton.transform.localScale = Vector3.one;

        // REMOTE CONFIG IS COLLECT COIN
        _isCollectCoinRemoteConfig = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_COLLECT_COIN_WIN_LEVEL, true);
        //_multiplierCoinWinLevel = RemoteConfigController.GetIntConfig(FirebaseConfig.MULTIPLIER_COIN_WIN_LEVEL, 2);
        //comment de test
        _multiplierCoinWinLevel = 2;

        coinContainer.gameObject.SetActive(_isCollectCoinRemoteConfig);
        x5CoinButton.gameObject.SetActive(_isCollectCoinRemoteConfig);

        if (!_isCollectCoinRemoteConfig)
        {
            //continueButton.GetComponent<RectTransform>().localPosition += new Vector3(0, 80, 0);
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
        //

        CheckEvent();

        Show();

        SoundManager.Instance.PlaySoundWin();

        await Task.Delay(1000);

        if (this == null || gameObject == null) return;

        _currentCollectedCoin = coinClaimDefault;

        AnimateOtherResources();

        continueButton.interactable = true;
        returnHomeButton.interactable = true;
        x5CoinButton.interactable = true;
        //x5CoinButton.interactable =!_isAlreadyWatchRewardedAd;
        TrackLevelWin();

        _isAtLastLevel = GamePersistentVariable.isDisableLoopLevel
                             && currentLevel.Value >= LevelManager.Ins.LevelDataContainer.MaxLevel;
        Debug.Log($"[WinPopup] OnLevelWinAsync: currentLevel={currentLevel.Value}, MaxLevel={LevelManager.Ins.LevelDataContainer.MaxLevel}, isDisableLoopLevel={GamePersistentVariable.isDisableLoopLevel}, _isAtLastLevel={_isAtLastLevel}");

        if (!_isAtLastLevel)
        {
            if (currentLevel.Value - UserData.NumLevelLoop >= LevelManager.Ins.LevelDataContainer.MaxLevel)
            {
                await UserData.SetNumLevelLoop(UserData.NumLevelLoop + 1);
            }
            currentLevel.Value++;
            currentLevel.Save();
            DataUtility.Save(GameConstants.CURRENT_LEVEL_LOOP, -1);

            GamePersistentVariable.currentLevel = currentLevel.Value;
        }
        // If at the last level with loop disabled, keep currentLevel as-is so LevelLoader
        // will reload the same last level and show the "Level Coming Soon" popup.

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

    private void AnimateOtherResources()
    {
        _tweens.Add(Tween.Custom(0, 3, duration: timeCollectCoin, onValueChange: newVal =>
        {
            starText.text = $"{(int)newVal}";
        }));

        int targetNormal = coinClaimDefault;
        _tweens.Add(Tween.Custom(0, targetNormal, duration: timeCollectCoin, onValueChange: newVal =>
        {
            coinTextNormal.text = $"x{(int)newVal}";
        }));

        int targetAds = _multiplierCoinWinLevel * coinClaimDefault;
        _tweens.Add(Tween.Custom(0, targetAds, duration: timeCollectCoin, onValueChange: newVal =>
        {
            coinTextAds.text = $"x{(int)newVal}";
        }));

        CoinChallengeData coinChallengeData = DataUtility.Load(GameConstants.COIN_CHALLENGE_DATA, new CoinChallengeData());

        bool isUnlocked = currentLevel.Value >= 30;

        if (isUnlocked && !coinChallengeData.IsCompleted())
        {
            _tweens.Add(Tween.Custom(0, GamePersistentVariable.coinChallengeItemGainedInLevel, duration: timeCollectCoin, onValueChange: newVal =>
            {
                goldPassKeyText.text = $"{(int)newVal}";
            }));
        }
    }

    private void Continue()
    {
        ClaimCoins(coinClaimDefault, imgCoinContinue);
    }

    private void ClaimCoins(int amount, Image startCoinImage)
    {
        continueButton.interactable = false;
        returnHomeButton.interactable = false;
        x5CoinButton.interactable = false;

        Vector3 targetPos = GetWinCoinTextCanvasLocalPosition();
        Vector3 startPos = GetCoinStartGainCanvasLocalPosition(startCoinImage);

        userResourcesObserver.Load();
        int currentCoins = (int)userResourcesObserver.UserResources.CoinQuantity;
        int finalCoins = currentCoins + amount;

        int coinsLandedCount = 0;
        int coinsToSpawn = 10;

        collectCoinWithCallbackEvent?.Invoke(startPos, targetPos, amount, (index) =>
        {
            coinsLandedCount++;
            int displayedCoins = (int)Mathf.Lerp(currentCoins, finalCoins, (float)coinsLandedCount / coinsToSpawn);
            if (txCoinResource != null)
            {
                txCoinResource.text = $"{displayedCoins}";
            }

            if (index % 2 == 0)
            {
                SoundManager.Instance.PlaySoundCollectCoin();
            }

            if (fxGainCoin != null)
            {
                fxGainCoin.gameObject.SetActive(false);
                fxGainCoin.gameObject.SetActive(true);
                fxGainCoin.Play();
            }

            if (coinsLandedCount == coinsToSpawn)
            {
                _tweens.Add(Tween.Delay(0.5f).OnComplete(() =>
                {
                    if (_isCollectCoinRemoteConfig)
                    {
                        userResourcesObserver.UserResources.CoinQuantity += amount;
                        userResourcesObserver.Save();

                        if (UserResourcesManager.Instance != null)
                        {
                            UserResourcesManager.Instance.RefreshCoinText();
                        }

                        SaferioTracking.TrackResourceSource(
                            currentLevel.Value, "Coin", amount, (int)userResourcesObserver.UserResources.CoinQuantity, "win_level");
                    }

                    _currentCollectedCoin = 0;
                    ShowInterstitialOrTransition();
                }));
            }
        });
    }


    private void ShowInterstitialOrTransition()
    {
        if (_isAlreadyWatchRewardedAd)
        {
            ExecuteTransition();
        }
        else
        {
            DoubleInterAdsConfig doubleInterAdsConfig = GamePersistentVariable.doubleInterAdsConfig;

            if (doubleInterAdsConfig.IsValidDoubleInter() && currentLevel.Value >= doubleInterAdsConfig.minLevel)
            {
                AdmobAdsMax.Instance.ShowDoubleInterstitialAds
                (
                    ExecuteTransition, isShowBreakAds: true
                );
            }
            else
            {
                AdmobAdsMax.Instance.ShowInterstitial(actionIniterClose: ExecuteTransition);
            }
        }
    }

    private void ExecuteTransition()
    {
        Debug.Log($"[WinPopup] ExecuteTransition: _isAtLastLevel={_isAtLastLevel}, currentLevel={currentLevel.Value}");
        if (_isAtLastLevel)
        {
            if (AdsBreakScreen.Instance != null)
            {
                AdsBreakScreen.Instance.Hide();
            }
            LevelLoader.TriggerLevelComingSoon();
        }
        else
        {
            goLevelEvent?.Invoke(currentLevel.Value);
        }
        HideImmediately();

        SaferioTracking.TrackInterstitialAdCompleted(currentLevel.Value, "next_level");

        AdsDurationObserver.duration = 0;
        GamePersistentVariable.isAutoTestingLevel = false;
    }

    public static void TriggerGoLevel(int level)
    {
        goLevelEvent?.Invoke(level);
    }

    public Vector2 GetWinCoinTextCanvasLocalPosition()
    {
        if (imgCoin == null) return Vector2.zero;
        return ((Vector2)imgCoin.transform.position - 0.5f * GamePersistentVariable.canvasSize) / GamePersistentVariable.canvasScale;
    }

    public Vector2 GetCoinStartGainCanvasLocalPosition(Image coinStartGain)
    {
        if (coinStartGain == null) return Vector2.zero;
        return ((Vector2)coinStartGain.transform.position - 0.5f * GamePersistentVariable.canvasSize) / GamePersistentVariable.canvasScale;
    }

    #region X5 Coin
    private void X5Coin()
    {
        OnRewaredAdCompleted();
        ////comment de test
        //AdmobAdsMax.Instance.ShowVideoReward(
        //    OnRewaredAdCompleted, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: ActionWatchVideo.MultiplyCoinWinLevel);
    }

    private void OnRewaredAdCompleted()
    {
        x5CoinButton.interactable = false;
        continueButton.interactable = false;

        _tweens.Add(Tween.Scale(x5CoinButton.transform, 1.1f, duration: 0.15f)
            .OnComplete(() => _tweens.Add(Tween.Scale(x5CoinButton.transform, 0, duration: 0.3f)))
        );

        // Claim rewarded coins (80 coins) starting from imgCoinX5
        ClaimCoins(_multiplierCoinWinLevel * coinClaimDefault, imgCoinX5);

        SaferioTracking.TrackRewardedAdCompleted(currentLevel.Value, "multiply_coin_win_level");

        _isAlreadyWatchRewardedAd = true;
    }

    private void ShowAdsNotLoadedPopup()
    {
        InvokeShowAdNotLoadedPopupEvent();
    }

    private void OnMultiplyCoinTextTranslated()
    {
        // localizedMultiplyCoinText.UpdateTranslationWithParameter(GameConstants.VALUE_PARAMETER, $"{_multiplierCoinWinLevel}");
    }
    #endregion

    private void ReturnHome()
    {
        returnHomeButton.interactable = false;

        if (_isCollectCoinRemoteConfig && _currentCollectedCoin > 0)
        {
            userResourcesObserver.UserResources.CoinQuantity += _currentCollectedCoin;
            userResourcesObserver.Save();

            if (UserResourcesManager.Instance != null)
            {
                UserResourcesManager.Instance.RefreshCoinText();
            }
        }

        Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
    }

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
        if (currentLevel.Value <= LevelManager.Ins.LevelDataContainer.MaxLevel)
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
            ReturnHome();

            return true;
        }

        StarLevelData starLevelData = DataUtility.Load(GameConstants.STAR_LEVEL_DATA, new StarLevelData());

        if (starLevelData.starCollected >= GameConstants.REQUIRE_STAR)
        {
            ReturnHome();

            return true;
        }

        if (currentLevel.Value % 10 == 0 && currentLevel.Value > 0)
        {
            ReturnHome();

            return true;
        }

        if (_isCoinChallengeClaimable)
        {
            ReturnHome();

            return true;
        }

        return false;
    }

    private void CheckEvent()
    {
        //CoinChallengeData coinChallengeData = DataUtility.Load(GameConstants.COIN_CHALLENGE_DATA, new CoinChallengeData());

        //bool isUnlocked = currentLevel.Value >= 30;

        //DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;

        //bool isAvailable = dayOfWeek == DayOfWeek.Monday || dayOfWeek == DayOfWeek.Tuesday;

        //isAvailable = false;

        //if (isUnlocked && isAvailable && !coinChallengeData.IsCompleted())
        //{
        //    eventItemGroup.gameObject.SetActive(true);

        //    coinGroup.localPosition = coinGroup.localPosition.ChangeX(-280);
        //    starGroup.localPosition = coinGroup.localPosition.ChangeX(0);
        //    eventItemGroup.localPosition = coinGroup.localPosition.ChangeX(280);
        //}
        //else
        //{
        //    eventItemGroup.gameObject.SetActive(false);

        //    coinGroup.localPosition = coinGroup.localPosition.ChangeX(-140);
        //    starGroup.localPosition = coinGroup.localPosition.ChangeX(140);
        //}
    }
    #endregion
}
