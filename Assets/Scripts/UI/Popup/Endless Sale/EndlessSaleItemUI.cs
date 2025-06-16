using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndlessSaleItemUI : MonoBehaviour
{
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text[] quantityTexts;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private GameObject lockObject;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private DailyRewardDataContainer rewardDataContainer;

    [Header("CUSTOMIZE")]
    [SerializeField] private int index;

    #region EVENT
    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;
    public static event Action refreshDailyRewardEvent;
    #endregion

    private void Awake()
    {
        buyButton.onClick.AddListener(Claim);

        Init();
    }

    private void Init()
    {
        int coinQuantity = rewardDataContainer.Items[index].CoinQuantity;
        int[] boosterQuantites = rewardDataContainer.Items[index].BoosterQuantities;

        int currentIndex = 0;

        if (coinQuantity > 0)
        {
            quantityTexts[0].text = $"{coinQuantity}";
        }
        // else
        // {
        //     for (int i = 0; i < boosterQuantites.Length; i++)
        //     {
        //         if (boosterQuantites[i] > 0)
        //         {
        //             // rewardIcons[i].sprite = iconSprites[1 + i];

        //             quantityTexts[currentIndex].text = $"{boosterQuantites[i]}";

        //             currentIndex++;
        //         }
        //     }
        // }
    }

    private void Claim()
    {
        Tween.Scale(buyButton.transform, 1.1f, duration: 0.15f).OnComplete(() =>
        {
            Tween.Scale(buyButton.transform, 0, duration: 0.3f).OnComplete(() =>
            {
                int coinQuantity = rewardDataContainer.Items[index].CoinQuantity;
                int[] boosterQuantites = rewardDataContainer.Items[index].BoosterQuantities;

                userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
                userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
                userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

                userResourcesObserver.ChangeCoin(coinQuantity);

                showResourcesEarnPopupEvent?.Invoke(coinQuantity, boosterQuantites, false);
            });
        });

        EndlessSaleData endlessSaleData = DataUtility.Load(GameConstants.ENDLESS_SALE_DATA, new EndlessSaleData());

        endlessSaleData.RewardLevel++;
    }
}
