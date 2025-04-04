using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class ChallengeViewBase : MonoBehaviour
{
    public ChallengeType typeChallenge;
    [SerializeField] protected GameObject iconObj;
    [SerializeField] protected Transform posStarSpawn;
    [SerializeField] protected GameObject iconAnim;
    [SerializeField] protected StartChestEffect starClaim;
    [SerializeField] protected Text progessTxt;
    [SerializeField] protected Image progessImg;
    [SerializeField] protected Button challengeBtn;
    [SerializeField] protected Text timeLeftTxt;
    [SerializeField] protected bool isHasProgess;
    [HideInInspector] public ChallengeBase controller;


    public void AwakeHomeSetup()
    {
        this.controller = GameController.Instance.challengeController.GetChallenges(typeChallenge);
        this.controller.view = this;
    }

    public virtual void Init()
    {
        starClaim.Init();
        if (isHasProgess)
        {
            if (controller.data.RememberCurrentNumItem > 0)
            {
                int indexRewardBefore = controller.data.CurrentIndexReward - 1 > 0 ? controller.data.CurrentIndexReward - 1 : 0;
                int currentProgessShow = controller.data.CurrentIndexReward > 0 ? Mathf.Abs(controller.data.RememberCurrentNumItem - controller.config.rewards[indexRewardBefore].numItemRequire) : controller.data.RememberCurrentNumItem;
                //currentProgessShow = CurrentIndexReward - yêu cầu của mốc trước đó
               
                int targetShow = Mathf.Abs(controller.CurrentItemRequirement - controller.config.rewards[indexRewardBefore].numItemRequire);
                if (targetShow == 0)
                    targetShow = controller.config.rewards[indexRewardBefore].numItemRequire;
                //if (currentProgessShow == 0)
                //    currentProgessShow = targetShow;

                progessTxt.text = currentProgessShow + "/" + targetShow;
                SetProgessBarImg((float)currentProgessShow / targetShow, false);
            }
            else
            {
                int currentProgessShow = controller.data.RememberCurrentNumItem;
                int targetShow = controller.config.rewards[0].numItemRequire;

                progessTxt.text = currentProgessShow + "/" + targetShow;
                SetProgessBarImg((float)currentProgessShow / targetShow, false);
            }
        }
        challengeBtn.onClick.AddListener(OnClickOpenBox);
        Debug.Log("Tatbatchanlange");
        if (controller.IsChallengeActive())
        {
            iconObj.gameObject.SetActive(false);
        }
        else
        {
            iconObj.gameObject.SetActive(true);
        }
    }

    public virtual void InitState()
    {
        if (!controller.IsChallengeActive())
            return;

        ShowProgess();
    }

    protected virtual void ShowProgess()
    {
        if (controller.data.RememberCurrentNumItem != controller.data.CurrentNumItem)
        {
            starClaim.ShowStar(
                posStarSpawn.position,
                iconAnim.transform.position,
                Mathf.Abs(controller.data.CurrentNumItem - controller.data.RememberCurrentNumItem),
                actionStarDone: () =>
                {
                    if (isHasProgess)
                    {
                        float currentProgess =
                        (float)controller.data.CurrentNumItem
                        / controller.CurrentItemRequirement;
                        progessImg.DOFillAmount(currentProgess, 0.3f).OnComplete(() =>
                        {
                            controller.actionOpenBox?.Invoke();
                        });
                    }
                    else
                    {
                        controller.actionOpenBox?.Invoke();
                    }
                });
        }
    }

    protected virtual void SetProgessBarImg(float fillAmountValue, bool isAnim = true, float timeFill = 0.3f, UnityAction actionProgessDone = null)
    {
        if (fillAmountValue > 1)
            fillAmountValue = 1;
        if (fillAmountValue < 0)
            fillAmountValue = 0;

        if (isAnim)
        {
            progessImg.DOFillAmount(fillAmountValue, timeFill).OnComplete(() => { actionProgessDone?.Invoke(); });
        }
        else
        {
            progessImg.fillAmount = fillAmountValue;
        }

    }

    public virtual void OnOpenBoxDone()
    {
        if (isHasProgess)
        {
            if (controller.IsClaimAllReward)
            {
                SetProgessBarImg(1);
                progessTxt.text = Localization.Get("s_done");
            }
            else
            {
                if (controller.data.CurrentNumItem > 0)
                {
                    int indexRewardReal = controller.data.CurrentIndexReward - 1 > 0 ? controller.data.CurrentIndexReward - 1 : 0;
                    int currentProgessShow = Mathf.Abs(controller.data.CurrentNumItem - controller.config.rewards[indexRewardReal].numItemRequire);
                    int targetShow = Mathf.Abs(controller.CurrentItemRequirement - controller.config.rewards[indexRewardReal].numItemRequire);
                    if (targetShow == 0)
                        targetShow = controller.config.rewards[indexRewardReal].numItemRequire;

                    progessTxt.text = currentProgessShow + "/" + targetShow;
                    SetProgessBarImg((float)currentProgessShow / targetShow);
                }
                else
                {
                    int currentProgessShow = controller.data.CurrentNumItem;
                    int targetShow = controller.config.rewards[0].numItemRequire;

                    progessTxt.text = currentProgessShow + "/" + targetShow;
                    SetProgessBarImg((float)currentProgessShow / targetShow);
                }
            }
        }
    }


    public void ShowCoolDownEnd(float time)
    {
        timeLeftTxt.text = TimeManager.ShowTime6((long)time);
    }

    protected virtual void OnClickOpenBox()
    {
        controller.OpenBox();
    }
}
