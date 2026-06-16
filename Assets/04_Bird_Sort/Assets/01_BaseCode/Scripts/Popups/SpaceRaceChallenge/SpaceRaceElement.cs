using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UniRx;

public class SpaceRaceElement : MonoBehaviour
{
    [System.Serializable]
    public class RewardUI
    {
        public Image rewardIcon;
        public Text rankTxt;
        public Image farmeRank;
    }

    [System.Serializable]
    public class Ship
    {
        public Text levelPassTxt;
        public GameObject shipBody;
    }

    public int ID;
    [SerializeField] protected RewardUI rewardsUI;
    [SerializeField] protected Ship ships;
    [SerializeField] protected Text nameMemberTxt;
    [SerializeField] protected RawImage flag;

    protected IDisposable _downloadFlagDispose;

    [SerializeField] protected Sprite[] rankRewardSprs;
    [SerializeField] protected Sprite[] farmeRankSprs;

    [SerializeField] protected GameObject posLeft;
    [SerializeField] protected GameObject posRight;

    [SerializeField] protected GameObject fxFinish;
    public bool isMe;
    protected SpaceRaceChallengeBox box;

    [SerializeField] protected InfoChestController infoChestController;

    public void InitState(int rank, MemberRace memberData, SpaceRaceChallengeBox box)
    {
        if (isMe)
        {
            nameMemberTxt.text = Localization.Get("s_me");
        }
        else
        {
            nameMemberTxt.text = memberData.nameMember;
        }
        SetUpFlag(memberData.linkFlag);
        this.box = box;
        rewardsUI.rankTxt.text = (rank + 1).ToString();
        // rank = Mathf.Clamp(rank, 0, rankRewardSprs.Length - 1);

        if (rank >= rankRewardSprs.Length)
        {
            rewardsUI.farmeRank.gameObject.SetActive(false);
            rewardsUI.rewardIcon.gameObject.SetActive(false);
        }
        else
        {
            rewardsUI.farmeRank.gameObject.SetActive(true);
            rewardsUI.rewardIcon.gameObject.SetActive(true);

            rewardsUI.farmeRank.sprite = farmeRankSprs[rank];
            rewardsUI.rewardIcon.sprite = rankRewardSprs[rank];
            infoChestController.chest.sprite = rankRewardSprs[rank];
        }



        ships.levelPassTxt.text = memberData.CurrentLevelPass.ToString();
        fxFinish.gameObject.SetActive(false);
        if (memberData.RememberCurrentLevelPass != memberData.CurrentLevelPass)
        {
            ships.shipBody.transform.DOKill();
            ships.shipBody.transform.localPosition = BezierCurves.GetPoint_LinearIntrepolation(posLeft.transform.localPosition, posRight.transform.localPosition, (float)memberData.RememberCurrentLevelPass / RaceSpaceChallenge.NumLevelPassToWinEvent);

            if (!memberData.IsFinished && memberData.CurrentLevelPass >= RaceSpaceChallenge.NumLevelPassToWinEvent)
            {
                float newPosX = posRight.GetComponent<RectTransform>().anchoredPosition.x;
                ships.shipBody.transform.GetComponent<RectTransform>().DOAnchorPosX(newPosX, 1f).SetEase(Ease.InBack);
                OnFinishRace(rank);
                if (memberData.isMe)
                    box.OnMeFinishRace();
            }
            else
            {
                float newPosX = BezierCurves.GetAmount_LinearIntrepolation(posLeft.GetComponent<RectTransform>().anchoredPosition.x, posRight.GetComponent<RectTransform>().anchoredPosition.x, (float)memberData.CurrentLevelPass / RaceSpaceChallenge.NumLevelPassToWinEvent);
                ships.shipBody.transform.GetComponent<RectTransform>().DOAnchorPosX(newPosX, 1f).SetEase(Ease.InBack);
                box.ActiveInteractBtnClose();
                if (memberData.CurrentLevelPass >= RaceSpaceChallenge.NumLevelPassToWinEvent)
                {
                    fxFinish.gameObject.SetActive(true);
                }
            }

            memberData.RememberCurrentLevelPass = memberData.CurrentLevelPass;
        }
        else
        {
            ships.shipBody.transform.DOKill();
            ships.shipBody.transform.localPosition = BezierCurves.GetPoint_LinearIntrepolation(posLeft.transform.localPosition, posRight.transform.localPosition, (float)memberData.CurrentLevelPass / RaceSpaceChallenge.NumLevelPassToWinEvent);
            box.ActiveInteractBtnClose();
            if (memberData.CurrentLevelPass >= RaceSpaceChallenge.NumLevelPassToWinEvent)
            {
                fxFinish.gameObject.SetActive(true);
            }
        }

        if (rank <= 2)
        {
            infoChestController.gameObject.SetActive(true);
            var challengeController = (RaceSpaceChallenge)GameController.Instance.challengeController.GetChallenges(ChallengeType.RaceSpaceChallenge);
            var rewards = challengeController.config.rewards[rank].rewardData;
            infoChestController.Init(rewards, "");
        }
        else
        {
            infoChestController.gameObject.SetActive(false);
        }    
    }

    private void SetUpFlag(string linkFlag)
    {
        if (!string.IsNullOrEmpty(linkFlag))
        {
            _downloadFlagDispose = Context.DownloadOrCache(linkFlag, false)
                .CatchIgnore()
                .Subscribe(www => flag.texture = www).AddTo(this);
        }
        else
        {
            flag.gameObject.SetActive(false);
        }
    }

    private void OnFinishRace(int rank)
    {
        fxFinish.gameObject.SetActive(true);

        StartCoroutine(Helper.StartAction(() =>
        {
            if (isMe)
            {
                if (rank <= 2)
                {
                    var challengeController = (RaceSpaceChallenge)GameController.Instance.challengeController.GetChallenges(ChallengeType.RaceSpaceChallenge);
                    var rewards = challengeController.config.rewards[rank].rewardData;

                    TypeChest typeChest = TypeChest.RaceSpaceRank_1;
                    switch (rank)
                    {
                        case 0:
                            typeChest = TypeChest.RaceSpaceRank_1;
                            break;
                        case 1:
                            typeChest = TypeChest.RaceSpaceRank_2;
                            break;
                        case 2:
                            typeChest = TypeChest.RaceSpaceRank_3;
                            break;
                    }

                    GPlayFuntPuzzle.GPopupRewardChest.Setup().Show(typeChest, rewards, Reason.race_space_challenge, isHasVideo: challengeController.config.is_video_x_reward, challengeController.config.num_x_reward);
                }
            }

            box.ActiveInteractBtnClose();
        }, 1f));

        if (RaceSpaceChallenge.IsRacing)
        {
            box.ActiveContinueBtn();
        }
    }

    private void OnEnable()
    {
        infoChestController.DisableInfo();
    }
}
