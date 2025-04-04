using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static ChallengeSegemtData;

public enum ChallengeType
{
    WeeklyChallenge = 0,
    RoyalPass = 1,
    CoinChallenge = 2,
    DiamodChallenge = 3,
    RaceSpaceChallenge = 4
}

public class ChallengeString
{
    public const string CURRENT_NUM_ITEM = "current_num_item_";
    public const string REMEMBER_CURRENT_NUM_ITEM = "remember_current_num_item_";
    public const string CURRENT_INDEX_REWARD = "current_index_reward_";

    public const string TIME_END_CHALLENGE = "time_end_challenge_";
}

[System.Serializable]
public class DataChallenge
{
    protected ChallengeType type;

    public int currentNumItemSync;
    public int CurrentNumItem
    {
        get
        {
            return PlayerPrefs.GetInt(ChallengeString.CURRENT_NUM_ITEM + type, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(ChallengeString.CURRENT_NUM_ITEM + type, value);
            GPlayerPrefs.Save();
        }
    }

    public int currentIndexRewardSync;
    public int CurrentIndexReward
    {
        get
        {
            return PlayerPrefs.GetInt(ChallengeString.CURRENT_INDEX_REWARD + type, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(ChallengeString.CURRENT_INDEX_REWARD + type, value);
            GPlayerPrefs.Save();
        }
    }

    public int rememberCurrentNumItemSync;
    public int RememberCurrentNumItem
    {
        get
        {
            return PlayerPrefs.GetInt(ChallengeString.REMEMBER_CURRENT_NUM_ITEM + type, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(ChallengeString.REMEMBER_CURRENT_NUM_ITEM + type, value);
            GPlayerPrefs.Save();
        }
    }

    public long timeEndChallengeSync;
    public DateTime TimeEndChallenge
    {
        get
        {
            if (GPlayerPrefs.HasKey(ChallengeString.TIME_END_CHALLENGE + type))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString(ChallengeString.TIME_END_CHALLENGE + type));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                GPlayerPrefs.SetString(ChallengeString.TIME_END_CHALLENGE + type, newDateTime.ToBinary().ToString());
                GPlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            GPlayerPrefs.SetString(ChallengeString.TIME_END_CHALLENGE + type, value.ToBinary().ToString());
            GPlayerPrefs.Save();
        }
    }

    public DataChallenge()
    {

    }

    public DataChallenge(ChallengeType type)
    {
        this.type = type;
    }

    public static DataChallenge GetData(ChallengeType type)
    {
        DataChallenge dt = new DataChallenge(type);

        dt.currentNumItemSync = PlayerPrefs.GetInt(ChallengeString.CURRENT_NUM_ITEM + type, 0);
        dt.currentIndexRewardSync = PlayerPrefs.GetInt(ChallengeString.CURRENT_INDEX_REWARD + type, 0);
        dt.rememberCurrentNumItemSync = PlayerPrefs.GetInt(ChallengeString.REMEMBER_CURRENT_NUM_ITEM + type, 0);

        if (GPlayerPrefs.HasKey(ChallengeString.TIME_END_CHALLENGE + type))
        {
            var temp = Convert.ToInt64(PlayerPrefs.GetString(ChallengeString.TIME_END_CHALLENGE + type));
            dt.timeEndChallengeSync = temp;
        }
        else
        {
            var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
            dt.timeEndChallengeSync = newDateTime.ToBinary();
        }

        return dt;
    }

    public static void SetData(ChallengeType type, DataChallenge data)
    {
        GPlayerPrefs.SetInt(ChallengeString.CURRENT_NUM_ITEM + type, data.currentNumItemSync);
        GPlayerPrefs.SetInt(ChallengeString.CURRENT_INDEX_REWARD + type, data.currentIndexRewardSync);
        GPlayerPrefs.SetString(ChallengeString.TIME_END_CHALLENGE + type, data.timeEndChallengeSync.ToString());
        GPlayerPrefs.SetInt(ChallengeString.REMEMBER_CURRENT_NUM_ITEM + type, data.rememberCurrentNumItemSync);
    }

    public void ResetData()
    {
        CurrentNumItem = 0;
        CurrentIndexReward = 0;
        RememberCurrentNumItem = 0;
    }

    public virtual void ResetDay()
    {

    }
}


public abstract class ChallengeBase : MonoBehaviour
{
    public ChallengeType typeChallenge;
    public Reason reasonClaimReward;
    [HideInInspector] public DataChallenge data;
    [HideInInspector] public ChallengeConfigData config;
    [HideInInspector] public ChallengeViewBase view;

    public UnityAction actionOpenBox;

    public UnityAction actionCoolDownEndDoneInHome;
    protected float timeCoolDownEnd;
    protected float timerCoolDownEnd;
    protected bool isCoolDownEnd;

    public float TimeLeft => timerCoolDownEnd;

    protected SuggetPopupHomeController.PriovitySugget priovitySugget;

    public ItemChallenge itemChallengePrefab;
    public Sprite iconItem;



    public virtual int CurrentItemRequirement
    {
        get
        {
            return config.rewards[Mathf.Clamp(data.CurrentIndexReward, 0, config.rewards.Count - 1)].numItemRequire;
        }
    }

    public virtual bool IsClaimAllReward
    {
        get
        {
            return data.CurrentIndexReward >= config.rewards.Count;
        }
    }

    public virtual void Init()
    {
        data = new DataChallenge(typeChallenge);
        config = GameController.Instance.segmentController.GetCurrentSegment().challengeData.GetChallengeConfig(typeChallenge);
    }

    public virtual void InitHome()
    {
        isCoolDownEnd = false;
        config = GameController.Instance.segmentController.GetCurrentSegment().challengeData.GetChallengeConfig(typeChallenge);
        priovitySugget = SuggetPopupHomeController.PriovitySugget.Special;
        if (IsChallengeActive())
        {
            if (data.TimeEndChallenge > UnbiasedTime.Instance.Now())
            {
                timeCoolDownEnd = TimeManager.CaculateTime(UnbiasedTime.Instance.Now(), data.TimeEndChallenge);
                timerCoolDownEnd = timeCoolDownEnd;
                isCoolDownEnd = true;
            }
            else
            {
                isCoolDownEnd = false;
            }
        }

        CheckChallenge();
        view.Init();

    }

    public virtual void InitStateHome()
    {
        view.InitState();

        if (data.CurrentNumItem < CurrentItemRequirement)
        {
            data.RememberCurrentNumItem = data.CurrentNumItem;
        }
    }

    public virtual bool IsChallengeActive()
    {
        Debug.Log("Checked");
        if (!config.IsActive)
            return false;
        if (UseProfile.CurrentLevel < config.level_unlock)
            return false;
        if (UnbiasedTime.Instance.Now() > data.TimeEndChallenge)
            return false;

        return true;
    }

    public virtual bool IsHasClaimReward()
    {
        if (!IsChallengeActive())
            return false;

        if (data.CurrentNumItem >= CurrentItemRequirement)
            return true;

        return false;
    }

    public bool IsCanSpawnItem()
    {
        if (!IsChallengeActive())
            return false;

        return IsCanSpawnItemHandle();
    }

    protected virtual bool IsCanSpawnItemHandle()
    {
        return true;
    }

    public virtual void CheckChallenge()
    {
        if (!IsChallengeActive())
            return;

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

    protected virtual List<RewardRandom> ClaimReward()
    {
        List<RewardRandom> rewards = new List<RewardRandom>();
        for (int i = 0; i < config.rewards.Count; i++)
        {
            var reward = config.rewards[i].GetReward();
            if (reward.typeItem != GiftType.None)
            {
                GameController.Instance.dataContain.giftDatabase.Claim(reward.typeItem, reward.amount, reasonClaimReward, reward.isUnlimited);
                rewards.Add(reward);
            }
        }

        return rewards;
    }

    protected virtual void ActionOpenBox(List<RewardRandom> rewards)
    {
        HomeController.Instance.suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
        {
            actionShow = () =>
            {
                data.RememberCurrentNumItem = data.CurrentNumItem;
                data.CurrentIndexReward++;
                OpenBox(isSuggetHome: true);
                view.OnOpenBoxDone();
            },
            priovity = priovitySugget
        }
            );

    }

    public virtual List<ItemChallenge> SpawnItemChallegen(Transform parent)
    {
        if (!IsChallengeActive())
            return null;

        List<ItemChallenge> lstItem = new List<ItemChallenge>();

        int numItemSpawn = 0;
        if (UseProfile.CurrentLevel == config.level_unlock)//cho Tutorial
            numItemSpawn = 3;
        else
            numItemSpawn = config.GetNumItemSpawn();
        for (int i = 0; i < numItemSpawn; i++)
        {
            if (itemChallengePrefab != null)
            {
                var it = Instantiate(itemChallengePrefab, parent);
                lstItem.Add(it);
            }
        }

        return lstItem;
    }

    public int GetNumItemSpawn()
    {
        return config.GetNumItemSpawn();
    }

    public virtual void AddItem(int amount)
    {
        data.CurrentNumItem += amount;
    }

    public virtual void ResetSeason()
    {
        data.ResetData();
    }

    public virtual void ResetDay()
    {
        data.ResetDay();
    }

    public abstract void OpenBox(bool isSuggetHome = false);

    protected virtual void CoolDownEnd()
    {
        if (isCoolDownEnd)
        {
            timerCoolDownEnd -= Time.unscaledDeltaTime;
            if (view != null)
            {
                view.ShowCoolDownEnd(timerCoolDownEnd);
            }

            if (timerCoolDownEnd <= 0)
            {
                OnCoolDownEndDone();
                isCoolDownEnd = false;
            }
        }
    }

    protected virtual void OnCoolDownEndDone()
    {
        actionCoolDownEndDoneInHome?.Invoke();
    }

    protected virtual void Update()
    {
        CoolDownEnd();
    }

    public virtual void OnLeftHome()
    {
        actionCoolDownEndDoneInHome = null;
        actionOpenBox = null;
    }
}
