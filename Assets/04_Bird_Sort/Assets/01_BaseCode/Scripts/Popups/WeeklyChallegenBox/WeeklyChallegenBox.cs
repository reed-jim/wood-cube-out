using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using static ChallengeSegemtData;
using DG.Tweening;

public class WeeklyChallegenBox : BaseBox
{
    private static WeeklyChallegenBox instance;

    public static WeeklyChallegenBox Setup(bool isSaveBox = false, bool isPostEventPopupClose = false, UnityAction actionOpenBoxSave = null, Action<BaseBox> OnLoadedFirst = null, Action<BaseBox> OnLoaded = null)
    {
        if (instance == null)
        {
            BoxController.Instance.GetBoxAsync(
                instance,
                (m_instance) =>
                {
                    instance = (WeeklyChallegenBox)m_instance;
                    instance.Show();
                    instance.Init();
                    instance.InitState();
                    if (OnLoadedFirst != null)
                        OnLoadedFirst(instance);

                    instance.isPostEventPopupClose = isPostEventPopupClose;
                },
                (instance) =>
                {
                    instance.Show();
                    instance.InitBoxSave(isSaveBox, actionOpenBoxSave);
                    if (OnLoaded != null)
                        OnLoaded(instance);

                    instance.isPostEventPopupClose = isPostEventPopupClose;
                },
                PathPrefabs.WEEKLY_CHALLEGEN_BOX);
        }
        else
        {
            instance.Show();
            instance.InitBoxSave(isSaveBox, actionOpenBoxSave);
            if (OnLoaded != null)
            {
                OnLoaded(instance);
                // instance.InitState();
            }

            instance.InitState();
            instance.isPostEventPopupClose = isPostEventPopupClose;
        }

        return instance;
    }

    [Header("Top")]
    [SerializeField] protected Text progessTxt;
    [SerializeField] protected Image progessImg;
    [SerializeField] protected Text timeLeftTxt;
    [SerializeField] protected GameObject doneChallengeObj;
    [SerializeField] protected RewardElement rewardElement;
    [SerializeField] protected GameObject coolDownObj;
    [SerializeField] protected Button infoBtn;

    [Header("Grand Prize")]
    [SerializeField] protected RewardElement grandPrize;

    [Header("Rewards")]
    [SerializeField] private RewardElementWeeklyChallenge rewardElementPrefab;
    [SerializeField] private Transform contentRewards;
    private ChallengeConfigData config;

    private IEnumerator moveScrollToCurrentRewardCroutine;
    [SerializeField] private RectTransform contentRewardRect;
    private ChallengeBase challengeController;
    private bool isCoolDownEnd;
    private float timerCoolDownEnd;

    public void Init()
    {
        challengeController = GameController.Instance.challengeController.GetChallenges(ChallengeType.WeeklyChallenge);
        config = challengeController.config;

        for (int i = config.rewards.Count - 1; i >= 0; i--)
        {
            int index = i;
            var rewardElement = Instantiate<RewardElementWeeklyChallenge>(rewardElementPrefab, contentRewards);
            rewardElement.Init(
                isCurrentReward: challengeController.data.CurrentIndexReward != index ? false : true,
                isClaimed: challengeController.data.CurrentIndexReward > index ? true : false,
                reward: config.rewards[index].GetReward(),
                numRequire: config.rewards[index].numItemRequire,
                index + 1
                );
        }

        infoBtn.onClick.AddListener(OpenInfoBox);
    }

