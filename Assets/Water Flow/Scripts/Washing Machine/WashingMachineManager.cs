using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Utilities;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;

namespace AntsFlow
{
    public class WashingMachineManager : Singleton<WashingMachineManager>
    {
        [Header("Bucket")]
        public List<FactionGrainRequirement> factionGrainRequirements;
        public float[] pourXPositions;
        public int pourPositionIndex;

        public LevelItem levelItem;
        public Camera mainCamera;

        public List<WashingMachineSlot> washingMachineSlots;
        public List<WashingMachineStartSlot> washingMachineStartSlots;
        public GrainPool grainPool;

        public List<Color> poolFaction;

        public int difficultyLevel;
        public int minAntNumberThreshold;
        public int maxAntNumberThreshold;
        public AnimationCurve difficultyCurve;

        private int totalTile;
        public int numAntBoxSpawned;
        public List<WashingMachineItem> ghostAntBoxes;
        public Coroutine pendingLoseLevelCoroutine;

        public IntVariable currentLevel;

        public static event Action winLevelEvent;
        public static event Action loseLevelEvent;
        public static event Action ghostAntBoxesClearedEvent;
        public static event Action finishUsingEnableAllAntBoxesBoosterEvent;

        protected override void MoreActionOnAwake()
        {
            base.MoreActionOnAwake();

            RevivePopup.reviveEvent += Revive;
            WaterFlowGameInput.useClearOneColorBoosterEvent += UseBoosterClearOneColor;
            WashingMachineItem.selectWashingMachineEvent += OnAntBoxSelected;
            LevelLoader.startLevelEvent += OnLevelReplayed;
            SettingPopup.replayLevelEvent += OnLevelReplayed;
            LosePopup.replayLevelEvent += OnLevelReplayed;
            DebugPopup.switchLevelEvent += OnLevelReplayed;
            TileManager.winLevelEvent += OnLevelReplayed;
        }

        void OnDestroy()
        {
            RevivePopup.reviveEvent -= Revive;
            WaterFlowGameInput.useClearOneColorBoosterEvent -= UseBoosterClearOneColor;
            WashingMachineItem.selectWashingMachineEvent -= OnAntBoxSelected;
            LevelLoader.startLevelEvent -= OnLevelReplayed;
            SettingPopup.replayLevelEvent -= OnLevelReplayed;
            LosePopup.replayLevelEvent -= OnLevelReplayed;
            DebugPopup.switchLevelEvent -= OnLevelReplayed;
            TileManager.winLevelEvent -= OnLevelReplayed;
        }

        private void OnAntBoxSelected(WashingMachineItem antBox)
        {
            if (GamePersistentVariable.boosterMode == BoosterMode.EnableAllAntBoxes)
            {
                Tween.PositionY(mainCamera.transform, 0, duration: 0.3f);

                GamePersistentVariable.boosterMode = BoosterMode.None;

                finishUsingEnableAllAntBoxesBoosterEvent?.Invoke();
            }

            CheckNoAntBoxLeftOnStartSlots();
        }

        public void StartLevel(AntsFlow.LevelItem levelItem)
        {
            this.levelItem = levelItem;

            try
            {
                int difficultyMultiplier = RemoteConfigController.GetIntConfig("difficultyMultiplier", 1);
                int difficultyAddend = RemoteConfigController.GetIntConfig("difficultyAddend", 0);

                difficultyLevel = difficultyAddend + difficultyMultiplier * levelItem.difficulty;
            }
            catch
            {
                difficultyLevel = levelItem.difficulty;
            }

            numAntBoxSpawned = 0;

            ChickenManager.Ins.SpawnFirstChickensUsingLevelConfig(levelItem);

            SetupFactionGrainRequirements();

            // SpecialElementManager.Ins.Setup(levelItem);

            List<WashingMachineItem> washingMachineItems = SpawnFirstWashingMachines(levelItem.numStartAntBoxes);

            if (levelItem.isTutorialLevel)
            {
                TutorialLevelOne.Ins.StartTutorial(washingMachineItems);
            }
            else
            {
                TutorialLevelOne.Ins.HideTutorial();
            }

            // if (levelItem.isTutorialSpeedUpLevel)
            // {
            //     SpeedUpTutorialManager.Ins.StartTutorial();
            // }
        }

