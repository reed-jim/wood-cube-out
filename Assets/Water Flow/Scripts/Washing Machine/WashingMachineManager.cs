using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;

public class WashingMachineManager : Singleton<WashingMachineManager>
{
    public List<WashingMachineSlot> washingMachineSlots;
    public List<WashingMachineStartSlot> washingMachineStartSlots;
    public List<WashingMachineItem> washingMachineItems;

    public List<GameFaction> poolFaction;

    public int totalClothingPiece;

    public WashingMachineSlot GetAvailableWashingMachineSlot()
    {
        for (int i = 0; i < washingMachineSlots.Count; i++)
        {
            WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

            if (washingMachineSlot.washingMachineItem == null)
            {
                return washingMachineSlot;
            }
        }

        return null;
    }

    public void MoveItemToSlot(WashingMachineItem washingMachineItem)
    {
        WashingMachineSlot washingMachineSlot = GetAvailableWashingMachineSlot();

        washingMachineSlot.washingMachineItem = washingMachineItem;

        washingMachineItem.isMoving = true;

        Tween.Position(washingMachineItem.transform, washingMachineSlot.transform.position.AddZ(-1), duration: 0.3f)
        .OnComplete(() =>
        {
            washingMachineItem.isMoving = false;
        });
    }

    public bool MoveClothesToWashingMachine(ClothingPiece clothingPiece)
    {
        for (int i = 0; i < washingMachineSlots.Count; i++)
        {
            WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

            if (washingMachineSlot.washingMachineItem != null)
            {
                WashingMachineItem washingMachineItem = washingMachineSlot.washingMachineItem;

                if (washingMachineItem.isMoving)
                {
                    continue;
                }

                if (washingMachineItem.number > 0 && clothingPiece.faction == washingMachineItem.faction)
                {
                    clothingPiece.MoveToWashingMachine(washingMachineItem);

                    washingMachineItem.ReceiveClothes();

                    if (washingMachineItem.number == 0)
                    {
                        washingMachineSlot.washingMachineItem = null;
                    }

                    return true;
                }
            }
        }

        return false;
    }

    public void SpawnFirstWashingMachines()
    {
        poolFaction = new List<GameFaction>();

        for (int i = 0; i < totalClothingPiece; i++)
        {
            poolFaction.Add(CommonUtil.GetRandomEnumValue<GameFaction>(numLastValuesToSkip: 2));
        }

        Dictionary<GameFaction, int> sortedPoolFaction = new Dictionary<GameFaction, int>();

        for (int i = 0; i < ClothesManager.Ins.clothingPieces.Count; i++)
        {
            ClothingPiece clothingPiece = ClothesManager.Ins.clothingPieces[i];

            clothingPiece.Init(poolFaction.First());

            CommonUtil.SumValueOfDictionary(sortedPoolFaction, clothingPiece.faction, 1);

            poolFaction.RemoveAt(0);
        }

        sortedPoolFaction = sortedPoolFaction.OrderByDescending(item => item.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        for (int i = 0; i < washingMachineStartSlots.Count; i++)
        {
            WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

            WashingMachineItem washingMachineItem = ObjectPoolingEverything.GetFromPool<WashingMachineItem>(GameConstants.washingMachine);

            washingMachineStartSlot.washingMachine = washingMachineItem;

            KeyValuePair<GameFaction, int> keyValuePair = sortedPoolFaction.ElementAt(i);

            washingMachineItem.Init(washingMachineStartSlot, keyValuePair.Key, keyValuePair.Value);
        }
    }

    public void SpawnNewWashingMachine()
    {
        for (int j = 0; j < washingMachineStartSlots.Count; j++)
        {
            WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[j];

            if (washingMachineStartSlot.washingMachine == null)
            {
                WashingMachineItem washingMachineItem = ObjectPoolingEverything.GetFromPool<WashingMachineItem>(GameConstants.washingMachine);

                washingMachineItem.gameObject.SetActive(true);
                washingMachineItem.transform.SetParent(washingMachineStartSlot.transform);

                washingMachineItem.transform.localPosition = new Vector3(0, -5, 0);
                Tween.LocalPositionY(washingMachineItem.transform, 0, duration: 0.3f);

                washingMachineStartSlot.washingMachine = washingMachineItem;

                break;
            }
        }
    }

    #region Pool Faction
    public GameFaction GetNextFactionInPool()
    {
        GameFaction nextFaction = poolFaction.First();

        poolFaction.RemoveAt(0);

        return nextFaction;
    }
    #endregion
}
