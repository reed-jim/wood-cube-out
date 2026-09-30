using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpaceEventManager : MonoBehaviour
{
    public static SpaceEventManager Instance { get; private set; }

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
    public event Action OnEventsUpdated;

    public IReadOnlyList<ActiveSpaceEvent> ActiveEvents => activeEvents;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        
        OnEventStarted?.Invoke(newEvent);
        OnEventsUpdated?.Invoke();
    }

    private void ExpireEvent(ActiveSpaceEvent activeEvent)
    {
        activeEvents.Remove(activeEvent);
        OnEventExpired?.Invoke(activeEvent);
        OnEventsUpdated?.Invoke();
    }

    #region Save & Load API Hooks

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
