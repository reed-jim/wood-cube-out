using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using static ChallengeSegemtData;

public class CoinChallengeBox : BaseBox
{
    private static CoinChallengeBox instance;

    public static CoinChallengeBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<CoinChallengeBox>(PathPrefabs.COIN_CHALLENGE_BOX));
            instance.Init();
        }

        instance.InitState();
        instance.isPostEventPopupClose = isPostEventPopupClose;
        return instance;
    }

 

    [SerializeField] private Button continueBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Text timeLeftText;

    [SerializeField] protected Text progressTxt;
    [SerializeField] protected Slider progress;
    [SerializeField] protected GameObject doneChallengeObj;
    [SerializeField] protected GameObject coolDownObj;

    private ChallengeBase challengeController;
    private ChallengeConfigData config;

    private LevelConfig _levelConfig;

    private void Update()
    {
        if (challengeController != null)
        {
            timeLeftText.text = TimeManager.ShowTime6((long)challengeController.TimeLeft);
        }
    }

    private void Init()
    {
        closeBtn.onClick.AddListener(Close);
        continueBtn.onClick.AddListener(OnClickPlay);

        challengeController = GameController.Instance.challengeController.GetChallenges(ChallengeType.CoinChallenge);
        config = challengeController.config;
    }

    private void InitState()
    {
        doneChallengeObj.gameObject.SetActive(false);
        if (challengeController.data.CurrentIndexReward >= challengeController.config.parmaExtral.Length)
        {
            doneChallengeObj.gameObject.SetActive(true);
            return;
        }
        int currentProgessShow = 0;
        int targetShow = 1;
        if (challengeController.data.RememberCurrentNumItem > 0)
        {
            int indexRewardBefore = challengeController.data.CurrentIndexReward - 1 > 0 ? challengeController.data.CurrentIndexReward - 1 : 0;
            currentProgessShow = challengeController.data.CurrentIndexReward > 0 ? Mathf.Abs(challengeController.data.RememberCurrentNumItem - (int)challengeController.config.parmaExtral[indexRewardBefore])
                : challengeController.data.RememberCurrentNumItem;
            //currentProgessShow = CurrentIndexReward - yêu cầu của mốc trước đó
            targetShow = Mathf.Abs(challengeController.CurrentItemRequirement - (int)challengeController.config.parmaExtral[indexRewardBefore]);
            if (targetShow == 0)
                targetShow = (int)challengeController.config.parmaExtral[indexRewardBefore];
        }
        else
        {
            currentProgessShow = challengeController.data.RememberCurrentNumItem;
            targetShow = (int)challengeController.config.parmaExtral[0];
        }

        progressTxt.text = currentProgessShow + "/" + targetShow;
        progress.value = (float)currentProgessShow / targetShow;
    }

    protected override void DoClose()
    {
        if (isAnim)
        {
            if (canvasGruop != null)
            {
                canvasGruop.DOKill();
                canvasGruop.alpha = 1;

                canvasGruop.DOFade(0, 0.1f).OnComplete(() =>
                {
                    if (isPostEventPopupClose)
                        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CLOSE_POPUP);
                    this.gameObject.SetActive(false);
                });
            }
            else
            {
                if (isPostEventPopupClose)
                    EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CLOSE_POPUP);
                this.gameObject.SetActive(false);
            }
        }
        else
        {
            if (isPostEventPopupClose)
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CLOSE_POPUP);
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

    private void OnClickPlay()
    {
        Close();
        GameController.Instance.musicManager.PlayClickSound();
    }
}