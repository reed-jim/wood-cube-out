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
            RewardBundleData rewardBundleData = goldPassDataContainer.goldPassRewards[i];

            goldPassMilestoneItems[i].Setup(rewardBundleData, goldPassMilestoneData);
        }
    }

    private IEnumerator Counting()
    {
        WaitForSeconds waitOneSecond = new WaitForSeconds(1);

        while (true)
        {
            UpdateRemainingReplenishLife();

            yield return waitOneSecond;
        }
    }

    private void UpdateRemainingReplenishLife()
    {
        if (goldPassDataContainer.endTimestamp == 0)
        {
            goldPassDataContainer.endTimestamp =
                new DateTimeOffset(DateTime.Now).AddDays(goldPassDataContainer.numDays).ToUnixTimeSeconds();
        }

        // TimeSpan timeSinceStart = DateTime.Now - DateTimeOffset.FromUnixTimeSeconds(goldPassDataContainer.endTimestamp).DateTime;

        double remainingTime = (DateTimeOffset.FromUnixTimeSeconds(goldPassDataContainer.endTimestamp).DateTime - DateTime.Now).TotalSeconds;

        timeCounterText.text = $"{ConvertTimeFormat(remainingTime)}";
    }

    #region UTIL
    private string ConvertTimeFormat(double time)
    {
        return $"{TimeSpan.FromSeconds(time).ToString(@"mm\:ss")}";
    }
    #endregion
}
