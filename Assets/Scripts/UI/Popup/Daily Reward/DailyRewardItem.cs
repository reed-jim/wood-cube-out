using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardItem : MonoBehaviour
{
    [SerializeField] private Button claimButton;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private DailyRewardDataContainer dailyRewardDataContainer;

    [Header("CUSTOMIZE")]
    [SerializeField] private int dayIndex;

    private int _day;

    #region EVENT
    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;
    #endregion

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
                int coinQuantity = dailyRewardDataContainer.Items[dayIndex].CoinQuantity;
                int[] boosterQuantites = dailyRewardDataContainer.Items[dayIndex].BoosterQuantities;

                showResourcesEarnPopupEvent?.Invoke(coinQuantity, boosterQuantites, false);

                claimButton.gameObject.SetActive(false);
            });
        });
    }
}
