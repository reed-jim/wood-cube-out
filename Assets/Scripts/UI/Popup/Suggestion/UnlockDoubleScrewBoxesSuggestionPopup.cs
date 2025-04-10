using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Localization;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockDoubleScrewBoxesSuggestionPopup : BasePopup
{
    [SerializeField] private Button claimButton;
    [SerializeField] private Button noThanksButton;
    [SerializeField] private TMP_Text timeText;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;

    private Coroutine _countingCoroutine;

    protected override void MoreActionInAwake()
    {
        claimButton.onClick.AddListener(Claim);
        noThanksButton.onClick.AddListener(Hide);
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        _countingCoroutine = StartCoroutine(Counting());
    }

    private IEnumerator Counting()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        int remaningTime = 10;

        timeText.text = $"{remaningTime}s";

        while (remaningTime > 0)
        {
            // CommonUtil.StopAllTweens(_tweens);

            SoundManager.Instance.PlaySoundCountingTime();

            _tweens.Add(Tween.Scale(claimButton.transform, 1.05f, cycles: 2, cycleMode: CycleMode.Yoyo, duration: 0.4f)
            .OnComplete(() =>
            {
                remaningTime--;

                timeText.text = $"{remaningTime}s";

                _tweens.Add(Tween.Scale(claimButton.transform, 1f, cycles: 2, cycleMode: CycleMode.Yoyo, duration: 0.1f, ease: Ease.Linear));
            }));

            yield return waitForSeconds;
        }

        Hide();
    }

    private void Claim()
    {
        AdmobAdsMax.Instance.ShowVideoReward(
            OnRewaredAdCompleted, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: ActionWatchVideo.ClaimDoubleScrewBoxes);

        if (_countingCoroutine != null)
        {
            StopCoroutine(_countingCoroutine);
        }
    }

    private void OnRewaredAdCompleted()
    {
        base.Hide(onCompletedAction: () =>
        {
            ScrewBoxManager.Instance.UnlockAllScrewBoxes();

            SaferioTracking.TrackRewardedAdCompleted(currentLevel.Value, $"ClaimDoubleScrewBoxes");
        });
    }

    private void ShowAdsNotLoadedPopup()
    {
        InvokeShowAdNotLoadedPopupEvent();
    }
}
