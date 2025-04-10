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
        if (CommonUtil.GetParentActiveChildCount(transform.parent) > 1)
        {
            return;
        }

        // delay enable buttons
        closeButton.interactable = false;
        claimButton.interactable = false;
        noThanksButton.interactable = false;

        closeButton.transform.localScale = Vector3.zero;
        claimButton.transform.localScale = Vector3.zero;
        noThanksButton.transform.localScale = Vector3.zero;

        timeText.text = $"{10}s";

        base.Show(ShowButtons);

        void ShowButtons()
        {
            _tweens.Add(Tween.Scale(closeButton.transform, 1, duration: 0.3f));
            _tweens.Add(Tween.Scale(claimButton.transform, 1, duration: 0.3f));
            _tweens.Add(Tween.Scale(noThanksButton.transform, 1, duration: 0.3f)
                .OnComplete(() =>
                {
                    closeButton.interactable = true;
                    claimButton.interactable = true;
                    noThanksButton.interactable = true;

                    _countingCoroutine = StartCoroutine(Counting());
                })
            );
        }
    }

    private IEnumerator Counting()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        int remaningTime = 10;

        while (remaningTime > 0)
        {
            // CommonUtil.StopAllTweens(_tweens);

            // SoundManager.Instance.PlaySoundTightenScrew();

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
        Hide();

        InvokeShowAdNotLoadedPopupEvent();
    }
}
