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
    
    public static int activePopupCount = 0;
    public static bool IsAnyPopupOpen => activePopupCount > 0;
    #endregion

    #region EVENT
    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action popupShowEvent;
    public static event Action popupHideEvent;
    public static event Action<bool> enableSwipingScreenEvent;
    public static event Action showAdNotLoadedPopupEvent;
    public static event Action<string, string> showNotificationEvent;
    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;
    #endregion

    #region Life Cycle
    private void Awake()
    {
        switchRouteEvent += OnRouteSwitched;
        SwitchRouteButton.switchRouteEvent += OnRouteSwitched;
        TopBar.showPopupEvent += OnRouteSwitched;
        //MenuScreen.switchRouteEvent += OnRouteSwitched;
        GameplayScreen.switchRouteEvent += OnRouteSwitched;
        LevelLoader.switchRouteEvent += OnRouteSwitched;

        //UserLivesUI.switchRouteEvent += OnRouteSwitched;
        //LeaderboardScreen.switchRouteEvent += OnRouteSwitched;

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
            activePopupCount++;
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
        switchRouteEvent -= OnRouteSwitched;
        SwitchRouteButton.switchRouteEvent -= OnRouteSwitched;
        TopBar.showPopupEvent -= OnRouteSwitched;
        //MenuScreen.switchRouteEvent -= OnRouteSwitched;
        GameplayScreen.switchRouteEvent -= OnRouteSwitched;
        LevelLoader.switchRouteEvent -= OnRouteSwitched;

        //UserLivesUI.switchRouteEvent -= OnRouteSwitched;
        //LeaderboardScreen.switchRouteEvent -= OnRouteSwitched;

        UnregisterMoreEvent();

        CommonUtil.StopAllTweens(_tweens);

        if (_isShown)
        {
            activePopupCount--;
            if (activePopupCount < 0) activePopupCount = 0;
        }

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
    #endregion

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

    #region Show
    protected virtual void Show(Action onCompletedAction = null)
    {
        if (_isInTransition)
        {
            return;
        }

        ActualShowAfterAd(onCompletedAction);

        _isInTransition = true;
    }

    private void ActualShowAfterAd(Action onCompletedAction = null)
    {
        gameObject.SetActive(true);

        _transitionAnimation.Show(OnTransitionCompleted);

        popupShowEvent?.Invoke();

        _isShown = true;
        _isInTransition = true;
        activePopupCount++;

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
    #endregion

    #region Hide
    protected virtual void Hide(Action onCompletedAction = null)
    {
        if (_isInTransition)
        {
            return;
        }

        if (_isShown)
        {
            ActualHideAfterAd(onCompletedAction);

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
        activePopupCount--;
        if (activePopupCount < 0) activePopupCount = 0;

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
            activePopupCount--;
            if (activePopupCount < 0) activePopupCount = 0;

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
    #endregion

    #region Common
    protected void SwitchRoute(ScreenRoute route)
    {
        switchRouteEvent?.Invoke(route);
    }

    protected void ShowNotification(string title, string description)
    {
        showNotificationEvent?.Invoke(title, description);
    }

    protected void InvokeShowAdNotLoadedPopupEvent()
    {
        showAdNotLoadedPopupEvent?.Invoke();
    }

    public static void TriggerShowAdNotLoadedPopupEvent()
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
    #endregion
}
