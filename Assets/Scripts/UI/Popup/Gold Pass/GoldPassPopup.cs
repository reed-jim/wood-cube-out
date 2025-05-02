using System;
using System.Collections;
using TMPro;
using UnityEditor.Embree;
using UnityEngine;
using UnityEngine.UI;

public class GoldPassPopup : BasePopup
{
    [SerializeField] private TMP_Text timeCounterText;
    [SerializeField] private Button helpButton;
    [SerializeField] private Slider progress;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text passLevelText;

    [SerializeField] private GoldPassDataContainer goldPassDataContainer;

    private GoldPassMilestoneItem[] goldPassMilestoneItems;

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        Setup();

        StartCoroutine(Counting());
    }

    private void Setup()
    {
        goldPassMilestoneItems = TransformUtil.GetComponentsFromAllChildren<GoldPassMilestoneItem>(transform).ToArray();

        for (int i = 0; i < goldPassMilestoneItems.Length; i++)
        {
            GoldPassMilestoneData goldPassMilestoneData = goldPassDataContainer.goldPassMilestones[i];
            DailyRewardData rewardBundleData = goldPassDataContainer.goldPassRewards[i];

            goldPassMilestoneItems[i].Setup(rewardBundleData, goldPassMilestoneData);
        }

        float currentPassLevelProgress = GetCurrentPassLevelProgress();

        if (currentPassLevelProgress > 0.19f)
        {
            progress.value = currentPassLevelProgress;
            progressText.text = $"{(currentPassLevelProgress * 100).ToString("F1")}%";
        }
        else
        {

        }

        passLevelText.text = $"{goldPassDataContainer.goldPassData.passLevel}";
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
        if (goldPassDataContainer.goldPassData.endTimestamp == 0)
        {
            goldPassDataContainer.goldPassData.endTimestamp =
                new DateTimeOffset(DateTime.Now).AddDays(goldPassDataContainer.numDays).ToUnixTimeSeconds();

            DataUtility.Save(GameConstants.GOLD_PASS_DATA_CONTAINER, goldPassDataContainer.goldPassData);
        }

        // TimeSpan timeSinceStart = DateTime.Now - DateTimeOffset.FromUnixTimeSeconds(goldPassDataContainer.endTimestamp).DateTime;

        double remainingTime = (DateTimeOffset.FromUnixTimeSeconds(goldPassDataContainer.goldPassData.endTimestamp).DateTime - DateTime.Now).TotalSeconds;

        timeCounterText.text = $"{ConvertTimeFormat(remainingTime)}";
    }

    private void Claim()
    {

    }

    private float GetCurrentPassLevelProgress()
    {
        return (float)goldPassDataContainer.goldPassData.keyCollected /
            goldPassDataContainer.goldPassMilestones[goldPassDataContainer.goldPassData.passLevel].KeyRequired;
    }

    #region UTIL
    private string ConvertTimeFormat(double totalSeconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

        return string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}",
                        (int)time.TotalDays,
                        time.Hours,
                        time.Minutes,
                        time.Seconds);
    }
    #endregion
}
