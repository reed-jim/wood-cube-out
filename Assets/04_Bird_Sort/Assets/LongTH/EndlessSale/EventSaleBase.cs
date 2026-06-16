using System.Collections.Generic;
using System;
using UnityEngine;
using static SaleSegemntData;
using UnityEngine.Events;
using Castle.Core.Internal;

public enum EventSaleType
{
    None = 0,
    Halloween = 1,
    Noel = 2,
    NewYearEve = 3
}

public class EventString
{
    public const string CURRENT_NUM_ITEM = "current_num_item_"; // t?ng item hi?n t?i
    public const string REMEMBER_CURRENT_NUM_ITEM = "remember_current_num_item_"; // item c?a 1 stage
    public const string CURRENT_INDEX_REWARD = "current_index_reward_"; // reward hi?n t?i

    public const string TIME_END_EVENT = "time_end_event_";
}

[System.Serializable]
public class DataEvent
{
    protected EventSaleType type;

    public int currentNumItemSync;
    public int CurrentNumItem
    {
        get
        {
            return PlayerPrefs.GetInt(EventString.CURRENT_NUM_ITEM + type, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(EventString.CURRENT_NUM_ITEM + type, value);
            GPlayerPrefs.Save();
        }
    }

    public int currentIndexRewardSync;
    public int CurrentIndexReward
    {
        get
        {
            return PlayerPrefs.GetInt(EventString.CURRENT_INDEX_REWARD + type, 1);
        }
        set
        {
            GPlayerPrefs.SetInt(EventString.CURRENT_INDEX_REWARD + type, value);
            GPlayerPrefs.Save();
        }
    }

    public int rememberCurrentNumItemSync;
    public int RememberCurrentNumItem
    {
        get
        {
            return PlayerPrefs.GetInt(EventString.REMEMBER_CURRENT_NUM_ITEM + type, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(EventString.REMEMBER_CURRENT_NUM_ITEM + type, value);
            GPlayerPrefs.Save();
        }
    }

    public long timeEndEventSync;
    public DateTime TimeEndEvent
    {
        get
        {
            if (GPlayerPrefs.HasKey(EventString.TIME_END_EVENT + type))
            {
                return DateTime.Parse(PlayerPrefs.GetString(EventString.TIME_END_EVENT + type));
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                GPlayerPrefs.SetString(EventString.TIME_END_EVENT + type, newDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                GPlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            GPlayerPrefs.SetString(EventString.TIME_END_EVENT + type, value.ToString("yyyy-MM-dd HH:mm:ss"));
            GPlayerPrefs.Save();
        }
    }

    public DataEvent()
    {

    }

    public DataEvent(EventSaleType type)
    {
        this.type = type;
    }

    public static DataEvent GetData(EventSaleType type)
    {
        DataEvent dt = new DataEvent(type);

        dt.currentNumItemSync = PlayerPrefs.GetInt(EventString.CURRENT_NUM_ITEM + type, 0);
        dt.currentIndexRewardSync = PlayerPrefs.GetInt(EventString.CURRENT_INDEX_REWARD + type, 0);
        dt.rememberCurrentNumItemSync = PlayerPrefs.GetInt(EventString.REMEMBER_CURRENT_NUM_ITEM + type, 0);

        if (GPlayerPrefs.HasKey(EventString.TIME_END_EVENT + type))
        {
            var temp = Convert.ToInt64(PlayerPrefs.GetString(EventString.TIME_END_EVENT + type));
            dt.timeEndEventSync = temp;
        }
        else
        {
            var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
            dt.timeEndEventSync = newDateTime.ToBinary();
        }

        return dt;
    }

    public static void SetData(EventSaleType type, DataEvent data)
    {
        GPlayerPrefs.SetInt(EventString.CURRENT_NUM_ITEM + type, data.currentNumItemSync);
        GPlayerPrefs.SetInt(EventString.CURRENT_INDEX_REWARD + type, data.currentIndexRewardSync);
        GPlayerPrefs.SetString(EventString.TIME_END_EVENT + type, data.timeEndEventSync.ToString());
        GPlayerPrefs.SetInt(EventString.REMEMBER_CURRENT_NUM_ITEM + type, data.rememberCurrentNumItemSync);
    }

    public void ResetData()
    {
        CurrentNumItem = 0;
        CurrentIndexReward = 1;
        RememberCurrentNumItem = 0;
    }

    public virtual void ResetDay()
    {

    }
}


public abstract class EventBase : MonoBehaviour
{
    public EventSaleType typeEvent;
    public Reason reasonClaimReward;
    [HideInInspector] public DataEvent data;
    [HideInInspector] public EndlessSale config;
    [HideInInspector] public EventSaleViewBase view;

    public UnityAction actionOpenBox;

    public UnityAction actionCoolDownEndDoneInHome;
    protected float timerCoolDownEnd;
    protected bool isCoolDownEnd;

    public float TimeLeft => timerCoolDownEnd;

    protected SuggetPopupHomeController.PriovitySugget priovitySugget;

    public EventSaleItem itemEventPrefab;
    public Sprite iconItem;



    public virtual int CurrentItemRequirement
    {
        get
        {
            return config.rewardData[Mathf.Clamp(data.CurrentIndexReward, 0, config.rewardData.Count - 1)].requireToken;
        }
    }

    public virtual bool IsClaimAllReward
    {
        get
        {
            return data.CurrentIndexReward >= config.rewardData.Count;
        }
    }

    public virtual bool EndlessEnd
    {
        get
        {
            return PlayerPrefs.GetInt("Endless_done", 0) == 1;
        }
        set
        {
            GPlayerPrefs.SetInt("Endless_done", value ? 1 : 0);
            GPlayerPrefs.Save();
        }
    }

    public virtual void Init()
    {
        data = new DataEvent(typeEvent);
        config = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndlessConfig(typeEvent);
    }

    public virtual void InitHome()
    {
        isCoolDownEnd = false;
        config = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndlessConfig(typeEvent);
        priovitySugget = SuggetPopupHomeController.PriovitySugget.Special;
        if (IsEventActive())
        {
            if (data.TimeEndEvent > UnbiasedTime.Instance.Now())
            {
                timerCoolDownEnd = TimeManager.CaculateTime(UnbiasedTime.Instance.Now(), data.TimeEndEvent);
                isCoolDownEnd = true;
            }
            else
            {
                isCoolDownEnd = false;
            }
        }
        CheckEvent();
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

    public virtual bool IsEventActive()
    {
        if (!config.IsActive)
            return false;
        if (UseProfile.CurrentLevel < config.level_unlock)
            return false;

        if (EndlessEnd)
            return false;

        if (UnbiasedTime.Instance.Now() > data.TimeEndEvent)
            return false;

        return true;
    }

    public virtual int LevelBackToHome()
    {
        if (config.IsActive)
        {
            return config.level_unlock;
        }

        return 0;
    }

    public virtual bool IsHasClaimReward()
    {
        if (!IsEventActive())
            return false;

        if (data.CurrentNumItem >= CurrentItemRequirement)
            return true;

        return false;
    }

    public bool IsCanSpawnItem()
    {
        if (!IsEventActive())
            return false;

        return IsCanSpawnItemHandle();
    }

    protected virtual bool IsCanSpawnItemHandle()
    {
        return true;
    }

    public virtual void CheckEvent()
    {
        if (!IsEventActive())
            return;

        if (data.RememberCurrentNumItem != data.CurrentNumItem)
        {
            if (data.CurrentNumItem >= CurrentItemRequirement)
            {
                HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow();

                actionOpenBox = () =>
                {
                    ActionOpenBox();
                    ClaimReward();
                    actionOpenBox = null;
                };
            }
        }
    }

    protected virtual List<RewardRandom> ClaimReward()
    {
        List<RewardRandom> rewards = new List<RewardRandom>();
        for (int i = 0; i < config.rewardData.Count; i++)
        {
            var reward = config.GetReward();
            foreach (var item in reward.rewardData)
            {
                if (item.typeItem != GiftType.None)
                {
                    BirdSortGameController.Instance.dataContain.giftDatabase.Claim(item.typeItem, item.amount, reasonClaimReward, item.isUnlimited);
                    rewards.Add(item);
                }
            }

        }
        return rewards;
    }

    protected virtual void ActionOpenBox()
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

    public virtual List<EventSaleItem> SpawnItemEvent(Transform parent)
    {
        if (!IsEventActive())
            return null;

        List<EventSaleItem> lstItem = new List<EventSaleItem>();

        int numItemSpawn = 0;
        if (UseProfile.CurrentLevel == config.level_unlock)//cho Tutorial
            numItemSpawn = 3;
        else
            numItemSpawn = config.GetNumItemSpawn();
        for (int i = 0; i < numItemSpawn; i++)
        {
            if (itemEventPrefab != null)
            {
                var it = Instantiate(itemEventPrefab, parent);
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
                view.ShowCoolDownEnd((long)timerCoolDownEnd);
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

