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
    [SerializeField] private Button debugNextDayButton;

    private DailyRewardItem[] _dailyRewardItems;

    public static event Action claimDailyRewardEvent;
    public static event Action refreshDailyRewardEvent;

    protected override void RegisterMoreEvent()
    {
        ClaimRewardPopup.showClaimRewardPopupEvent += OnClaimRewardPopupShown;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;
        DailyRewardItem.enableClaimDailyRewardButtonsEvent += EnableClaimButtons;

        claimButton.onClick.AddListener(Claim);

        try
        {
            debugNextDayButton.onClick.AddListener(DebugNextDay);
        }
        catch (System.Exception)
        {

        }
    }

    protected override void UnregisterMoreEvent()
    {
        ClaimRewardPopup.showClaimRewardPopupEvent -= OnClaimRewardPopupShown;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
        DailyRewardItem.enableClaimDailyRewardButtonsEvent -= EnableClaimButtons;
    }

    protected override void MoreActionInAwake()
    {
        _dailyRewardItems = TransformUtil.GetComponentsFromAllChildren<DailyRewardItem>(transform).ToArray();
    }

    protected override void Show(Action onCompletedAction = null)
    {
        bool isRewardAvailable = DailyRewardUtil.IsNewDay();

        if (isRewardAvailable)
        {
             //GameController.Instance.admobAds.ResetAdsCooldown();
        }

        base.Show(onCompletedAction);

        Refresh();
    }

    private void Refresh()
    {
        claimButton.gameObject.SetActive(false);
        claimX2Button.gameObject.SetActive(false);

        // for (int i = 0; i < _dailyRewardItems.Length; i++)
        // {
        //     if (DailyRewardUtil.IsDailyRewardReadyForDay(i))
        //     {
        //         claimButton.gameObject.SetActive(true);
        //         claimX2Button.gameObject.SetActive(true);

        //         break;
        //     }
        // }
    }

    private void Claim()
    {
        claimDailyRewardEvent?.Invoke();
    }

    private void OnClaimRewardPopupShown(bool isShow)
    {
        float start = 0;
        float end = 1;

        if (isShow)
        {
            start = 1;
            end = 0;

            popupCanvasGroup.interactable = false;
        }

        _tweens.Add(Tween.Custom(start, end, duration: 0.3f, onValueChange: newVal =>
        {
            popupCanvasGroup.alpha = newVal;
        }));
    }

    private void OnResourcesEarnPopupShown(bool isShow)
    {
        float start = 0;
        float end = 1;

        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (isShow)
        {
            return;

            start = 1;
            end = 0;

            popupCanvasGroup.interactable = false;
        }
        else
        {
            popupCanvasGroup.alpha = 1;
            popupCanvasGroup.interactable = true;

            HideImmediately();
        }

        _tweens.Add(Tween.Custom(start, end, duration: 0.3f, onValueChange: newVal =>
        {
            popupCanvasGroup.alpha = newVal;
        })
            .OnComplete(() =>
            {
                Refresh();

                popupCanvasGroup.interactable = true;
            })
        );
    }

    private void EnableClaimButtons()
    {
        _tweens.Add(Tween.Delay(0.3f)
            .OnComplete(() =>
            {
                claimButton.gameObject.SetActive(true);
                claimX2Button.gameObject.SetActive(true);
            })
        );
    }

    private void DebugNextDay()
    {
        GamePersistentVariable.currentDailyRewardIndex++;

        if (GamePersistentVariable.currentDailyRewardIndex > 7)
        {
            GamePersistentVariable.currentDailyRewardIndex = 1;
        }

        DataUtility.Save("last_timestamp_claim_daily_reward", 0);
        DataUtility.Save("current_daily_reward_day", GamePersistentVariable.currentDailyRewardIndex);

        refreshDailyRewardEvent?.Invoke();
    }
}
