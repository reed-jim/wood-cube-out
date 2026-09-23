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

    public void CreateElectionCompleteEvent(LeadershipCharacterData leadershipCharacterData)
    {
        ElectionCompleteEventDataItem electionCompleteEventData = new ElectionCompleteEventDataItem();

        electionCompleteEventData.eventType = IngameEventType.ElectionComplete;
        electionCompleteEventData.leadershipCharacterData = leadershipCharacterData;

        AddIngameEventData(electionCompleteEventData);
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

    public virtual object[] GetParams()
    {
        return null;
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
