using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSaleController : MonoBehaviour
{
    public List<EventBase> lstEventSales;

    public void Init()
    {
        for (int i = 0; i < lstEventSales.Count; i++)
        {
            lstEventSales[i].Init();
        }
    }

    public void InitHome()
    {
        for (int i = 0; i < lstEventSales.Count; i++)
        {
            lstEventSales[i].InitHome();
        }
    }

    public void InitStateHome()
    {
        for (int i = 0; i < lstEventSales.Count; i++)
        {
            lstEventSales[i].InitStateHome();
        }
    }

    public void OnLeftHome()
    {
        for (int i = 0; i < lstEventSales.Count; i++)
        {
            lstEventSales[i].OnLeftHome();
        }
    }

    public EventBase GetEventSales(EventSaleType type)
    {
        for (int i = 0; i < lstEventSales.Count; i++)
        {
            if (lstEventSales[i].typeEvent == type)
            {
                return lstEventSales[i];
            }
        }

        return null;
    }

    public EventBase GetEventSaleActive()
    {
        for (int i = 0; i < lstEventSales.Count; i++)
        {
            if (lstEventSales[i].IsEventActive())
            {
                return lstEventSales[i];
            }
        }

        return null;
    }

    public Dictionary<EventSaleType, List<EventSaleItem>> SpawnEventSaleGamePlay(Transform parentItem)
    {
        Dictionary<EventSaleType, List<EventSaleItem>> itemsDict = new Dictionary<EventSaleType, List<EventSaleItem>>();
        for (int i = 0; i < lstEventSales.Count; i++)
        {
            int index = i;
            if (!GameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndlessConfig(lstEventSales[index].typeEvent).IsActive)
            {
                continue;
            }
            List<EventSaleItem> lstTemp = lstEventSales[index].SpawnItemEvent(parentItem);
            if (lstTemp != null)
            {
                var lstItem = new List<EventSaleItem>(lstTemp);
                if (lstItem != null)
                {
                    if (!itemsDict.ContainsKey(lstEventSales[index].typeEvent))
                    {
                        itemsDict.Add(lstEventSales[index].typeEvent, lstItem);
                    }
                }
            }
        }

        return itemsDict;
    }

    public virtual void AddItem(EventSaleType type, int amount)
    {
        for (int i = 0; i < lstEventSales.Count; i++)
        {
            if (lstEventSales[i].typeEvent == type)
            {
                lstEventSales[i].AddItem(amount);
                return;
            }
        }
    }

    public void ResetDay()
    {
        for (int i = 0; i < lstEventSales.Count; i++)
        {
            lstEventSales[i].ResetDay();
        }
    }

    public bool IsHasEventSaleReward()
    {
        for (int i = 0; i < lstEventSales.Count; i++)
        {
            if (lstEventSales[i].IsHasClaimReward())
                return true;
        }

        return false;
    }
}
