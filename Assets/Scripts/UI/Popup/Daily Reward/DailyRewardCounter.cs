using System;
using System.Collections;
using Lean.Localization;
using PrimeTween;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardCounter : SaferioTweenBehaviour
{
    [SerializeField] private TMP_Text timeCounterText;
    [SerializeField] private Transform claimNotification;
    [SerializeField] private GameObject buttonNameContainer;

    [SerializeField] private GameObject lockContainer;
    [SerializeField] private Button selectButton;
    [SerializeField] private LeanLocalizedTextMeshProUGUI unlockLevelText;

    [SerializeField] private IntVariable currentLevel;

    [Header("Components")] 
    [SerializeField] private SkeletonGraphic giftSkeGraphics;

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
        giftSkeGraphics?.gameObject.SetActive(true);
        StartCoroutine(IEEnableGiftSkeGraphicsDelay(1.5f));
        Refresh();
        
        IEnumerator IEEnableGiftSkeGraphicsDelay(float delay)
        {
            
            yield return new WaitForSeconds(delay);
            giftSkeGraphics?.gameObject.SetActive(true);
        }
    }

    private void OnRemoteConfigReady()
    {
        DailyRewardConfig dailyRewardConfig = GamePersistentVariable.dailyRewardConfig;

        if (currentLevel.ValueUsingPureJson >= dailyRewardConfig.unlockLevel)
        {
            lockContainer.SetActive(false);

            // selectButton.interactable = true;
        }
        else
        {
            AddTween(Tween.Delay(0.3f).OnComplete(() =>
            {
                lockContainer.SetActive(true);

                unlockLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{dailyRewardConfig.unlockLevel}");

                // selectButton.interactable = false;
            }));
        }
    }

    private void Refresh()
    {
        bool isRewardAvailable = DailyRewardUtil.IsNewDay();
        bool isUnlocked = currentLevel.ValueUsingPureJson >= GamePersistentVariable.dailyRewardConfig.unlockLevel;

        if (isRewardAvailable && isUnlocked)
        {
            claimNotification.gameObject.SetActive(true);

            StartCoroutine(AnimateClaimNotification());
        }
        else
        {
            claimNotification.gameObject.SetActive(false);
        }

        DailyRewardConfig dailyRewardConfig = GamePersistentVariable.dailyRewardConfig;

        if (currentLevel.ValueUsingPureJson >= dailyRewardConfig.unlockLevel)
        {
            lockContainer.SetActive(false);

            // selectButton.interactable = true;
        }
        else
        {
            AddTween(Tween.Delay(0.3f).OnComplete(() =>
            {
                lockContainer.SetActive(true);

                unlockLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{dailyRewardConfig.unlockLevel}");

                // selectButton.interactable = false;
            }));
        }
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
