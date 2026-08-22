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
        public Vector2[] pourPositions;
        public int pourPositionIndex;

        public LevelItem levelItem;
        public Camera mainCamera;

        public List<WashingMachineSlot> washingMachineSlots;
        public List<WashingMachineStartSlot> washingMachineStartSlots;
        public GrainPool grainPool;

        public List<Color> poolFaction;

        private int totalTile;
        public int numAntBoxSpawned;
        public List<WashingMachineItem> ghostAntBoxes;
        public Coroutine pendingLoseLevelCoroutine;

        public IntVariable currentLevel;

        [Header("Difficulty")]
        public int difficultyLevel;
        public int minAntNumberThreshold;
        public int maxAntNumberThreshold;
        public AnimationCurve difficultyCurve;
        public int totalFaction;
        public LevelDifficultyReduceOnRevive levelDifficultyReduceOnRevive;
        public LevelDifficultyReduceOnConsecutiveStuck levelDifficultyReduceOnConsecutiveStuck;

        [Header("Manual Level Bucket Spawn")]
        public LevelFactionConfig levelFactionConfig;

        #region Optimize
        private int[] rowPowerCache;
        #endregion

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

            InitializeRowPowerCache(10);
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

            if (levelItem.isUseManualFactionSetupForFirstAntBoxes)
            {
                levelFactionConfig = levelItem.levelFactionConfig;
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

            if (levelItem.isTutorialSpeedUpLevel)
            {
                SpeedUpTutorialManager.Ins.StartTutorial();
            }
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

            totalFaction = factionGrainRequirements.Count;
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
            Vector2 grainPoolWorldSize = GrainPool.Ins.grainPoolSpriteRenderer.bounds.size;

            float totalWidth = 0.8f * grainPoolWorldSize.x;
            int numColumn = 5;
            int numRow = 2;
            int numSlot = numColumn * numRow;
            Vector2 distance = new Vector2(totalWidth / numColumn, 4.5f);
            Vector2 start = new Vector2(-0.5f * totalWidth, 0);

            pourPositions = new Vector2[numSlot];

            for (int i = 0; i < numRow; i++)
            {
                for (int j = 0; j < numColumn; j++)
                {
                    pourPositions[j + numColumn * i] = start + new Vector2(j * distance.x, i * distance.y);
                }
            }

            pourPositions.Shuffle();
        }

        public Vector2 GetPourPosition()
        {
            if (pourPositions == null || pourPositions.Length == 0)
            {
                SetupPourPositions();
            }

            if (pourPositionIndex >= pourPositions.Length - 1)
            {
                pourPositionIndex = 0;

                // pourPositions.Shuffle();
            }
            else
            {
                pourPositionIndex++;
            }

            return pourPositions[pourPositionIndex];
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

            Color nextFaction = Color.white;
            int numAnt = 0;
            bool isFound = false;

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

                isFound = false;

                if (levelItem.isUseManualFactionSetupForFirstAntBoxes)
                {
                    foreach (var item in levelFactionConfig.items)
                    {
                        if (item.items.Count == 0)
                        {
                            continue;
                        }

                        if (item.column == washingMachineStartSlot.column)
                        {
                            nextFaction = item.items[0].faction;
                            numAnt = item.items[0].number;

                            item.items.RemoveAt(0);

                            isFound = true;

                            break;
                        }
                    }
                }
                else
                {
                    GetNextAntSpawnByDifficulty(difficultyLevel: difficulty, out nextFaction, out numAnt);

                    isFound = true;
                }

                if (isFound)
                {
                    washingMachineItem.Init(washingMachineStartSlot, nextFaction, numAnt, onCompleteAction: () =>
                    {
                        SpecialElementManager.Ins.SpawnAntBoxLinkFirstAntBoxes(index);
                    });

                    MinusFactionGrainRequirement(nextFaction, numAnt);
                }
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

                            // try set faction for bucket
                            if (washingMachineItem.number == 0)
                            {
                                Color nextFaction;
                                int numAnt;

                                if (GetNextAntSpawnByDifficulty(difficultyLevel: GetCurrentLevelDifficulty(), out nextFaction, out numAnt))
                                {
                                    washingMachineItem.Init(washingMachineStartSlot, nextFaction, numAnt);

                                    MinusFactionGrainRequirement(nextFaction, numAnt);
                                }
                                else
                                {
                                    washingMachineItem.Reset();

                                    otherWashingMachineStartSlot.washingMachine = null;

                                    continue;
                                }
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
            }
        }

        public void SpawnNewWashingMachine()
        {
            Color nextFaction = Color.white;
            int numAnt = 0;
            bool isFound = false;

            if (!levelItem.isUseManualFactionSetupForFirstAntBoxes)
            {
                if (GetNextAntSpawnByDifficulty(difficultyLevel: GetCurrentLevelDifficulty(), out nextFaction, out numAnt))
                {
                    isFound = true;
                }
                else
                {
                    return;
                }
            }

            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

                if (washingMachineStartSlot.washingMachine == null)
                {
                    if (levelItem.isUseManualFactionSetupForFirstAntBoxes)
                    {
                        foreach (var item in levelFactionConfig.items)
                        {
                            if (item.items.Count == 0)
                            {
                                continue;
                            }

                            if (item.column == washingMachineStartSlot.column)
                            {
                                try
                                {
                                    nextFaction = item.items[0].faction;
                                    numAnt = item.items[0].number;

                                    FactionGrainRequirement factionGrainRequirement =
                                        factionGrainRequirements.First(requirement => requirement.faction == nextFaction);

                                    if (factionGrainRequirement.totalGrain >= numAnt)
                                    {
                                        item.items.RemoveAt(0);

                                        isFound = true;

                                        break;
                                    }
                                }
                                catch (System.Exception)
                                {

                                }
                            }
                        }

                        if (!isFound && washingMachineStartSlot.row <= 1)
                        {
                            if (GetNextAntSpawnByDifficulty(difficultyLevel: GetCurrentLevelDifficulty(), out nextFaction, out numAnt))
                            {
                                isFound = true;
                            }
                        }
                    }

                    if (!isFound)
                    {
                        continue;
                    }

                    numAntBoxSpawned++;

                    WashingMachineItem washingMachineItem = GetAntBoxFromPool(washingMachineStartSlot.row, washingMachineStartSlot.column);

                    washingMachineItem.gameObject.SetActive(true);
                    washingMachineItem.transform.SetParent(washingMachineStartSlot.transform);

                    washingMachineItem.transform.localPosition = new Vector3(0, -5, 0);
                    Tween.LocalPositionY(washingMachineItem.transform, 0, duration: 0.3f);

                    washingMachineStartSlot.washingMachine = washingMachineItem;

                    // don't set faction for bucket from row 2
                    if (washingMachineStartSlot.row > 1)
                    {
                        nextFaction = FactionMapper.Ins.allFactions[0];
                        numAnt = 0;
                    }

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

            bool isNoGrainRequiredLeft = factionGrainRequirements.Sum(item => item.totalGrain) == 0;

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

            if (isNoGrainRequiredLeft)
            {
                foreach (var item in factionGrainRequirements)
                {
                    CommonUtil.SumValueOfDictionary(maxGrainPerFaction, item.faction, item.totalGrain);
                }
            }
            else
            {
                maxGrainPerFaction = GetNumAntSpawnPerFactionOrderByDifficulty(isIncludeStartSlot: false);
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
            Color nextFaction;
            int numAnt;

            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineItem bucket = washingMachineStartSlots[i].washingMachine;

                if (bucket != null)
                {
                    bucket.TemporarilyEnable();

                    if (bucket.number == 0 && GetNextAntSpawnByDifficulty(difficultyLevel: 0, out nextFaction, out numAnt))
                    {
                        bucket.Init(bucket.washingMachineStartSlot, nextFaction, numAnt);

                        MinusFactionGrainRequirement(nextFaction, numAnt);
                    }
                }
            }

            Tween.PositionY(mainCamera.transform, -2, duration: 0.3f);
        }

        public void SetBucketsFaction()
        {
            Color nextFaction;
            int numAnt;

            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineItem bucket = washingMachineStartSlots[i].washingMachine;

                if (bucket != null)
                {
                    if (bucket.number == 0 && GetNextAntSpawnByDifficulty(difficultyLevel: 0, out nextFaction, out numAnt))
                    {
                        bucket.Init(bucket.washingMachineStartSlot, nextFaction, numAnt);

                        MinusFactionGrainRequirement(nextFaction, numAnt);
                    }
                }
            }
        }

        public void UseBoosterClearOneColor(Color selectedFaction)
        {
            List<BaseChicken> chickensToClear = ChickenManager.Ins.chickens
                .Where(chicken => !chicken.isDone && chicken.faction == selectedFaction).ToList();

            bool isLast = false;

            int numGrainRequiredRemoved = 0;

            for (int i = 0; i < chickensToClear.Count; i++)
            {
                BaseChicken chicken = chickensToClear[i];

                isLast = i == chickensToClear.Count - 1;

                chicken.SetActiveSpine(true);
                chicken.ForceStopEatingAndFlyOut(isLast);

                ChickenManager.Ins.chickenRenderer.RemoveChickenRendering(chicken);

                numGrainRequiredRemoved += chicken.numGrainPileRequiredLeft;
            }

            MinusFactionGrainRequirement(selectedFaction, numGrainRequiredRemoved);

            AdjustAntBoxAfterClearingOneColor(chickensToClear);

            ChickenManager.Ins.StopHighlightChickensSequentially();

            GamePersistentVariable.levelBoosterUseTime++;
        }

        private void AdjustAntBoxAfterClearingOneColor(List<BaseChicken> chickensToClear)
        {
            if (chickensToClear.Count == 0)
            {
                return;
            }

            Color factionToClear = chickensToClear[0].faction;

            List<WashingMachineItem> antBoxes = new List<WashingMachineItem>();

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

            // // Clear eating chickens
            // List<BaseChicken> eatingChickensOfFaction =
            //     ChickenManager.Ins.chickens.Where(
            //         chicken => chicken.faction == factionToClear && chicken.isEating && !chicken.isDone).ToList();

            // for (int i = 0; i < eatingChickensOfFaction.Count; i++)
            // {
            //     bool isLast = i == eatingChickensOfFaction.Count - 1;

            //     eatingChickensOfFaction[i].ForceStopEatingAndFlyOut(isLast);
            // }

            GrainPool.Ins.AddGrain(-totalRemovedGrain);
            GrainSimulation.Ins.RemoveAllGrainsOfFaction(factionToClear);
        }
        #endregion

        #region Check Win
        private void CheckNoAntBoxLeftOnStartSlots()
        {
            int numAntBoxLeftOnStartSlots = washingMachineStartSlots.Count(item => item.washingMachine != null);

            if (numAntBoxLeftOnStartSlots <= 0)
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
                GamePersistentVariable.eatingChickens.Count <= 0 &&
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

            yield return new WaitForSeconds(GameConstants.delayActualLoseLevel);

            while (GamePersistentVariable.eatingChickens.Count > 0)
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
            GamePersistentVariable.levelReviveTime++;
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

            GamePersistentVariable.isLevelLost = false;

            mainCamera.transform.position = Vector3.zero.ChangeZ(mainCamera.transform.position.z);

            ChickenManager.Ins.StopHighlightChickensSequentially();
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
            //     DebugUtil.Log($"GetNextAntSpawnByDifficulty {item.Value}", item.Key);
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

        public Dictionary<Color, int> GetNumNotBlockedTilePerFaction(bool isIncludeStartSlot = true)
        {
            Dictionary<Color, int> sortedPoolFaction = new Dictionary<Color, int>();

            // float normalChickenDifficultyWeight = 0.01f;
            // float stuckChickenDifficultyWeight = 0.0005f;

            for (int i = 0; i < ChickenManager.Ins.chickens.Count; i++)
            {
                BaseChicken chicken = ChickenManager.Ins.chickens[i];

                if (chicken.isDone)
                {
                    continue;
                }

                int chickenPoint = 0;
                int firstRow = chicken.GetFirstRow();
                firstRow = Mathf.Min(firstRow, rowPowerCache.Length - 1);

                // if (chicken.isStuck)
                // {
                //     chickenPoint = (int)(stuckChickenDifficultyWeight * rowPowerCache[firstRow]);
                // }
                // else
                // {
                //     chickenPoint = (int)(normalChickenDifficultyWeight * rowPowerCache[firstRow]);
                // }

                chickenPoint = chicken.numGrainPileRequiredLeft * rowPowerCache[firstRow];

                CommonUtil.SumValueOfDictionary(sortedPoolFaction, chicken.faction, chickenPoint);
            }

            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot bucketStartSlot = washingMachineStartSlots[i];

                if (bucketStartSlot.washingMachine != null)
                {
                    if (isIncludeStartSlot)
                    {
                        if (bucketStartSlot.washingMachine.number != 0)
                        {
                            CommonUtil.SumValueOfDictionary(
                                sortedPoolFaction, bucketStartSlot.washingMachine.faction, -bucketStartSlot.washingMachine.number * rowPowerCache[bucketStartSlot.row]);
                        }
                    }
                    else
                    {
                        WashingMachineItem antBox = bucketStartSlot.washingMachine;

                        if (antBox.isShuffled || antBox.isDouble || antBox is HiddenAntBox || antBox is FreezedAntBox)
                        {
                            if (bucketStartSlot.washingMachine.number != 0)
                            {
                                CommonUtil.SumValueOfDictionary(
                                    sortedPoolFaction, bucketStartSlot.washingMachine.faction, -bucketStartSlot.washingMachine.number * rowPowerCache[bucketStartSlot.row]);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < GrainPool.Ins.grainPiles.Count; i++)
            {
                GrainPile grainPile = GrainPool.Ins.grainPiles[i];

                CommonUtil.SumValueOfDictionary(sortedPoolFaction, grainPile.faction, -grainPile.remainingNumber * rowPowerCache[0]);
            }

            if (GamePersistentVariable.pouringBucket != null)
            {
                CommonUtil.SumValueOfDictionary(
                    sortedPoolFaction, GamePersistentVariable.pouringBucket.faction, -GamePersistentVariable.pouringBucket.remainingNumber * rowPowerCache[0]);

                GamePersistentVariable.pouringBucket = null;
            }

            sortedPoolFaction = sortedPoolFaction.OrderByDescending(item => item.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Debug.Log($"GetNumAntSpawnPerFactionOrderByDifficulty ================");

            // foreach (var item in sortedPoolFaction)
            // {
            //     DebugUtil.Log($"GetNumAntSpawnPerFactionOrderByDifficulty {item.Value}", item.Key);
            // }

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
            return ChickenManager.Ins.currentProgress;

            int numDoneTile = TileManager.Ins.tiles.Count(tile => tile.isDone);

            return numDoneTile / (float)totalTile;
        }

        private int GetCurrentLevelDifficulty()
        {
            float progress = GetLevelProgress();

            int factionDifficultyLevel = (int)((difficultyLevel / 10f) * totalFaction);

            int difficultyAtCurrentProgress = (int)(factionDifficultyLevel * difficultyCurve.Evaluate(progress));

            for (int i = levelDifficultyReduceOnRevive.items.Count - 1; i >= 0; i--)
            {
                LevelDifficultyReduceOnReviveItem item = levelDifficultyReduceOnRevive.items[i];

                if (difficultyLevel >= item.minDifficulty)
                {
                    if (GamePersistentVariable.levelReviveTime >= item.minReviveTime)
                    {
                        difficultyAtCurrentProgress = 0;
                    }
                    else if (GamePersistentVariable.levelBoosterUseTime >= item.minReviveTime)
                    {
                        difficultyAtCurrentProgress = 0;
                    }

                    break;
                }
            }

            for (int i = levelDifficultyReduceOnConsecutiveStuck.items.Count - 1; i >= 0; i--)
            {
                LevelDifficultyReduceOnReviveItem item = levelDifficultyReduceOnConsecutiveStuck.items[i];

                if (difficultyLevel >= item.minDifficulty)
                {
                    if (GamePersistentVariable.numConsecutiveStuckBucket >= item.minReviveTime)
                    {
                        difficultyAtCurrentProgress = 0;

                        GamePersistentVariable.numConsecutiveStuckBucket = 0;
                    }

                    break;
                }
            }

            // Debug.Log($"GetCurrentLevelDifficulty {difficultyAtCurrentProgress} {difficultyLevel} {progress} {difficultyCurve.Evaluate(progress)}");

            return difficultyAtCurrentProgress;
        }
        #endregion

        #region Util
        void InitializeRowPowerCache(int maxRow)
        {
            rowPowerCache = new int[maxRow + 1];

            for (int i = 0; i <= maxRow; i++)
            {
                int diff = maxRow - i;
                diff = Mathf.Max(diff, 1);
                rowPowerCache[i] = (int)Mathf.Pow(diff, 3f);
            }
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

[Serializable]
public class LevelDifficultyReduceOnRevive
{
    public List<LevelDifficultyReduceOnReviveItem> items;
}

[Serializable]
public class LevelDifficultyReduceOnConsecutiveStuck
{
    public List<LevelDifficultyReduceOnReviveItem> items;
}

[Serializable]
public class LevelDifficultyReduceOnReviveItem
{
    public int minDifficulty;
    public int minReviveTime;
}
