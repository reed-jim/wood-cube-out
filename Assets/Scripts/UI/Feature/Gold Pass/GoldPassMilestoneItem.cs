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

    [Header("CUSTOMIZE")]
    [SerializeField] private bool isChestReward;

    public void Setup(DailyRewardData rewardData, GoldPassMilestoneData goldPassMilestoneData)
    {
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

        doneState.gameObject.SetActive(goldPassMilestoneData.IsPassed);
        freePassDoneIcon.gameObject.SetActive(goldPassMilestoneData.IsClaimed);

        goldPassLock.SetActive(!GamePersistentVariable.isGoldPassUnlocked);
    }
}