        #region Chicken Feed    
        public void SetupFactionGrainRequirements()
        {
            factionGrainRequirements = new List<FactionGrainRequirement>();

            foreach (var chicken in ChickenManager.Ins.chickens)
            {
                if (factionGrainRequirements.Any(item => item.faction == chicken.faction))
                {
                    FactionGrainRequirement factionGrainRequirement = factionGrainRequirements.First(item => item.faction == chicken.faction);

                    factionGrainRequirement.totalGrain += chicken.numGrainPileRequired;
                }
                else
                {
                    FactionGrainRequirement factionGrainRequirement = new FactionGrainRequirement()
                    {
                        faction = chicken.faction,
                        totalGrain = chicken.numGrainPileRequired
                    };

                    factionGrainRequirements.Add(factionGrainRequirement);
                }
            }
        }

        public void MinusFactionGrainRequirement(Color faction, int number)
        {
            if (factionGrainRequirements.Any(item => item.faction == faction))
            {
                FactionGrainRequirement factionGrainRequirement = factionGrainRequirements.First(item => item.faction == faction);

                factionGrainRequirement.totalGrain -= number;
            }
        }

        public void SetupPourPositions()
        {
            float totalWidth = 0.8f * GamePersistentVariable.screenSizeWorld.x;
            int numSlot = 8;
            float distance = totalWidth / numSlot;
            float start = -0.5f * totalWidth;

            pourXPositions = new float[numSlot];

            for (int i = 0; i < numSlot; i++)
            {
                pourXPositions[i] = start + i * distance;
            }

            pourXPositions.Shuffle();
        }

        public float GetPourPositionX()
        {
            if (pourXPositions == null || pourXPositions.Length == 0)
            {
                SetupPourPositions();
            }

            if (pourPositionIndex >= pourXPositions.Length - 1)
            {
                pourPositionIndex = 0;

                pourXPositions.Shuffle();
            }
            else
            {
                pourPositionIndex++;
            }

            return pourXPositions[pourPositionIndex];
        }
        #endregion

