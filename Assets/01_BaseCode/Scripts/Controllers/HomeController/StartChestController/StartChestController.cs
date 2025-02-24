using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StartChestController : MonoBehaviour
{
    public static int RememberCurrentStar
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.REMEMBER_CURRENT_START_CHEST, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.REMEMBER_CURRENT_START_CHEST, value);
            GPlayerPrefs.Save();
        }
    }

    public static int CurrentStar
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_START_CHEST, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.CURRENT_START_CHEST, value);
            GPlayerPrefs.Save();
        }
    }

    [HideInInspector] public CoreGameData.StartChestData startChestData;
    public UnityAction actionOpenChest;

    public StartChestView view;

    public static bool IsHasChest()
    {
        return CurrentStar >= GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.starChestConfig.startRequirement;
    }

    public void Init()
    {
        startChestData = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.starChestConfig;
        view.Init(this);
        actionOpenChest = null;
        CheckStarChest();
    }

    public void InitState()
    {
        view.InitState();
        if (CurrentStar < startChestData.startRequirement)
        {
            RememberCurrentStar = CurrentStar;
        }
    }

    public void CheckStarChest()
    {
        if (UseProfile.CurrentLevel < startChestData.unlockLevel)
            return;
        Debug.Log("============= CheckStarChest ===============");
        if (RememberCurrentStar != CurrentStar)
        {
            if (CurrentStar >= startChestData.startRequirement)
            {
                HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow();

                actionOpenChest = () => 
                {
                    ActionOpenChest(ClaimReward());
                    actionOpenChest = null;
                };
            }
        }
    }

    private List<RewardRandom> ClaimReward()
    {
        List<RewardRandom> rewards = new List<RewardRandom>();
        for (int i = 0; i < startChestData.rewards.Count; i++)
        {
            var reward = startChestData.rewards[i].GetReward();
            if (reward.typeItem != GiftType.None)
            {
                GameController.Instance.dataContain.giftDatabase.Claim(reward.typeItem, reward.amount, Reason.open_chest_star);
                rewards.Add(reward);
            }
        }

        return rewards;
    }

    private void ActionOpenChest(List<RewardRandom> rewards)
    {
        HomeController.Instance.suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
        {
            actionShow = () => 
            {
                CurrentStar -= startChestData.startRequirement;
                if (CurrentStar < 0)
                    CurrentStar = 0;
                
                RememberCurrentStar = CurrentStar;
                GPlayFuntPuzzle.GPopupRewardChest.Setup(true).Show(TypeChest.StarChest, rewards, Reason.open_chest_star,
GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.starChestConfig.is_video_x_reward,
GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.starChestConfig.num_x_reward);
                //OpenChestBox.Setup(true).Show(TypeChest.StarChest, rewards);
                view.OnOpenChestDone();
            },
            priovity = SuggetPopupHomeController.PriovitySugget.Special
        }
            );
    }

    public void OpenChestByButton()
    {
       var rewards = ClaimReward();

        CurrentStar -= startChestData.startRequirement;
        if (CurrentStar < 0)
            CurrentStar = 0;

        RememberCurrentStar = CurrentStar;
        GPlayFuntPuzzle.GPopupRewardChest.Setup(true).Show(TypeChest.StarChest, rewards, Reason.open_chest_level, GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.starChestConfig.is_video_x_reward, GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.starChestConfig.num_x_reward);
        view.OnOpenChestDone();
    }    
}
