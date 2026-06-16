using BirdSort2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class EventSaleInGameController : SerializedMonoBehaviour
{
    public Dictionary<EventSaleType, List<EventSaleItem>> saleItems = new Dictionary<EventSaleType, List<EventSaleItem>>();
    [SerializeField] protected Transform parentItem;
    public Dictionary<EventSaleType, int> scoreItems = new Dictionary<EventSaleType, int>();
    public Dictionary<EventSaleType, int> PredictItems = new Dictionary<EventSaleType, int>();

    private Dictionary<EventSaleType, int> numItemAppearDoneDict = new Dictionary<EventSaleType, int>();
    public List<BirdSort2.BirdBase> lsBird;
    internal void SpawnItems()
    {
        saleItems = new Dictionary<EventSaleType, List<EventSaleItem>>(BirdSortGameController.Instance.eventSaleController.SpawnEventSaleGamePlay(parentItem));

        lsBird = new List<BirdSort2.BirdBase>();
        var tempLsStand = GamePlayController_BirdSort.Instance.level.standsInLevel_Real;

        foreach (var item in tempLsStand)
        {
            for (int i = 0; i < item.lsBirdNoExtra.Count; i++)
            {
                lsBird.Add(item.lsBirdNoExtra[i]);
            }

        }

        foreach (var lstItem in saleItems)
        {
            if (!PredictItems.ContainsKey(lstItem.Key))
            {
                PredictItems.Add(lstItem.Key, lstItem.Value.Count);
            }
            if (!numItemAppearDoneDict.ContainsKey(lstItem.Key))
            {
                numItemAppearDoneDict.Add(lstItem.Key, 0);
            }
            for (int i = 0; i < lstItem.Value.Count; i++)
            {
                int index = i;
                BirdSort2.BirdBase birdRandom = randomBird();
                birdRandom.lsItemEvent.Add(lstItem.Value[index]);
                //lstItem.Value[index].checkPointIn = checkPoints[index];
                lstItem.Value[index].Init(birdRandom.gameObject.transform, () =>
                {
                    numItemAppearDoneDict[lstItem.Key]++;

                    CheckItemAppearDone(lstItem.Key, () =>
                    {

                    });
                }, false);
            }

            scoreItems.Add(lstItem.Key, 0);
            BirdSort2.GamePlayController_BirdSort.Instance.itemShowFarmeController.SetUpFrame(lstItem.Key);
        }
        //GetItemEventWinLevel();
        BirdSort2.BirdBase randomBird()
        {
            var ran = UnityEngine.Random.Range(0, lsBird.Count);
            var bird = lsBird[ran];
            lsBird.RemoveAt(ran);
            return bird;
        }
    }

    private void CheckItemAppearDone(EventSaleType typeEvent, UnityAction actionDone)
    {
        if (numItemAppearDoneDict.ContainsKey(typeEvent))
        {
            if (saleItems.ContainsKey(typeEvent))
            {
                if (saleItems[typeEvent] != null)
                {
                    if (numItemAppearDoneDict[typeEvent] >= saleItems[typeEvent].Count)
                    {
                        actionDone?.Invoke();
                    }
                }
            }
        }
    }

    public void AddScoreItem(EventSaleType eventType, int value)
    {
        if (scoreItems.ContainsKey(eventType))
        {
            scoreItems[eventType] += value;
        }
        else
        {
            scoreItems.Add(eventType, value);
        }
    }

    public int GetScoreItem(EventSaleType eventType)
    {
        if (scoreItems.ContainsKey(eventType))
        {
            return scoreItems[eventType];
        }
        else
        {
            return 0;
        }
    }

    public void WinLevel()
    {
        //Challenge
        foreach (var it in scoreItems)
        {
            if (it.Value > 0)
            {
                var eve = BirdSortGameController.Instance.eventSaleController.GetEventSales(it.Key);
                if (eve != null)
                {
                    var index = 1;
#if TESTER
                    if (BirdSortGameController.Instance.useProfile.IsRemoveAds)
                    {
                        index = 3;
                    }
#endif
                    eve.AddItem(it.Value * index);
                }
            }
        }
    }

}
