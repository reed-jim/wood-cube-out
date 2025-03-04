using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardItem : MonoBehaviour
{
    [SerializeField] private Button claimButton;

    [Header("CUSTOMIZE")]
    [SerializeField] private int dayIndex;

    private int _day;

    void Awake()
    {
        claimButton.onClick.AddListener(Claim);

        _day = dayIndex + 1;

        if (DailyRewardUtil.IsClaimedTodayReward(_day))
        {
            claimButton.gameObject.SetActive(false);
        }
        else
        {
            claimButton.gameObject.SetActive(true);
        }
    }

    private void Claim()
    {
        DataUtility.Save(GameConstants.LAST_DAY_CLAIM_DAILY_REWARD, DateTime.Now.Day);
        DataUtility.Save(GameConstants.NUM_DAY_CLAIMED_REWARD, DataUtility.Load(GameConstants.NUM_DAY_CLAIMED_REWARD, 0) + 1);

        Tween.Scale(claimButton.transform, 1.1f, duration: 0.15f).OnComplete(() =>
        {
            Tween.Scale(claimButton.transform, 0, duration: 0.3f).OnComplete(() =>
            {
                claimButton.gameObject.SetActive(false);
            });
        });
    }
}
