using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpaceEventManager : Singleton<SpaceEventManager>
{
    [Header("Configurations")]
    [SerializeField] private List<SpaceEventSO> availableEventPool;
    [SerializeField] private float spawnInterval = 60f;
    [SerializeField] private int maxSimultaneousEvents = 3;

    // Runtime state tracking
    private readonly List<ActiveSpaceEvent> activeEvents = new List<ActiveSpaceEvent>();
    private float spawnTimer;

    // Events for UI and Buff systems to subscribe to
    public event Action<ActiveSpaceEvent> OnEventStarted;
    public event Action<ActiveSpaceEvent> OnEventExpired;
    public static event Action OnEventsUpdated;

    public IReadOnlyList<ActiveSpaceEvent> ActiveEvents => activeEvents;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        LoadSpaceEventData();
    }

    private void Update()
    {
        TickSpawns();
        TickActiveEvents(Time.deltaTime);
    }

    private void TickSpawns()
    {
        if (activeEvents.Count >= maxSimultaneousEvents) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnRandomEvent();
        }
    }

    private void TickActiveEvents(float deltaTime)
    {
        for (int i = activeEvents.Count - 1; i >= 0; i--)
        {
            activeEvents[i].RemainingTime -= deltaTime;
            if (activeEvents[i].RemainingTime <= 0)
            {
                ExpireEvent(activeEvents[i]);
            }
        }
    }

    public void TrySpawnRandomEvent()
    {
        if (availableEventPool.Count == 0) return;

        SpaceEventSO randomConfig = availableEventPool[UnityEngine.Random.Range(0, availableEventPool.Count)];
        StartEvent(randomConfig, randomConfig.durationInSeconds);
    }

    private void StartEvent(SpaceEventSO config, float duration)
    {
        ActiveSpaceEvent newEvent = new ActiveSpaceEvent(config, duration);
        activeEvents.Add(newEvent);

        ApplySpaceEventEffects();

        OnEventStarted?.Invoke(newEvent);
        OnEventsUpdated?.Invoke();
    }

    private void ExpireEvent(ActiveSpaceEvent activeEvent)
    {
        activeEvents.Remove(activeEvent);

        ApplySpaceEventEffects();

        OnEventExpired?.Invoke(activeEvent);
        OnEventsUpdated?.Invoke();
    }

    #region Effect
    private void ApplySpaceEventEffects()
    {
        Dictionary<StatType, StatModifier> statModifierDictionary = new Dictionary<StatType, StatModifier>();

        for (int i = 0; i < ActiveEvents.Count; i++)
        {
            ActiveSpaceEvent activeSpaceEvent = ActiveEvents[i];

            for (int j = 0; j < activeSpaceEvent.Config.buffs.Count; j++)
            {
                StatModifierData buff = activeSpaceEvent.Config.buffs[j];

                if (statModifierDictionary.ContainsKey(buff.statType))
                {
                    statModifierDictionary[buff.statType].Value += buff.value;
                }
                else
                {
                    statModifierDictionary.Add(buff.statType, new StatModifier(buff.statType, buff.modType, buff.value, statModifierSource: StatModifierSource.SpaceEvent));
                }
            }
        }

        for (int i = 0; i < NodeManager.Ins.nodes.Count; i++)
        {
            BaseCharacter node = NodeManager.Ins.nodes[i];

            if (node.nodeData.isUnlocked)
            {
                node.serviceLocator.stats.RemoveAllModifiersFromSource(StatModifierSource.SpaceEvent);

                foreach (var statType in statModifierDictionary.Keys)
                {
                    node.serviceLocator.stats.AddModifier(statModifierDictionary[statType]);
                }
            }
        }
    }
    #endregion

    #region Save & Load API Hooks

    public void LoadSpaceEventData()
    {
        SpaceEventSaveData spaceEventSaveData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey("spaceEventSaveData"), GetDefaultSpaceEventData());

        LoadSaveData(spaceEventSaveData);
    }

    public void SaveSpaceEventData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey("spaceEventSaveData"), GetSaveData());
    }

    public SpaceEventSaveData GetDefaultSpaceEventData()
    {
        SpaceEventSaveData spaceEventSaveData = new SpaceEventSaveData();

        spaceEventSaveData.activeEvents = new List<SpaceEventData>();

        return spaceEventSaveData;
    }

    public SpaceEventSaveData GetSaveData()
    {
        SpaceEventSaveData saveData = new SpaceEventSaveData
        {
            activeEvents = activeEvents.Select(e => new SpaceEventData(e.Config.eventID, e.RemainingTime)).ToList()
        };
        return saveData;
    }

    public void LoadSaveData(SpaceEventSaveData saveData)
    {
        // Clear current running events
        for (int i = activeEvents.Count - 1; i >= 0; i--)
        {
            ExpireEvent(activeEvents[i]);
        }

        if (saveData == null || saveData.activeEvents == null) return;

        foreach (var data in saveData.activeEvents)
        {
            SpaceEventSO config = availableEventPool.FirstOrDefault(e => e.eventID == data.eventID);
            if (config != null && data.remainingTime > 0)
            {
                StartEvent(config, data.remainingTime);
            }
        }
    }

    #endregion
}

public class ActiveSpaceEvent
{
    public SpaceEventSO Config { get; }
    public float RemainingTime { get; set; }

    public ActiveSpaceEvent(SpaceEventSO config, float remainingTime)
    {
        Config = config;
        RemainingTime = remainingTime;
    }
}
