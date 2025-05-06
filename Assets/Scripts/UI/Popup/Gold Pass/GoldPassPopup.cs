using System;
using System.Collections;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoldPassPopup : BasePopup
{
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private TMP_Text timeCounterText;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button closeTutorialButton;
    [SerializeField] private Slider progress;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text passLevelText;

    [SerializeField] private GameObject goldPassMilestonePrefab;
    [SerializeField] private Transform goldPassMilestoneContainer;
    [SerializeField] private GameObject goldPassMilestoneLevelPrefab;
    [SerializeField] private Transform goldPassMilestoneLevelContainer;
    [SerializeField] private GoldPassDataContainer goldPassDataContainer;

    private GoldPassMilestoneItem[] goldPassMilestoneItems;

    protected override void RegisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;

        helpButton.onClick.AddListener(() => ShowTutorial(true));
        closeTutorialButton.onClick.AddListener(() => ShowTutorial(false));
    }

    protected override void UnregisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
    }

    protected override void MoreActionInAwake()
    {
        tutorialCanvasGroup.gameObject.SetActive(false);

        Generate();
    }

    protected override void Show(Action onCompletedAction = null)
    {
        progress.value = 0;

        base.Show(onCompletedAction: () =>
        {
            AnimateProgress();
        });

        Setup();

        StartCoroutine(Counting());
    }

    private void Generate()
    {
        GoldPassMilestoneData[] goldPassMilestones = goldPassDataContainer.goldPassMilestones;

        goldPassMilestoneItems = new GoldPassMilestoneItem[goldPassMilestones.Length];

        UIUtil.SetSizeY(goldPassMilestoneContainer.GetComponent<RectTransform>(),
            goldPassMilestones.Length * 1.3f * goldPassMilestonePrefab.GetComponent<RectTransform>().sizeDelta.y);

        for (int i = 0; i < goldPassMilestones.Length; i++)
        {
            GameObject item = Instantiate(goldPassMilestonePrefab, goldPassMilestoneContainer);

            item.name = $"Milestone {i}";

            goldPassMilestoneItems[i] = item.GetComponent<GoldPassMilestoneItem>();

            RectTransform rectTransform = item.GetComponent<RectTransform>();

            UIUtil.SetLocalPositionY(rectTransform, -(i + 0.5f) * 1.2f * rectTransform.sizeDelta.y);

            GameObject levelDisplay = Instantiate(goldPassMilestoneLevelPrefab, goldPassMilestoneLevelContainer);

            levelDisplay.GetComponent<FollowOther>().Other = item.transform;
            levelDisplay.GetComponentInChildren<TMP_Text>().text = $"{i + 1}";
        }
    }

    private void Setup()
    {
        goldPassMilestoneItems = TransformUtil.GetComponentsFromAllChildren<GoldPassMilestoneItem>(transform).ToArray();

        for (int i = 0; i < goldPassMilestoneItems.Length; i++)
        {
            int index = i;

            goldPassMilestoneItems[i].Setup(index);
        }

        // float currentPassLevelProgress = GetCurrentPassLevelProgress();

        // if (currentPassLevelProgress > 0.19f)
        // {
        //     progress.value = currentPassLevelProgress;
        // }
        // else
        // {
        //     progress.value = 0;
        // }

        // progressText.text = $"{(currentPassLevelProgress * 100).ToString("F0")}%";
        passLevelText.text = $"{goldPassDataContainer.goldPassData.passLevel}";
    }

    private void AnimateProgress()
    {
        float currentPassLevelProgress = GetCurrentPassLevelProgress();

        if (currentPassLevelProgress > 0.19f)
        {
            progress.value = 0;

            _tweens.Add(Tween.Custom(progress.value, currentPassLevelProgress, duration: 0.3f, onValueChange: newVal =>
            {
                progress.value = newVal;
                progressText.text = $"{(newVal * 100).ToString("F0")}%";
            }));
        }
        else
        {
            progress.value = 0;
            progressText.text = $"{(currentPassLevelProgress * 100).ToString("F0")}%";
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

    // private void UpdateRemainingTime()
    // {
    //     if (goldPassDataContainer.goldPassData.endTimestamp == 0)
    //     {
    //         goldPassDataContainer.goldPassData.endTimestamp =
    //             new DateTimeOffset(DateTime.Now).AddDays(goldPassDataContainer.numDays).ToUnixTimeSeconds();

    //         DataUtility.Save(GameConstants.GOLD_PASS_DATA_CONTAINER, goldPassDataContainer.goldPassData);
    //     }

    //     // TimeSpan timeSinceStart = DateTime.Now - DateTimeOffset.FromUnixTimeSeconds(goldPassDataContainer.endTimestamp).DateTime;

    //     double remainingTime = (DateTimeOffset.FromUnixTimeSeconds(goldPassDataContainer.goldPassData.endTimestamp).DateTime - DateTime.Now).TotalSeconds;

    //     timeCounterText.text = $"{ConvertTimeFormat(remainingTime)}";
    // }

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

    private void ShowTutorial(bool isShow)
    {
        if (isShow)
        {
            tutorialCanvasGroup.gameObject.SetActive(true);

            tutorialCanvasGroup.alpha = 0;

            _tweens.Add(Tween.Alpha(tutorialCanvasGroup, 1, duration: 0.3f).OnComplete(() =>
            {

            }));
        }
        else
        {
            _tweens.Add(Tween.Alpha(tutorialCanvasGroup, 0, duration: 0.3f).OnComplete(() =>
            {
                tutorialCanvasGroup.gameObject.SetActive(false);
            }));
        }
    }

    private float GetCurrentPassLevelProgress()
    {
        return (float)goldPassDataContainer.goldPassData.keyCollected /
            goldPassDataContainer.goldPassMilestones[goldPassDataContainer.goldPassData.passLevel].KeyRequired;
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
