using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DailyLoginController : MonoBehaviour
{
    private CoreGameData.DailyLoginData dailyLoginConfig;

    public static DateTime LastTimeClaimReward
    {
        get
        {
            if (GPlayerPrefs.HasKey(StringHelper.LAST_TIME_CLAIM_REWARD_DAILY_LOGIN))
            {
                var temp = Convert.ToInt64(GPlayerPrefs.GetString(StringHelper.LAST_TIME_CLAIM_REWARD_DAILY_LOGIN));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                GPlayerPrefs.SetString(StringHelper.LAST_TIME_CLAIM_REWARD_DAILY_LOGIN, newDateTime.ToBinary().ToString());
                GPlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            GPlayerPrefs.SetString(StringHelper.LAST_TIME_CLAIM_REWARD_DAILY_LOGIN, value.ToBinary().ToString());
            GPlayerPrefs.Save();
        }
    }

    public static int CurrentReward
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_ID_DAILY_REWARD, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.CURRENT_ID_DAILY_REWARD, value);
            GPlayerPrefs.Save();
        }
    }

    public static int CurrentWeekReward
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_WEEK_DAILY_REWARD, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.CURRENT_WEEK_DAILY_REWARD, value);
            PlayerPrefs.Save();
        }
    }

    public static bool IsClaimRewardToDay
    {
        get
        {

            return PlayerPrefs.GetInt(StringHelper.IS_CLAIMED_REWARD_DAILY_LOGIN, 0) == 1;

        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.IS_CLAIMED_REWARD_DAILY_LOGIN, value ? 1 : 0);
            GPlayerPrefs.Save();

        }
    }

    [HideInInspector] public bool isHasReward;

    [SerializeField] protected DailyLoginButton dailyLoginButton;

    private bool isInited;
    public void Init()
    {
        dailyLoginConfig = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.dailyLoginConfig;
        isHasReward = CheckRewardLogin();

        if (UseProfile.CurrentLevel > dailyLoginConfig.unlockLevel)
        {
            InitState();
        }
        else if (UseProfile.CurrentLevel == dailyLoginConfig.unlockLevel)
        {
            if (isHasReward)
            {
                HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow();
                HomeController.Instance.suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
                {
                    actionShow = () => { DailyLoginBox.Setup(true).Show(); },
                    priovity = SuggetPopupHomeController.PriovitySugget.HighQuality
                });
            }
            InitState();
        }
        else
        {
            dailyLoginButton.gameObject.SetActive(false);
        }
        isInited = true;
    }

    public void InitState()
    {
        if (UseProfile.CurrentLevel < dailyLoginConfig.unlockLevel)
            return;
        isHasReward = CheckRewardLogin();
        BaseDataButtonEvent btnData = new BaseDataButtonEvent();
        if (!isHasReward)
        {
            btnData.timeLife = TimeManager.TimeLeftPassTheDay(UnbiasedTime.Instance.Now());
        }
        else
        {
            btnData.timeLife = -1000;
        }

        dailyLoginButton.gameObject.SetActive(true);
        dailyLoginButton.transform.SetAsFirstSibling();
        if (!isInited)
            dailyLoginButton.Init(btnData, OnClickDailyLogin, isReplaceIcon: false);
        else
            dailyLoginButton.InitState(btnData);
    }

    public bool CheckRewardLogin()
    {
        long timePass = TimeManager.CaculateTime(TimeManager.ParseTimeStartDay(LastTimeClaimReward), TimeManager.ParseTimeStartDay(UnbiasedTime.Instance.Now()));
        if (timePass >= 86400)
        {
            IsClaimRewardToDay = false;
            if (timePass > 86400)
            {
                CurrentReward = 0;
                CurrentWeekReward = 0;
            }

            return true;
        }

        return false;
    }

    public void OnClickDailyLogin()
    {
        DailyLoginBox.Setup().Show();
    }

    public void ClaimReward()
    {
        LastTimeClaimReward = UnbiasedTime.Instance.Now();
        IsClaimRewardToDay = true;
        CurrentReward++;
        if (CurrentReward >= GameController.Instance.dataContain.dailyLoginDatabase.rewardDays.Length)
        {
            CurrentReward = 0;
            CurrentWeekReward++;
        }
        InitState();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            var data = GameController.Instance.dataContain.dailyLoginDatabase.rewardDays[6];
            List<RewardRandom> rewardsShow = new List<RewardRandom>();
            for (int i = 0; i < data.rewards.Length; i++)
            {
                int index = i;
                //GameController.Instance.dataContain.giftDatabase.Claim(data.rewards[index].typeItem, data.rewards[index].amount, Reason.daily_login, isUnlimited: data.rewards[index].isUnlimited);
                rewardsShow.Add(data.rewards[index]);
            }

            GPlayFuntPuzzle.GPopupRewardChest.Setup().Show(TypeChest.DailyLoginChest, rewardsShow, Reason.daily_login, 
                GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.dailyLoginConfig.is_video_x_reward, 
                GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.dailyLoginConfig.num_x_reward);
            //OpenChestBox.Setup().Show(TypeChest.DailyLoginChest, rewardsShow);
        }    
    }
}
