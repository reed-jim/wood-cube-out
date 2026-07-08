using System;
using Lean.Localization;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class BuyBoosterPopup : BasePopup
{
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private Button buyWithDiamondButton;
    [SerializeField] private Image buyWithDiamondButtonForeground;
    [SerializeField] private Button getWithAdsButton;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI title;
    [SerializeField] private LeanLocalizedTextMeshProUGUI boosterDescription;

    [Header("COIN BALANCE")]
    [SerializeField] private RectTransform coinBalanceCointaner;
    [SerializeField] private TMP_Text coinBalanceText;

    #region REMOTE CONFIG REWARDED ADS
    [SerializeField] private RectTransform coinButtonRT;
    [SerializeField] private RectTransform adsButtonRT;
    #endregion

    [Header("BOOSTER SPRITE")]
    [SerializeField] private Sprite[] boosterSprites;
    [SerializeField] private string[] leanTranslationTitle;
    [SerializeField] private string[] leanTranslationNames;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private BoosterDataObserver boosterDataObserver;
    [SerializeField] private IntVariable currentLevel;

    private BoosterType _boosterType;
    private bool _isBoughtBreakObjectBoosterByCoin;
    private Vector3 _coinButtonPosition;
    private Vector3 _adsButtonPosition;

    public static event Action<BoosterType> useBoosterEvent;
    public static event Action unlockScrewBoxEvent;
    public static event Action<ScreenRoute> openIAPShopPopupEvent;
    public static event Action<int> updateBoosterQuantityEvent;
    public static event Action<int> updateCoinQuantityEvent;
    protected override void MoreActionInAwake()
    {
        _coinButtonPosition = coinButtonRT.localPosition;
        _adsButtonPosition = adsButtonRT.localPosition;
    }

    protected override void RegisterMoreEvent()
    {
        BoosterUI.showBuyBoosterPopupEvent += ShowBuyBoosterPopup;
        BoosterItemUI.showBuyBoosterPopupEvent += ShowBuyBoosterPopup;
        ScrewBoxUI.showBuyBoosterPopupEvent += ShowBuyBoosterPopup;
        TileSlotContainer.showBuyBoosterPopupEvent += ShowBuyBoosterPopup;
        ScrewSelectionInput.breakObjectEvent += OnConfirmedBreakObject;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;
        //GameplayScreen.coinTextChangedEvent += OnCoinTextChanged;

        buyWithDiamondButton.onClick.AddListener(GetWithCoin);
        getWithAdsButton.onClick.AddListener(GetWithAds);
        userResourcesObserver.updateCoinEvent += UpdateCoinBalanceText;
    }

    protected override void UnregisterMoreEvent()
    {
        BoosterUI.showBuyBoosterPopupEvent -= ShowBuyBoosterPopup;
        BoosterItemUI.showBuyBoosterPopupEvent -= ShowBuyBoosterPopup;
        ScrewBoxUI.showBuyBoosterPopupEvent -= ShowBuyBoosterPopup;
        TileSlotContainer.showBuyBoosterPopupEvent -= ShowBuyBoosterPopup;
        ScrewSelectionInput.breakObjectEvent -= OnConfirmedBreakObject;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
        userResourcesObserver.updateCoinEvent -= UpdateCoinBalanceText;
    }

    private void ShowBuyBoosterPopup(BoosterType boosterType)
    {
        Show();

        _boosterType = boosterType;

        icon.sprite = boosterSprites[(int)_boosterType];

        UIUtil.SetSizeKeepRatioY(icon, 0.25f * container.sizeDelta.x);

        costText.text = $"{boosterDataObserver.BoosterCosts[(int)_boosterType]}";
        title.TranslationName = leanTranslationTitle[(int)_boosterType];
        boosterDescription.TranslationName = leanTranslationNames[(int)_boosterType];

        getWithAdsButton.onClick.RemoveAllListeners();

        if (boosterType == BoosterType.UnlockScrewBox)
        {
            getWithAdsButton.onClick.AddListener(UnlockScrewBoxWithAds);
        }
        else
        {
            getWithAdsButton.onClick.AddListener(GetWithAds);
        }

        SetupRewardedAdsButton();
    }

    private void GetWithCoin()
    {
        userResourcesObserver.Load();

        int boosterIndex = (int)_boosterType;

        if (userResourcesObserver.UserResources.CoinQuantity >= boosterDataObserver.BoosterCosts[boosterIndex])
        {
            if (_boosterType == BoosterType.UnlockScrewBox)
            {
                userResourcesObserver.ChangeCoin(-boosterDataObserver.BoosterCosts[boosterIndex]);

                if (UserResourcesManager.Instance != null)
                {
                    UserResourcesManager.Instance.RefreshCoinText();
                }

                SaferioTracking.TrackResourceSink(
                    currentLevel.Value, "Coin",
                    boosterDataObserver.BoosterCosts[boosterIndex], (int)userResourcesObserver.UserResources.CoinQuantity, $"Buy_Booster_{_boosterType}");

                Hide(onCompletedAction: () =>
                {
                    unlockScrewBoxEvent?.Invoke();
                });

                return;
            }
            else
            {
                userResourcesObserver.ChangeCoin(-boosterDataObserver.BoosterCosts[boosterIndex]);
                userResourcesObserver.ChangeBoosterQuantity(boosterIndex, 1);

                if (UserResourcesManager.Instance != null)
                {
                    UserResourcesManager.Instance.RefreshCoinText();
                }

                SaferioTracking.TrackResourceSink(
                    currentLevel.Value, "Coin",
                    boosterDataObserver.BoosterCosts[boosterIndex], (int)userResourcesObserver.UserResources.CoinQuantity, $"Buy_Booster_{_boosterType}");

                updateBoosterQuantityEvent?.Invoke(boosterIndex);

                SaferioTracking.TrackResourceSource(
                    currentLevel.Value, _boosterType.ToString(), 1, userResourcesObserver.UserResources.BoosterQuantities[boosterIndex], "buy_with_coin");
            }

            userResourcesObserver.Save();

            Hide(onCompletedAction: () =>
            {
                // useBoosterEvent?.Invoke(_boosterType); // Remove auto-use
            });
        }
        else
        {
            Hide(onCompletedAction: () =>
            {
                openIAPShopPopupEvent?.Invoke(ScreenRoute.Booster);
            });
        }
    }

    private void GetWithAds()
    {
        ActionWatchVideo actionWatchVideo;

        switch (_boosterType)
        {
            case BoosterType.AddScrewPort: actionWatchVideo = ActionWatchVideo.AddHole; break;
            case BoosterType.Shuffle: actionWatchVideo = ActionWatchVideo.BreakObject; break;
            case BoosterType.ClearScrewPorts: actionWatchVideo = ActionWatchVideo.ClearHoles; break;
            default: actionWatchVideo = ActionWatchVideo.AddHole; break;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            NoAdsLoadedPopup.OnOkClickedNoInternet = OnRewaredAdCompleted;
            SwitchRoute(ScreenRoute.NoInternet);
        }
        else
        {
            AdmobAdsMax.Instance.ShowVideoReward(OnRewaredAdCompleted, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: actionWatchVideo);
        }
    }

    private void OnRewaredAdCompleted()
    {
        userResourcesObserver.ChangeBoosterQuantity((int)_boosterType, 1);

        updateBoosterQuantityEvent?.Invoke((int)_boosterType);

        Action completeAction = () =>
        {
            SaferioTracking.TrackRewardedAdCompleted(currentLevel.Value, $"Booster_{_boosterType}");
        };

        if (_isShown)
        {
            Hide(onCompletedAction: completeAction);
        }
        else
        {
            completeAction();
        }
    }

    private void UnlockScrewBoxWithAds()
    {
        isAbleToHideOnClickOutside = false;

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            NoAdsLoadedPopup.OnOkClickedNoInternet = OnRewaredAdCompletedToUnlockBox;
            SwitchRoute(ScreenRoute.NoInternet);
        }
        else
        {
            AdmobAdsMax.Instance.ShowVideoReward(
                OnRewaredAdCompletedToUnlockBox, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: ActionWatchVideo.UnlockScrewBox);
        }
    }

    private void OnRewaredAdCompletedToUnlockBox()
    {
        Action completeAction = () =>
        {
            unlockScrewBoxEvent?.Invoke();

            SaferioTracking.TrackRewardedAdCompleted(currentLevel.Value, "UnlockScrewBox");

            isAbleToHideOnClickOutside = true;
        };

        if (_isShown)
        {
            Hide(onCompletedAction: completeAction);
        }
        else
        {
            completeAction();
        }
    }

    private void OnConfirmedBreakObject()
    {
        if (_isBoughtBreakObjectBoosterByCoin)
        {
            userResourcesObserver.ChangeCoin(-boosterDataObserver.BoosterCosts[(int)_boosterType]);

            if (UserResourcesManager.Instance != null)
            {
                UserResourcesManager.Instance.RefreshCoinText();
            }

            SaferioTracking.TrackResourceSink(
                    currentLevel.Value, "Coin",
                    boosterDataObserver.BoosterCosts[1], (int)userResourcesObserver.UserResources.CoinQuantity, $"Buy_Booster_{_boosterType}");

            _isBoughtBreakObjectBoosterByCoin = false;
        }
    }

    private void ShowAdsNotLoadedPopup()
    {
        SwitchRoute(ScreenRoute.NoInternet);

        isAbleToHideOnClickOutside = true;
    }

    private void OnResourcesEarnPopupShown(bool isShow)
    {
        float start = 0;
        float end = 1;

        if (isShow)
        {
            start = 1;
            end = 0;
        }

        if (!isShow)
        {
            SetupRewardedAdsButton();
        }

        _tweens.Add(Tween.Custom(start, end, duration: 0.3f, onValueChange: newVal =>
        {
            popupCanvasGroup.alpha = newVal;
        }));
    }

    #region REMOTE CONFIG REWARDED AD
    private void SetupRewardedAdsButton(bool forceEnable = false)
    {
        // bool isEnableRewardedAds = GamePersistentVariable.isEnableRewardedAds || currentLevel.Value < GamePersistentVariable.minLevelDisableRewardedAds;

        // if (isEnableRewardedAds || forceEnable)
        // {
        //     adsButtonRT.gameObject.SetActive(true);
        //     coinBalanceCointaner.gameObject.SetActive(false);

        //     coinButtonRT.localPosition = _coinButtonPosition;
        //     adsButtonRT.localPosition = _adsButtonPosition;
        // }
        // else
        // {
        //     adsButtonRT.gameObject.SetActive(false);
        //     coinBalanceCointaner.gameObject.SetActive(true);

        //     // coinButtonRT.localPosition = _coinButtonPosition - (_coinButtonPosition - _adsButtonPosition) / 2;

        userResourcesObserver.Load();

        coinBalanceText.text = $"{userResourcesObserver.UserResources.CoinQuantity}";
    }
    #endregion

    private void UpdateCoinBalanceText()
    {
        if (coinBalanceText != null)
        {
            coinBalanceText.text = $"{userResourcesObserver.UserResources.CoinQuantity}";
        }
    }
}




