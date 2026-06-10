using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class RevivePopup : BasePopup
{
    [SerializeField] private Button reviveByDiamondButton;
    [SerializeField] private Image reviveByDiamondButtonForeground;
    [SerializeField] private Image boosterImage;
    [SerializeField] private Button reviveByAdsButton;
    [SerializeField] private TMP_Text costText;

    [Header("COIN BALANCE")]
    [SerializeField] private RectTransform coinBalanceCointaner;
    [SerializeField] private TMP_Text coinBalanceText;

    [Header("SPRITE")]
    [SerializeField] private Sprite unlockBoxSprite;
    [SerializeField] private Sprite clearHolesSprite;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private BoosterDataObserver boosterDataObserver;
    [SerializeField] private LevelObserver levelObserver;

    #region PRIVATE FIELD
    private BoosterType _currentBoosterOnRevive;
    private RectTransform _reviveByDiamondButtonRT;
    private RectTransform _reviveByAdsButtonRT;
    private Vector3 _reviveCoinButtonPosition;
    private Vector3 _reviveAdsButtonPosition;
    private int _popupShowTime;
    #endregion

    public static event Action<BoosterType> reviveEvent;
    public static event Action reviveRefusedEvent;
    public static event Action<ScreenRoute> openIAPShopPopupEvent;

    protected override void MoreActionInAwake()
    {
        costText.text = $"{boosterDataObserver.BoosterCosts.Last()}";

        _reviveByDiamondButtonRT = reviveByDiamondButton.GetComponent<RectTransform>();
        _reviveByAdsButtonRT = reviveByAdsButton.GetComponent<RectTransform>();

        _reviveCoinButtonPosition = _reviveByDiamondButtonRT.localPosition;
        _reviveAdsButtonPosition = _reviveByAdsButtonRT.localPosition;
    }

    protected override void RegisterMoreEvent()
    {
        GameStateLose.showRevivePopupEvent += ShowRevivePopup;

        reviveByDiamondButton.onClick.AddListener(ReviveByDiamond);
        reviveByAdsButton.onClick.AddListener(ReviveByAds);
    }

    protected override void MoreActionOnDestroy()
    {
        GameStateLose.showRevivePopupEvent -= ShowRevivePopup;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        SetupRewardedAdsButton();

        // _popupShowTime++;

        // if (_popupShowTime == 2)
        // {
        //     SetupRewardedAdsButton(forceEnable: true);

        //     _popupShowTime = 0;
        // }
    }

    protected override void Hide()
    {
        base.Hide(onCompletedAction: () =>
        {
            reviveRefusedEvent?.Invoke();
        });
    }

    private void ShowRevivePopup(BoosterType boosterType)
    {
        if (GamePersistentVariable.isAutoTestingLevel)
        {
            Show(onCompletedAction: () => ReviveByDiamond());
        }
        else
        {
            Show();
        }

        SoundManager.Instance.PlaySoundLose();

        // if (boosterType == BoosterType.UnlockScrewBox)
        // {
        //     boosterImage.sprite = unlockBoxSprite;
        // }
        // else if (boosterType == BoosterType.ClearScrewPorts)
        // {
        //     boosterImage.sprite = clearHolesSprite;
        // }

        UIUtil.SetSizeKeepRatioY(boosterImage, 0.2f * container.sizeDelta.x);

        reviveByDiamondButton.interactable = true;

        _currentBoosterOnRevive = boosterType;

        // SetupRewardedAdsButton();
    }

    private void ReviveByDiamond()
    {
        // if (_isInTransition)
        // {
        //     return;
        // }

        userResourcesObserver.Load();

        reviveByDiamondButton.interactable = false;

        if (userResourcesObserver.UserResources.CoinQuantity >= boosterDataObserver.BoosterCosts.Last())
        {
            base.Hide(onCompletedAction: () =>
            {
                reviveEvent?.Invoke(_currentBoosterOnRevive);

                SaferioTracking.TrackReviveEvent(currentLevel.Value, "coin", _currentBoosterOnRevive, levelObserver.Progress);

                reviveByDiamondButton.interactable = true;
            });

            userResourcesObserver.ChangeCoin(-boosterDataObserver.BoosterCosts.Last());

            SaferioTracking.TrackResourceSink(
                    currentLevel.Value, "Coin",
                    200, (int)userResourcesObserver.UserResources.CoinQuantity, $"Revive");

            // userResourcesObserver.UserResources.CoinQuantity -= boosterDataObserver.BoosterCosts.Last();

            if (_currentBoosterOnRevive == BoosterType.ClearScrewPorts)
            {
                userResourcesObserver.ChangeBoosterQuantity(2, 1);
            }

            // userResourcesObserver.Save();
        }
        else
        {
            Hide(onCompletedAction: () =>
            {
                openIAPShopPopupEvent?.Invoke(ScreenRoute.Revive);

                reviveByDiamondButton.interactable = true;
            });
        }
    }

    private void ReviveByAds()
    {
        if (_isInTransition)
        {
            return;
        }

        OnRewaredAdCompleted();

        return;

        ActionWatchVideo actionWatchVideo;

        switch (_currentBoosterOnRevive)
        {
            case BoosterType.AddScrewPort: actionWatchVideo = ActionWatchVideo.AddHole; break;
            case BoosterType.BreakObject: actionWatchVideo = ActionWatchVideo.BreakObject; break;
            case BoosterType.ClearScrewPorts: actionWatchVideo = ActionWatchVideo.ClearHoles; break;
            case BoosterType.UnlockScrewBox: actionWatchVideo = ActionWatchVideo.UnlockScrewBox; break;
            default: actionWatchVideo = ActionWatchVideo.UnlockScrewBox; break;
        }

        AdmobAdsMax.Instance.ShowVideoReward(OnRewaredAdCompleted, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: actionWatchVideo);
    }

    private void OnRewaredAdCompleted()
    {
        base.Hide(onCompletedAction: () =>
        {
            reviveEvent?.Invoke(_currentBoosterOnRevive);

            SaferioTracking.TrackReviveEvent(currentLevel.Value, "rewarded_ads", _currentBoosterOnRevive, levelObserver.Progress);
            SaferioTracking.TrackRewardedAdCompleted(currentLevel.Value, $"Revive_{_currentBoosterOnRevive}");
        });
    }

    private void ShowAdsNotLoadedPopup()
    {
        InvokeShowAdNotLoadedPopupEvent();

        // switchRouteEvent?.Invoke(ScreenRoute.NoInternet);
    }

    #region REMOTE CONFIG REWARDED AD
    private void SetupRewardedAdsButton(bool forceEnable = false)
    {
        // bool isEnableRewardedAds = GamePersistentVariable.isEnableRewardedAds || currentLevel.Value < GamePersistentVariable.minLevelDisableRewardedAds;

        // if (isEnableRewardedAds || forceEnable)
        // {
        //     reviveByAdsButton.gameObject.SetActive(true);
        //     coinBalanceCointaner.gameObject.SetActive(false);

        //     _reviveByDiamondButtonRT.localPosition = _reviveCoinButtonPosition;
        //     _reviveByAdsButtonRT.localPosition = _reviveAdsButtonPosition;
        // }
        // else
        // {
        //     reviveByAdsButton.gameObject.SetActive(false);
        //     coinBalanceCointaner.gameObject.SetActive(true);

        //     // _reviveByDiamondButtonRT.localPosition = _reviveCoinButtonPosition - (_reviveCoinButtonPosition - _reviveAdsButtonPosition) / 2;

        //     userResourcesObserver.Load();

        //     coinBalanceText.text = $"{userResourcesObserver.UserResources.CoinQuantity}";
        // }
    }
    #endregion
}