        public WashingMachineSlot MoveItemToSlot(WashingMachineItem washingMachineItem, Action onCompleteAction = null)
        {
            if (grainPool.IsMaxCapacity())
            {
                return null;
            }
            else
            {
                grainPool.AddGrain(washingMachineItem.number);
            }

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

                    MoveAntBoxTop();

                    onCompleteAction?.Invoke();
                });
            });

            return washingMachineSlot;
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
                int index = i;

                WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

                if (washingMachineStartSlot.washingMachine != null)
                {
                    ObjectPoolingEverything.ReturnToPool(GameConstants.washingMachine, washingMachineStartSlot.washingMachine.gameObject);
                }

                numAntBoxSpawned++;

                WashingMachineItem washingMachineItem = GetAntBoxFromPool(washingMachineStartSlot.row, washingMachineStartSlot.column);

                washingMachineItems.Add(washingMachineItem);

                washingMachineStartSlot.washingMachine = washingMachineItem;

                int difficulty = i / 3;

                if (currentLevel.Value <= 10)
                {
                    difficulty = 0;

                    if (i == 5)
                    {
                        difficulty = 1;
                    }
                }

                if (levelItem.overrideInitialDifficulty)
                {
                    difficulty = levelItem.initialDifficulty;
                }

                GetNextAntSpawnByDifficulty(difficultyLevel: difficulty, out nextFaction, out numAnt);

                if (levelItem.isUseManualFactionSetupForFirstAntBoxes)
                {
                    nextFaction = levelItem.levelFactionConfig.items[i].faction;
                    numAnt = levelItem.levelFactionConfig.items[i].numAnt;
                }

                washingMachineItem.Init(washingMachineStartSlot, nextFaction, numAnt, onCompleteAction: () =>
                {
                    SpecialElementManager.Ins.SpawnAntBoxLinkFirstAntBoxes(index);
                });

                MinusFactionGrainRequirement(nextFaction, numAnt);
            }

            return washingMachineItems;
        }

        public void MoveAntBoxTop()
        {
            bool isAlreadySpawnNewBox = false;

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

                            if (washingMachineStartSlot.row == 1 && washingMachineItem is FreezedAntBox)
                            {
                                (washingMachineItem as FreezedAntBox).AssignRequirementNumber();
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

                    if (currentLevel.Value <= 10)
                    {
                        if (!isFound && !isAlreadySpawnNewBox)
                        {
                            if (washingMachineStartSlot.row < 2)
                            {
                                SpawnNewWashingMachine();
                            }
                            else
                            {
                                if (!isAlreadySpawnNewBox)
                                {
                                    SpawnNewWashingMachine();

                                    isAlreadySpawnNewBox = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!isFound)
                        {
                            SpawnNewWashingMachine();
                        }
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

                    WashingMachineItem washingMachineItem = GetAntBoxFromPool(washingMachineStartSlot.row, washingMachineStartSlot.column);

                    washingMachineItem.gameObject.SetActive(true);
                    washingMachineItem.transform.SetParent(washingMachineStartSlot.transform);

                    washingMachineItem.transform.localPosition = new Vector3(0, -5, 0);
                    Tween.LocalPositionY(washingMachineItem.transform, 0, duration: 0.3f);

                    washingMachineStartSlot.washingMachine = washingMachineItem;

                    washingMachineItem.Init(washingMachineStartSlot, nextFaction, numAnt);

                    MinusFactionGrainRequirement(nextFaction, numAnt);

                    SpecialElementManager.Ins.SpawnAntBoxLink();

                    break;
                }
            }
        }

        private WashingMachineItem GetAntBoxFromPool(int rowIndex, int columnIndex)
        {
            string key = GameConstants.washingMachine;

            LevelElement levelElement = SpecialElementManager.Ins.GetNextLevelElement(rowIndex, columnIndex);

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
            else if (levelElement == LevelElement.WarriorAntBox)
            {
                key = GameConstants.warriorAntBox;
            }
            else if (levelElement == LevelElement.SnakeAntBox)
            {
                key = GameConstants.snakeAntBox;
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
            // List<BaseTile> randomPickedTiles = new List<BaseTile>();

            // Dictionary<Color, int> numTileWithNoAntPerFaction = GetNumTileWithNoAntPerFaction();

            // for (int i = 0; i < TileManager.Ins.tiles.Count; i++)
            // {
            //     BaseTile tile = TileManager.Ins.tiles[i];

            //     if (tile.gameObject.activeInHierarchy && !tile.isAntApproaching)
            //     {
            //         randomPickedTiles.Add(tile);
            //     }
            // }

            // randomPickedTiles.Shuffle();
            // randomPickedTiles = randomPickedTiles.Take(10).ToList();

            // for (int i = 0; i < randomPickedTiles.Count; i++)
            // {
            //     BaseTile tile = randomPickedTiles[i];

            //     for (int j = 0; j < washingMachineStartSlots.Count; j++)
            //     {
            //         WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[j];
            //         WashingMachineItem washingMachine = washingMachineStartSlot.washingMachine;

            //         if (washingMachine != null)
            //         {
            //             if (washingMachine.faction == tile.Faction && washingMachine.number > 0)
            //             {
            //                 Tween.Position(tile.transform, washingMachine.transform.position, duration: 0.3f)
            //                 .OnComplete(() =>
            //                 {
            //                     ParticleSystem antDissappearFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.antDissapearFxPrefab);

            //                     antDissappearFx.transform.position = tile.transform.position;
            //                     antDissappearFx.Play();

            //                     tile.gameObject.SetActive(false);
            //                 });

            //                 tile.isAntApproaching = true;

            //                 washingMachine.ReceiveTile();
            //             }
            //         }
            //     }
            // }
        }

        public void UseBoosterShuffle()
        {
            Dictionary<WashingMachineItem, WashingMachineStartSlot> slotOfShuffleBuckets = new Dictionary<WashingMachineItem, WashingMachineStartSlot>();

            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

                if (washingMachineStartSlot.washingMachine != null)
                {
                    WashingMachineItem bucket = washingMachineStartSlot.washingMachine;

                    if (bucket.isDouble || bucket is HiddenAntBox || bucket is FreezedAntBox)
                    {
                        continue;
                    }

                    slotOfShuffleBuckets.Add(bucket, washingMachineStartSlot);

                    // restore faction required grain count
                    if (factionGrainRequirements.Any(item => item.faction == bucket.faction))
                    {
                        FactionGrainRequirement factionGrainRequirement = factionGrainRequirements.First(item => item.faction == bucket.faction);

                        factionGrainRequirement.totalGrain += bucket.number;
                    }
                }
            }

            Dictionary<Color, int> maxGrainPerFaction = new Dictionary<Color, int>();

            if (slotOfShuffleBuckets.Count > 6)
            {
                maxGrainPerFaction = GetNumAntSpawnPerFactionOrderByDifficulty();
            }
            else
            {
                foreach (var item in factionGrainRequirements)
                {
                    CommonUtil.SumValueOfDictionary(maxGrainPerFaction, item.faction, item.totalGrain);
                }
            }

            List<FactionGrainRequirement> bucketDataItems = SplitGrain(maxGrainPerFaction, slotOfShuffleBuckets.Count);

            int index = 0;

            foreach (var bucket in slotOfShuffleBuckets.Keys)
            {
                FactionGrainRequirement bucketData = bucketDataItems[index];

                bucket.Init(slotOfShuffleBuckets[bucket], bucketData.faction, bucketData.totalGrain);

                MinusFactionGrainRequirement(bucketData.faction, bucketData.totalGrain);

                index++;
            }

            List<FactionGrainRequirement> SplitGrain(Dictionary<Color, int> grainByFaction, int exactCount)
            {
                List<FactionGrainRequirement> result = new List<FactionGrainRequirement>(exactCount);

                Dictionary<Color, int> remaining = new Dictionary<Color, int>();

                foreach (var faction in grainByFaction.Keys)
                {
                    remaining.Add(faction, grainByFaction[faction]);
                }

                Color validFaction = Color.white;

                for (int i = 0; i < exactCount; i++)
                {
                    bool isFound = false;

                    foreach (var faction in remaining.Keys)
                    {
                        if (remaining[faction] >= 10)
                        {
                            if (result.Any(item => item.faction == faction))
                            {
                                validFaction = faction;

                                continue;
                            }
                            else
                            {
                                FactionGrainRequirement factionGrainRequirement = new FactionGrainRequirement();

                                factionGrainRequirement.faction = faction;
                                factionGrainRequirement.totalGrain = 10;

                                result.Add(factionGrainRequirement);

                                remaining[faction] -= 10;

                                isFound = true;

                                break;
                            }
                        }
                    }

                    if (!isFound)
                    {
                        FactionGrainRequirement factionGrainRequirement = new FactionGrainRequirement();

                        factionGrainRequirement.faction = validFaction;
                        factionGrainRequirement.totalGrain = 10;

                        result.Add(factionGrainRequirement);

                        remaining[validFaction] -= 10;
                    }
                }

                foreach (var faction in remaining.Keys)
                {
                    if (remaining[faction] > 0)
                    {
                        for (int i = 0; i < result.Count; i++)
                        {
                            FactionGrainRequirement factionGrainRequirement = result[i];

                            if (factionGrainRequirement.faction == faction)
                            {
                                factionGrainRequirement.totalGrain += remaining[faction];

                                break;
                            }
                        }
                    }
                }

                return result;
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
            List<BaseChicken> chickensToClear = ChickenManager.Ins.chickens
                .Where(chicken => !chicken.isDone && chicken.faction == selectedFaction && !chicken.isEating).ToList();

            bool isLast = false;

            int numGrainRequiredRemoved = 0;

            for (int i = 0; i < chickensToClear.Count; i++)
            {
                BaseChicken chicken = chickensToClear[i];

                isLast = i == chickensToClear.Count - 1;

                chicken.SetActiveSpine(true);
                chicken.ForceFlyOut(isLast);

                ChickenManager.Ins.chickenRenderer.RemoveChickenRendering(chicken);

                numGrainRequiredRemoved += chicken.numGrainPileRequiredLeft;
            }

            MinusFactionGrainRequirement(selectedFaction, numGrainRequiredRemoved);

            AdjustAntBoxAfterClearingOneColor(chickensToClear);

            // for (int i = 0; i < chickensToClear.Count; i++)
            // {
            //     int index = i;

            //     BaseChicken chicken = chickensToClear[i];

            //     Vector3 initialPosition = chicken.transform.position;

            //     chicken.isEating = true;

            //     Tween.ScaleY(chicken.transform, 1.6f, duration: 0.3f);
            //     Tween.PositionY(chicken.transform, chicken.transform.position.y + 1.6f, duration: 0.3f)
            //     .OnComplete(() =>
            //     {
            //         Tween.ScaleY(chicken.transform, 1, duration: 0.15f);
            //         Tween.Position(chicken.transform, initialPosition, duration: 0.15f)
            //         .OnComplete(() =>
            //         {
            //             SaferioTween.ScaleDownWithPunch(chicken.transform, onCompleteAction: () =>
            //             {
            //                 chicken.Complete();

            //                 if (index == chickensToClear.Count - 1)
            //                 {
            //                     ChickenManager.Ins.CheckWinLevel();
            //                 }
            //             });

            //             if (index == chickensToClear.Count - 1)
            //             {
            //                 SoundManager.Instance.PlaySound(GameConstants.tileHitGroundSound);
            //             }
            //         });
            //     });
            // }

            // ChickenManager.Ins.MoveChickensDown();
        }

        private void AdjustAntBoxAfterClearingOneColor(List<BaseChicken> chickensToClear)
        {
            if (chickensToClear.Count == 0)
            {
                return;
            }

            Color factionToClear = chickensToClear[0].faction;

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

            for (int i = 0; i < antBoxes.Count; i++)
            {
                WashingMachineItem antBox = antBoxes[i];

                if (antBox.number > 0 && antBox.faction == factionToClear)
                {
                    antBox.number = 0;
                    antBox.washingMachineStartSlot.washingMachine = null;
                    antBox.washingMachineStartSlot = null;
                }
            }

            MoveAntBoxTop();

            for (int i = 0; i < antBoxes.Count; i++)
            {
                WashingMachineItem antBox = antBoxes[i];

                if (antBox.faction == factionToClear)
                {
                    antBox.UpdateNumber();

                    if (antBox.number <= 0)
                    {
                        antBox.Done();
                    }
                }
            }

            // Clear grail pile
            int totalRemovedGrain = 0;

            for (int i = 0; i < GrainPool.Ins.grainPiles.Count; i++)
            {
                GrainPile grainPile = GrainPool.Ins.grainPiles[i];

                if (grainPile.faction == factionToClear)
                {
                    totalRemovedGrain += grainPile.remainingNumber;
                    grainPile.remainingNumber = 0;
                }
            }

            GrainPool.Ins.AddGrain(-totalRemovedGrain);

            GrainSimulation.Ins.RemoveAllGrainsOfFaction(factionToClear);
        }
        #endregion

        #region Check Win
        private void CheckNoAntBoxLeftOnStartSlots()
        {
            int numAntBoxLeftOnStartSlots = washingMachineStartSlots.Count(item => item.washingMachine != null);

            if (numAntBoxLeftOnStartSlots <= 1)
            {
                UpSpeedAnt.Ins.UpSpeedNearEndLevel();
            }
        }

        private void CheckWinLevel()
        {
            bool isWin = poolFaction.Count + ClothesManager.Ins.clothingPieces.Count == 0;

            if (isWin)
            {
                OnLevelReplayed();

                winLevelEvent?.Invoke();
            }
        }
        #endregion

        #region Check Lose
        public bool IsLose()
        {
            return
                GrainPool.Ins.IsMaxCapacity() &&
                GamePersistentVariable.numEatingChicken <= 0 &&
                GamePersistentVariable.numMovingChicken <= 0;
        }

        public bool CheckLoseLevel()
        {
            bool isLose = IsLose();

            if (isLose)
            {
                if (pendingLoseLevelCoroutine != null)
                {
                    StopCoroutine(pendingLoseLevelCoroutine);
                }

                pendingLoseLevelCoroutine = StartCoroutine(PendingLoseLevel());

                GamePersistentVariable.isPendingLoseLevel = true;
            }

            return isLose;
        }

        private IEnumerator PendingLoseLevel()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

            while (GamePersistentVariable.numEatingChicken > 0)
            {
                yield return waitForSeconds;
            }

            if (IsLose() && !GamePersistentVariable.isLevelLost)
            {
                loseLevelEvent?.Invoke();

                GamePersistentVariable.isLevelLost = true;
            }

            GamePersistentVariable.isPendingLoseLevel = false;
        }

        public bool IsNoSlotAvailable()
        {
            return washingMachineSlots.All(item => item.isLocked || item.washingMachineItem != null);
        }

        private void Revive(BoosterType boosterType)
        {
            grainPool.CheckFeedGhostChickens();

            GamePersistentVariable.isLevelLost = false;

            // List<WashingMachineItem> antBoxesToClear = new List<WashingMachineItem>();
            // ghostAntBoxes = new List<WashingMachineItem>();

            // for (int i = 0; i < washingMachineSlots.Count; i++)
            // {
            //     WashingMachineSlot slot = washingMachineSlots[i];

            //     if (slot.washingMachineItem != null)
            //     {
            //         antBoxesToClear.Add(slot.washingMachineItem);
            //         ghostAntBoxes.Add(slot.washingMachineItem);

            //         if (antBoxesToClear.Count == 2)
            //         {
            //             break;
            //         }
            //     }
            // }

            // foreach (var item in antBoxesToClear)
            // {
            //     item.CheckAntsPathGhostMode();
            // }

            // GamePersistentVariable.isLevelLost = false;
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

        private void OnLevelReplayed()
        {
            if (pendingLoseLevelCoroutine != null)
            {
                StopCoroutine(pendingLoseLevelCoroutine);
            }

            ghostAntBoxes.Clear();

            GamePersistentVariable.numActiveAnt = 0;

            // if (currentLevel.Value < 10)
            // {
            //     maxAntNumberThreshold = 80;
            // }
            // else
            // {
            //     maxAntNumberThreshold = 35;
            // }

            // if (currentLevel.Value <= 5)
            // {
            //     minAntNumberThreshold = 50;
            // }
            // else if (currentLevel.Value <= 10)
            // {
            //     minAntNumberThreshold = 40;
            // }
            // else
            // {
            //     minAntNumberThreshold = 10;
            // }
        }

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

            int maxValidIndex = 0;

            for (int j = 0; j < numSpawnPerFactionOrderByDifficulty.Count; j++)
            {
                if (numSpawnPerFactionOrderByDifficulty.ElementAt(j).Value > 0)
                {
                    maxValidIndex = j;
                }
            }

            int clampedDifficultyLevel = Mathf.Min(difficultyLevel, maxValidIndex);

            for (int j = clampedDifficultyLevel; j < numSpawnPerFactionOrderByDifficulty.Count; j++)
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

            for (int i = 0; i < ChickenManager.Ins.chickens.Count; i++)
            {
                BaseChicken chicken = ChickenManager.Ins.chickens[i];

                if (chicken.isDone)
                {
                    continue;
                }

                CommonUtil.SumValueOfDictionary(result, chicken.faction, chicken.numGrainPileRequiredLeft);
            }

            return result;
        }

        public Dictionary<Color, int> GetMaxAntRequiredPerFaction(bool isIncludeStartSlot = true)
        {
            Dictionary<Color, int> result = GetRemaniningTilePerFaction();

            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

                if (washingMachineStartSlot.washingMachine != null)
                {
                    if (isIncludeStartSlot)
                    {
                        CommonUtil.SumValueOfDictionary(result, washingMachineStartSlot.washingMachine.faction, -washingMachineStartSlot.washingMachine.number);
                    }
                    else
                    {
                        WashingMachineItem antBox = washingMachineStartSlot.washingMachine;

                        if (antBox.isShuffled || antBox.isDouble || antBox is HiddenAntBox || antBox is FreezedAntBox)
                        {
                            CommonUtil.SumValueOfDictionary(result, washingMachineStartSlot.washingMachine.faction, -washingMachineStartSlot.washingMachine.number);
                        }
                    }
                }
            }

            for (int i = 0; i < GrainPool.Ins.grainPiles.Count; i++)
            {
                GrainPile grainPile = GrainPool.Ins.grainPiles[i];

                CommonUtil.SumValueOfDictionary(result, grainPile.faction, -grainPile.remainingNumber);
            }

            return result;
        }

        public Dictionary<Color, int> GetNumNotBlockedTilePerFaction(bool isIncludeStartSlot = true)
        {
            Dictionary<Color, int> sortedPoolFaction = new Dictionary<Color, int>();

            ChickenManager.Ins.CheckChickensStuck();

            for (int i = 0; i < ChickenManager.Ins.chickens.Count; i++)
            {
                BaseChicken chicken = ChickenManager.Ins.chickens[i];

                if (chicken.isDone)
                {
                    continue;
                }

                bool isBlocked = chicken.isStuck;

                CommonUtil.SumValueOfDictionary(sortedPoolFaction, chicken.faction, isBlocked ? 0 : chicken.numGrainPileRequiredLeft);
            }

            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

                if (washingMachineStartSlot.washingMachine != null)
                {
                    if (isIncludeStartSlot)
                    {
                        if (washingMachineStartSlot.washingMachine.number != 0)
                        {
                            CommonUtil.SumValueOfDictionary(sortedPoolFaction, washingMachineStartSlot.washingMachine.faction, -washingMachineStartSlot.washingMachine.number);
                        }
                    }
                    else
                    {
                        WashingMachineItem antBox = washingMachineStartSlot.washingMachine;

                        if (antBox.isShuffled || antBox.isDouble || antBox is HiddenAntBox || antBox is FreezedAntBox)
                        {
                            if (washingMachineStartSlot.washingMachine.number != 0)
                            {
                                CommonUtil.SumValueOfDictionary(
                                    sortedPoolFaction, washingMachineStartSlot.washingMachine.faction, -washingMachineStartSlot.washingMachine.number);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < GrainPool.Ins.grainPiles.Count; i++)
            {
                GrainPile grainPile = GrainPool.Ins.grainPiles[i];

                CommonUtil.SumValueOfDictionary(sortedPoolFaction, grainPile.faction, -grainPile.remainingNumber);
            }

            sortedPoolFaction = sortedPoolFaction.OrderByDescending(item => item.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return sortedPoolFaction;
        }

        public Dictionary<Color, int> GetNumAntSpawnPerFactionOrderByDifficulty(bool isIncludeStartSlot = true)
        {
            Dictionary<Color, int> result = new Dictionary<Color, int>();
            Dictionary<Color, int> numNotBlockedTilePerFaction = GetNumNotBlockedTilePerFaction(isIncludeStartSlot);

            // Debug.Log($"GetNumAntSpawnPerFactionOrderByDifficulty ================");

            foreach (var faction in numNotBlockedTilePerFaction.Keys)
            {
                int maxNumGrain = 0;

                if (factionGrainRequirements.Any(item => item.faction == faction))
                {
                    FactionGrainRequirement factionGrainRequirement = factionGrainRequirements.First(item => item.faction == faction);

                    maxNumGrain = factionGrainRequirement.totalGrain;
                }

                // DebugUtil.Log($"GetNumAntSpawnPerFactionOrderByDifficulty {maxNumGrain}", faction);

                int maxValue = maxNumGrain;
                maxValue = Mathf.Min(maxValue, maxAntNumberThreshold);

                if (maxValue <= minAntNumberThreshold)
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
            int first = Mathf.CeilToInt(min / 10f) * 10;
            int last = Mathf.FloorToInt(max / 10f) * 10;

            // At least one multiple of 10 exists in range
            if (first <= last)
            {
                int count = ((last - first) / 10) + 1;
                return first + UnityEngine.Random.Range(0, count) * 10;
            }

            // No multiple of 10 exists in range
            return min;
        }

        private float GetLevelProgress()
        {
            return 0;

            int numDoneTile = TileManager.Ins.tiles.Count(tile => tile.isDone);

            return numDoneTile / (float)totalTile;
        }

        private int GetCurrentLevelDifficulty()
        {
            float progress = GetLevelProgress();

            int difficultyAtCurrentProgress = (int)(difficultyLevel * difficultyCurve.Evaluate(progress));

            // Debug.Log($"GetCurrentLevelDifficulty {difficultyAtCurrentProgress} {difficultyLevel} {progress} {difficultyCurve.Evaluate(progress)}");

            return difficultyAtCurrentProgress;
        }
        #endregion
    }
}

[Serializable]
public class FactionGrainRequirement
{
    public Color faction;
    public int totalGrain;
}
