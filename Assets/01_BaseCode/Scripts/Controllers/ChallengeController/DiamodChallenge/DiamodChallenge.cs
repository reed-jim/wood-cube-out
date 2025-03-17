using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DiamodChallenge : CoinChallenge
{
    public override bool IsOpenedTut
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.IS_OPENED_TUT_DIAMOD_CHALLENGE, 0) == 1;
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.IS_OPENED_TUT_DIAMOD_CHALLENGE, value ? 1 : 0);
            GPlayerPrefs.Save();
        }
    }


    public override bool IsChallengeActive()
    {
        if (UseProfile.CurrentLevel < config.level_unlock)
            return false;
        if (!config.IsActive)
            return false;
        DayOfWeek dayActive = TimeManager.ParseTimeStartDay(UnbiasedTime.Instance.Now()).DayOfWeek;
        if (dayActive != DayOfWeek.Wednesday && dayActive != DayOfWeek.Thursday)
        {
            return false;
        }

        return true;
    }


    protected override void ActionOpenBox(List<RewardRandom> rewards)
    {
        HomeController.Instance.suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
        {
            actionShow = () =>
            {
                data.RememberCurrentNumItem = data.CurrentNumItem;
                data.CurrentIndexReward++;

                TypeChest typeChest = TypeChest.DiamodChest_1;
                switch(data.CurrentIndexReward)
                {
                    case 1:
                        typeChest = TypeChest.DiamodChest_1;
                        break;
                    case 2:
                        typeChest = TypeChest.DiamodChest_2;
                        break;
                    case 3:
                        typeChest = TypeChest.DiamodChest_3;
                        break;
                }
                GPlayFuntPuzzle.GPopupRewardChest.Setup(true).Show(typeChest, rewards, Reason.diamod_challenge, config.is_video_x_reward, config.num_x_reward);
                //OpenChestBox.Setup(true).Show(typeChest, rewards);
                view.OnOpenBoxDone();
            },
            priovity = SuggetPopupHomeController.PriovitySugget.Special
        });
    }

    public override void OpenBox(bool isSuggetHome = false)
    {
        DiamodChallengeBox.Setup(isSuggetHome).Show();
    }

    public override void ResetSeason()
    {
        base.ResetSeason();
        data.TimeEndChallenge = TimeManager.ParseTimeStartWeek(UnbiasedTime.Instance.Now()).AddDays(4);
        IsOpenedTut = false;
    }
}
