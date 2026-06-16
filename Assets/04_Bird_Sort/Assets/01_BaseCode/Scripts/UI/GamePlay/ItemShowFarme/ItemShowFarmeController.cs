using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShowFarmeController : MonoBehaviour
{
    [SerializeField] private ItemShowFarme itemShowFarmePrefab;
    [SerializeField] private Transform parentItem;
    private Dictionary<ChallengeType, ItemShowFarme> poolItem;
    private Dictionary<EventSaleType, ItemShowFarme> poolItemEvent;

    public void Init()
    {
        poolItem = new Dictionary<ChallengeType, ItemShowFarme>();
        poolItemEvent = new Dictionary<EventSaleType, ItemShowFarme>();
   
    }

    public void SetUpFrame(ChallengeType type)
    {
        var farme = Instantiate(itemShowFarmePrefab, parentItem);
        farme.Init(type);

        poolItem.Add(type, farme);
        farme.Close();
    }
    public void SetUpFrame(EventSaleType type)
    {
        var farme = Instantiate(itemShowFarmePrefab, parentItem);
        farme.Init(type);

        poolItemEvent.Add(type, farme);
        farme.Close();
    }

    public ItemShowFarme GetFarme(ChallengeType type)
    {
        if (poolItem.ContainsKey(type))
            return poolItem[type];
        else
            return null;
    }

    public ItemShowFarme GetFarme(EventSaleType type)
    {
        if (poolItemEvent.ContainsKey(type))
            return poolItemEvent[type];
        else
            return null;
    }
}
