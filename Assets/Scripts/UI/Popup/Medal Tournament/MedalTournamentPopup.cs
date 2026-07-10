using System;
using System.Collections;
using System.Threading.Tasks;
using Lean.Localization;
using PrimeTween;
using Saferio.Util;
using Spine.Unity;
using TMPro;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class MedalTournamentPopup : BasePopup
{
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private CanvasGroup rewardCanvasGroup;
    [SerializeField] private ObjectPoolingScrollView scrollView;
    [SerializeField] private TMP_Text timeCounterText;

    [SerializeField] private SkeletonGraphic giftRankOne;
    [SerializeField] private SkeletonGraphic giftRankTwo;
    [SerializeField] private SkeletonGraphic giftRankThree;
    [SerializeField] private SkeletonGraphic giftRankOther;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private DailyRewardDataContainer medalTournamentRewardDataContainer;

    protected override void RegisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;
    }

    protected override void UnregisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
    }

    protected override async void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        // ShowReward();

        // CheckShowLeaderboardAsync();

        StartCoroutine(Counting());

        scrollView.SetMaxItemNumber(LeaderboardDataContainer.Items.Length);

        rewardCanvasGroup.gameObject.SetActive(false);
    }

    // void Start()
    // {
    //     CheckShowLeaderboardAsync();
    // }

    private void Claim()
    {
        int rank = 0;

        int coinQuantity = medalTournamentRewardDataContainer.Items[rank].CoinQuantity;
        int[] boosterQuantites = medalTournamentRewardDataContainer.Items[rank].BoosterQuantities;

        userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
        userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
        userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

        userResourcesObserver.ChangeCoin(coinQuantity);

        InvokeShowResourcesEarnPopupPopupEvent(coinQuantity, boosterQuantites, false);

        _tweens.Add(Tween.Alpha(rewardCanvasGroup, 0, duration: 0.3f).OnComplete(() =>
        {
            rewardCanvasGroup.gameObject.SetActive(false);
        }));
    }

    private IEnumerator Counting()
    {
        WaitForSeconds waitOneSecond = new WaitForSeconds(1);

        while (true)
        {
            UpdateRemainingTime();

            yield return waitOneSecond;
        }
    }

    private void UpdateRemainingTime()
    {
        DateTime now = DateTime.UtcNow;

        int daysUntilEndOfWeek = ((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7;

        DateTime endOfWeek = new DateTime(now.Year, now.Month, now.Day)
            .AddDays(daysUntilEndOfWeek)
            .AddHours(23)
            .AddMinutes(59)
            .AddSeconds(59);

        TimeSpan timeLeft = endOfWeek - now;

        double remaningSecond = timeLeft.TotalSeconds;

        timeCounterText.text = $"{CommonUtil.ConvertTimeFormat(remaningSecond)}";
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
}

public class MedalTournamentData
{
    public bool isIntroDone;
}
