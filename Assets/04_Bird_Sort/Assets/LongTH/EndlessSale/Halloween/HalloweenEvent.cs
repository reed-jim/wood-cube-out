using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

public class HalloweenEvent : EventBase
{
    public virtual bool IsOpenedTut
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.IS_OPENED_TUT_EVENT_SALE, 0) == 1;
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.IS_OPENED_TUT_EVENT_SALE, value ? 1 : 0);
            GPlayerPrefs.Save();
        }
    }

    [HideInInspector] public Dictionary<int, List<ListRewardContent>> rewardsDict;
    protected List<int> requirementsReward;


    public override int CurrentItemRequirement
    {
        get { return (int)config.rewardData[Mathf.Clamp(data.CurrentIndexReward, 0, config.rewardData.Count - 1)].requireToken; }
    }

    public override bool IsClaimAllReward
    {
        get { return data.CurrentIndexReward >= (int)config.rewardData.Count; }
    }

    public override void Init()
    {
        base.Init();
        if (IsEventActive())
        {
            rewardsDict = new Dictionary<int, List<ListRewardContent>>();
            requirementsReward = new List<int>();
            for (int i = 0; i < config.rewardData.Count; i++)
            {
                int numRequire = (int)config.rewardData[i].requireToken;
                requirementsReward.Add(numRequire);
                for (int j = 0; j < config.rewardData.Count; j++)
                {
                    if (config.rewardData[j].requireToken == numRequire)
                    {
                        if (rewardsDict.ContainsKey(numRequire))
                        {
                            ListRewardContent dt = new ListRewardContent();
                            dt.rewards = config.rewardData[j].rewardData;
                            rewardsDict[numRequire].Add(dt);
                        }
                        else
                        {
                            ListRewardContent dt = new ListRewardContent();
                            dt.rewards = config.rewardData[j].rewardData;

                            List<ListRewardContent> rws = new List<ListRewardContent>();
                            rws.Add(dt);

                            rewardsDict.Add(numRequire, rws);
                        }
                    }
                }
            }
        }
    }

    public override void InitHome()
    {
        if (IsEventActive())
        {
            if (UnbiasedTime.Instance.Now() > data.TimeEndEvent)
            {
                ResetSeason();
            }
        }
        base.InitHome();

        priovitySugget = SuggetPopupHomeController.PriovitySugget.SuperStarLegendaryShow;

        if (IsEventActive())
        {

            if (!IsOpenedTut)
            {
                HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow(); Debug.Log(this.name);
                HomeController.Instance.suggetPopupHomeController.AddActionShow(
                    new SuggetPopupHomeController.SuggetPopupData()
                    {
                        actionShow = () =>
                        {
                            OpenBox(true);
                        },
                        priovity = SuggetPopupHomeController.PriovitySugget.HighQuality
                    });
            }

            actionCoolDownEndDoneInHome = ResetSeason;
        }
    }


    public override bool IsEventActive()
    {
        if (UseProfile.CurrentLevel < config.level_unlock)
            return false;

        if (EndlessEnd)
            return false;

        if (BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive() != null)
        {
            DateTime startDate = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive().start;
            DateTime endDate = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive().end;
            if ((UnbiasedTime.Instance.Now() - endDate).TotalSeconds >= 0 || (UnbiasedTime.Instance.Now() - startDate).TotalSeconds < 0)
                return false;
        }
        else
        {
            return false;
        }

        return true;
    }

    public override void CheckEvent()
    {
        if (!IsEventActive())
            return;

        if (data.CurrentIndexReward > config.rewardData.Count - 1)
        {
            data.RememberCurrentNumItem = data.CurrentNumItem;
            return;
        }

        if (data.RememberCurrentNumItem != data.CurrentNumItem)
        {
            if (data.CurrentNumItem >= CurrentItemRequirement)
            {
                HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow(); Debug.Log(this.name);

                actionOpenBox = () =>
                {
                    ActionOpenBox();
                    ClaimReward();
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
                    BirdSortGameController.Instance.dataContain.giftDatabase.Claim(rewards[i].typeItem, rewards[i].amount,
                        reasonClaimReward, rewards[i].isUnlimited);
                }
        }

        return rewards;
    }

    protected override void ActionOpenBox()
    {
        HomeController.Instance.suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
        {
            actionShow = () =>
            {
                //data.RememberCurrentNumItem = data.CurrentNumItem;
                data.CurrentIndexReward++;
                EndLessSaleBox.Setup(true).Show();
                view.OnOpenBoxDone();
            },
            priovity = SuggetPopupHomeController.PriovitySugget.Special
        });
    }

    public override void OpenBox(bool isSuggetHome = false)
    {
        if (!IsOpenedTut)
        {
            EndLessTutBox.Setup(isSuggetHome).Show();
            IsOpenedTut = true;
        }
        else
        {
            EndLessSaleBox.Setup(isSuggetHome).Show();
        }

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
        data.TimeEndEvent = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive().end;
        StartCoroutine(ReloadHome());
    }

    IEnumerator ReloadHome()
    {
        yield return new WaitForSecondsRealtime(3f);
        BirdSortGameController.Instance.packController.InitState();
        HomeController.Instance.eventBaseView.Init();
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.RELOAD_SALE_HOME);
    }
}