    public void InitState()
    {
        doneChallengeObj.gameObject.SetActive(false);
        int indexCurrentReward = 0;
        for (int i = config.rewards.Count - 1; i >= 0; i--)
        {
            int index = i;
            bool isCurrentReward = challengeController.data.CurrentIndexReward != index ? false : true;

            if (isCurrentReward)
            {
                if (moveScrollToCurrentRewardCroutine != null)
                    StopCoroutine(moveScrollToCurrentRewardCroutine);

                moveScrollToCurrentRewardCroutine = MoveScrollToCurrentReward(index);
                StartCoroutine(moveScrollToCurrentRewardCroutine);
                indexCurrentReward = index;
                break;
            }
        }

        int currentProgessShow = 0;
        int targetShow = 1;
        if (challengeController.data.RememberCurrentNumItem > 0)
        {
             int indexRewardBefore = challengeController.data.CurrentIndexReward - 1 > 0 ? challengeController.data.CurrentIndexReward - 1 : 0;
             currentProgessShow = challengeController.data.CurrentIndexReward > 0 ? Mathf.Abs(challengeController.data.RememberCurrentNumItem - challengeController.config.rewards[indexRewardBefore].numItemRequire) : challengeController.data.RememberCurrentNumItem;
            //currentProgessShow = CurrentIndexReward - yêu cầu của mốc trước đó
             targetShow = Mathf.Abs(challengeController.CurrentItemRequirement - challengeController.config.rewards[indexRewardBefore].numItemRequire);
            if (targetShow == 0)
                targetShow = challengeController.config.rewards[indexRewardBefore].numItemRequire;
            //if (currentProgessShow == 0)
            //    currentProgessShow = targetShow;

           
        }
        else
        {
             currentProgessShow = challengeController.data.RememberCurrentNumItem;
             targetShow = challengeController.config.rewards[0].numItemRequire;
        }

        progessTxt.text = currentProgessShow + "/" + targetShow;
        progessImg.fillAmount = (float)currentProgessShow / targetShow;

        var reward = config.rewards[indexCurrentReward].GetReward();
        rewardElement.gameObject.SetActive(true);
        rewardElement.Init
               (
               GameController.Instance.dataContain.giftDatabase.GetIconItem(reward.typeItem),
               reward.amount,
               isUnlimitedItem: reward.isUnlimited,
               isX2Item: reward.typeItem != GiftType.X2WeeklyChallgeneItem ? false : true,
               isAnim: false
              );

        timerCoolDownEnd = TimeManager.CaculateTime(UnbiasedTime.Instance.Now(), challengeController.data.TimeEndChallenge);
        if (timerCoolDownEnd <= 0)
        {
            isCoolDownEnd = false;
        }
        else
        {
            isCoolDownEnd = true;
        }

        var rewardEnd = config.rewards[config.rewards.Count - 1].GetReward();
        grandPrize.Init
               (
               GameController.Instance.dataContain.giftDatabase.GetIconItem(rewardEnd.typeItem),
               rewardEnd.amount,
               isUnlimitedItem: rewardEnd.isUnlimited,
               isX2Item: rewardEnd.typeItem != GiftType.X2WeeklyChallgeneItem ? false : true,
               isAnim: false
              );

        var currentReward = ((WeeklyChallegen)this.challengeController).GetCurrentRewardData();
        if (currentReward == null)
        {
            rewardElement.gameObject.SetActive(false);
            progessTxt.text = Localization.Get("s_complete");
            doneChallengeObj.gameObject.SetActive(true);
            progessImg.fillAmount = 1;
            coolDownObj.gameObject.SetActive(true);
            return;
        }
    }

    private  IEnumerator MoveScrollToCurrentReward(int index)
    {
        contentRewardRect.DOAnchorPosY(3735, 0.03f);
        yield return new WaitForSeconds(0.3f);
        contentRewardRect.DOKill();
        contentRewardRect.DOAnchorPosY(3735 - index * 125, 0.2f).SetEase(Ease.Linear);
    }

    protected override void DoClose()
    {
        if (isAnim)
        {
            if (canvasGruop != null)
            {
                canvasGruop.DOKill();
                canvasGruop.alpha = 1;
                mainPanel.DOScale(0, 0.5f).SetUpdate(true).SetEase(Ease.InBack).OnComplete(() =>
                {

                    this.gameObject.SetActive(false);
                });
                //canvasGruop.DOFade(0, 0.1f);
            }
            else
            {
                this.gameObject.SetActive(false);
            }
        }
        else
        {
            this.gameObject.SetActive(false);
        }

        if (canvasGruop != null)
        {
            canvasGruop.interactable = false;
        }
        //this.gameObject.SetActive(false);
        if (!isPopup)
        {
            if (canvasGroupPanel != null)
                canvasGroupPanel.blocksRaycasts = true;
        }
    }
    protected virtual void CoolDown()
    {
        if (isCoolDownEnd)
        {
            timerCoolDownEnd -= Time.unscaledDeltaTime;
            timeLeftTxt.text = TimeManager.ShowTime6((long)timerCoolDownEnd);

            if (timerCoolDownEnd <= 0)
            {
                isCoolDownEnd = false;
            }
        }
    }

    private void OpenInfoBox()
    {
        WeeklyChallegenTutBox.Setup().Show();
    }    

    private void Update()
    {
        CoolDown();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (moveScrollToCurrentRewardCroutine != null)
            StopCoroutine(moveScrollToCurrentRewardCroutine);
    }
}
