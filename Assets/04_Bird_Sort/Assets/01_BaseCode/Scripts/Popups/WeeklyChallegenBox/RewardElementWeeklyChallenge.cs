using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardElementWeeklyChallenge : MonoBehaviour
{
    [SerializeField] protected GameObject otherFarmeObj;
    [SerializeField] protected GameObject currentFarmeObj;
    [SerializeField] protected GameObject fxcurrentFarme;

    [SerializeField] protected GameObject lockObj;
    [SerializeField] protected GameObject doneObj;

    [SerializeField] protected RewardElement rewardElement;
    [SerializeField] protected Text indexRewardTxt;


    public void Init(bool isCurrentReward, bool isClaimed, RewardRandom reward, int numRequire, int indexReward)
    {
        otherFarmeObj.gameObject.SetActive(false);
        currentFarmeObj.gameObject.SetActive(false);
        fxcurrentFarme.gameObject.SetActive(false);
        lockObj.gameObject.SetActive(false);
        doneObj.gameObject.SetActive(false);

        if (isCurrentReward)
        {
            currentFarmeObj.gameObject.SetActive(true);
            fxcurrentFarme.gameObject.SetActive(true);
        }   
        else
        {
            otherFarmeObj.gameObject.SetActive(true);

            if (isClaimed)
            {
                doneObj.gameObject.SetActive(true);
            }
            else
            {
                lockObj.gameObject.SetActive(true);
            }
        }

       
        indexRewardTxt.text = indexReward.ToString();
        rewardElement.Init
           (
           GameController.Instance.dataContain.giftDatabase.GetIconItem(reward.typeItem),
           reward.amount,
           isUnlimitedItem: reward.isUnlimited,
           isX2Item: reward.typeItem != GiftType.X2WeeklyChallgeneItem ? false : true,
           isAnim: false
          );
    }
}
