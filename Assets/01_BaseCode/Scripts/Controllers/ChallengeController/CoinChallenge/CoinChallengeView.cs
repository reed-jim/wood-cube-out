using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CoinChallengeView : ChallengeViewBase
{
    public GameObject iconHomeObj;

    public override void Init()
    {
        base.Init();
        if (controller.data.RememberCurrentNumItem > 0)
        {
            int indexRewardBefore = controller.data.CurrentIndexReward - 1 > 0 ? controller.data.CurrentIndexReward - 1 : 0;
            int currentProgessShow = controller.data.CurrentIndexReward > 0 ? Mathf.Abs(controller.data.RememberCurrentNumItem - (int)controller.config.parmaExtral[indexRewardBefore]) : controller.data.RememberCurrentNumItem;
            //currentProgessShow = CurrentIndexReward - yêu cầu của mốc trước đó

            int targetShow = Mathf.Abs(controller.CurrentItemRequirement - (int)controller.config.parmaExtral[indexRewardBefore]);
            if (targetShow == 0)
                targetShow = (int)controller.config.parmaExtral[indexRewardBefore];
            //if (currentProgessShow == 0)
            //    currentProgessShow = targetShow;

            if (progessTxt != null)
            {
                progessTxt.text = currentProgessShow + "/" + targetShow;
            }
            SetProgessBarImg((float)currentProgessShow / targetShow, false);
        }
        else
        {
            int currentProgessShow = controller.data.RememberCurrentNumItem;
            int targetShow = (int)controller.config.parmaExtral[0];
            if (targetShow == 0)
                targetShow = 1;

            if (progessTxt != null)
            {
                progessTxt.text = currentProgessShow + "/" + targetShow;
            }
            SetProgessBarImg((float)currentProgessShow / targetShow, false);
        }
    }

    public override void InitState()
    {
        if (!controller.IsChallengeActive())
        {
            iconHomeObj.SetActive(false);
            return;
        }
        else
        {
            iconHomeObj.SetActive(true);
        }


        if (controller.data.RememberCurrentNumItem != controller.data.CurrentNumItem)
        {
            var prevItem = controller.data.RememberCurrentNumItem;
            var currentItem = controller.data.CurrentNumItem;

            Action actionStar = () =>
            {
                starClaim.ShowStar(
                    posStarSpawn.position,
                    iconAnim.transform.position,
                    Mathf.Abs(currentItem - prevItem),
                    actionStarDone: () =>
                    {
                        int indexRewardBefore = controller.data.CurrentIndexReward - 1 > 0 ? controller.data.CurrentIndexReward - 1 : 0;
                        int currentProgessShow = controller.data.CurrentIndexReward > 0 ? Mathf.Abs(controller.data.CurrentNumItem - (int)controller.config.parmaExtral[indexRewardBefore]) : controller.data.CurrentNumItem;
                        //currentProgessShow = CurrentIndexReward - yêu cầu của mốc trước đó

                        int targetShow = Mathf.Abs(controller.CurrentItemRequirement - (int)controller.config.parmaExtral[indexRewardBefore]);
                        if (targetShow == 0)
                            targetShow = (int)controller.config.parmaExtral[indexRewardBefore];
                        float currentProgess = (float)currentProgessShow / targetShow;

                        progessImg.DOFillAmount(currentProgess, 0.3f).OnComplete(() =>
                        {
                            controller.actionOpenBox?.Invoke();
                        });
                    });
            };
            if (HomeController.Instance.suggetPopupHomeController.IsHasSuperStarLegendaryShow)
            {
                StartCoroutine(Helper.StartAction(() => { actionStar.Invoke(); },
                    () => HomeController.Instance.suggetPopupHomeController.IsHasSuperStarLegendaryShow == false));
            }
            else
            {
                actionStar.Invoke();
            }
        }
        // controller.data.CurrentNumItem += controller.data.RememberCurrentNumItem;
    }


    public override void OnOpenBoxDone()
    {
        if (isHasProgess)
        {
            if (controller.IsClaimAllReward)
            {
                SetProgessBarImg(1);
                if (progessTxt != null)
                {
                    progessTxt.text = Localization.Get("s_done");
                }
            }
            else
            {
                if (controller.data.CurrentNumItem > 0)
                {
                    int indexRewardReal = controller.data.CurrentIndexReward - 1 > 0 ? controller.data.CurrentIndexReward - 1 : 0;
                    int currentProgessShow = Mathf.Abs(controller.data.CurrentNumItem - (int)controller.config.parmaExtral[indexRewardReal]);
                    int targetShow = Mathf.Abs(controller.CurrentItemRequirement - (int)controller.config.parmaExtral[indexRewardReal]);
                    if (targetShow == 0)
                        targetShow = (int)controller.config.parmaExtral[indexRewardReal];

                    if (progessTxt != null)
                    {
                        progessTxt.text = currentProgessShow + "/" + targetShow;
                    }
                    SetProgessBarImg((float)currentProgessShow / targetShow);
                }
                else
                {
                    int currentProgessShow = controller.data.CurrentNumItem;
                    int targetShow = (int)controller.config.parmaExtral[0];

                    if (progessTxt != null)
                    {
                        progessTxt.text = currentProgessShow + "/" + targetShow;
                    }
                    SetProgessBarImg((float)currentProgessShow / targetShow);
                }
            }
        }
    }
}