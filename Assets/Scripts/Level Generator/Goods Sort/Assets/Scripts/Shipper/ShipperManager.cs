using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Utilities;
using UnityEngine;
using static GameEnum;

public class ShipperManager : Singleton<ShipperManager>
{
    private ShipperOrder[] _currentShipperOrders;
    public ShipperOrder[] CurrentShipperOrders => _currentShipperOrders;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        LevelLoader.startLevelEvent += OnLevelStarted;
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
    }

    private IEnumerator RandomGeneratingOrders()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        yield return new WaitForSeconds(3);

        ShipperUI.Instance.Request();
    }

    public ShipperOrder[] GenerateShipperOrder()
    {
        _currentShipperOrders = new ShipperOrder[2];

        BaseItem[] validItems = ItemManager.Instance.Items.Where(item => item.gameObject.activeInHierarchy).ToArray();
        List<string> validFactions = validItems.Select(item => item.TYPE).Distinct().ToList();

        for (int i = 0; i < _currentShipperOrders.Length; i++)
        {
            string selectedFaction = validFactions[UnityEngine.Random.Range(0, validFactions.Count)];

            validFactions.Remove(selectedFaction);

            _currentShipperOrders[i] = new ShipperOrder()
            {
                faction = selectedFaction,
                maxSeconds = UnityEngine.Random.Range(80, 120)
            };
        }

        return _currentShipperOrders;
    }

    private void OnLevelStarted()
    {
        StartCoroutine(RandomGeneratingOrders());
    }
}

public class ShipperOrder
{
    public string faction;
    public int maxSeconds;
    public bool isDone;
}
