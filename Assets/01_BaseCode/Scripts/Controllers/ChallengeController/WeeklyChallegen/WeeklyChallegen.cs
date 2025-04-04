using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeeklyChallegen : ChallengeBase
{
    public override void InitHome()
    {
       // if (UseProfile.CurrentLevel >= config.level_unlock)
        {
            if (UnbiasedTime.Instance.Now() > data.TimeEndChallenge)
            {
                //Hết 1 tuần
                ResetSeason();
            }
        }

        //base.InitHome();

        priovitySugget = SuggetPopupHomeController.PriovitySugget.SuperStarLegendaryShow;

        if (UseProfile.CurrentLevel == config.level_unlock)
        {
            HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow();
            HomeController.Instance.suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
            {
                actionShow = () => { WeeklyChallegenTutBox.Setup(true).Show(); },
                priovity = SuggetPopupHomeController.PriovitySugget.HighQuality
            });
        }

        actionCoolDownEndDoneInHome = () => { ResetSeason(); };
    }

    //public override void InitStateHome()
    //{
    //    view.InitState();
    //}

    public RewardRandom GetCurrentRewardData()
    {
        if (data.CurrentIndexReward > config.rewards.Count - 1)
            return null;

        RewardRandom currentReward = config.rewards[Mathf.Clamp(data.CurrentIndexReward, 0, config.rewards.Count - 1)].rewardData[0];
        return currentReward;
    }



    public override bool IsChallengeActive()
    {
        Debug.Log("Checked");
        if (!config.IsActive)
            return false;
        if (UseProfile.CurrentLevel < config.level_unlock)
            return false;

        if (GetCurrentRewardData() == null)//Đã nhận quà cuối
            return false;


        return true;
    }

    public override void CheckChallenge()
    {
        if (!IsChallengeActive())
            return;

        if (data.RememberCurrentNumItem != data.CurrentNumItem)
        {
            HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow(true);
            if (data.CurrentNumItem >= CurrentItemRequirement)
            {
                HomeController.Instance.suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
                {
                    actionShow = () =>
                    {
                        

                        //Show Quà
                        ((WeeklyChallegenView)view).ShowClaimReward();
                    },
                    priovity = SuggetPopupHomeController.PriovitySugget.SuperStarLegendaryShow
                }
           );

                actionOpenBox = () =>
                {
                    ClaimReward();
                    data.RememberCurrentNumItem = data.CurrentNumItem;
                    data.CurrentIndexReward++;
                    HomeController.Instance.suggetPopupHomeController.SuggetSuperStarLegendaryShow();
                    actionOpenBox = null;
                };
            }
            else
            {
                HomeController.Instance.suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
                {
                    actionShow = () =>
                    {

                    },
                    priovity = SuggetPopupHomeController.PriovitySugget.SuperStarLegendaryShow
                });

                actionOpenBox = () =>
                {
                    HomeController.Instance.suggetPopupHomeController.SuggetSuperStarLegendaryShow();
                    HomeController.Instance.suggetPopupHomeController.NextSuggetSuperStarLegendaryShow();//Call 2 lần vì bắt đầu và kết thúc cùng 1 farme
                    actionOpenBox = null;
                };

               // data.RememberCurrentNumItem = data.CurrentNumItem;
            }
        }
    }

    protected override List<RewardRandom> ClaimReward()
    {
        List<RewardRandom> rewards = new List<RewardRandom>();
        var reward = config.rewards[data.CurrentIndexReward].GetReward();
        if (reward.typeItem != GiftType.None)
        {
            GameController.Instance.dataContain.giftDatabase.Claim(reward.typeItem, reward.amount, reasonClaimReward, reward.isUnlimited);
            rewards.Add(reward);
        }

        return rewards;
    }

    protected override void ActionOpenBox(List<RewardRandom> rewards)
    {

    }


    public override void OpenBox(bool isSuggetHome = false)
    {
        WeeklyChallegenBox.Setup(isSuggetHome);
    }

    public override void ResetSeason()
    {
        base.ResetSeason();
        data.TimeEndChallenge = TimeManager.ParseTimeStartWeek(UnbiasedTime.Instance.Now()).AddDays(7);
    }
}
