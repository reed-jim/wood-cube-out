using System;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardPopup : BasePopup
{
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button claimX2Button;

    public static event Action claimDailyRewardEvent;

    protected override void RegisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;
        DailyRewardItem.enableClaimDailyRewardButtonsEvent += EnableClaimButtons;

        claimButton.onClick.AddListener(Claim);
    }

    protected override void UnregisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
        DailyRewardItem.enableClaimDailyRewardButtonsEvent -= EnableClaimButtons;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        claimButton.gameObject.SetActive(false);
        claimX2Button.gameObject.SetActive(false);
    }

    private void Claim()
    {
        claimDailyRewardEvent?.Invoke();
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

        _tweens.Add(Tween.Custom(start, end, duration: 0.3f, onValueChange: newVal =>
        {
            popupCanvasGroup.alpha = newVal;
        }));
    }

    private void EnableClaimButtons()
    {
        claimButton.gameObject.SetActive(true);
        claimX2Button.gameObject.SetActive(true);
    }
}
