using System;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Utilities;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Unity.VisualScripting;
using UnityEngine;
using static GameEnum;

namespace AntsFlow
{
    public class WashingMachineManager : Singleton<WashingMachineManager>
    {
        public AntsFlow.LevelItem levelItem;

        public List<WashingMachineSlot> washingMachineSlots;
        public List<WashingMachineStartSlot> washingMachineStartSlots;

        public List<Color> poolFaction;

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

        public void StartLevel(AntsFlow.LevelItem levelItem)
        {
            this.levelItem = levelItem;

            TileManager.Ins.tiles = TransformUtil.GetComponentsFromAllChildren<BaseTile>(levelItem.transform);

            SpawnFirstWashingMachines();
        }

        public WashingMachineSlot MoveItemToSlot(WashingMachineItem washingMachineItem)
        {
            WashingMachineSlot washingMachineSlot = GetAvailableWashingMachineSlot();

            if (washingMachineSlot == null)
            {
                CheckLoseLevel();

                return null;
            }

            washingMachineSlot.washingMachineItem = washingMachineItem;

            washingMachineItem.isMoving = true;

            Tween.Position(washingMachineItem.transform, washingMachineSlot.transform.position.AddZ(-6), duration: 0.3f)
            .OnComplete(() =>
            {
                washingMachineItem.isMoving = false;
                washingMachineItem.washingMachineStartSlot.washingMachine = null;

                washingMachineItem.SpawnAnts();

                SpawnNewWashingMachine();
            });

            return washingMachineSlot;
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
            poolFaction = new List<Color>();

            poolFaction.AddRange(levelItem.poolFaction);

            Color nextFaction;
            int numAnt;

            List<WashingMachineItem> washingMachineItems = new List<WashingMachineItem>();

            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

                if (washingMachineStartSlot.washingMachine != null)
                {
                    ObjectPoolingEverything.ReturnToPool(GameConstants.washingMachine, washingMachineStartSlot.washingMachine.gameObject);
                }

                WashingMachineItem washingMachineItem = ObjectPoolingEverything.GetFromPool<WashingMachineItem>(GameConstants.washingMachine);

                washingMachineItems.Add(washingMachineItem);

                washingMachineStartSlot.washingMachine = washingMachineItem;

                GetNextAntSpawnByDifficulty(difficultyLevel: i, out nextFaction, out numAnt);

                washingMachineItem.Init(washingMachineStartSlot, nextFaction, numAnt);
            }

            return washingMachineItems;
        }

