using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.RemoteConfig;
using Lean.Localization;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedMultiplyCoinText;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;
    [SerializeField] private LevelDataContainer levelDataContainer;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private GoldPassDataContainer goldPassDataContainer;

    [Header("CUSTOMIZE")]
    [SerializeField] private float timeCollectCoin;

    private bool _isCollectCoinRemoteConfig;
    private int _multiplierCoinWinLevel;
    private int _currentCollectedCoin;
    private bool _isAlreadyWatchRewardedAd;

    public static event Action<int> goLevelEvent;
    public static event Action nextLevelEvent;
    public static event Action<Vector3, int> collectCoinEvent;

    protected override void RegisterMoreEvent()
    {
        GameStateWin.winLevelEvent += OnLevelWinAsync;
        DebugPopup.winLevelEvent += OnLevelWinAsync;
        localizedMultiplyCoinText.textTranslatedEvent += OnMultiplyCoinTextTranslated;

        continueButton.onClick.AddListener(Continue);
        returnHomeButton.onClick.AddListener(ReturnHome);
        x5CoinButton.onClick.AddListener(X5Coin);
    }

    protected override void UnregisterMoreEvent()
    {
        GameStateWin.winLevelEvent -= OnLevelWinAsync;
        DebugPopup.winLevelEvent -= OnLevelWinAsync;
        localizedMultiplyCoinText.textTranslatedEvent -= OnMultiplyCoinTextTranslated;
    }

    private async void OnLevelWinAsync()
    {
        AdmobAdsMax.Instance.ShowBannerCollap();

        coinText.text = $"{0}";
        starText.text = $"{0}";
        continueButton.transform.localScale = Vector3.zero;

        continueButton.interactable = true;
        returnHomeButton.interactable = true;
        x5CoinButton.interactable = true;

        x5CoinButton.transform.localScale = Vector3.one;

        // REMOTE CONFIG IS COLLECT COIN
        _isCollectCoinRemoteConfig = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_COLLECT_COIN_WIN_LEVEL, false);
        _multiplierCoinWinLevel = RemoteConfigController.GetIntConfig(FirebaseConfig.MULTIPLIER_COIN_WIN_LEVEL, 5);

        coinContainer.gameObject.SetActive(_isCollectCoinRemoteConfig);
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
        //

        Show();

        SoundManager.Instance.PlaySoundWin();

        await Task.Delay(500);

        _currentCollectedCoin = 0;

        CollectCoin(10);

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
        HandleGoldPass();
        HandleCoinChallenge();
        HandleWeeklyChallenge();
        HandleStarChest();
        #endregion

        GameLeaderboardManager.Instance.AddPlayerScoreToLeaderboard();
    }

    private void CollectCoin(int numCoin, Action onCompletedAction = null)
    {
        coinText.text = $"{_currentCollectedCoin}";

        int deltaCoin = (int)(numCoin / 10f);

        _tweens.Add(Tween.Custom(_currentCollectedCoin, numCoin, duration: timeCollectCoin, onValueChange: newVal =>
        {
            coinText.text = $"{(int)newVal}";
        })
        .OnComplete(() =>
            {
                if (_isCollectCoinRemoteConfig)
                {
                    userResourcesObserver.UserResources.CoinQuantity += numCoin - _currentCollectedCoin;

                    userResourcesObserver.Save();

                    _currentCollectedCoin += numCoin;

                    SaferioTracking.TrackResourceSource(
                        currentLevel.Value, "Coin", numCoin - _currentCollectedCoin, (int)userResourcesObserver.UserResources.CoinQuantity, "win_level");
                }

                _tweens.Add(Tween.Scale(continueButton.transform, 1, duration: timeCollectCoin));

                onCompletedAction?.Invoke();
            })
        );

        _tweens.Add(Tween.Custom(0, 3, duration: timeCollectCoin, onValueChange: newVal =>
        {
            starText.text = $"{(int)newVal}";
        }));
    }

    private async void Continue()
    {
        // collectCoinEvent?.Invoke(coinContainer.localPosition - new Vector3(0, coinContainer.sizeDelta.y), 500);

        // await Task.Delay(3000);

        if (_isAlreadyWatchRewardedAd)
        {
            OnInterstitialAdCompleted();
        }
        else
        {
            continueButton.interactable = false;

            AdmobAdsMax.Instance.ShowInterstitial(actionIniterClose: OnInterstitialAdCompleted);
        }
    }

    private void OnInterstitialAdCompleted()
    {
        _tweens.Add(Tween.Delay(0.5f).OnComplete(() =>
        {
            HideImmediately();
        }));

        goLevelEvent?.Invoke(currentLevel.Value);

        SaferioTracking.TrackInterstitialAdCompleted(currentLevel.Value, "next_level");

        AdsDurationObserver.duration = 0;
        GamePersistentVariable.isAutoTestingLevel = false;
    }

    #region X5 Coin
    private void X5Coin()
    {
        AdmobAdsMax.Instance.ShowVideoReward(
            OnRewaredAdCompleted, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: ActionWatchVideo.MultiplyCoinWinLevel);
    }

    private void OnRewaredAdCompleted()
    {
        x5CoinButton.interactable = false;
        continueButton.interactable = false;

        _tweens.Add(Tween.Scale(x5CoinButton.transform, 1.1f, duration: 0.15f)
            .OnComplete(() => _tweens.Add(Tween.Scale(x5CoinButton.transform, 0, duration: 0.3f)))
        );

        continueButton.transform.localScale = Vector3.zero;

        _tweens.Add(Tween.Scale(continueButton.transform, 1.1f, duration: 0.3f)
            .OnComplete(() => _tweens.Add(Tween.Scale(continueButton.transform, 1, duration: 0.15f)
            .OnComplete(() => continueButton.interactable = true)
            ))
        );

        CollectCoin(_multiplierCoinWinLevel * 10);

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
        if (currentLevel.Value <= levelDataContainer.MaxLevel)
        {
            SaferioTracking.TrackLevelEnd(currentLevel.Value, isSuccess: true, EndLevelReason.Win.ToString());
        }
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
        CoinChallengeData coinChallengeData = DataUtility.Load(GameConstants.COIN_CHALLENGE_DATA, new CoinChallengeData());

        coinChallengeData.gemCollected++;

        DataUtility.Save(GameConstants.COIN_CHALLENGE_DATA, coinChallengeData);
    }

    private void HandleWeeklyChallenge()
    {
        WeeklyChallengeData challengeData = DataUtility.Load(GameConstants.WEEKLY_CHALLENGE_DATA, new WeeklyChallengeData());

        challengeData.EventItemCollected++;

        DataUtility.Save(GameConstants.WEEKLY_CHALLENGE_DATA, challengeData);
    }

    private void HandleStarChest()
    {
        StarLevelData data = DataUtility.Load(GameConstants.STAR_LEVEL_DATA, new StarLevelData());

        data.starCollected += 3;

        DataUtility.Save(GameConstants.STAR_LEVEL_DATA, data);
    }
    #endregion
}
