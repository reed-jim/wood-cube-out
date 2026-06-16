using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NoelEvent : HalloweenEvent
{
    //public virtual bool IsOpenedTut
    //{
    //    get
    //    {
    //        return PlayerPrefs.GetInt(StringHelper.IS_OPENED_TUT_EVENT_SALE, 0) == 1;
    //    }
    //    set
    //    {
    //        GPlayerPrefs.SetInt(StringHelper.IS_OPENED_TUT_EVENT_SALE, value ? 1 : 0);
    //        GPlayerPrefs.Save();
    //    }
    //}

    //public string rewardClaimed
    //{
    //    get
    //    {
    //        return PlayerPrefs.GetString(StringHelper.REWARD_HAD_CLAIMED, "");
    //    }
    //    set
    //    {
    //        GPlayerPrefs.GetString(StringHelper.REWARD_HAD_CLAIMED, value);
    //        GPlayerPrefs.Save();
    //    }
    //}

    //[HideInInspector] public Dictionary<int, List<ListRewardContent>> rewardsDict;
    //protected List<int> requirementsReward;


    //public override int CurrentItemRequirement
    //{
    //    get { return (int)config.rewardData[Mathf.Clamp(data.CurrentIndexReward, 0, config.rewardData.Count - 1)].requireToken; }
    //}

    //public override bool IsClaimAllReward
    //{
    //    get { return data.CurrentIndexReward >= (int)config.rewardData.Count; }
    //}

    //public override void Init()
    //{
    //    base.Init();
    //    rewardsDict = new Dictionary<int, List<ListRewardContent>>();
    //    requirementsReward = new List<int>();
    //    for (int i = 0; i < config.rewardData.Count; i++)
    //    {
    //        int numRequire = (int)config.rewardData[i].requireToken;
    //        requirementsReward.Add(numRequire);
    //        for (int j = 0; j < config.rewardData.Count; j++)
    //        {
    //            if (config.rewardData[j].requireToken == numRequire)
    //            {
    //                if (rewardsDict.ContainsKey(numRequire))
    //                {
    //                    ListRewardContent dt = new ListRewardContent();
    //                    dt.rewards = config.rewardData[j].rewardData;
    //                    rewardsDict[numRequire].Add(dt);
    //                }
    //                else
    //                {
    //                    ListRewardContent dt = new ListRewardContent();
    //                    dt.rewards = config.rewardData[j].rewardData;

    //                    List<ListRewardContent> rws = new List<ListRewardContent>();
    //                    rws.Add(dt);

    //                    rewardsDict.Add(numRequire, rws);
    //                }
    //            }
    //        }
    //    }
    //}

    //public override void InitHome()
    //{
    //    //if (IsEventActive())
    //    //{
    //    //    if (UnbiasedTime.Instance.Now() > data.TimeEndEvent)
    //    //    {
    //    //        ResetSeason();
    //    //    }
    //    //}

    //    base.InitHome();

    //    priovitySugget = SuggetPopupHomeController.PriovitySugget.SuperStarLegendaryShow;

    //    if (IsEventActive())
    //    {
    //        if (!IsOpenedTut)
    //        {
    //            HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow();
    //            HomeController.Instance.suggetPopupHomeController.AddActionShow(
    //                new SuggetPopupHomeController.SuggetPopupData()
    //                {
    //                    actionShow = () => { OpenBox(true); },
    //                    priovity = SuggetPopupHomeController.PriovitySugget.HighQuality
    //                });
    //            IsOpenedTut = true;
    //        }
    //    }

    //    actionCoolDownEndDoneInHome = ResetSeason;
    //}


    //public override bool IsEventActive()
    //{
    //    if (UseProfile.CurrentLevel < config.level_unlock)
    //        return false;

    //    //if (data.CurrentIndexReward > requirementsReward.Count - 1) //?ã nh?n quà cu?i
    //    //    return false;

    //    return true;
    //}

    //public override void CheckEvent()
    //{
    //    if (!IsEventActive())
    //        return;

    //    if (data.CurrentIndexReward > config.rewardData.Count - 1)
    //    {
    //        data.RememberCurrentNumItem = data.CurrentNumItem;
    //        return;
    //    }

    //    if (data.RememberCurrentNumItem != data.CurrentNumItem)
    //    {
    //        if (data.CurrentNumItem >= CurrentItemRequirement)
    //        {
    //            HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow();

    //            actionOpenBox = () =>
    //            {
    //                ActionOpenBox(ClaimReward());
    //                actionOpenBox = null;
    //            };
    //        }
    //    }
    //}

    //public List<RewardRandom> GetReward()
    //{
    //    List<RewardRandom> rewards = new List<RewardRandom>();
    //    var rewardDt = rewardsDict[requirementsReward[data.CurrentIndexReward]];
    //    for (int z = 0; z < rewardDt.Count; z++)
    //    {
    //        float maxWeight = 0f;
    //        for (int i = 0; i < rewardDt[z].rewards.Count; i++)
    //        {
    //            maxWeight += rewardDt[z].rewards[i].weight;
    //        }
    //        float randomNum = UnityEngine.Random.Range(0f, maxWeight);
    //        float currentWeight = 0f;
    //        for (int i = 0; i < rewardDt[z].rewards.Count; i++)
    //        {
    //            int index = i;
    //            currentWeight += rewardDt[z].rewards[index].weight;
    //            if (currentWeight > randomNum)
    //            {
    //                rewards.Add(rewardDt[z].rewards[index]);
    //                break;
    //            }
    //        }
    //    }

    //    return rewards;
    //}


    //protected override List<RewardRandom> ClaimReward()
    //{
    //    var rewards = GetReward();
    //    if (rewards != null)
    //    {
    //        for (int i = 0; i < rewards.Count; i++)
    //            if (rewards[i].typeItem != GiftType.None)
    //            {
    //                BirdSortGameController.Instance.dataContain.giftDatabase.Claim(rewards[i].typeItem, rewards[i].amount,
    //                    reasonClaimReward, rewards[i].isUnlimited);
    //            }
    //    }

    //    return rewards;
    //}

    //protected override void ActionOpenBox(List<RewardRandom> rewards)
    //{
    //    HomeController.Instance.suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
    //    {
    //        actionShow = () =>
    //        {
    //            data.RememberCurrentNumItem = data.CurrentNumItem;
    //            data.CurrentIndexReward++;
    //            EndLessSaleBox.Setup(true).Show();
    //            view.OnOpenBoxDone();
    //        },
    //        priovity = SuggetPopupHomeController.PriovitySugget.Special
    //    });
    //}

    //public override void OpenBox(bool isSuggetHome = false)
    //{
    //    EndLessSaleBox.Setup(isSuggetHome).Show();
    //}

    //protected override bool IsCanSpawnItemHandle()
    //{
    //    if (IsClaimAllReward)
    //        return false;

    //    return true;
    //}

    //public override void ResetSeason()
    //{
    //    base.ResetSeason();
    //    data.TimeEndEvent = TimeManager.ParseTimeStartWeek(UnbiasedTime.Instance.Now()).AddDays(2);
    //    IsOpenedTut = false;
    //}
}
