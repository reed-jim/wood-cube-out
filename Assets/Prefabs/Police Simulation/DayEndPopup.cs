using System;
using System.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayEndPopup : BasePopup
{
    [SerializeField] private Image judgmentScore;
    [SerializeField] private Image rewardProgress;

    [SerializeField] private Button continueButton;
    [SerializeField] private Button x5CoinButton;
    [SerializeField] private TMP_Text coinText;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private float timeCollectCoin;

    private int _multiplierCoinWinLevel;
    private int _currentCollectedCoin;
    private bool _isAlreadyWatchRewardedAd;

    public static event Action<int> collectCoinEvent;

    protected override void RegisterMoreEvent()
    {
        continueButton.onClick.AddListener(Continue);
        x5CoinButton.onClick.AddListener(X5Coin);
    }

    protected override void Show(Action onCompletedAction = null)
    {
        coinText.text = $"{0}";
        continueButton.transform.localScale = Vector3.zero;

        continueButton.interactable = true;
        x5CoinButton.interactable = true;

        x5CoinButton.transform.localScale = Vector3.one;

        SoundManager.Instance.PlaySoundWin();

        base.Show(onCompletedAction: () =>
        {
            _currentCollectedCoin = 0;

            int coinQuantity = GamePersistentVariable.isHardMode ? 30 : 10;

            judgmentScore.fillAmount = GamePersistentVariable.judgmentScore;

            CollectCoin(coinQuantity);

            currentLevel.Value++;
            currentLevel.Save();
            DataUtility.Save(GameConstants.CURRENT_LEVEL_LOOP, -1);

            GamePersistentVariable.currentLevel = currentLevel.Value;

            _isAlreadyWatchRewardedAd = false;
        });
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
                userResourcesObserver.UserResources.CoinQuantity += numCoin - _currentCollectedCoin;

                userResourcesObserver.Save();

                _currentCollectedCoin += numCoin;

                SaferioTracking.TrackResourceSource(
                    currentLevel.Value, "Coin", numCoin - _currentCollectedCoin, (int)userResourcesObserver.UserResources.CoinQuantity, "win_level");

                collectCoinEvent?.Invoke(numCoin);



                _tweens.Add(Tween.Scale(continueButton.transform, 1, duration: timeCollectCoin));

                onCompletedAction?.Invoke();
            })
        );
    }

    private async void Continue()
    {
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
                AdmobAdsMax.Instance.ShowDoubleInterstitialAds
                (
                    OnInterstitialAdCompleted, isShowBreakAds: true
                );
            }
            else
            {
                AdmobAdsMax.Instance.ShowInterstitial(actionIniterClose: OnInterstitialAdCompleted);
            }
        }
    }

    private void OnInterstitialAdCompleted()
    {
        Hide(onCompletedAction: () =>
        {
            UnpauseDayTime();
        });
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
    #endregion
}
