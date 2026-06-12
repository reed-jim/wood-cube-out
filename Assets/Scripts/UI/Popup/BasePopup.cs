using System;
using System.Collections.Generic;
using NamPhuThuy.Events;
using NamPhuThuy.GamePlay;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GameEnum;

public class BasePopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] protected RectTransform container;
    [SerializeField] protected Button closeButton;
    [SerializeField] protected Button detectClickOutsideButton;

    [Header("CUSTOMIZE")]
    [SerializeField] private ScreenRoute route;
    [SerializeField] private bool isRouteActiveFromStart;
    [SerializeField] protected bool isAbleToHideOnClickOutside;
    [SerializeField] protected bool isShowingInterAdOnInteract;

    #region PRIVATE FIELD
    private ISaferioUIAnimation _transitionAnimation;
    protected bool _isShown;
    protected bool _isInTransition;
    protected List<Tween> _tweens;
    #endregion

    #region EVENT
    public static event Action popupShowEvent;
    public static event Action popupHideEvent;
    public static event Action<bool> enableSwipingScreenEvent;
    public static event Action showAdNotLoadedPopupEvent;
    public static event Action<ScreenRoute> switchPopupRouteEvent;
    public static event Action<string, string> showNotificationEvent;
    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;
    
    /*[Header("Event Channels")]
    [SerializeField] protected SwitchRouteChannelSO switchRouteChannel;
    protected SwitchRouteChannelSO SwitchRouteChannel
    {
        get
        {
            if (switchRouteChannel == null)
            {
                switchRouteChannel = Resources.Load<SwitchRouteChannelSO>(nameof(SwitchRouteChannelSO));
            }
            return switchRouteChannel;
        }
    }*/
   
    #endregion

    private void Awake()
    {
        switchPopupRouteEvent += OnRouteSwitched;
        SwitchRouteButton.switchRouteEvent += OnRouteSwitched;
        TopBar.showPopupEvent += OnRouteSwitched;
        MenuScreen.switchRouteEvent += OnRouteSwitched;
        GameplayScreen.switchRouteEvent += OnRouteSwitched;
        LevelLoader.switchRouteEvent += OnRouteSwitched;
        
        SettingPopup.switchRouteEvent += OnRouteSwitched;
        IAPShopPopup.switchRouteEvent += OnRouteSwitched;
        BuyBoosterPopup.switchRouteEvent += OnRouteSwitched;
        PausePopup.switchRouteEvent += OnRouteSwitched;
        
        // SwitchRouteChannel.Event += OnRouteSwitched;
        
        UserLivesUI.switchRouteEvent += OnRouteSwitched;
        LeaderboardScreen.switchRouteEvent += OnRouteSwitched;
        ContractCalculator.switchRouteEvent += OnRouteSwitched;

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }

        if (detectClickOutsideButton != null)
        {
            detectClickOutsideButton.onClick.AddListener(Hide);
        }

        RegisterMoreEvent();

        _tweens = new List<Tween>();
        _transitionAnimation = GetComponent<ISaferioUIAnimation>();

        if (isRouteActiveFromStart)
        {
            gameObject.SetActive(true);

            _isShown = true;
        }
        else
        {
            gameObject.SetActive(false);
        }

        MoreActionInAwake();
    }

    void Update()
    {
        HideOnClickOutside();
    }

    private void OnDestroy()
    {
        switchPopupRouteEvent -= OnRouteSwitched;
        SwitchRouteButton.switchRouteEvent -= OnRouteSwitched;
        TopBar.showPopupEvent -= OnRouteSwitched;
        MenuScreen.switchRouteEvent -= OnRouteSwitched;
        GameplayScreen.switchRouteEvent -= OnRouteSwitched;
       
        
        LevelLoader.switchRouteEvent -= OnRouteSwitched;
        
        UserLivesUI.switchRouteEvent -= OnRouteSwitched;
        LeaderboardScreen.switchRouteEvent -= OnRouteSwitched;
        ContractCalculator.switchRouteEvent -= OnRouteSwitched;

        PausePopup.switchRouteEvent -= OnRouteSwitched;
        SettingPopup.switchRouteEvent -= OnRouteSwitched;
        IAPShopPopup.switchRouteEvent -= OnRouteSwitched;
        BuyBoosterPopup.switchRouteEvent -= OnRouteSwitched;
        
        // SwitchRouteChannel.Event -= OnRouteSwitched;
        
        UnregisterMoreEvent();

        CommonUtil.StopAllTweens(_tweens);

        MoreActionOnDestroy();
    }

    protected virtual void RegisterMoreEvent()
    {

    }

    protected virtual void UnregisterMoreEvent()
    {

    }

    protected virtual void MoreActionInAwake()
    {

    }

    protected virtual void MoreActionOnDestroy()
    {

    }

    private void OnRouteSwitched(ScreenRoute route)
    {
        if (route == this.route)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    // private void OnRouteSwitchedWithoutAd(ScreenRoute route)
    // {
    //     if (route == this.route)
    //     {
    //         ShowWithoutAd();
    //     }
    //     else
    //     {

    //     }
    // }

    protected virtual void Show(Action onCompletedAction = null)
    {
        if (_isInTransition)
        {
            return;
        }

        ActualShowAfterAd(onCompletedAction);

        // if (isShowingInterAdOnInteract && GamePersistentVariable.doubleInterAdsConfig.IsValidShowInterInteractPopup())
        // {
        //      GameController.Instance.admobAds.ShowInterstitialAdmob(onAdClosed: () => ActualShowAfterAd(onCompletedAction), isShowAdBreak: true);
        // }
        // else
        // {
        //     ActualShowAfterAd(onCompletedAction);
        // }

        _isInTransition = true;
    }

    // protected void ShowWithoutAd(Action onCompletedAction = null)
    // {
    //     if (_isInTransition)
    //     {
    //         return;
    //     }

    //     ActualShowAfterAd(onCompletedAction);
    // }

    private void ActualShowAfterAd(Action onCompletedAction = null)
    {
        gameObject.SetActive(true);

        _transitionAnimation.Show(OnTransitionCompleted);

        popupShowEvent?.Invoke();

        _isShown = true;
        _isInTransition = true;

        enableSwipingScreenEvent?.Invoke(false);

        void OnTransitionCompleted()
        {
            _isInTransition = false;

            onCompletedAction?.Invoke();
        }
    }

    protected void Show()
    {
        Show(onCompletedAction: null);
    }

    protected virtual void Hide(Action onCompletedAction = null)
    {
        if (_isInTransition)
        {
            return;
        }

        if (_isShown)
        {
            ActualHideAfterAd(onCompletedAction);

            // if (isShowingInterAdOnInteract && GamePersistentVariable.doubleInterAdsConfig.IsValidShowInterInteractPopup())
            // {
            //      GameController.Instance.admobAds.ShowInterstitialAdmob(onAdClosed: () => ActualHideAfterAd(onCompletedAction), isShowAdBreak: true);
            // }
            // else
            // {
            //     ActualHideAfterAd(onCompletedAction);
            // }

            _isInTransition = true;
        }
    }

    protected virtual void HideWithoutAd(Action onCompletedAction = null)
    {
        if (_isInTransition)
        {
            return;
        }

        if (_isShown)
        {
            ActualHideAfterAd(onCompletedAction);
        }
    }

    private void ActualHideAfterAd(Action onCompletedAction = null)
    {
        _transitionAnimation.Hide(OnTransitionCompleted);

        popupHideEvent?.Invoke();

        _isShown = false;
        _isInTransition = true;

        AfterHide();

        SoundManager.Instance.PlaySoundClose();

        enableSwipingScreenEvent?.Invoke(true);

        void OnTransitionCompleted()
        {
            _isInTransition = false;

            onCompletedAction?.Invoke();

            gameObject.SetActive(false);
        }
    }

    protected virtual void Hide()
    {
        Hide(onCompletedAction: null);
    }

    protected virtual void HideImmediately()
    {
        if (_isInTransition)
        {
            return;
        }

        if (_isShown)
        {
            gameObject.SetActive(false);

            popupHideEvent?.Invoke();

            _isShown = false;

            AfterHide();

            enableSwipingScreenEvent?.Invoke(true);
        }
    }

    protected virtual void AfterHide()
    {

    }

    private void HideOnClickOutside()
    {
        if (!isAbleToHideOnClickOutside)
        {
            return;
        }

        if (_isInTransition)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var raycastResults = new System.Collections.Generic.List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            bool clickedOutside = true;

            RectTransform[] children = TransformUtil.GetComponentsFromAllChildren<RectTransform>(transform).ToArray();

            bool isFound = false;

            foreach (var result in raycastResults)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (result.gameObject == children[i].gameObject)
                    {
                        clickedOutside = false;

                        break;
                    }
                }

                if (isFound)
                {
                    break;
                }
            }

            if (clickedOutside)
            {
                Hide();
            }
        }
    }

    protected void SwitchRoute(ScreenRoute route)
    {
        switchPopupRouteEvent?.Invoke(route);
    }

    protected void ShowNotification(string title, string description)
    {
        showNotificationEvent?.Invoke(title, description);
    }

    protected void InvokeShowAdNotLoadedPopupEvent()
    {
        showAdNotLoadedPopupEvent?.Invoke();
    }

    protected void InvokeShowResourcesEarnPopupPopupEvent(int coinQuantity, int[] boosterQuantites, bool isRemoveAds)
    {
        showResourcesEarnPopupEvent?.Invoke(coinQuantity, boosterQuantites, isRemoveAds);
    }

    protected void AddTween(Tween tween)
    {
        _tweens.Add(tween);
    }
}
