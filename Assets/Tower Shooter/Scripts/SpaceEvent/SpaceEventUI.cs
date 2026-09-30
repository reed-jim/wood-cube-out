using System.Collections.Generic;
using UnityEngine;

public class SpaceEventUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform itemContainer;

    private readonly List<SpaceEventItemUI> spawnedItems = new List<SpaceEventItemUI>();

    void Awake()
    {
        SpaceEventManager.OnEventsUpdated += RefreshList;
    }

    void OnDestroy()
    {
        SpaceEventManager.OnEventsUpdated -= RefreshList;
    }

    private void OnEnable()
    {
        RefreshList();
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
        CommonUtil.ReturnListToPool(spawnedItems, item => item.gameObject);

        // Populate new UI items
        var activeEvents = SpaceEventManager.Ins.ActiveEvents;
        foreach (var activeEvent in activeEvents)
        {
            SpaceEventItemUI spaceEventItemUI = ObjectPoolingEverything.GetFromPool<SpaceEventItemUI>(GameConstants.spaceEventItemUI);

            CommonUtil.SetParentAndResetScale(spaceEventItemUI.transform, itemContainer);

            spaceEventItemUI.Bind(activeEvent);

            spawnedItems.Add(spaceEventItemUI);
        }
    }
}
