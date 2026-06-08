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
        public LevelItem levelItem;
        public Camera mainCamera;

        public List<WashingMachineSlot> washingMachineSlots;
        public List<WashingMachineStartSlot> washingMachineStartSlots;

        public List<Color> poolFaction;

        public int difficultyLevel;

        public static event Action winLevelEvent;
        public static event Action loseLevelEvent;
        public static event Action finishUsingEnableAllAntBoxesBoosterEvent;

        protected override void MoreActionOnAwake()
        {
            base.MoreActionOnAwake();

            RevivePopup.reviveEvent += Revive;
            BaseTile.useClearOneColorBoosterEvent += UseBoosterClearOneColor;
            WashingMachineItem.selectWashingMachineEvent += OnAntBoxSelected;
        }

        void OnDestroy()
        {
            RevivePopup.reviveEvent -= Revive;
            BaseTile.useClearOneColorBoosterEvent -= UseBoosterClearOneColor;
            WashingMachineItem.selectWashingMachineEvent -= OnAntBoxSelected;
        }

        private void OnAntBoxSelected(WashingMachineItem antBox)
        {
            if (GamePersistentVariable.boosterMode == BoosterMode.EnableAllAntBoxes)
            {
                Tween.PositionY(mainCamera.transform, 0, duration: 0.3f);

                GamePersistentVariable.boosterMode = BoosterMode.None;

                finishUsingEnableAllAntBoxesBoosterEvent?.Invoke();
            }
        }

        public void StartLevel(AntsFlow.LevelItem levelItem)
        {
            this.levelItem = levelItem;

            TileManager.Ins.tiles = TransformUtil.GetComponentsFromAllChildren<BaseTile>(levelItem.transform);
            TileManager.Ins.UpdateTileCount();

            List<WashingMachineItem> washingMachineItems = SpawnFirstWashingMachines();

            if (levelItem.isTutorialLevel)
            {
                TutorialLevelOne.Ins.StartTutorial(washingMachineItems);
            }
            else
            {
                TutorialLevelOne.Ins.HideTutorial();
            }
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

            Tween.Scale(washingMachineItem.transform, 1.3f * washingMachineItem.transform.localScale.x, duration: 0.3f, cycles: 2, cycleMode: CycleMode.Yoyo);
            Tween.Position(washingMachineItem.transform, washingMachineSlot.transform.position.AddZ(-0.1f), duration: 0.3f)
            .OnComplete(() =>
            {
                washingMachineItem.isMoving = false;
                washingMachineItem.washingMachineStartSlot.washingMachine = null;

                washingMachineItem.SpawnAnts();

                MoveAntBoxTop();
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

                        MoveAntBoxTop();
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

            SpecialElementManager.Ins.SpawnAntBoxLink();

            return washingMachineItems;
        }

        public void MoveAntBoxTop()
        {
            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

                if (washingMachineStartSlot.washingMachine == null)
                {
                    int x = (int)washingMachineStartSlot.transform.position.x;

                    bool isFound = false;

                    for (int j = i + 1; j < washingMachineStartSlots.Count; j++)
                    {
                        WashingMachineStartSlot otherWashingMachineStartSlot = washingMachineStartSlots[j];

                        if (otherWashingMachineStartSlot.washingMachine != null &&
                            (int)otherWashingMachineStartSlot.transform.position.x == x
                        )
                        {
                            WashingMachineItem washingMachineItem = otherWashingMachineStartSlot.washingMachine;

                            if (washingMachineItem.isMoving)
                            {
                                continue;
                            }

                            washingMachineItem.transform.SetParent(washingMachineStartSlot.transform);
                            Tween.LocalPosition(washingMachineItem.transform, Vector3.zero.AddZ(-1), duration: 0.3f)
                            .OnComplete(() =>
                            {
                                washingMachineItem.Setup(washingMachineStartSlot);

                                washingMachineItem.isMoving = false;
                            });

                            washingMachineItem.washingMachineStartSlot = washingMachineStartSlot;
                            washingMachineStartSlot.washingMachine = washingMachineItem;
                            otherWashingMachineStartSlot.washingMachine = null;

                            washingMachineItem.isMoving = true;

                            isFound = true;

                            break;
                        }
                    }

                    if (!isFound)
                    {
                        SpawnNewWashingMachine();
                    }
                }
            }
        }

        public void SpawnNewWashingMachine()
        {
            Color nextFaction;
            int numAnt;

            if (GetNextAntSpawnByDifficulty(difficultyLevel: difficultyLevel, out nextFaction, out numAnt))
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
                    WashingMachineItem washingMachineItem = GetAntBoxFromPool();

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

        private WashingMachineItem GetAntBoxFromPool()
        {
            string key = GameConstants.washingMachine;

            LevelElement levelElement = SpecialElementManager.Ins.nextLevelElement;

            if (levelElement == LevelElement.NormalAntBox)
            {
                key = GameConstants.washingMachine;
            }
            else if (levelElement == LevelElement.HiddenAntBox)
            {
                key = GameConstants.hiddenAntBox;
            }
            else if (levelElement == LevelElement.FreezedAntBox)
            {
                key = GameConstants.freezedAntBox;
            }

            SpecialElementManager.Ins.RandomizeNextLevelElement();

            WashingMachineItem antBox = ObjectPoolingEverything.GetFromPool<WashingMachineItem>(key);

            return antBox;
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
            randomPickedTiles = randomPickedTiles.Take(10).ToList();

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
            Dictionary<Color, int> poolFactionSortedByDifficulty = GetRemaniningTilePerFaction();

            for (int i = 0; i < washingMachineSlots.Count; i++)
            {
                WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

                if (washingMachineSlot.washingMachineItem != null)
                {
                    CommonUtil.SumValueOfDictionary(poolFactionSortedByDifficulty, washingMachineSlot.washingMachineItem.faction, -washingMachineSlot.washingMachineItem.number);
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

                    KeyValuePair<Color, int> keyValuePair = poolFactionSortedByDifficulty.ElementAt(index);

                    washingMachine.Init(washingMachineStartSlot, keyValuePair.Key, keyValuePair.Value);

                    index++;
                }
            }
        }

        public void UseBoosterEnableAllAntBoxes()
        {
            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineItem antBox = washingMachineStartSlots[i].washingMachine;

                if (antBox != null)
                {
                    antBox.TemporarilyEnable();
                }
            }

            Tween.PositionY(mainCamera.transform, -2, duration: 0.3f);
        }

        public void UseBoosterClearOneColor(Color selectedFaction)
        {
            List<BaseTile> tilesToClear = TileManager.Ins.tiles
                .Where(tile => tile.gameObject.activeInHierarchy && tile.Faction == selectedFaction && !tile.isAntApproaching).ToList();

            for (int i = 0; i < tilesToClear.Count; i++)
            {
                BaseTile tile = tilesToClear[i];

                SaferioTween.ScaleDownWithPunch(tile.transform, onCompleteAction: () =>
                {
                    tile.Complete();
                });
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

        public List<WashingMachineSlot> GetAvailableWashingMachineSlots()
        {
            List<WashingMachineSlot> slots = new List<WashingMachineSlot>();

            for (int i = 0; i < washingMachineSlots.Count; i++)
            {
                WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

                if (!washingMachineSlot.isLocked && washingMachineSlot.washingMachineItem == null)
                {
                    slots.Add(washingMachineSlot);
                }
            }

            return slots;
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

                // int minValue = Mathf.Min(numNotBlockedTileOfFaction, maxAntRequiredPerFaction[faction]);
                int minValue = 1;
                int maxValue = maxAntRequiredPerFaction[faction];

                // CommonUtil.SumValueOfDictionary(result, faction, maxValue);
                CommonUtil.SumValueOfDictionary(result, faction, CommonUtil.GaussianRange((int)(maxValue / 2f), maxValue));
            }

            return result;
        }
        #endregion
    }
}
