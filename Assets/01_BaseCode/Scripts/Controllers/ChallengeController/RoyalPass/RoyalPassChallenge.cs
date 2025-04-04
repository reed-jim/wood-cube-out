using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoyalPassChallenge : ChallengeBase
{
    public static bool IsActiveRoyalPack
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.IS_ACTIVE_ROYAL_PACK, 0) == 1;
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.IS_ACTIVE_ROYAL_PACK, value ? 1 : 0);
            GPlayerPrefs.Save();
        }
    }

    public static void SetClaimedReward(int indexReward, int rewardSide, bool isClaimed)
    {
        if (rewardSide == 0)//Free
            GPlayerPrefs.SetInt(StringHelper.IS_CLAIMED_ROYAL_PACK_INDEX_REWARD + indexReward, isClaimed ? 1 : 0);
        else
            GPlayerPrefs.SetInt(StringHelper.IS_CLAIMED_ROYAL_PACK_INDEX_REWARD + indexReward + "_RoyalPack", isClaimed ? 1 : 0);
    }

    public static bool IsClaimedReward(int indexReward, int rewardSide)
    {
        if (rewardSide == 0)//Free
            return GPlayerPrefs.GetInt(StringHelper.IS_CLAIMED_ROYAL_PACK_INDEX_REWARD + indexReward, 0) == 1 ? true : false;
        else
            return GPlayerPrefs.GetInt(StringHelper.IS_CLAIMED_ROYAL_PACK_INDEX_REWARD + indexReward + "_RoyalPack", 0) == 1 ? true : false;
    }

    public static bool IsOpenedTut
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.IS_OPENED_TUT_ROYAL_PACK, 0) == 1;
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.IS_OPENED_TUT_ROYAL_PACK, value ? 1 : 0);
            GPlayerPrefs.Save();
        }
    }

  

    public override void InitHome()
    {
        if (UnbiasedTime.Instance.Now() > data.TimeEndChallenge)
        {
            //Hết 1 tháng
            ResetSeason();
        }

        

        base.InitHome();

        priovitySugget = SuggetPopupHomeController.PriovitySugget.Special;

        if (UseProfile.CurrentLevel == config.level_unlock)
        {
            // Tut royal pass
            if (!IsOpenedTut)
            {
                HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow();
                HomeController.Instance.suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
                {
                    actionShow = () => 
                    {
                        IsOpenedTut = true;
                        RoyalPassBox.Setup(true); 
                    },
                    priovity = priovitySugget
                });
            }
        }

        actionCoolDownEndDoneInHome = () => { ResetSeason(); };
    }

    public override void CheckChallenge()
    {
        if (!IsChallengeActive())
            return;

        if (data.CurrentIndexReward > config.rewards.Count - 1)
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

    public RewardRandom GetCurrentRewardData()
    {
        if (data.CurrentIndexReward > config.rewards.Count - 1)
            return null;

        RewardRandom currentReward = config.rewards[Mathf.Clamp(data.CurrentIndexReward, 0, config.rewards.Count - 1)].rewardData[0];
        return currentReward;
    }

    protected override List<RewardRandom> ClaimReward()
    {
        List<RewardRandom> rewards = new List<RewardRandom>();
        for (int i = 0; i < config.rewards.Count; i++)
        {
            var reward = config.rewards[i].GetReward();
            if (reward.typeItem != GiftType.None)
            {
                rewards.Add(reward);
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
                OpenBox(isSuggetHome: true);
                view.OnOpenBoxDone();
            },
            priovity = priovitySugget
        }
            );

    }


    public override void OpenBox(bool isSuggetHome = false)
    {
        RoyalPassBox.Setup(isSuggetHome);
    }

    public override void ResetSeason()
    {
        base.ResetSeason();
        data.TimeEndChallenge = TimeManager.GetLastDayOfMonth(UnbiasedTime.Instance.Now().AddDays(1));
        for (int i = 0; i < config.rewards.Count; i++)
        {
            SetClaimedReward(i, 0, false);
            SetClaimedReward(i, 1, false);
        }
        data.CurrentIndexReward = 1;
        IsActiveRoyalPack = false;
    }

    public override bool IsChallengeActive()
    {
        Debug.Log("Checked");
        if (!config.IsActive)
            return false;
        if (UseProfile.CurrentLevel < config.level_unlock)
            return false;

        return true;
    }

    protected override bool IsCanSpawnItemHandle()
    {
        if (data.CurrentIndexReward > config.rewards.Count - 1)
            return false;

        return true;
    }
}
