using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static GameEnum;

public class LivesShopPopup : BasePopup
{
    [SerializeField] private Button refillByAdsButton;
    [SerializeField] private Button refillByCoinButton;
    [SerializeField] private Button refuseButton;
    [SerializeField] private TMP_Text currentLivesText;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    public static event Action<int> changeLivesNumberEvent;
    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action runPendingReplayCommandEvent;
    public static event Action runPendingReturnHomeCommandEvent;
    public static event Action<ScreenRoute> openIAPShopPopupEvent;
    public static event Action<float> updateCoinTextEvent;
    public static event Action lackCoinReplenishLiveEvent;
    public static event Action startGameEvent;

    protected override void MoreActionInAwake()
    {
        refillByCoinButton.onClick.AddListener(RefillByCoin);
        refillByAdsButton.onClick.AddListener(RefillByAds);
        refuseButton.onClick.AddListener(Refuse);
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        LivesData livesData = DataUtility.Load(GameConstants.USER_LIVES_DATA, new LivesData());

        currentLivesText.text = $"{livesData.CurrentLives}";
    }

    private void RefillByAds()
    {
         GameController.Instance.admobAds.ShowVideoReward(
            OnRewaredAdCompletedToRefillLives, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: ActionWatchVideo.UnlockScrewBox);
    }

    private void OnRewaredAdCompletedToRefillLives()
    {
        ActualRefill();

        SaferioTracking.TrackRewardedAdCompleted(GamePersistentVariable.currentLevel, "Refill Live");
    }

    private void ShowAdsNotLoadedPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.NoInternet);
    }

    private void RefillByCoin()
    {
        userResourcesObserver.Load();

        if (userResourcesObserver.UserResources.CoinQuantity >= 100)
        {
            userResourcesObserver.ChangeCoin(-100);

            updateCoinTextEvent?.Invoke(userResourcesObserver.UserResources.CoinQuantity);

            ActualRefill();
        }
        else
        {
            Hide(onCompletedAction: () =>
            {
                // In Gameplay
                openIAPShopPopupEvent?.Invoke(ScreenRoute.LivesShop);

                // In Menu
                lackCoinReplenishLiveEvent?.Invoke();
            });
        }
    }

    private void ActualRefill()
    {
        changeLivesNumberEvent?.Invoke(1);

        Hide(
            onCompletedAction: () =>
            {
                if (GamePersistentVariable.isPendingStartLevel)
                {
                    startGameEvent?.Invoke();

                    GamePersistentVariable.isPendingStartLevel = false;
                }
            }
        );

        if (GamePersistentVariable.isPendingReplay)
        {
            runPendingReplayCommandEvent?.Invoke();

            GamePersistentVariable.isPendingReplay = false;
        }
    }

    private void Refuse()
    {
        base.Hide(onCompletedAction: () =>
        {
            if (GamePersistentVariable.isLevelLost)
            {
                ReturnHomeOnRejectedToRefillLive();
            }
        });

        GamePersistentVariable.isPendingStartLevel = false;
        GamePersistentVariable.isPendingReplay = false;
    }

    private void ReturnHomeOnRejectedToRefillLive()
    {
        GamePersistentVariable.isLevelLost = false;

        Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
    }
}
