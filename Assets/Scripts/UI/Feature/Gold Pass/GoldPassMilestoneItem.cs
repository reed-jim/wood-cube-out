using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoldPassMilestoneItem : MonoBehaviour
{
    #region SHARED
    [SerializeField] private RectTransform container;
    [SerializeField] private GameObject doneState;
    [SerializeField] private TMP_Text passNumberText;
    [SerializeField] private Sprite[] rewardSprites;
    [SerializeField] private Button claimButton;
    #endregion

    #region FREE PASS
    [SerializeField] private TMP_Text freePassRewardQuantity;
    [SerializeField] private Image freePassRewardIcon;
    [SerializeField] private Image freePassDoneIcon;
    [SerializeField] private GameObject freePassChest;
    #endregion

    #region GOLD PASS
    [SerializeField] private TMP_Text goldPassRewardQuantity;
    [SerializeField] private GameObject goldPassLock;
    [SerializeField] private Image goldPassRewardIcon;
    [SerializeField] private Image goldPassDoneIcon;
    [SerializeField] private GameObject goldPassChest;
    #endregion

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private GoldPassDataContainer goldPassDataContainer;
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private bool isChestReward;

    private int _index;

    #region EVENT
    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;
    #endregion

    private void Awake()
    {
        claimButton.onClick.AddListener(Claim);
    }

    public void Setup(int index)
    {
        GoldPassMilestoneData goldPassMilestoneData = goldPassDataContainer.goldPassMilestones[index];
        DailyRewardData rewardData = goldPassDataContainer.goldPassRewards[index];

        int numReward = 0;

        int singleRewardQuantity = 0;
        int rewardSpriteIndex = 0;

        if (rewardData.CoinQuantity > 0)
        {
            numReward++;

            singleRewardQuantity = rewardData.CoinQuantity;
            rewardSpriteIndex = 0;
        }

        for (int i = 0; i < rewardData.BoosterQuantities.Length; i++)
        {
            if (rewardData.BoosterQuantities[i] > 0)
            {
                numReward++;

                singleRewardQuantity = rewardData.BoosterQuantities[i];
                rewardSpriteIndex = 1 + i;
            }
        }

        bool isSingleReward = numReward == 1;

        freePassChest.SetActive(!isSingleReward);
        goldPassChest.SetActive(!isSingleReward);

        freePassRewardIcon.gameObject.SetActive(isSingleReward);
        freePassRewardQuantity.gameObject.SetActive(isSingleReward);

        goldPassRewardIcon.gameObject.SetActive(isSingleReward);
        goldPassRewardQuantity.gameObject.SetActive(isSingleReward);

        freePassRewardQuantity.text = $"{singleRewardQuantity}";
        goldPassRewardQuantity.text = $"{3 * singleRewardQuantity}";

        freePassRewardIcon.sprite = rewardSprites[rewardSpriteIndex];
        goldPassRewardIcon.sprite = rewardSprites[rewardSpriteIndex];

        UIUtil.SetSizeKeepRatioX(freePassRewardIcon, 0.5f * container.sizeDelta.y);
        UIUtil.SetSizeKeepRatioX(goldPassRewardIcon, 0.5f * container.sizeDelta.y);

        doneState.gameObject.SetActive(goldPassMilestoneData.IsPassed && !goldPassMilestoneData.IsClaimed);
        freePassDoneIcon.gameObject.SetActive(goldPassMilestoneData.IsClaimed);

        claimButton.interactable = goldPassMilestoneData.IsPassed && !goldPassMilestoneData.IsClaimed;

        goldPassLock.SetActive(!GamePersistentVariable.isGoldPassUnlocked);

        _index = index;
    }

    private void Claim()
    {
        int coinQuantity = goldPassDataContainer.goldPassRewards[_index].CoinQuantity;
        int[] boosterQuantites = goldPassDataContainer.goldPassRewards[_index].BoosterQuantities;

        userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
        userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
        userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

        userResourcesObserver.ChangeCoin(coinQuantity);

        showResourcesEarnPopupEvent?.Invoke(coinQuantity, boosterQuantites, false);

        goldPassDataContainer.goldPassMilestones[_index].IsClaimed = true;

        Setup(_index);
    }
}
