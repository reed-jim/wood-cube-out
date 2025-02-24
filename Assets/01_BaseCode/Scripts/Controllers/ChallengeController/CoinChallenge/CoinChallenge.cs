using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CoinChallenge : ChallengeBase
{
    public virtual bool IsOpenedTut
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.IS_OPENED_TUT_COIN_CHALLENGE, 0) == 1;
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.IS_OPENED_TUT_COIN_CHALLENGE, value ? 1 : 0);
            GPlayerPrefs.Save();
        }
    }

    [HideInInspector] public Dictionary<int, List<ListRewardContent>> rewardsDict;
    protected List<int> requirementsReward;


    public override int CurrentItemRequirement
    {
        get { return (int)config.parmaExtral[Mathf.Clamp(data.CurrentIndexReward, 0, config.parmaExtral.Length - 1)]; }
    }

    public override bool IsClaimAllReward
    {
        get { return data.CurrentIndexReward >= (int)config.parmaExtral.Length; }
    }

    public override void Init()
    {
        base.Init();
        rewardsDict = new Dictionary<int, List<ListRewardContent>>();
        requirementsReward = new List<int>();
        for (int i = 0; i < config.parmaExtral.Length; i++)
        {
            int numRequire = (int)config.parmaExtral[i];
            requirementsReward.Add(numRequire);
            for (int j = 0; j < config.rewards.Count; j++)
            {
                if (config.rewards[j].numItemRequire == numRequire)
                {
                    if (rewardsDict.ContainsKey(numRequire))
                    {
                        ListRewardContent dt = new ListRewardContent();
                        dt.rewards = config.rewards[j].rewardData;
                        rewardsDict[numRequire].Add(dt);
                    }
                    else
                    {
                        ListRewardContent dt = new ListRewardContent();
                        dt.rewards = config.rewards[j].rewardData;

                        List<ListRewardContent> rws = new List<ListRewardContent>();
                        rws.Add(dt);

                        rewardsDict.Add(numRequire, rws);
                    }
                }
            }
        }
    }

    public override void InitHome()
    {
        if (IsChallengeActive())
        {
            if (UnbiasedTime.Instance.Now() > data.TimeEndChallenge)
            {
                ResetSeason();
            }
        }

        base.InitHome();

        priovitySugget = SuggetPopupHomeController.PriovitySugget.SuperStarLegendaryShow;

        if (IsChallengeActive())
        {
            if (!IsOpenedTut)
            {
                HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow();
                HomeController.Instance.suggetPopupHomeController.AddActionShow(
                    new SuggetPopupHomeController.SuggetPopupData()
                    {
                        actionShow = () => { OpenBox(true); },
                        priovity = SuggetPopupHomeController.PriovitySugget.HighQuality
                    });
                IsOpenedTut = true;
            }
        }

        actionCoolDownEndDoneInHome = ResetSeason;
    }


    public override bool IsChallengeActive()
    {
        if (UseProfile.CurrentLevel < config.level_unlock)
            return false;

//        if (data.CurrentIndexReward > requirementsReward.Count - 1) //Đã nhận quà cuối
//            return false;

        DayOfWeek dayActive = TimeManager.ParseTimeStartDay(UnbiasedTime.Instance.Now()).DayOfWeek;
        if (dayActive != DayOfWeek.Monday && dayActive != DayOfWeek.Tuesday)
        {
            return false;
        }

        return true;
    }

    public override void CheckChallenge()
    {
        if (!IsChallengeActive())
            return;

        if (data.CurrentIndexReward > config.parmaExtral.Length - 1)
        {
            data.RememberCurrentNumItem = data.CurrentNumItem;
            return;
        }

        if (data.RememberCurrentNumItem != data.CurrentNumItem)
        {
            if (data.CurrentNumItem >= CurrentItemRequirement)
            {
                HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow();

                actionOpenBox = () =>
                {
                    ActionOpenBox(ClaimReward());
                    actionOpenBox = null;
                };
            }
        }
    }

    public List<RewardRandom> GetReward()
    {
        List<RewardRandom> rewards = new List<RewardRandom>();
        var rewardDt = rewardsDict[requirementsReward[data.CurrentIndexReward]];
        for (int z = 0; z < rewardDt.Count; z++)
        {
            float maxWeight = 0f;
            for (int i = 0; i < rewardDt[z].rewards.Count; i++)
            {
                maxWeight += rewardDt[z].rewards[i].weight;
            }
            float randomNum = UnityEngine.Random.Range(0f, maxWeight);
            float currentWeight = 0f;
            for (int i = 0; i < rewardDt[z].rewards.Count; i++)
            {
                int index = i;
                currentWeight += rewardDt[z].rewards[index].weight;
                if (currentWeight > randomNum)
                {
                    rewards.Add(rewardDt[z].rewards[index]);
                    break;
                }
            }
        }

        return rewards;
    }


    protected override List<RewardRandom> ClaimReward()
    {
        var rewards = GetReward();
        if (rewards != null)
        {
            for (int i = 0; i < rewards.Count; i++)
                if (rewards[i].typeItem != GiftType.None)
                {
                    GameController.Instance.dataContain.giftDatabase.Claim(rewards[i].typeItem, rewards[i].amount,
                        reasonClaimReward, rewards[i].isUnlimited);
                }
        }

        return rewards;
    }

    protected override void ActionOpenBox(List<RewardRandom> rewards)
    {
        HomeController.Instance.suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
        {
            actionShow = () =>
            {
                data.RememberCurrentNumItem = data.CurrentNumItem;
                data.CurrentIndexReward++;
                GPlayFuntPuzzle.GPopupRewardChest.Setup(true).Show(TypeChest.CoinChest, rewards, Reason.coin_challenge, config.is_video_x_reward, config.num_x_reward);
                //OpenChestBox.Setup(true).Show(TypeChest.CoinChest, rewards);
                view.OnOpenBoxDone();
            },
            priovity = SuggetPopupHomeController.PriovitySugget.Special
        });
    }

    public override void OpenBox(bool isSuggetHome = false)
    {
        CoinChallengeBox.Setup(isSuggetHome).Show();
    }

    protected override bool IsCanSpawnItemHandle()
    {
        if (IsClaimAllReward)
            return false;

        return true;
    }

    public override void ResetSeason()
    {
        base.ResetSeason();
        data.TimeEndChallenge = TimeManager.ParseTimeStartWeek(UnbiasedTime.Instance.Now()).AddDays(2);
        IsOpenedTut = false;
    }
}