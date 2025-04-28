using TMPro;
using UnityEngine;

public class GoldPassMilestoneItem : MonoBehaviour
{
    #region SHARED
    [SerializeField] private GameObject doneState;
    [SerializeField] private TMP_Text passNumberText;
    #endregion

    #region FREE PASS
    [SerializeField] private TMP_Text freePassRewardQuantity;
    [SerializeField] private GameObject freePassDoneIcon;
    [SerializeField] private GameObject freePassChest;
    #endregion

    #region GOLD PASS
    [SerializeField] private TMP_Text goldPassRewardQuantity;
    [SerializeField] private GameObject goldPassLock;
    [SerializeField] private GameObject goldPassDoneIcon;
    [SerializeField] private GameObject goldPassChest;
    #endregion

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private GoldPassDataContainer goldPassDataContainer;

    [Header("CUSTOMIZE")]
    [SerializeField] private bool isChestReward;

    public void Setup(RewardBundleData rewardBundleData, GoldPassMilestoneData goldPassMilestoneData)
    {
        freePassRewardQuantity.text = $"{rewardBundleData.Quantity}";
        goldPassRewardQuantity.text = $"{3 * rewardBundleData.Quantity}";

        doneState.gameObject.SetActive(goldPassMilestoneData.IsPassed);
        freePassDoneIcon.gameObject.SetActive(goldPassMilestoneData.IsClaimed);
    }
}
