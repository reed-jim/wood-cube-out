using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ShiftBreakPopup : BasePopup
{
    [SerializeField] private TMP_Text countdownText;

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction: () =>
        {
            StartCountdown();
        });
    }

    private void StartCountdown()
    {
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        int timeLeft = 5;

        while (timeLeft >= 0)
        {
            countdownText.text = $"{timeLeft}";

            timeLeft--;

            yield return waitForSeconds;
        }

        AdmobAdsMax.Instance.ShowMRec();

        AdmobAdsMax.Instance.ShowDoubleInterstitialAds(onDoubleInterClosed: () =>
        {
            AdmobAdsMax.Instance.HideMRec();

            Hide(onCompletedAction: () =>
            {
                UnpauseDayTime();
            });
        });
    }
}
