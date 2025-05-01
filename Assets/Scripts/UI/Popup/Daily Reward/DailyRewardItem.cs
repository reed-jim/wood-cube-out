using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardItem : MonoBehaviour
{
    [SerializeField] private Button claimButton;
    [SerializeField] private Image[] rewardIcons;
    [SerializeField] private Image activeState;
    [SerializeField] private GameObject doneState;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text[] quantityTexts;

    [SerializeField] private Sprite[] iconSprites;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private DailyRewardDataContainer dailyRewardDataContainer;

    [Header("CUSTOMIZE")]
    [SerializeField] private int dayIndex;

    private int _day;

    #region EVENT
    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;
    public static event Action refreshDailyRewardEvent;
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

        int currentIndex = 0;

        if (coinQuantity > 0)
        {
            // rewardIcons[0].sprite = iconSprites[0];

            quantityTexts[0].text = $"{coinQuantity}";
        }
        else
        {
            for (int i = 0; i < boosterQuantites.Length; i++)
            {
                if (boosterQuantites[i] > 0)
                {
                    // rewardIcons[i].sprite = iconSprites[1 + i];

                    quantityTexts[currentIndex].text = $"{boosterQuantites[i]}";

                    currentIndex++;
                }
            }
        }

        _day = dayIndex + 1;

        dayText.text = $"Day {_day}";

        activeState.gameObject.SetActive(DailyRewardUtil.IsDailyRewardReadyForDay(_day));
        doneState.gameObject.SetActive(DailyRewardUtil.IsClaimedDailyReward(_day));

        claimButton.gameObject.SetActive(false);
    }

    private void Claim()
    {
        if (!DailyRewardUtil.IsDailyRewardReadyForDay(_day))
        {
            return;
        }

        DataUtility.Save("last_timestamp_claim_daily_reward", new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds());
        DataUtility.Save($"is_claim_daily_reward_day_{_day}", true);
        DataUtility.Save($"current_daily_reward_day", GamePersistentVariable.currentDailyRewardIndex + 1);

        Tween.Scale(claimButton.transform, 1.1f, duration: 0.15f).OnComplete(() =>
        {
            Tween.Scale(claimButton.transform, 0, duration: 0.3f).OnComplete(() =>
            {
                int coinQuantity = dailyRewardDataContainer.Items[dayIndex].CoinQuantity;
                int[] boosterQuantites = dailyRewardDataContainer.Items[dayIndex].BoosterQuantities;

                userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
                userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
                userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

                userResourcesObserver.ChangeCoin(coinQuantity);

                showResourcesEarnPopupEvent?.Invoke(coinQuantity, boosterQuantites, false);

                claimButton.gameObject.SetActive(false);

                Init();
            });
        });
    }
}
