using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static ChallengeSegemtData;
using UnityEngine.Events;
using UnityEngine.UI;
public class WeeklyChallegenView : ChallengeViewBase
{
    public Transform eggPosProgess;
    public Transform posStartReward;
    public Transform posShowClaimReward;
    public RewardHomeWeekly currentRewardShow;
    [SerializeField] protected GameObject doneChallengeObj;

    [SerializeField] private GameObject lockObj;
    [SerializeField] private GameObject unlockObj;
    [SerializeField] private Text levelUnlockTxt;

    
    [SerializeField] private AudioClip eggComeClip;
    [SerializeField] private AudioSource eggComeSource;

    public override void Init()
    {
        base.Init();
    }

    public override void InitState()
    {
        doneChallengeObj.gameObject.SetActive(false);
        ShowCurrentRewards(false);
        if (!controller.IsChallengeActive())
        {
            if (UseProfile.CurrentLevel < controller.config.level_unlock)
            {
                lockObj.gameObject.SetActive(true);
                unlockObj.gameObject.SetActive(false);
                challengeBtn.interactable = false;
                levelUnlockTxt.text = string.Format(Localization.Get("s_unlock_at"), controller.config.level_unlock);
            }
            return;
        }
        else
        {
            lockObj.gameObject.SetActive(false);
            unlockObj.gameObject.SetActive(true);
            challengeBtn.interactable = true;
        }    
        ShowProgess();
       
    }

    public void ShowCurrentRewards(bool isAnim)
    {
        var currentReward = ((WeeklyChallegen)this.controller).GetCurrentRewardData();
        if (currentReward == null)
        {
            DoneWeeklyChallenge();
            return;
        }

        currentRewardShow.gameObject.SetActive(true);
        currentRewardShow.Init
            (
            GameController.Instance.dataContain.giftDatabase.GetIconItem(currentReward.typeItem),
            currentReward.amount,
            isUnlimitedItem: currentReward.isUnlimited,
            isX2Item: currentReward.typeItem != GiftType.X2WeeklyChallgeneItem ? false : true,
            isAnim: false
           );
    }


    protected override void SetProgessBarImg(float fillAmountValue, bool isAnim = true, float timeFill = 0.3f, UnityAction actionProgessDone = null)
    {
        if (fillAmountValue > 1)
            fillAmountValue = 1;
        if (fillAmountValue < 0)
            fillAmountValue = 0;

        if(isAnim)
        {
            progessImg.transform.DOKill();
            progessImg.transform.DOScaleX(fillAmountValue, 0.45f).OnComplete(()=> { actionProgessDone?.Invoke(); });
        }   
        else
        {
            progessImg.transform.localScale = new Vector3(fillAmountValue, progessImg.transform.localScale.y, progessImg.transform.localScale.z);
        }    
      
    }

    protected override void ShowProgess()
    {
        if (controller.data.RememberCurrentNumItem != controller.data.CurrentNumItem)
        {
            var itemFX = HomeController.Instance.homeScene.itemViewHome.GetItemFX();
            itemFX.transform.position = posStarSpawn.position;
            itemFX.Show(
                GiftType.WeeklyChallgeneItem,
                 Mathf.Abs(controller.data.CurrentNumItem - controller.data.RememberCurrentNumItem),
               eggPosProgess.transform.position,
                10,
                () =>
                {
                    int indexRewardBefore = controller.data.CurrentIndexReward - 1 > 0 ? controller.data.CurrentIndexReward - 1 : 0;
                    int currentProgessShow = controller.data.CurrentIndexReward > 0 ? Mathf.Abs(controller.data.CurrentNumItem - controller.config.rewards[indexRewardBefore].numItemRequire) : controller.data.CurrentNumItem;
                    //currentProgessShow = CurrentIndexReward - yêu cầu của mốc trước đó
                    int targetShow = Mathf.Abs(controller.CurrentItemRequirement - controller.config.rewards[indexRewardBefore].numItemRequire);
                    if (targetShow == 0)
                        targetShow = controller.config.rewards[indexRewardBefore].numItemRequire;
                    if (currentProgessShow == 0)
                        currentProgessShow = targetShow;

                    progessTxt.text = currentProgessShow + "/" + targetShow;
                    SetProgessBarImg((float)currentProgessShow / targetShow, true, actionProgessDone : ()=> 
                    {
                        controller.actionOpenBox?.Invoke();
                    });

                    GameController.Instance.musicManager.PlayOneShot(eggComeClip, eggComeSource);
                    var fx = GameController.Instance.fXController.GetFX(FXController.TypeFX.ClaimStarChestHome);
                    fx.transform.position = eggPosProgess.transform.position;

                }, isTime: false, destinationFollow: eggPosProgess.transform, timeDelay: 0, scale: 0.75f, isJump: true, timeJump: 0.7f);
        }

    }

    public void ShowClaimReward()
    {
        currentRewardShow.OnClaimWeeklyReward(posStartReward.transform.position, posShowClaimReward.position, () => { OnOpenBoxDone(); }, this);
    }


    public void DoneWeeklyChallenge()
    {
        currentRewardShow.gameObject.SetActive(false);
        progessTxt.text = Localization.Get("s_complete");
        doneChallengeObj.gameObject.SetActive(true);
        SetProgessBarImg(1, false);
    }    

    public override void OnOpenBoxDone()
    {
        progessImg.transform.localScale = new Vector3(0, progessImg.transform.localScale.y, progessImg.transform.localScale.z);
        base.OnOpenBoxDone();
        ShowCurrentRewards(true);
    }
}
