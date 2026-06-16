using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class RoyalPassView : ChallengeViewBase
{
    public GameObject notiObj;

    private Action<object> actionClaimedReward;

    public override void Init()
    {
        base.Init();
        actionClaimedReward = (s) => { notiObj.gameObject.SetActive(IsHasNoti()); };
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.CHECK_NOTI_CLAIM_REWARD_ROYAL_PASS, actionClaimedReward);
    }

    public override void InitState()
    {
        base.InitState();
        notiObj.gameObject.SetActive(IsHasNoti());
    }

    protected override void ShowProgess()
    {
        if (controller.data.RememberCurrentNumItem != controller.data.CurrentNumItem)
        {
            int rememberCurrentNumItem = controller.data.RememberCurrentNumItem;
            int currentNumItem = controller.data.CurrentNumItem;

            UnityAction actionStar = () =>
            {
                starClaim.ShowStar(
                    posStarSpawn.position,
                    iconAnim.transform.position,
                    Mathf.Abs(currentNumItem - rememberCurrentNumItem),
                    actionStarDone: () =>
                    {
                        if (isHasProgess)
                        {
                            int indexRewardBefore = controller.data.CurrentIndexReward - 1 > 0 ? controller.data.CurrentIndexReward - 1 : 0;
                            int currentProgessShow = controller.data.CurrentIndexReward > 0 ? Mathf.Abs(controller.data.CurrentNumItem - controller.config.rewards[indexRewardBefore].numItemRequire) : controller.data.CurrentNumItem;
                            //currentProgessShow = CurrentIndexReward - yêu cầu của mốc trước đó
                            int targetShow = Mathf.Abs(controller.CurrentItemRequirement - controller.config.rewards[indexRewardBefore].numItemRequire);
                            if (targetShow == 0)
                                targetShow = controller.config.rewards[indexRewardBefore].numItemRequire;
                            if (currentProgessShow == 0)
                                currentProgessShow = targetShow;

                            SetProgessBarImg((float)currentProgessShow / targetShow, true, actionProgessDone: () =>
                            {
                                controller.actionOpenBox?.Invoke();
                                notiObj.gameObject.SetActive(IsHasNoti());
                            });
                        }
                        else
                        {
                            controller.actionOpenBox?.Invoke();
                            notiObj.gameObject.SetActive(IsHasNoti());
                        }
                    });
            };

            if (HomeController.Instance.suggetPopupHomeController.IsHasSuperStarLegendaryShow)
            {
                StartCoroutine(Helper.StartAction(() =>
                {
                    actionStar.Invoke();
                }, () => HomeController.Instance.suggetPopupHomeController.IsHasSuperStarLegendaryShow == false));

            }
            else
            {
                actionStar.Invoke();
            }
        }
    }

    public override void OnOpenBoxDone()
    {
        base.OnOpenBoxDone();
        notiObj.gameObject.SetActive(IsHasNoti());
    }    

    protected bool IsHasNoti()
    {
        for (int i = 0; i <= controller.data.CurrentIndexReward - 1; i++)
        {
            if(!RoyalPassChallenge.IsClaimedReward(i, 0))
            {
                return true;
            }    
        }
        if (RoyalPassChallenge.IsActiveRoyalPack)
        {
            for (int i = 0; i <= controller.data.CurrentIndexReward - 1; i++)
            {
                if (!RoyalPassChallenge.IsClaimedReward(i, 1))
                {
                    return true;
                }
            }
        }

        return false;
    }    

    private void OnDestroy()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.CHECK_NOTI_CLAIM_REWARD_ROYAL_PASS, actionClaimedReward);
    }    
}
