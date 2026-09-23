using System;
using System.Collections.Generic;
using UnityEngine;

public enum IngameEventType
{
    MilestoneReached,
    NewStarDiscovered,
    ElectionComplete,
    CharacterDeath
}

public class IngameEventManager : Singleton<IngameEventManager>
{
    public IngameEventScreen ingameEventScreen;
    public IngameEventDetailUI ingameEventDetailUI;

    public IngameEventData ingameEventData;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        MultiverseIdleGameplayScreen.openIngameEventScreenEvent += ShowIngameEventScreen;
    }

    void OnDestroy()
    {
        MultiverseIdleGameplayScreen.openIngameEventScreenEvent -= ShowIngameEventScreen;
    }

    public void ShowIngameEventScreen()
    {
        ingameEventScreen.Show(ingameEventData);
    }

    public void ShowIngameEventDetail(IngameEventType eventType, params object[] args)
    {
        ingameEventDetailUI.ShowIngameEventDetail(eventType, args);
    }

    public void CreateStarDiscoveryEvent(StarPathConfig starPathConfig)
    {
        StarDiscoveryEventDataItem starDiscoveryEventDataItem = new StarDiscoveryEventDataItem();

        starDiscoveryEventDataItem.dateTimeString = IdleTimeManager.Ins.GetCurrentDateTime();
        starDiscoveryEventDataItem.eventType = IngameEventType.NewStarDiscovered;
        starDiscoveryEventDataItem.starPathConfigId = starPathConfig.id;

        AddIngameEventData(starDiscoveryEventDataItem);
    }

    public void CreateElectionCompleteEvent(LeadershipCharacterData leadershipCharacterData)
    {
        ElectionCompleteEventDataItem electionCompleteEventDataItem = new ElectionCompleteEventDataItem();

        electionCompleteEventDataItem.dateTimeString = IdleTimeManager.Ins.GetCurrentDateTime();
        electionCompleteEventDataItem.eventType = IngameEventType.ElectionComplete;
        electionCompleteEventDataItem.leadershipCharacterData = leadershipCharacterData;

        AddIngameEventData(electionCompleteEventDataItem);
    }

    public void CreateCharacterDeathEvent(LeadershipCharacterData leadershipCharacterData)
    {
        ElectionCompleteEventDataItem electionCompleteEventDataItem = new ElectionCompleteEventDataItem();

        electionCompleteEventDataItem.dateTimeString = IdleTimeManager.Ins.GetCurrentDateTime();
        electionCompleteEventDataItem.eventType = IngameEventType.CharacterDeath;
        electionCompleteEventDataItem.leadershipCharacterData = leadershipCharacterData;

        AddIngameEventData(electionCompleteEventDataItem);
    }

    #region Save/Load
    private void LoadIngameEventData()
    {
        ingameEventData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey("ingameEventData"), GetDefaultIngameEventData());
    }

    private void SaveIngameEventData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey("ingameEventData"), ingameEventData);
    }

    private void AddIngameEventData(IngameEventDataItem ingameEventDataItem)
    {
        ingameEventData.ingameEventDataItems.Add(ingameEventDataItem);

        SaveIngameEventData();
    }

    private IngameEventData GetDefaultIngameEventData()
    {
        IngameEventData ingameEventData = new IngameEventData();

        ingameEventData.ingameEventDataItems = new List<IngameEventDataItem>();

        return ingameEventData;
    }
    #endregion
}

[Serializable]
public class IngameEventData
{
    public List<IngameEventDataItem> ingameEventDataItems;
}

[Serializable]
public class IngameEventDataItem
{
    public IngameEventType eventType;
    public string title;
    public string description;
    public string dateTimeString;

    public virtual object[] GetParams()
    {
        return null;
    }
}

public class StarDiscoveryEventDataItem : IngameEventDataItem
{
    public int starPathConfigId;

    public override object[] GetParams()
    {
        return new object[] { starPathConfigId };
    }
}

public class ElectionCompleteEventDataItem : IngameEventDataItem
{
    public LeadershipCharacterData leadershipCharacterData;

    public override object[] GetParams()
    {
        return new object[] { leadershipCharacterData };
    }
}
