using System;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.UI;

public class SettingItemUI : MonoBehaviour
{
    [SerializeField] private Image activeState;
    [SerializeField] private Image inactiveState;
    [SerializeField] private Image cross;
    [SerializeField] private Button selectButton;

    public bool isActive;
    private bool _isInTransition;

    private Action<bool> onSwitched;

    void Awake()
    {
        selectButton.onClick.AddListener(Switch);
    }

    public void RegisterOnSwitchedEvent(Action<bool> action)
    {
        onSwitched = action;
    }

    public void SetState(bool isActive)
    {
        if (isActive)
        {
            SaferioTween.AlphaShow(activeState);
            SaferioTween.AlphaHide(inactiveState);
            SaferioTween.AlphaHide(cross);
        }
        else
        {
            SaferioTween.AlphaHide(activeState);
            SaferioTween.AlphaShow(inactiveState);
            SaferioTween.AlphaShow(cross);
        }

        this.isActive = isActive;
    }

    private void Switch()
    {
        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        isActive = !isActive;

        if (isActive)
        {
            SaferioTween.AlphaShow(activeState);
            SaferioTween.AlphaHide(inactiveState);
            SaferioTween.AlphaHide(cross, onCompleteAction: () =>
            {
                _isInTransition = false;

                onSwitched?.Invoke(isActive);
            });
        }
        else
        {
            SaferioTween.AlphaHide(activeState, onCompleteAction: () =>
            {
                _isInTransition = false;

                onSwitched?.Invoke(isActive);
            });
            SaferioTween.AlphaShow(inactiveState);
            SaferioTween.AlphaShow(cross);
        }
    }
}
