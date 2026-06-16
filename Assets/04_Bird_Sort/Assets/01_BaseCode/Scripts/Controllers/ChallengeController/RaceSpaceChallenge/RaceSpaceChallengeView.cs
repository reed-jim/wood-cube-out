using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class RaceSpaceChallengeView : ChallengeViewBase
{
    [SerializeField] private GameObject rankMeObj;
    [SerializeField] private Text rankMeTxt;

    public override void Init()
    {
        starClaim.Init();
        SetProgessBarImg((float)((RaceSpaceChallenge)controller).playerRace.RememberCurrentLevelPass / RaceSpaceChallenge.NumLevelPassToWinEvent, false);
        challengeBtn.onClick.AddListener(OnClickOpenBox);

        if (!controller.IsChallengeActive())
        {
            if (iconObj != null)
                iconObj.gameObject.SetActive(false);
        }
        else
        {
            if (iconObj != null)
                iconObj.gameObject.SetActive(true);
        }

        if (RaceSpaceChallenge.IsRacing)
        {
            rankMeObj.SetActive(true);
        }
        else
        {
            rankMeObj.SetActive(false);
        }
    }

    protected override void ShowProgess()
    {
        if (RaceSpaceChallenge.IsRacing)
        {
            rankMeObj.SetActive(true);
            rankMeTxt.text = (((RaceSpaceChallenge)controller).GetRankMe() + 1).ToString();
        }
        else
        {
            rankMeObj.SetActive(false);
        }

        var control = (RaceSpaceChallenge)controller;
        if (control.playerRace.RememberCurrentLevelPass != control.playerRace.CurrentLevelPass)
        {
            int turnShow = Mathf.Abs(control.playerRace.CurrentLevelPass - control.playerRace.RememberCurrentLevelPass);
            UnityAction actionProgess = () =>
            {
                starClaim.ShowStar(
                posStarSpawn.position,
                iconAnim.transform.position,
                turnShow,
                actionStarDone: () =>
                {
                    float currentProgess =
                    (float)control.playerRace.CurrentLevelPass
                    / RaceSpaceChallenge.NumLevelPassToWinEvent;
                    progessImg.DOFillAmount(currentProgess, 0.3f).OnComplete(() =>
                    {
                        controller.actionOpenBox?.Invoke();
                    });
                });

                if (control.playerRace.CurrentLevelPass < RaceSpaceChallenge.NumLevelPassToWinEvent)
                {
                    control.playerRace.RememberCurrentLevelPass = control.playerRace.CurrentLevelPass;
                }
            };

            if (HomeController.Instance.suggetPopupHomeController.IsHasSuperStarLegendaryShow)
            {
                StartCoroutine(Helper.StartAction(() =>
                {
                    actionProgess.Invoke();
                }, () => HomeController.Instance.suggetPopupHomeController.IsHasSuperStarLegendaryShow == false));

            }
            else
            {
                actionProgess.Invoke();
            }
            return;
        }
        
        if(control.IsHasMemberFinishRace())
        {
            if (HomeController.Instance.suggetPopupHomeController.IsHasSuperStarLegendaryShow)
            {
                StartCoroutine(Helper.StartAction(() =>
                {
                    controller.actionOpenBox?.Invoke();
                }, () => HomeController.Instance.suggetPopupHomeController.IsHasSuperStarLegendaryShow == false));

            }
            else
            {
                controller.actionOpenBox?.Invoke();
            }
            return;
        }   
    }

    protected override void OnClickOpenBox()
    {
        if (RaceSpaceChallenge.IsRacing)
        {
            controller.OpenBox();
            GameController.Instance.musicManager.PlayClickSound();
        }
        else
        {
            JoinSpaceRaceBox.Setup().Show();
        }
    }

    public void ResetRace()
    {
        rankMeObj.SetActive(false);
        progessImg.fillAmount = 0;
    }
}
