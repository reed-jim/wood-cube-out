using System;
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

    public List<GameFaction> poolFaction;

    public int totalClothingPiece;

    public static event Action winLevelEvent;
    public static event Action loseLevelEvent;

    public void MoveItemToSlot(WashingMachineItem washingMachineItem)
    {
        WashingMachineSlot washingMachineSlot = GetAvailableWashingMachineSlot();

        if (washingMachineSlot == null)
        {
            CheckLoseLevel();
            
            return;
        }

        washingMachineSlot.washingMachineItem = washingMachineItem;

        washingMachineItem.isMoving = true;

        Tween.Position(washingMachineItem.transform, washingMachineSlot.transform.position.AddZ(-1), duration: 0.3f)
        .OnComplete(() =>
        {
            washingMachineItem.isMoving = false;
            washingMachineItem.washingMachineStartSlot.washingMachine = null;

            SpawnNewWashingMachine();
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
                    clothingPiece.MoveToWashingMachine(washingMachineItem, onCompleteAction: () =>
                    {
                        washingMachineItem.ReceiveClothes(onCompleteAction: () =>
                        {
                            if (washingMachineItem.number == 0)
                            {
                                washingMachineSlot.washingMachineItem = null;

                                CheckWinLevel();
                            }
                        });
                    });
                    ClothesManager.Ins.clothingPieces.Remove(clothingPiece);

                    return true;
                }
            }
        }

        return false;
    }

    #region Spawn
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
        Dictionary<GameFaction, int> sortedPoolFaction = GetScorePerFaction();

        if (sortedPoolFaction.ElementAt(0).Value <= 0)
        {
            return;
        }

        for (int i = 0; i < washingMachineStartSlots.Count; i++)
        {
            WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

            if (washingMachineStartSlot.washingMachine == null)
            {
                WashingMachineItem washingMachineItem = ObjectPoolingEverything.GetFromPool<WashingMachineItem>(GameConstants.washingMachine);

                washingMachineItem.gameObject.SetActive(true);
                washingMachineItem.transform.SetParent(washingMachineStartSlot.transform);

                washingMachineItem.transform.localPosition = new Vector3(0, -5, 0);
                Tween.LocalPositionY(washingMachineItem.transform, 0, duration: 0.3f);

                washingMachineStartSlot.washingMachine = washingMachineItem;

                KeyValuePair<GameFaction, int> keyValuePair = sortedPoolFaction.ElementAt(0);

                washingMachineItem.Init(washingMachineStartSlot, keyValuePair.Key, keyValuePair.Value);

                break;
            }
        }
    }
    #endregion

    #region Pool Faction
    public GameFaction GetNextFactionInPool()
    {
        if (poolFaction.Count == 0)
        {
            return GameFaction.None;
        }

        GameFaction nextFaction = poolFaction.First();

        poolFaction.RemoveAt(0);

        return nextFaction;
    }
    #endregion

    #region Check Win
    private void CheckWinLevel()
    {
        Debug.Log($"CheckWinLevel {poolFaction.Count} {ClothesManager.Ins.clothingPieces.Count}");

        bool isWin = poolFaction.Count + ClothesManager.Ins.clothingPieces.Count == 0;

        if (isWin)
        {
            winLevelEvent?.Invoke();
        }
    }
    #endregion

    #region Check Lose
    private void CheckLoseLevel()
    {
        bool isLose = washingMachineSlots.All(item => item.washingMachineItem != null);

        if (isLose)
        {
            loseLevelEvent?.Invoke();
        }
    }
    #endregion 

    #region Util
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

    public Dictionary<GameFaction, int> GetScorePerFaction()
    {
        Dictionary<GameFaction, int> sortedPoolFaction = new Dictionary<GameFaction, int>();

        for (int i = 0; i < poolFaction.Count; i++)
        {
            CommonUtil.SumValueOfDictionary(sortedPoolFaction, poolFaction[i], 1);
        }

        for (int i = 0; i < ClothesManager.Ins.clothingPieces.Count; i++)
        {
            CommonUtil.SumValueOfDictionary(sortedPoolFaction, ClothesManager.Ins.clothingPieces[i].faction, 1);
        }

        for (int i = 0; i < washingMachineStartSlots.Count; i++)
        {
            WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

            if (washingMachineStartSlot.washingMachine != null)
            {
                CommonUtil.SumValueOfDictionary(sortedPoolFaction, washingMachineStartSlot.washingMachine.faction, -washingMachineStartSlot.washingMachine.number);
            }
        }

        for (int i = 0; i < washingMachineSlots.Count; i++)
        {
            WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

            if (washingMachineSlot.washingMachineItem != null)
            {
                CommonUtil.SumValueOfDictionary(sortedPoolFaction, washingMachineSlot.washingMachineItem.faction, -washingMachineSlot.washingMachineItem.number);
            }
        }

        Debug.Log($"GetScorePerFaction ****************");

        foreach (var item in sortedPoolFaction)
        {
            Debug.Log($"GetScorePerFaction {item.Key} {item.Value}");
        }

        sortedPoolFaction = sortedPoolFaction.OrderByDescending(item => item.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return sortedPoolFaction;
    }
    #endregion
}
