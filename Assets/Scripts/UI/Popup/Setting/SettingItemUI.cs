using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class SettingItemUI : MonoBehaviour
{
    [SerializeField] private Button changeStateButton;
    [SerializeField] private Image activeState;
    [SerializeField] private Image inactiveState;
    [SerializeField] private Image crossIcon;

    private bool _isActive;

    public bool IsActive
    {
        get => _isActive;
        set => _isActive = value;
    }

    private Action<bool> _onStateChangeStarted;

    void Awake()
    {
        changeStateButton.onClick.AddListener(ChangeState);
    }

    public void SetState(bool isActive)
    {
        activeState.gameObject.SetActive(isActive);
        inactiveState.gameObject.SetActive(!isActive);
        crossIcon.gameObject.SetActive(!isActive);

        _isActive = isActive;
    }

    public void ChangeState()
    {
        _isActive = !_isActive;

        if (_isActive)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }

        _onStateChangeStarted?.Invoke(_isActive);
    }

    public void RegisterChangeStateEvent(Action<bool> onStateChangeStarted)
    {
        _onStateChangeStarted = onStateChangeStarted;
    }

    public void Activate()
    {
        activeState.gameObject.SetActive(true);

        activeState.color = ColorUtil.WithAlpha(Color.white, 0);

        Tween.Alpha(activeState, 1, duration: 0.3f);
        Tween.Alpha(inactiveState, 0, duration: 0.3f)
        .OnComplete(() =>
            {
                inactiveState.gameObject.SetActive(true);
                crossIcon.gameObject.SetActive(true);
            }
        );

        Tween.Alpha(crossIcon, 0, duration: 0.3f);
    }

    public void Deactivate()
    {
        inactiveState.gameObject.SetActive(true);
        crossIcon.gameObject.SetActive(true);

        inactiveState.color = ColorUtil.WithAlpha(Color.white, 0);
        crossIcon.color = ColorUtil.WithAlpha(Color.white, 0);

        Tween.Alpha(activeState, 0, duration: 0.3f)
        .OnComplete(() =>
            {
                activeState.gameObject.SetActive(false);
            }
        );
        Tween.Alpha(inactiveState, 1, duration: 0.3f);
        Tween.Alpha(crossIcon, 1, duration: 0.3f);
    }
}
