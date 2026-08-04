using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DailyLoginElement : MonoBehaviour
{
    [SerializeField] protected GameObject body;
    [SerializeField] protected Image iconImg;
    [SerializeField] protected GameObject unlimitedObj;
    [SerializeField] protected Text amountTxt;
    [SerializeField] protected Button claimBtn;
    [SerializeField] protected GameObject firstObj;
    [SerializeField] protected GameObject claimedObj;
    [SerializeField] protected GameObject canClaimObj;
    [SerializeField] protected Text dayTxt;
    [SerializeField] protected Color dayTxtPreClaim;
    [SerializeField] protected Color dayTxtMidClaim;
    [SerializeField] protected Color dayTxtAftClaim;
    protected DailyLoginItem data;
    protected int indexDay;

    public void Init(int indexDay)
    {
        this.data = BirdSortGameController.Instance.dataContain.dailyLoginDatabase.rewardDays[indexDay];
        this.indexDay = indexDay;

        claimBtn.onClick.AddListener(OnClickClaim);
    }

    public void InitState()
    {
        firstObj.gameObject.SetActive(true);
        claimedObj.gameObject.SetActive(false);
        canClaimObj.gameObject.SetActive(false);
        claimBtn.interactable = false;
        dayTxt.color = dayTxtPreClaim;
        body.transform.DOKill();
        body.transform.localScale = Vector3.one;

        if (!data.isChest)
        {
            iconImg.sprite = BirdSortGameController.Instance.dataContain.giftDatabase.GetIconItem(data.rewards[0].typeItem);
            
            if(data.rewards[0].isUnlimited)
            {
                unlimitedObj.gameObject.SetActive(true);
                amountTxt.text = TimeManager.ShowTime4(data.rewards[0].amount);
            }   
            else
            {
                unlimitedObj.gameObject.SetActive(false);
                amountTxt.text = data.rewards[0].amount.ToString();
            }    
                
        }

        if (data.id < DailyLoginController.CurrentReward)
        {
            claimedObj.gameObject.SetActive(true);
            firstObj.gameObject.SetActive(false);
            dayTxt.color = dayTxtAftClaim;
        }
        else if (data.id == DailyLoginController.CurrentReward)
        {
            bool isHasReward = HomeController.Instance.dailyLoginController.CheckRewardLogin();
            if(isHasReward)
            {
                if (DailyLoginController.IsClaimRewardToDay)
                {
                    claimedObj.gameObject.SetActive(true);
                    firstObj.gameObject.SetActive(false);
                    dayTxt.color = dayTxtAftClaim;
                }
                else
                {
                    canClaimObj.gameObject.SetActive(true);
                    claimBtn.interactable = true;
                    dayTxt.color = dayTxtMidClaim;
                    body.transform.DOScale(1.05f, 0.7f).SetLoops(-1, LoopType.Yoyo);
                    firstObj.gameObject.SetActive(false);
                }
            } 
        }

        dayTxt.text = string.Format(Localization.Get("s_day") , (this.indexDay + DailyLoginController.CurrentWeekReward * 7 + 1));
       
    }

    private void OnClickClaim()
    {
        if (!data.isChest)
        {
            BirdSortGameController.Instance.dataContain.giftDatabase.Claim(data.rewards[0].typeItem, data.rewards[0].amount, Reason.daily_login, isUnlimited: data.rewards[0].isUnlimited);

            //if(!data.rewards[0].isUnlimited)
            //{
            //    BirdSortGameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
            //  body.transform.position,
            //  data.rewards[0].typeItem,
            // data.rewards[0].amount,
            //   Color.white
            //  );
            //}    
            var config = BirdSortGameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.dailyLoginConfig;
            GPlayFuntPuzzle.GPopupRewardNormal.Setup().Show(
                new List<RewardRandom>()
                {
                    new RewardRandom() { typeItem = data.rewards[0].typeItem, amount = data.rewards[0].amount, isUnlimited = data.rewards[0].isUnlimited }

                },
                Reason.daily_login,
                isHasVideo: config.is_video_x_reward,
                numXReward: config.num_x_reward);

        }
        else
        {
            //M? h�m
            List<RewardRandom> rewardsShow = new List<RewardRandom>();
            for (int i = 0; i < data.rewards.Length; i++)
            {
                int index = i;
                BirdSortGameController.Instance.dataContain.giftDatabase.Claim(data.rewards[index].typeItem, data.rewards[index].amount, Reason.daily_login, isUnlimited: data.rewards[index].isUnlimited);
                rewardsShow.Add(data.rewards[index]);
            }

            var config = BirdSortGameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.dailyLoginConfig;
            GPlayFuntPuzzle.GPopupRewardChest.Setup().Show(TypeChest.DailyLoginChest, rewardsShow, Reason.daily_login, config.is_video_x_reward, config.num_x_reward);
            //OpenChestBox.Setup().Show(TypeChest.DailyLoginChest, rewardsShow);
        }
        Debug.Log("=============== Daily Claim");
        HomeController.Instance.dailyLoginController.ClaimReward();
        InitState();
        BirdSortGameController.Instance.musicManager.PlayClickSound();
    }
}
