using System;
using System.Collections;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeeklyChallengePopup : BasePopup
{
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text timeCounterText;

    [SerializeField] private Image nextRewardIcon;
    [SerializeField] private TMP_Text nextRewardQuantity;
    [SerializeField] private Sprite[] rewardSprites;

    [SerializeField] private DailyRewardDataContainer weeklyChallengeDataContainer;

    protected override void RegisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;
    }

    protected override void UnregisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
    }

    protected override void MoreActionInAwake()
    {
        LoadData();
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        AnimateProgress();

        StartCoroutine(Counting());

        Refresh();
    }

    private void LoadData()
    {
        WeeklyChallengeData weeklyChallengeData = DataUtility.Load(GameConstants.WEEKLY_CHALLENGE_DATA, new WeeklyChallengeData());

        if (weeklyChallengeData.Items == null)
        {
            weeklyChallengeData.Items = new WeeklyChallengeItemData[30];

            for (int i = 0; i < weeklyChallengeData.Items.Length; i++)
            {
                weeklyChallengeData.Items[i] = new WeeklyChallengeItemData();
            }

            DataUtility.Save(GameConstants.WEEKLY_CHALLENGE_DATA, weeklyChallengeData);
        }

        int currentChallengeLevel = (weeklyChallengeData.EventItemCollected - weeklyChallengeData.EventItemCollected % 2) / 2;

        if (currentChallengeLevel != weeklyChallengeData.CurrentChallengeIndex)
        {
            weeklyChallengeData.CurrentChallengeIndex = currentChallengeLevel;

            DataUtility.Save(GameConstants.WEEKLY_CHALLENGE_DATA, weeklyChallengeData);
        }

        GamePersistentVariable.weeklyChallengeData = weeklyChallengeData;
    }

    private void Refresh()
    {
        int currentIndex = GamePersistentVariable.weeklyChallengeData.CurrentChallengeIndex;

        int coinQuantity = weeklyChallengeDataContainer.Items[currentIndex].CoinQuantity;
        int[] boosterQuantites = weeklyChallengeDataContainer.Items[currentIndex].BoosterQuantities;

        if (coinQuantity > 0)
        {
            nextRewardIcon.sprite = rewardSprites[0];
            nextRewardQuantity.text = $"{coinQuantity}";
        }
        else
        {
            for (int i = 0; i < boosterQuantites.Length; i++)
            {
                if (boosterQuantites[i] > 0)
                {
                    nextRewardIcon.sprite = rewardSprites[i + 1];
                    nextRewardQuantity.text = $"{boosterQuantites[i]}";

                    break;
                }
            }
        }

        UIUtil.SetSizeKeepRatioY(nextRewardIcon, nextRewardIcon.rectTransform.sizeDelta.x);
    }

    private void AnimateProgress()
    {
        WeeklyChallengeData weeklyChallengeData = DataUtility.Load(GameConstants.WEEKLY_CHALLENGE_DATA, new WeeklyChallengeData());

        float progress = weeklyChallengeData.EventItemCollected % 2 / 2f;

        if (progress > 0.19f)
        {
            progressSlider.value = 0;

            _tweens.Add(Tween.Custom(progressSlider.value, progress, duration: 0.3f, onValueChange: newVal =>
            {
                progressSlider.value = newVal;
                progressText.text = $"{(newVal * 100).ToString("F0")}%";
            }));
        }
        else
        {
            progressSlider.value = 0;
            progressText.text = $"{(progress * 100).ToString("F0")}%";
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
