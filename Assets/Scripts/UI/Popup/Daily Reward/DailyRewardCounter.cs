using System;
using System.Collections;
using PrimeTween;
using TMPro;
using UnityEngine;

public class DailyRewardCounter : SaferioTweenBehaviour
{
    [SerializeField] private TMP_Text timeCounterText;
    [SerializeField] private Transform claimNotification;
    [SerializeField] private GameObject buttonNameContainer;

    protected override void MoreActionInAwake()
    {
        DailyRewardItem.claimDailyRewardEvent += Refresh;

        StartCoroutine(Counting());
    }

    protected override void MoreActionOnDestroy()
    {
        DailyRewardItem.claimDailyRewardEvent -= Refresh;
    }

    void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        bool isRewardAvailable = DailyRewardUtil.IsNewDay();

        if (isRewardAvailable)
        {
            claimNotification.gameObject.SetActive(true);

            StartCoroutine(AnimateClaimNotification());
        }
        else
        {
            claimNotification.gameObject.SetActive(false);
        }

        buttonNameContainer.SetActive(!isRewardAvailable);
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

    private IEnumerator AnimateClaimNotification()
    {
        WaitForSeconds waitOneSecond = new WaitForSeconds(0.3f);

        int step = 0;
        bool isWaiting = false;

        while (true)
        {
            if (!isWaiting)
            {
                if (step == 0)
                {
                    AddTween(Tween.Scale(claimNotification, 1.03f, duration: 0.3f)
                    .OnComplete(() =>
                    {
                        step = 1;
                        isWaiting = false;
                    }));
                }
                else
                {
                    AddTween(Tween.Scale(claimNotification, 0.97f, duration: 0.3f)
                    .OnComplete(() =>
                    {
                        step = 0;
                        isWaiting = false;
                    }));
                }

                isWaiting = true;
            }

            yield return waitOneSecond;
        }
    }

    private void UpdateRemainingTime()
    {
        DateTime now = DateTime.UtcNow;

        DateTime endOfDay = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, DateTimeKind.Utc);

        TimeSpan timeLeft = endOfDay - now;

        double remainingSeconds = timeLeft.TotalSeconds;

        timeCounterText.text = $"{CommonUtil.ToHHMMSS(remainingSeconds)}";
    }
}
