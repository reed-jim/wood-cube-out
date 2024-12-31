using System;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class BasePopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button closeButton;

    [Header("CUSTOMIZE")]
    [SerializeField] private ScreenRoute route;
    [SerializeField] private bool isRouteActiveFromStart;

    private ISaferioUIAnimation _transitionAnimation;
    private bool _isShown;

    public static event Action popupShowEvent;
    public static event Action popupHideEvent;

    private void Awake()
    {
        SwitchRouteButton.switchRouteEvent += OnRouteSwitched;
        TopBar.showPopupEvent += OnRouteSwitched;
        MenuScreen.switchRouteEvent += OnRouteSwitched;
        GameplayScreen.switchRouteEvent += OnRouteSwitched;

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }

        RegisterMoreEvent();

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

    private void OnDestroy()
    {
        SwitchRouteButton.switchRouteEvent -= OnRouteSwitched;
        TopBar.showPopupEvent -= OnRouteSwitched;
        MenuScreen.switchRouteEvent -= OnRouteSwitched;
        GameplayScreen.switchRouteEvent -= OnRouteSwitched;

        UnregisterMoreEvent();

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

    protected void Show()
    {
        gameObject.SetActive(true);

        _transitionAnimation.Show();

        popupShowEvent?.Invoke();

        _isShown = true;
    }

    protected void Hide()
    {
        if (_isShown)
        {
            _transitionAnimation.Hide();

            popupHideEvent?.Invoke();

            _isShown = false;

            AfterHide();

            PlayClickSound();
        }
    }

    protected virtual void AfterHide()
    {

    }

    private void PlayClickSound()
    {
        AudioSource clickSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.CLICK_SOUND);

        clickSound.Play();
    }
}