        public void SpawnNewWashingMachine()
        {
            Color nextFaction;
            int numAnt;

            if (GetNextAntSpawnByDifficulty(difficultyLevel: 5, out nextFaction, out numAnt))
            {

            }
            else
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

                    washingMachineItem.Init(washingMachineStartSlot, nextFaction, numAnt);

                    break;
                }
            }
        }
        #endregion

        #region Pool Faction
        public Color GetNextFactionInPool()
        {
            if (poolFaction.Count == 0)
            {
                return Color.black;
            }

            Color nextFaction = poolFaction.First();

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
            List<BaseTile> randomPickedTiles = new List<BaseTile>();

            Dictionary<Color, int> numTileWithNoAntPerFaction = GetNumTileWithNoAntPerFaction();

            for (int i = 0; i < TileManager.Ins.tiles.Count; i++)
            {
                BaseTile tile = TileManager.Ins.tiles[i];

                if (tile.gameObject.activeInHierarchy && !tile.isAntApproaching)
                {
                    randomPickedTiles.Add(tile);
                }
            }

            randomPickedTiles.Shuffle();
            randomPickedTiles = randomPickedTiles.Take(5).ToList();

            for (int i = 0; i < randomPickedTiles.Count; i++)
            {
                BaseTile tile = randomPickedTiles[i];

                for (int j = 0; j < washingMachineStartSlots.Count; j++)
                {
                    WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[j];
                    WashingMachineItem washingMachine = washingMachineStartSlot.washingMachine;

                    if (washingMachine != null)
                    {
                        if (washingMachine.faction == tile.Faction && washingMachine.number > 0)
                        {
                            Tween.Position(tile.transform, washingMachine.transform.position, duration: 0.3f)
                            .OnComplete(() =>
                            {
                                ParticleSystem antDissappearFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.antDissapearFxPrefab);

                                antDissappearFx.transform.position = tile.transform.position;
                                antDissappearFx.Play();

                                tile.gameObject.SetActive(false);
                            });

                            tile.isAntApproaching = true;

                            washingMachine.ReceiveTile();
                        }
                    }
                }
            }
        }

        public void UseBoosterShuffle()
        {
            // Dictionary<GameFaction, int> currentAvailableClothesPerFaction = GetCurrentAvailableClothesPerFaction();
            // Dictionary<GameFaction, int> poolFactionSortedByDifficulty = new Dictionary<GameFaction, int>();

            // for (int i = 0; i < ClothesManager.Ins.loopPathSlots.Count; i++)
            // {
            //     LoopPathSlot loopPathSlot = ClothesManager.Ins.loopPathSlots[i];

            //     if (loopPathSlot.clothingPiece != null)
            //     {
            //         ClothingPiece clothingPiece = loopPathSlot.clothingPiece;

            //         CommonUtil.SumValueOfDictionary(poolFactionSortedByDifficulty, clothingPiece.faction, 10);
            //     }
            // }

            // for (int i = 0; i < ClothesManager.Ins.waitingPathSlots.Count; i++)
            // {
            //     PathSlot pathSlot = ClothesManager.Ins.waitingPathSlots[i];

            //     if (pathSlot.clothingPiece != null)
            //     {
            //         ClothingPiece clothingPiece = pathSlot.clothingPiece;

            //         CommonUtil.SumValueOfDictionary(poolFactionSortedByDifficulty, clothingPiece.faction, 1);
            //     }
            // }

            // poolFactionSortedByDifficulty = poolFactionSortedByDifficulty.OrderByDescending(item => item.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // int index = 0;

            // for (int i = 0; i < washingMachineStartSlots.Count; i++)
            // {
            //     WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

            //     if (washingMachineStartSlot.washingMachine != null)
            //     {
            //         WashingMachineItem washingMachine = washingMachineStartSlot.washingMachine;

            //         KeyValuePair<GameFaction, int> keyValuePair = poolFactionSortedByDifficulty.ElementAt(index);

            //         washingMachine.Init(washingMachineStartSlot, keyValuePair.Key, currentAvailableClothesPerFaction[keyValuePair.Key]);

            //         index++;
            //     }
            // }
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

        private bool GetNextAntSpawnByDifficulty(int difficultyLevel, out Color nextFaction, out int numAnt)
        {
            Dictionary<Color, int> numSpawnPerFactionOrderByDifficulty = GetNumAntSpawnPerFactionOrderByDifficulty();

            for (int j = difficultyLevel; j < numSpawnPerFactionOrderByDifficulty.Count; j++)
            {
                if (numSpawnPerFactionOrderByDifficulty.ElementAt(j).Value > 0)
                {
                    KeyValuePair<Color, int> keyValuePair;

                    keyValuePair = numSpawnPerFactionOrderByDifficulty.ElementAt(j);

                    nextFaction = keyValuePair.Key;
                    numAnt = keyValuePair.Value;

                    return true;
                }
            }

            for (int j = 0; j < numSpawnPerFactionOrderByDifficulty.Count; j++)
            {
                if (numSpawnPerFactionOrderByDifficulty.ElementAt(j).Value > 0)
                {
                    KeyValuePair<Color, int> keyValuePair;

                    keyValuePair = numSpawnPerFactionOrderByDifficulty.ElementAt(j);

                    nextFaction = keyValuePair.Key;
                    numAnt = keyValuePair.Value;

                    return true;
                }
            }

            nextFaction = Color.white;
            numAnt = 0;

            return false;
        }

        public Dictionary<Color, int> GetRemaniningTilePerFaction()
        {
            Dictionary<Color, int> result = new Dictionary<Color, int>();

            for (int i = 0; i < TileManager.Ins.tiles.Count; i++)
            {
                BaseTile tile = TileManager.Ins.tiles[i];

                if (!tile.gameObject.activeInHierarchy || tile.isAntApproaching)
                {
                    continue;
                }

                CommonUtil.SumValueOfDictionary(result, tile.Faction, 1);
            }

            return result;
        }
        public Dictionary<Color, int> GetNumTileWithNoAntPerFaction()
        {
            Dictionary<Color, int> result = GetMaxAntRequiredPerFaction();

            return result;
        }

        public Dictionary<Color, int> GetMaxAntRequiredPerFaction()
        {
            Dictionary<Color, int> result = GetRemaniningTilePerFaction();

            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

                if (washingMachineStartSlot.washingMachine != null)
                {
                    CommonUtil.SumValueOfDictionary(result, washingMachineStartSlot.washingMachine.faction, -washingMachineStartSlot.washingMachine.number);
                }
            }

            for (int i = 0; i < washingMachineSlots.Count; i++)
            {
                WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

                if (washingMachineSlot.washingMachineItem != null)
                {
                    CommonUtil.SumValueOfDictionary(result, washingMachineSlot.washingMachineItem.faction, -washingMachineSlot.washingMachineItem.number);
                }
            }

            return result;
        }

        public Dictionary<Color, int> GetNumBlockagesPerFaction()
        {
            Dictionary<Color, int> sortedPoolFaction = new Dictionary<Color, int>();

            for (int i = 0; i < TileManager.Ins.tiles.Count; i++)
            {
                BaseTile tile = TileManager.Ins.tiles[i];

                if (!tile.gameObject.activeInHierarchy || tile.isAntApproaching)
                {
                    continue;
                }

                CommonUtil.SumValueOfDictionary(sortedPoolFaction, tile.Faction, tile.blockages.Count);
            }

            return sortedPoolFaction;
        }

        public Dictionary<Color, int> GetNumNotBlockedTilePerFaction()
        {
            Dictionary<Color, int> sortedPoolFaction = new Dictionary<Color, int>();

            for (int i = 0; i < TileManager.Ins.tiles.Count; i++)
            {
                BaseTile tile = TileManager.Ins.tiles[i];

                if (!tile.gameObject.activeInHierarchy || tile.isAntApproaching)
                {
                    continue;
                }

                CommonUtil.SumValueOfDictionary(sortedPoolFaction, tile.Faction, tile.blockages.Count < 4 ? 1 : 0);
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

        public Dictionary<Color, int> GetNumAntSpawnPerFactionOrderByDifficulty()
        {
            Dictionary<Color, int> result = new Dictionary<Color, int>();
            Dictionary<Color, int> numNotBlockedTilePerFaction = GetNumNotBlockedTilePerFaction();
            Dictionary<Color, int> maxAntRequiredPerFaction = GetMaxAntRequiredPerFaction();

            foreach (var faction in numNotBlockedTilePerFaction.Keys)
            {
                int numNotBlockedTileOfFaction = numNotBlockedTilePerFaction[faction];

                int minValue = Mathf.Min(numNotBlockedTileOfFaction, maxAntRequiredPerFaction[faction]);
                int maxValue = maxAntRequiredPerFaction[faction];

                CommonUtil.SumValueOfDictionary(result, faction, UnityEngine.Random.Range(minValue, maxValue));
            }

            return result;
        }
        #endregion
    }
}
