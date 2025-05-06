using System;
using System.Collections;
using System.Threading.Tasks;
using Lean.Localization;
using PrimeTween;
using Saferio.Util;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class MedalTournamentPopup : BasePopup
{
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private ObjectPoolingScrollView scrollView;
    [SerializeField] private TMP_Text timeCounterText;

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

        CheckShowLeaderboardAsync();

        StartCoroutine(Counting());
    }

    // void Start()
    // {
    //     CheckShowLeaderboardAsync();
    // }

    private async Task CheckShowLeaderboardAsync()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await GameLeaderboardManager.Instance.GetLeaderboardData();

        scrollView.Refresh();
    }

    private void ShowReward()
    {
        Tween.Delay(2).OnComplete(() =>
        {
            int rank = 0;

            int coinQuantity = medalTournamentRewardDataContainer.Items[rank].CoinQuantity;
            int[] boosterQuantites = medalTournamentRewardDataContainer.Items[rank].BoosterQuantities;

            userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
            userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
            userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

            userResourcesObserver.ChangeCoin(coinQuantity);

            InvokeShowResourcesEarnPopupPopupEvent(coinQuantity, boosterQuantites, false);
        });
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
