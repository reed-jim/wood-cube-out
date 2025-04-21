using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelChestController : MonoBehaviour
{
    public static int RememberLevelChest
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.REMEMBER_CURRENT_LEVEL_CHEST, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.REMEMBER_CURRENT_LEVEL_CHEST, value);
            GPlayerPrefs.Save();
        }
    }
    public static int CurrentLevelChest
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_LEVEL_CHEST, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.CURRENT_LEVEL_CHEST, value);
            GPlayerPrefs.Save();
        }
    }

    [HideInInspector] public CoreGameData.LevelChestData levelChestData;
    public UnityAction actionOpenChest;

    public LevelChestView view;

    public static bool IsHasChest()
    {
        return CurrentLevelChest >= GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelChestConfig.levelSpacing - 1;
    }

    public void Init()
    {
        levelChestData = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelChestConfig;
        view.Init(this);
       
        CheckLevelChest();
    }

    public void InitState()
    {
        view.InitState();
        if (CurrentLevelChest < levelChestData.levelSpacing - 1)
        {
            RememberLevelChest = CurrentLevelChest;
        }
    }

    public void CheckLevelChest()
    {
        if (UseProfile.CurrentLevel < levelChestData.unlockLevel)
            return;

        if (RememberLevelChest != CurrentLevelChest)
        {
            if (CurrentLevelChest >= levelChestData.levelSpacing - 1)
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
        for (int i = 0; i < levelChestData.rewards.Count; i++)
        {
            var reward = levelChestData.rewards[i].GetReward();
            if (reward.typeItem != GiftType.None)
            {
                GameController.Instance.dataContain.giftDatabase.Claim(reward.typeItem, reward.amount, Reason.open_chest_level);
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
                //CurrentLevelChest -= levelChestData.levelSpacing;
                //if (CurrentLevelChest < 0)
                    CurrentLevelChest = -1;
               
                RememberLevelChest = CurrentLevelChest;
                view.OnOpenChestDone();
                Debug.Log("Chest Rewards" + GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelChestConfig.is_video_x_reward);
                GPlayFuntPuzzle.GPopupRewardChest.Setup(true).Show(TypeChest.LevelChest, rewards, Reason.open_chest_level,
                GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelChestConfig.is_video_x_reward,
                GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelChestConfig.num_x_reward);
                //OpenChestBox.Setup(true).Show(TypeChest.LevelChest, rewards); 
            },
            priovity = SuggetPopupHomeController.PriovitySugget.Special
        }
            );
    }

    public void OpenChestByButton()
    {
        var rewards = ClaimReward();

        //CurrentLevelChest -= levelChestData.levelSpacing;
        //if (CurrentLevelChest < 0)
            CurrentLevelChest = -1;

        RememberLevelChest = CurrentLevelChest;
        view.OnOpenChestDone();
        GPlayFuntPuzzle.GPopupRewardChest.Setup(true).Show(TypeChest.LevelChest, rewards, Reason.open_chest_level,
GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelChestConfig.is_video_x_reward,
GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelChestConfig.num_x_reward);
        //OpenChestBox.Setup(true).Show(TypeChest.LevelChest, rewards); 
    }
}
