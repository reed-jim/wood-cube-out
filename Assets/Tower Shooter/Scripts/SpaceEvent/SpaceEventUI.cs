using System.Collections.Generic;
using UnityEngine;

public class SpaceEventUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private SpaceEventItemUI itemPrefab;

    private readonly List<SpaceEventItemUI> spawnedItems = new List<SpaceEventItemUI>();

    private void OnEnable()
    {
        if (SpaceEventManager.Instance != null)
        {
            SpaceEventManager.Instance.OnEventsUpdated += RefreshList;
            RefreshList();
        }
    }

    private void OnDisable()
    {
        if (SpaceEventManager.Instance != null)
        {
            SpaceEventManager.Instance.OnEventsUpdated -= RefreshList;
        }
    }

    private void Update()
    {
        // Update countdown timers smoothly every frame without rebuilding the list
        foreach (var item in spawnedItems)
        {
            item.UpdateUI();
        }
    }

    private void RefreshList()
    {
        // Clear old UI
        foreach (var item in spawnedItems)
        {
            Destroy(item.gameObject);
        }
        spawnedItems.Clear();

        // Populate new UI items
        var activeEvents = SpaceEventManager.Instance.ActiveEvents;
        foreach (var activeEvent in activeEvents)
        {
            SpaceEventItemUI newItem = Instantiate(itemPrefab, container);
            newItem.Bind(activeEvent);
            spawnedItems.Add(newItem);
        }
    }
}
