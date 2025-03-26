using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using static ChallengeSegemtData;

[System.Serializable]
public class RangeMinMax
{
    public float min;
    public float max;
}    

public class DiamodChallengeBox : BaseBox
{
    private static DiamodChallengeBox instance;

    public static DiamodChallengeBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<DiamodChallengeBox>(PathPrefabs.DIAMOD_CHALLENGE_BOX));
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
    [SerializeField] protected Slider progressImg;
    [SerializeField] protected GameObject[] doneChestsObj;
    [SerializeField] protected GameObject coolDownObj;

   
    private ChallengeBase challengeController;
    private int totalItemRequire;

    [SerializeField] private List<RangeMinMax> rangeProgess;

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

        challengeController = GameController.Instance.challengeController.GetChallenges(ChallengeType.DiamodChallenge);

        for (int i = 0; i < challengeController.config.parmaExtral.Length; i++)
        {
            totalItemRequire += (int)challengeController.config.parmaExtral[i];
        }
        
    }

    private void InitState()
    {
        if (challengeController.data.CurrentIndexReward >= challengeController.config.parmaExtral.Length)
        {
            progressImg.value = 1;
            for (int i = 0; i < doneChestsObj.Length; i++)
            {
                doneChestsObj[i].gameObject.SetActive(true);
            }
            progressTxt.text = Localization.Get("s_complete");
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

        int currentIndexReward = challengeController.data.CurrentIndexReward;
        progressTxt.text = string.Format(Localization.Get("s_progess_diamod_challenge"), currentProgessShow, targetShow);
        progressImg.value =  BezierCurves.GetAmount_LinearIntrepolation(rangeProgess[currentIndexReward].min, rangeProgess[currentIndexReward].max, ((float)currentProgessShow / targetShow));

        for (int i = 0; i < currentIndexReward; i++)
        {
            doneChestsObj[i].gameObject.SetActive(true);
        }
    }

    protected override void DoClose()
    {
        if (isAnim)
        {
            if (canvasGruop != null)
            {
                canvasGruop.DOKill();
                canvasGruop.alpha = 1;

                canvasGruop.DOFade(0, 0.1f).OnComplete(() => { this.gameObject.SetActive(false); });
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

    private void OnClickPlay()
    {
        Close();
        GameController.Instance.musicManager.PlayClickSound();
    }
}