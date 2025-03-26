using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardItem : MonoBehaviour
{
    [SerializeField] private Button claimButton;
    [SerializeField] private Image icon;
    [SerializeField] private Image activeState;
    [SerializeField] private TMP_Text quantityText;

    [SerializeField] private Sprite[] iconSprites;

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
        DailyRewardPopup.claimDailyRewardEvent += Claim;

        claimButton.onClick.AddListener(Claim);

        Init();
    }

    void OnDestroy()
    {
        DailyRewardPopup.claimDailyRewardEvent -= Claim;
    }

    private void Init()
    {
        int coinQuantity = dailyRewardDataContainer.Items[dayIndex].CoinQuantity;
        int[] boosterQuantites = dailyRewardDataContainer.Items[dayIndex].BoosterQuantities;

        if (coinQuantity > 0)
        {
            icon.sprite = iconSprites[0];

            quantityText.text = $"{coinQuantity}";
        }
        else
        {
            for (int i = 0; i < boosterQuantites.Length; i++)
            {
                if (boosterQuantites[i] > 0)
                {
                    icon.sprite = iconSprites[1 + i];

                    quantityText.text = $"{boosterQuantites[i]}";
                }
            }
        }

        _day = dayIndex + 1;

        if (DailyRewardUtil.IsClaimedTodayReward(_day))
        {
            activeState.gameObject.SetActive(false);
        }
        else
        {
            activeState.gameObject.SetActive(true);
        }

        claimButton.gameObject.SetActive(false);
    }

    private void Claim()
    {
        if (DailyRewardUtil.IsClaimedTodayReward(_day))
        {
            return;
        }

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

                Init();
            });
        });
    }
}
