using System;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class RevivePopup : BasePopup
{
    [SerializeField] private Button reviveByDiamondButton;
    [SerializeField] private Image boosterImage;
    [SerializeField] private Button reviveByAdsButton;
    [SerializeField] private TextMeshProUGUI costText;

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
    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action<float> updateCoinTextEvent;


    #region MonoBehaviour Callbacks
    
    protected override void MoreActionInAwake()
    {
        costText.text = $"{boosterDataObserver.BoosterCosts.Last()}";

        _reviveByDiamondButtonRT = reviveByDiamondButton.GetComponent<RectTransform>();
        _reviveByAdsButtonRT = reviveByAdsButton.GetComponent<RectTransform>();

        _reviveCoinButtonPosition = _reviveByDiamondButtonRT.localPosition;
        _reviveAdsButtonPosition = _reviveByAdsButtonRT.localPosition;
        
        
        var eventTrigger = peekButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
    
        var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { _isButtonPressed = true; });
        eventTrigger.triggers.Add(pointerDown);
    
        var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { OnButtonRelease(); });
        eventTrigger.triggers.Add(pointerUp);
    }
    
    protected override void RegisterMoreEvent()
    {
        GameStateLose.showRevivePopupEvent += ShowRevivePopup;

        reviveByDiamondButton.onClick.AddListener(ReviveByCoin);
        reviveByAdsButton.onClick.AddListener(ReviveByAds);
    }

    protected override void MoreActionOnDestroy()
    {
        GameStateLose.showRevivePopupEvent -= ShowRevivePopup;
    }

    void Update()
    {
        PeekPicture();
    }

    #endregion




    #region Override Methods

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);
        GamePersistentVariable.isReviveState = true;

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

    #endregion

    private void ShowRevivePopup(BoosterType boosterType)
    {
        if (GamePersistentVariable.isAutoTestingLevel)
        {
            Show(onCompletedAction: () => ReviveByCoin());
        }
        else
        {
            Show();
        }

        SoundManager.Instance.PlaySoundLose();

        if (boosterType == BoosterType.UnlockSpoolBox)
        {
            boosterImage.sprite = unlockBoxSprite;
        }
        else if (boosterType == BoosterType.ClearAlternativeSpools)
        {
            boosterImage.sprite = clearHolesSprite;
        }

        UIUtil.SetSizeKeepRatioY(boosterImage, 0.2f * container.sizeDelta.x);

        reviveByDiamondButton.interactable = true;

        _currentBoosterOnRevive = boosterType;

        // SetupRewardedAdsButton();
    }

    private void ReviveByCoin()
    {
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
            updateCoinTextEvent?.Invoke(userResourcesObserver.UserResources.CoinQuantity);

            SaferioTracking.TrackResourceSink(
                    currentLevel.Value, "Coin",
                    200, (int)userResourcesObserver.UserResources.CoinQuantity, $"Revive");

            // userResourcesObserver.UserResources.CoinQuantity -= boosterDataObserver.BoosterCosts.Last();

            if (_currentBoosterOnRevive == BoosterType.ClearAlternativeSpools)
            {
                userResourcesObserver.ChangeBoosterQuantity(2, 1);
            }

            // userResourcesObserver.Save();
        }
        else
        {
            Hide(onCompletedAction: () =>
            {
                SwitchRoute(GameEnum.ScreenRoute.IAPShopPopup, false);

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

        ActionWatchVideo actionWatchVideo;

        switch (_currentBoosterOnRevive)
        {
            case BoosterType.AddSpoolSlot: actionWatchVideo = ActionWatchVideo.AddHole; break;
            case BoosterType.MagicPick: actionWatchVideo = ActionWatchVideo.BreakObject; break;
            case BoosterType.ClearAlternativeSpools: actionWatchVideo = ActionWatchVideo.ClearHoles; break;
            case BoosterType.UnlockSpoolBox: actionWatchVideo = ActionWatchVideo.UnlockScrewBox; break;
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

    #region Private Methods
    
    [Header("Peek Level")]
    [SerializeField] private Button peekButton;
    private float _holdingDuration;
    private Tween _peekingPictureTween;
    private bool _isPeekingPicture;
    private bool _isButtonPressed;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float holdThreshold = 0.6f;

    public static event Action OnPeekPicture;
    public static event Action OnStopPeekPicture;

    private void PeekPicture()
    {
        if (_isButtonPressed)
        {
            _holdingDuration += Time.deltaTime;

            if (_holdingDuration > holdThreshold && !_isPeekingPicture)
            {
                _peekingPictureTween?.Kill();
                _peekingPictureTween = canvasGroup.DOFade(0, 0.3f);
                _isPeekingPicture = true;
                OnPeekPicture?.Invoke();
            }
        }
    }
    
    private void OnButtonRelease()
    {
        _isButtonPressed = false;
    
        if (_isPeekingPicture)
        {
            _peekingPictureTween?.Kill();
            _peekingPictureTween = canvasGroup.DOFade(1, 0.3f);
            _isPeekingPicture = false;
            OnStopPeekPicture?.Invoke();
        }
    
        _holdingDuration = 0;
    }

    #endregion

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
