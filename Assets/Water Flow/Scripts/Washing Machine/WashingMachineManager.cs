using System;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Utilities;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;

public class WashingMachineManager : Singleton<WashingMachineManager>
{
    public WashingMachine.LevelItem levelItem;

    public List<WashingMachineSlot> washingMachineSlots;
    public List<WashingMachineStartSlot> washingMachineStartSlots;

    public List<GameFaction> poolFaction;

    public static event Action winLevelEvent;
    public static event Action loseLevelEvent;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        RevivePopup.reviveEvent += Revive;
    }

    void OnDestroy()
    {
        RevivePopup.reviveEvent -= Revive;
    }

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
                    washingMachineItem.MinusClothingPiece();

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

                    ClothesManager.Ins.UpdateClothesCount();

                    return true;
                }
            }
        }

        return false;
    }

    public void MoveClothesToWashingMachine(ClothingPiece clothingPiece, WashingMachineStartSlot washingMachineStartSlot)
    {
        washingMachineStartSlot.washingMachine.MinusClothingPiece();

        clothingPiece.MoveToWashingMachine(washingMachineStartSlot.washingMachine, onCompleteAction: () =>
        {
            washingMachineStartSlot.washingMachine.ReceiveClothes(onCompleteAction: () =>
            {
                if (washingMachineStartSlot.washingMachine.number == 0)
                {
                    washingMachineStartSlot.washingMachine = null;

                    SpawnNewWashingMachine();
                    CheckWinLevel();
                }
            });
        });

        ClothesManager.Ins.clothingPieces.Remove(clothingPiece);

        ClothesManager.Ins.UpdateClothesCount();
    }

    #region Spawn
    public List<WashingMachineItem> SpawnFirstWashingMachines()
    {
        // poolFaction = new List<GameFaction>();

        // for (int i = 0; i < ClothesManager.Ins.totalClothingPiece; i++)
        // {
        //     poolFaction.Add(CommonUtil.GetRandomEnumValue<GameFaction>(numLastValuesToSkip: 2));
        // }

        poolFaction = new List<GameFaction>();

        poolFaction.AddRange(levelItem.poolFaction);

        ClothesManager.Ins.totalClothingPiece = poolFaction.Count;

        Dictionary<GameFaction, int> sortedPoolFaction = new Dictionary<GameFaction, int>();

        for (int i = 0; i < ClothesManager.Ins.clothingPieces.Count; i++)
        {
            ClothingPiece clothingPiece = ClothesManager.Ins.clothingPieces[i];

            clothingPiece.Init(poolFaction.First());

            CommonUtil.SumValueOfDictionary(sortedPoolFaction, clothingPiece.faction, 1);

            poolFaction.RemoveAt(0);
        }

        sortedPoolFaction = sortedPoolFaction.OrderByDescending(item => item.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        List<WashingMachineItem> washingMachineItems = new List<WashingMachineItem>();

        for (int i = 0; i < washingMachineStartSlots.Count; i++)
        {
            if (i >= sortedPoolFaction.Count)
            {
                break;
            }
            
            WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

            WashingMachineItem washingMachineItem = ObjectPoolingEverything.GetFromPool<WashingMachineItem>(GameConstants.washingMachine);

            washingMachineItems.Add(washingMachineItem);

            washingMachineStartSlot.washingMachine = washingMachineItem;

            KeyValuePair<GameFaction, int> keyValuePair = sortedPoolFaction.ElementAt(i);

            washingMachineItem.Init(washingMachineStartSlot, keyValuePair.Key, keyValuePair.Value);
        }

        return washingMachineItems;
    }

    public void SpawnNewWashingMachine()
    {
        Dictionary<GameFaction, int> sortedPoolFaction = GetScorePerFaction();

        if (sortedPoolFaction.Count == 0)
        {
            return;
        }

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

    #region Booster
    public void UnlockWashingMachineSlot()
    {
        for (int i = 0; i < washingMachineSlots.Count; i++)
        {
            WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

            if (washingMachineSlot.isLocked)
            {
                washingMachineSlot.Unlock();

                break;
            }
        }
    }

    public void UseMagicBooster()
    {
        List<ClothingPiece> randomPickedClothes = new List<ClothingPiece>();

        for (int i = 0; i < ClothesManager.Ins.clothingPieces.Count; i++)
        {
            ClothingPiece clothingPiece = ClothesManager.Ins.clothingPieces[i];

            randomPickedClothes.Add(clothingPiece);
        }

        randomPickedClothes.Shuffle();
        randomPickedClothes = randomPickedClothes.Take(5).ToList();

        for (int i = 0; i < randomPickedClothes.Count; i++)
        {
            ClothingPiece clothingPiece = randomPickedClothes[i];

            for (int j = 0; j < washingMachineStartSlots.Count; j++)
            {
                WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[j];

                if (washingMachineStartSlot.washingMachine != null)
                {
                    if (washingMachineStartSlot.washingMachine.faction == clothingPiece.faction && washingMachineStartSlot.washingMachine.number > 0)
                    {
                        PathSlot pathSlot = clothingPiece.GetComponentInParent<PathSlot>();

                        if (pathSlot != null)
                        {
                            pathSlot.clothingPiece = null;
                        }

                        LoopPathSlot loopPathSlot = clothingPiece.GetComponentInParent<LoopPathSlot>();

                        if (loopPathSlot != null)
                        {
                            loopPathSlot.clothingPiece = null;
                        }

                        MoveClothesToWashingMachine(clothingPiece, washingMachineStartSlot);

                        break;
                    }
                }
            }
        }

        ClothesManager.Ins.MoveDownClothesInWaitingPath();
    }

    public void UseBoosterShuffle()
    {
        Dictionary<GameFaction, int> currentAvailableClothesPerFaction = GetCurrentAvailableClothesPerFaction();
        Dictionary<GameFaction, int> poolFactionSortedByDifficulty = new Dictionary<GameFaction, int>();

        for (int i = 0; i < ClothesManager.Ins.loopPathSlots.Count; i++)
        {
            LoopPathSlot loopPathSlot = ClothesManager.Ins.loopPathSlots[i];

            if (loopPathSlot.clothingPiece != null)
            {
                ClothingPiece clothingPiece = loopPathSlot.clothingPiece;

                CommonUtil.SumValueOfDictionary(poolFactionSortedByDifficulty, clothingPiece.faction, 10);
            }
        }

        for (int i = 0; i < ClothesManager.Ins.waitingPathSlots.Count; i++)
        {
            PathSlot pathSlot = ClothesManager.Ins.waitingPathSlots[i];

            if (pathSlot.clothingPiece != null)
            {
                ClothingPiece clothingPiece = pathSlot.clothingPiece;

                CommonUtil.SumValueOfDictionary(poolFactionSortedByDifficulty, clothingPiece.faction, 1);
            }
        }

        poolFactionSortedByDifficulty = poolFactionSortedByDifficulty.OrderByDescending(item => item.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        int index = 0;

        for (int i = 0; i < washingMachineStartSlots.Count; i++)
        {
            WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

            if (washingMachineStartSlot.washingMachine != null)
            {
                WashingMachineItem washingMachine = washingMachineStartSlot.washingMachine;

                KeyValuePair<GameFaction, int> keyValuePair = poolFactionSortedByDifficulty.ElementAt(index);

                washingMachine.Init(washingMachineStartSlot, keyValuePair.Key, currentAvailableClothesPerFaction[keyValuePair.Key]);

                index++;
            }
        }
    }
    #endregion

    #region Check Win
    private void CheckWinLevel()
    {
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
        bool isLose = washingMachineSlots.All(item => item.isLocked || item.washingMachineItem != null);

        if (isLose)
        {
            loseLevelEvent?.Invoke();
        }
    }

    private void Revive(BoosterType boosterType)
    {
        for (int i = 0; i < washingMachineSlots.Count; i++)
        {
            WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

            if (washingMachineSlot.isLocked)
            {
                washingMachineSlot.Unlock();

                break;
            }
        }
    }
    #endregion

    #region Util
    public WashingMachineSlot GetAvailableWashingMachineSlot()
    {
        for (int i = 0; i < washingMachineSlots.Count; i++)
        {
            WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

            if (!washingMachineSlot.isLocked && washingMachineSlot.washingMachineItem == null)
            {
                return washingMachineSlot;
            }
        }

        return null;
    }

    public Dictionary<GameFaction, int> GetCurrentAvailableClothesPerFaction()
    {
        Dictionary<GameFaction, int> currentAvailableClothesPerFaction = new Dictionary<GameFaction, int>();

        for (int i = 0; i < ClothesManager.Ins.clothingPieces.Count; i++)
        {
            CommonUtil.SumValueOfDictionary(currentAvailableClothesPerFaction, ClothesManager.Ins.clothingPieces[i].faction, 1);
        }

        return currentAvailableClothesPerFaction;
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

        sortedPoolFaction = sortedPoolFaction.OrderByDescending(item => item.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return sortedPoolFaction;
    }
    #endregion
}
