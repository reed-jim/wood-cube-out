using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AntsFlow;
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
    [SerializeField] private TMP_Text normalCoinRewardText;
    [SerializeField] private TMP_Text rewardedCoinRewardText;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text starText;
    [SerializeField] private TMP_Text goldPassKeyText;
    [SerializeField] private RectTransform coinGroup;
    [SerializeField] private RectTransform starGroup;
    [SerializeField] private RectTransform eventItemGroup;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedMultiplyCoinText;
    [SerializeField] private RectTransform coinContinueButton;
    [SerializeField] private RectTransform coinX2Button;

    [SerializeField] private ParticleSystem completeFx;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private GoldPassDataContainer goldPassDataContainer;

    [Header("CUSTOMIZE")]
    [SerializeField] private int numCoinNormal;
    [SerializeField] private int numCoinRewardedAd;
    [SerializeField] private int numCoinNormalHardLevel;
    [SerializeField] private int numCoinRewardedAdHardLevel;
    [SerializeField] private float timeCollectCoin;
    [SerializeField] private float timeDelayShowContinueButton;

    private bool _isCollectCoinRemoteConfig;
    private int _multiplierCoinWinLevel;
    private int _currentCollectedCoin;
    private bool _isAlreadyWatchRewardedAd;

    #region EVENT
    private bool _isCoinChallengeClaimable;
    #endregion

    public static event Action<int> goLevelEvent;
    public static event Action nextLevelEvent;
    public static event Action<int> collectCoinEvent;
    public static event Action fetchLocalizedPriceIAPEvent;
    public static event Action resetLevelEvent;

    protected override void RegisterMoreEvent()
    {
        GameStateWin.winLevelEvent += OnLevelWinAsync;
        DebugPopup.winLevelEvent += OnLevelWinAsync;
        // RewardCollectingUI.collectCoinCompleteEvent += OnCollectCoinCompleted;

        continueButton.onClick.AddListener(Continue);
        x5CoinButton.onClick.AddListener(X5Coin);
    }

    protected override void UnregisterMoreEvent()
    {
        GameStateWin.winLevelEvent -= OnLevelWinAsync;
        DebugPopup.winLevelEvent -= OnLevelWinAsync;
        // RewardCollectingUI.collectCoinCompleteEvent -= OnCollectCoinCompleted;
    }

    private async void OnLevelWinAsync()
    {
        // GameController.Instance.admobAds.ShowBannerCollap();

        coinText.text = $"{0}";
        starText.text = $"{0}";
        goldPassKeyText.text = $"{0}";

        if (HardLevelManager.Ins.IsHardLevel())
        {
            normalCoinRewardText.text = $"{numCoinNormalHardLevel}";
            rewardedCoinRewardText.text = $"{numCoinRewardedAdHardLevel}";
        }
        else
        {
            normalCoinRewardText.text = $"{numCoinNormal}";
            rewardedCoinRewardText.text = $"{numCoinRewardedAd}";
        }

        continueButton.interactable = true;
        returnHomeButton.interactable = true;
        x5CoinButton.interactable = true;

        x5CoinButton.transform.localScale = Vector3.one;

        // coinContainer.gameObject.SetActive(_isCollectCoinRemoteConfig);
        x5CoinButton.gameObject.SetActive(_isCollectCoinRemoteConfig);

        if (!_isCollectCoinRemoteConfig)
        {
            continueButton.GetComponent<RectTransform>().localPosition += new Vector3(0, 80, 0);
        }
        else
        {
            // localizedMultiplyCoinText.transform.localScale = Vector3.zero;

            // _tweens.Add(Tween.Delay(0.2f).OnComplete(() =>
            // {
            //     localizedMultiplyCoinText.UpdateTranslationWithParameter(GameConstants.VALUE_PARAMETER, $"{_multiplierCoinWinLevel}");

            //     _tweens.Add(Tween.Scale(localizedMultiplyCoinText.transform, 1, duration: 0.3f));
            // }));
        }
        //

        // CheckEvent();

        Show();

        fetchLocalizedPriceIAPEvent?.Invoke();

        GamePersistentVariable.IAPLocation = "Win Popup";
        GamePersistentVariable.collectResourceMode = CollectResourceMode.UpdateText;

        SoundManager.Instance.PlaySoundWin();

        await Task.Delay(500);

        _currentCollectedCoin = 0;

        int coinQuantity = GamePersistentVariable.isHardMode ? 30 : 10;

        completeFx.Play();

        TrackLevelWin();

        currentLevel.Value++;
        currentLevel.Save();
        DataUtility.Save(GameConstants.CURRENT_LEVEL_LOOP, -1);

        GamePersistentVariable.currentLevel = currentLevel.Value;

        _isAlreadyWatchRewardedAd = false;

        DataUtility.Save("num_level_play_in_day", DataUtility.Load("num_level_play_in_day", 0) + 1);

        // GameLeaderboardManager.Instance.AddPlayerScoreToLeaderboard();
    }

    private void CollectCoin(int numCoin, Action onCompletedAction = null)
    {
        userResourcesObserver.UserResources.CoinQuantity += numCoin - _currentCollectedCoin;

        userResourcesObserver.Save();

        _currentCollectedCoin = numCoin;

        SaferioTracking.TrackResourceSource(
            currentLevel.Value, "Coin", numCoin - _currentCollectedCoin, (int)userResourcesObserver.UserResources.CoinQuantity, "win_level");

        return;

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

                    _currentCollectedCoin = numCoin;

                    SaferioTracking.TrackResourceSource(
                        currentLevel.Value, "Coin", numCoin - _currentCollectedCoin, (int)userResourcesObserver.UserResources.CoinQuantity, "win_level");
                }

                // _tweens.Add(Tween.Scale(continueButton.transform, 1, duration: timeCollectCoin, startDelay: timeDelayShowContinueButton)
                // .OnComplete(() =>
                // {
                //     continueButton.interactable = true;
                // })
                // );

                onCompletedAction?.Invoke();
            })
        );

        _tweens.Add(Tween.Custom(0, 3, duration: timeCollectCoin, onValueChange: newVal =>
        {
            starText.text = $"{(int)newVal}";
        }));
    }

    private void Continue()
    {
        x5CoinButton.interactable = false;
        continueButton.interactable = false;

        CollectCoin(HardLevelManager.Ins.IsHardLevel() ? numCoinNormalHardLevel : numCoinNormal);

        GamePersistentVariable.spawnFlyingCoinPosition = container.InverseTransformPoint(coinContinueButton.position);

        collectCoinEvent?.Invoke(_currentCollectedCoin);

        Tween.Delay(3f).OnComplete(() =>
        {
            NextLevel();
        });

        resetLevelEvent?.Invoke();
    }

    private void NextLevel()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (_isAlreadyWatchRewardedAd)
        {
            OnInterstitialAdCompleted();
        }
        else
        {
            continueButton.interactable = false;

            DoubleInterAdsConfig doubleInterAdsConfig = GamePersistentVariable.doubleInterAdsConfig;

            if (doubleInterAdsConfig.IsValidDoubleInter() && currentLevel.Value >= doubleInterAdsConfig.minLevel)
            {
                // GameController.Instance.admobAds.ShowInterstitial
                // (
                //     actionIniterClose: OnInterstitialAdCompleted
                // );
            }
            else
            {
                // GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: OnInterstitialAdCompleted);
            }
        }

        GamePersistentVariable.collectResourceMode = CollectResourceMode.PlayCollectAnimation;
    }

    private void OnInterstitialAdCompleted()
    {
        _tweens.Add(Tween.Delay(0.5f).OnComplete(() =>
        {
            HideImmediately();
        }));

        // bool isBackHome = HandleSuggestFeature();

        // if (!isBackHome)
        // {
        //     goLevelEvent?.Invoke(currentLevel.Value);
        // }

        goLevelEvent?.Invoke(currentLevel.Value);

        SaferioTracking.TrackInterstitialAdCompleted(currentLevel.Value, "next_level");

        GamePersistentVariable.isAutoTestingLevel = false;
    }

    #region X5 Coin
    private void X5Coin()
    {
        x5CoinButton.interactable = false;
        continueButton.interactable = false;

        // GameController.Instance.admobAds.ShowVideoReward(
        //    OnRewaredAdCompleted, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: ActionWatchVideo.MultiplyCoinWinLevel);

        resetLevelEvent?.Invoke();
    }

    private void OnRewaredAdCompleted()
    {
        x5CoinButton.interactable = false;
        continueButton.interactable = false;

        CollectCoin(HardLevelManager.Ins.IsHardLevel() ? numCoinRewardedAdHardLevel : numCoinRewardedAd);

        GamePersistentVariable.spawnFlyingCoinPosition = container.InverseTransformPoint(coinX2Button.position);

        collectCoinEvent?.Invoke(_currentCollectedCoin);

        SaferioTracking.TrackRewardedAdCompleted(currentLevel.Value, "multiply_coin_win_level");

        Tween.Delay(3f).OnComplete(() =>
        {
            NextLevel();
        });

        return;

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
        x5CoinButton.interactable = true;
        continueButton.interactable = true;

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
        SaferioTracking.TrackLevelWin(currentLevel.Value, levelBoosterObserver);

        TimeSpan playDuration = TimeSpan.FromSeconds(Time.time - GamePersistentVariable.startLevelTime);
        SaferioTracking.LogLevelComplete(currentLevel.Value);

        GamePersistentVariable.levelRetryTime = 0;

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
}
