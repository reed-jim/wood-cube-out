using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Utilities;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Unity.VisualScripting;
using UnityEditor.UIElements;
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
        public int minAntNumberThreshold;
        public int maxAntNumberThreshold;
        public AnimationCurve difficultyCurve;

        private int totalTile;
        public int numAntBoxSpawned;
        public List<WashingMachineItem> ghostAntBoxes;

        public static event Action winLevelEvent;
        public static event Action loseLevelEvent;
        public static event Action ghostAntBoxesClearedEvent;
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

            difficultyLevel = levelItem.difficulty;
            numAntBoxSpawned = 0;

            TileManager.Ins.tiles = TransformUtil.GetComponentsFromAllChildren<BaseTile>(levelItem.transform);
            TileManager.Ins.UpdateTileCount();
            TileManager.Ins.CalculateLevelCenter();

            SpecialElementManager.Ins.Setup(levelItem);

            List<WashingMachineItem> washingMachineItems = SpawnFirstWashingMachines(levelItem.numStartAntBoxes);

            if (levelItem.isTutorialLevel)
            {
                TutorialLevelOne.Ins.StartTutorial(washingMachineItems);
            }
            else
            {
                TutorialLevelOne.Ins.HideTutorial();
            }
        }

        public WashingMachineSlot MoveItemToSlot(WashingMachineItem washingMachineItem, Action onCompleteAction = null)
        {
            WashingMachineSlot washingMachineSlot = GetAvailableWashingMachineSlot();

            // if (washingMachineSlot == null)
            // {
            //     CheckLoseLevel();

            //     return null;
            // }

            washingMachineSlot.washingMachineItem = washingMachineItem;

            // bool isLose = CheckLoseLevel();

            washingMachineItem.isMoving = true;

            // Tween.Scale(washingMachineItem.transform, 1.3f * washingMachineItem.transform.localScale.x, duration: 0.3f, cycles: 2, cycleMode: CycleMode.Yoyo);
            Vector3 initialScale = washingMachineItem.transform.localScale;
            Tween.Position(washingMachineItem.transform, washingMachineSlot.transform.position.AddY(2.2f).AddZ(-0.1f), duration: 0.2f, ease: Ease.Linear);
            Tween.ScaleY(washingMachineItem.transform, 1.3f * washingMachineItem.transform.localScale.y, duration: 0.2f)
            .OnComplete(() =>
            {
                Tween.Scale(washingMachineItem.transform, initialScale, duration: 0.2f);

                Tween.Position(washingMachineItem.transform, washingMachineSlot.transform.position.AddZ(-0.1f), duration: 0.2f, ease: Ease.Linear)
                .OnComplete(() =>
                {
                    washingMachineItem.isMoving = false;
                    washingMachineItem.washingMachineStartSlot.washingMachine = null;

                    washingMachineItem.OpenTheBox();
                    washingMachineItem.CheckAntsPath();

                    MoveAntBoxTop();

                    onCompleteAction?.Invoke();
                });
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
        public List<WashingMachineItem> SpawnFirstWashingMachines(int numStartAntBoxes)
        {
            poolFaction = new List<Color>();

            poolFaction.AddRange(levelItem.poolFaction);

            totalTile = poolFaction.Count;

            Color nextFaction;
            int numAnt;

            List<WashingMachineItem> washingMachineItems = new List<WashingMachineItem>();

            for (int i = 0; i < numStartAntBoxes; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

                if (washingMachineStartSlot.washingMachine != null)
                {
                    ObjectPoolingEverything.ReturnToPool(GameConstants.washingMachine, washingMachineStartSlot.washingMachine.gameObject);
                }

                numAntBoxSpawned++;

                WashingMachineItem washingMachineItem = GetAntBoxFromPool();

                washingMachineItems.Add(washingMachineItem);

                washingMachineStartSlot.washingMachine = washingMachineItem;

                GetNextAntSpawnByDifficulty(difficultyLevel: GetCurrentLevelDifficulty(), out nextFaction, out numAnt);

                washingMachineItem.Init(washingMachineStartSlot, nextFaction, numAnt);
            }

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

                            washingMachineItem?.InvokeMoveUpEvent();

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

            if (GetNextAntSpawnByDifficulty(difficultyLevel: GetCurrentLevelDifficulty(), out nextFaction, out numAnt))
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
                    numAntBoxSpawned++;

                    WashingMachineItem washingMachineItem = GetAntBoxFromPool();

                    washingMachineItem.gameObject.SetActive(true);
                    washingMachineItem.transform.SetParent(washingMachineStartSlot.transform);

                    washingMachineItem.transform.localPosition = new Vector3(0, -5, 0);
                    Tween.LocalPositionY(washingMachineItem.transform, 0, duration: 0.3f);

                    washingMachineStartSlot.washingMachine = washingMachineItem;

                    washingMachineItem.Init(washingMachineStartSlot, nextFaction, numAnt);

                    SpecialElementManager.Ins.SpawnAntBoxLink();

                    break;
                }
            }
        }

        private WashingMachineItem GetAntBoxFromPool()
        {
            string key = GameConstants.washingMachine;

            LevelElement levelElement = SpecialElementManager.Ins.GetNextLevelElement();

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
            // Dictionary<Color, int> poolFactionSortedByDifficulty = GetRemaniningTilePerFaction();

            // for (int i = 0; i < washingMachineSlots.Count; i++)
            // {
            //     WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

            //     if (washingMachineSlot.washingMachineItem != null)
            //     {
            //         CommonUtil.SumValueOfDictionary(poolFactionSortedByDifficulty, washingMachineSlot.washingMachineItem.faction, -washingMachineSlot.washingMachineItem.number);
            //     }
            // }

            // poolFactionSortedByDifficulty = poolFactionSortedByDifficulty.OrderByDescending(item => item.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // int index = 0;

            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

                if (washingMachineStartSlot.washingMachine != null)
                {
                    WashingMachineItem washingMachine = washingMachineStartSlot.washingMachine;

                    if (washingMachine.isDouble || washingMachine is HiddenAntBox || washingMachine is FreezedAntBox)
                    {
                        continue;
                    }

                    Color nextFaction;
                    int numAnt;

                    GetNextAntSpawnByDifficulty(difficultyLevel: 0, out nextFaction, out numAnt, isIncludeStartSlot: false);

                    washingMachine.Init(washingMachineStartSlot, nextFaction, numAnt);
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

            // Remove blockages
            for (int i = 0; i < TileManager.Ins.tiles.Count; i++)
            {
                BaseTile tile = TileManager.Ins.tiles[i];

                if (!tile.isAntApproaching && !tilesToClear.Contains(tile))
                {
                    List<BaseTile> toRemoveBlockages = tilesToClear.Where(item => tile.blockages.Contains(item)).ToList();

                    for (int j = 0; j < toRemoveBlockages.Count; j++)
                    {
                        tile.blockages.Remove(toRemoveBlockages[j]);
                        tile.actualBlockages.Remove(toRemoveBlockages[j]);
                    }
                }
            }

            for (int i = 0; i < tilesToClear.Count; i++)
            {
                int index = i;

                BaseTile tile = tilesToClear[i];

                Vector3 initialPosition = tile.transform.position;

                tile.isAntApproaching = true;

                Tween.ScaleY(tile.transform, 1.6f, duration: 0.3f);
                Tween.PositionY(tile.transform, tile.transform.position.y + 1.6f, duration: 0.3f)
                .OnComplete(() =>
                {
                    Tween.ScaleY(tile.transform, 1, duration: 0.15f);
                    Tween.Position(tile.transform, initialPosition, duration: 0.15f)
                    .OnComplete(() =>
                    {
                        SaferioTween.ScaleDownWithPunch(tile.transform, onCompleteAction: () =>
                        {
                            tile.Complete();
                        });

                        if (index == tilesToClear.Count - 1)
                        {
                            SoundManager.Instance.PlaySound(GameConstants.tileHitGroundSound);
                        }
                    });
                });
            }

            AdjustAntBoxAfterClearingOneColor(tilesToClear);
        }

        private void AdjustAntBoxAfterClearingOneColor(List<BaseTile> tilesToClear)
        {
            if (tilesToClear.Count == 0)
            {
                return;
            }

            Color factionToClear = tilesToClear[0].Faction;

            List<WashingMachineItem> antBoxes = new List<WashingMachineItem>();

            for (int i = 0; i < washingMachineSlots.Count; i++)
            {
                if (washingMachineSlots[i].washingMachineItem != null)
                {
                    antBoxes.Add(washingMachineSlots[i].washingMachineItem);
                }
            }

            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                if (washingMachineStartSlots[i].washingMachine != null)
                {
                    antBoxes.Add(washingMachineStartSlots[i].washingMachine);
                }
            }

            int numToClear = tilesToClear.Count;

            for (int i = 0; i < antBoxes.Count; i++)
            {
                WashingMachineItem antBox = antBoxes[i];

                if (numToClear <= 0)
                {
                    break;
                }

                if (antBox.number > 0 && antBox.faction == tilesToClear[0].Faction)
                {
                    int numToRemove = Mathf.Min(antBox.number, numToClear);
                    antBox.number -= numToRemove;
                    numToClear -= numToRemove;

                    if (numToClear >= antBox.number)
                    {
                        if (!antBox.isInSlot)
                        {
                            antBox.washingMachineStartSlot.washingMachine = null;
                        }
                    }
                }
            }

            MoveAntBoxTop();

            for (int i = 0; i < antBoxes.Count; i++)
            {
                WashingMachineItem antBox = antBoxes[i];

                if (antBox.faction == tilesToClear[0].Faction)
                {
                    antBox.UpdateNumber();
                }
            }

            if (numToClear > 0)
            {
                var indices = poolFaction
                    .Select((item, index) => new { item, index })
                    .Where(x => x.item == factionToClear)
                    .Take(numToClear)
                    .Select(x => x.index)
                    .OrderByDescending(i => i);

                foreach (int index in indices)
                {
                    poolFaction.RemoveAt(index);
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
        public bool CheckLoseLevel()
        {
            int numAntBoxLeft = washingMachineSlots.Count;

            for (int i = 0; i < washingMachineSlots.Count; i++)
            {
                WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

                if (washingMachineSlot.isLocked)
                {
                    numAntBoxLeft--;
                }
                else
                {
                    WashingMachineItem antBox = washingMachineSlot.washingMachineItem;

                    if (antBox != null && antBox.isStucking)
                    {
                        numAntBoxLeft--;
                    }
                }
            }

            bool isLose = numAntBoxLeft == 0;

            if (isLose)
            {
                List<WashingMachineItem> stuckAntBoxes = washingMachineSlots.Where(item => item.washingMachineItem != null).Select(item => item.washingMachineItem).ToList();

                for (int i = 0; i < stuckAntBoxes.Count; i++)
                {
                    WashingMachineItem antBox = stuckAntBoxes[i];

                    antBox.PlaySelectFalseAnimationLoop();
                }

                StartCoroutine(PendingLoseLevel());
            }

            return isLose;
        }

        private IEnumerator PendingLoseLevel()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

            while (GamePersistentVariable.numActiveAnt > 0)
            {
                yield return waitForSeconds;
            }

            loseLevelEvent?.Invoke();

            GamePersistentVariable.isLevelLost = true;
        }

        public bool IsNoSlotAvailable()
        {
            return washingMachineSlots.All(item => item.isLocked || item.washingMachineItem != null);
        }

        private void Revive(BoosterType boosterType)
        {
            // Revive by unlock ant box
            // for (int i = 0; i < washingMachineSlots.Count; i++)
            // {
            //     WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

            //     if (washingMachineSlot.isLocked)
            //     {
            //         washingMachineSlot.Unlock();

            //         break;
            //     }
            // }

            List<WashingMachineItem> antBoxesToClear = new List<WashingMachineItem>();
            ghostAntBoxes = new List<WashingMachineItem>();

            for (int i = 0; i < washingMachineSlots.Count; i++)
            {
                WashingMachineSlot slot = washingMachineSlots[i];

                if (slot.washingMachineItem != null)
                {
                    antBoxesToClear.Add(slot.washingMachineItem);
                    ghostAntBoxes.Add(slot.washingMachineItem);

                    if (antBoxesToClear.Count == 2)
                    {
                        break;
                    }
                }
            }

            foreach (var item in antBoxesToClear)
            {
                item.CheckAntsPathGhostMode();
            }

            GamePersistentVariable.isLevelLost = false;
        }
        #endregion

        #region Ghost Ant
        public bool IsAnyGhostAntBox()
        {
            return ghostAntBoxes.Count > 0;
        }

        public void CompleteGhostAntBox(WashingMachineItem antBox)
        {
            ghostAntBoxes.Remove(antBox);

            if (ghostAntBoxes.Count == 0)
            {
                ghostAntBoxesClearedEvent?.Invoke();
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

        private bool GetNextAntSpawnByDifficulty(int difficultyLevel, out Color nextFaction, out int numAnt, bool isIncludeStartSlot = true)
        {
            Dictionary<Color, int> numSpawnPerFactionOrderByDifficulty = GetNumAntSpawnPerFactionOrderByDifficulty(isIncludeStartSlot);

            // Debug.Log($"GetNextAntSpawnByDifficulty =============== {difficultyLevel}");

            // foreach (var item in numSpawnPerFactionOrderByDifficulty)
            // {
            //     Debug.Log($"GetNextAntSpawnByDifficulty {ColorUtil.ColorToHex(item.Key)} {item.Value}");
            // }

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

        public Dictionary<Color, int> GetMaxAntRequiredPerFaction(bool isIncludeStartSlot = true)
        {
            Dictionary<Color, int> result = GetRemaniningTilePerFaction();

            if (isIncludeStartSlot)
            {
                for (int i = 0; i < washingMachineStartSlots.Count; i++)
                {
                    WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

                    if (washingMachineStartSlot.washingMachine != null)
                    {
                        CommonUtil.SumValueOfDictionary(result, washingMachineStartSlot.washingMachine.faction, -washingMachineStartSlot.washingMachine.number);
                    }
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
                // CommonUtil.SumValueOfDictionary(sortedPoolFaction, tile.Faction, tile.blockages.Count < 4 ? 50 : 0);
                // CommonUtil.SumValueOfDictionary(sortedPoolFaction, tile.Faction, tile.ring);
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

        public Dictionary<Color, int> GetNumAntSpawnPerFactionOrderByDifficulty(bool isIncludeStartSlot = true)
        {
            Dictionary<Color, int> result = new Dictionary<Color, int>();
            Dictionary<Color, int> numNotBlockedTilePerFaction = GetNumNotBlockedTilePerFaction();
            Dictionary<Color, int> maxAntRequiredPerFaction = GetMaxAntRequiredPerFaction(isIncludeStartSlot);

            foreach (var faction in numNotBlockedTilePerFaction.Keys)
            {
                int maxValue = maxAntRequiredPerFaction[faction];
                maxValue = Mathf.Min(maxValue, maxAntNumberThreshold);

                if (maxValue < minAntNumberThreshold)
                {
                    CommonUtil.SumValueOfDictionary(result, faction, maxValue);
                }
                else
                {
                    CommonUtil.SumValueOfDictionary(result, faction, RandomPreferMultipleOf5(minAntNumberThreshold, maxValue));
                }
            }

            return result;
        }

        public static int RandomPreferMultipleOf5(int min, int max)
        {
            int first = Mathf.CeilToInt(min / 5f) * 5;
            int last = Mathf.FloorToInt(max / 5f) * 5;

            // At least one multiple of 5 exists in range
            if (first <= last)
            {
                int count = ((last - first) / 5) + 1;
                return first + UnityEngine.Random.Range(0, count) * 5;
            }

            // No multiple of 5 exists in range
            return min;
        }

        private float GetLevelProgress()
        {
            return (totalTile - poolFaction.Count) / (float)totalTile;
        }

        private int GetCurrentLevelDifficulty()
        {
            float progress = GetLevelProgress();

            int difficultyAtCurrentProgress = (int)(difficultyLevel * difficultyCurve.Evaluate(progress));

            return difficultyAtCurrentProgress;
        }
        #endregion
    }
}
