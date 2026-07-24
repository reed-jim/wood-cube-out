using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Utilities;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
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
        public Coroutine pendingLoseLevelCoroutine;

        public IntVariable currentLevel;
        private int lastToggleFrame = -1;

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

            if (Time.frameCount != lastToggleFrame)
            {
                lastToggleFrame = Time.frameCount;

                // === Check trước: tìm hộp nào sẽ lên hàng đầu (row 0) ===
                HashSet<WashingMachineItem> boxesMovingToTop = new HashSet<WashingMachineItem>();
                for (int i = 0; i < washingMachineStartSlots.Count; i++)
                {
                    var slot = washingMachineStartSlots[i];
                    if (slot.row == 0 && slot.washingMachine == null)
                    {
                        int x = (int)slot.transform.position.x;
                        for (int j = i + 1; j < washingMachineStartSlots.Count; j++)
                        {
                            var otherSlot = washingMachineStartSlots[j];
                            if (otherSlot.washingMachine != null
                                && (int)otherSlot.transform.position.x == x
                                && !otherSlot.washingMachine.isMoving)
                            {
                                boxesMovingToTop.Add(otherSlot.washingMachine);
                                break;
                            }
                        }
                    }
                }

                // === Simulate toggle: check nếu toggle tất cả thì hàng đầu sẽ ra sao ===
                int numTopBoxesTotal = 0;
                int numTopOpenCloseBoxes = 0;
                int numTopOpenCloseWillBeClosed = 0;

                foreach (var slot in washingMachineStartSlots)
                {
                    WashingMachineItem box = slot.washingMachine;
                    if (box == null || box.isInSlot) continue;

                    bool willBeAtTopRow = (slot.row == 0) || boxesMovingToTop.Contains(box);
                    if (willBeAtTopRow)
                    {
                        numTopBoxesTotal++;
                        if (box.isOpenCloseType)
                        {
                            numTopOpenCloseBoxes++;
                            bool willBeClosedAfterToggle = box.isOpen; // Nếu đang mở thì toggle sẽ thành đóng
                            if (willBeClosedAfterToggle)
                            {
                                numTopOpenCloseWillBeClosed++;
                            }
                        }
                    }
                }

                // Chỉ áp dụng chống deadlock khi TẤT CẢ hộp ở hàng đầu đều là OpenCloseType và đều sẽ bị đóng
                bool wouldAllTopBeClosed = (numTopBoxesTotal > 0)
                                           && (numTopOpenCloseBoxes == numTopBoxesTotal)
                                           && (numTopOpenCloseWillBeClosed == numTopOpenCloseBoxes);

                // === Toggle, nhưng SKIP hộp sẽ lên hàng đầu nếu toggle sẽ gây TH tất cả đóng ===
                foreach (var startSlot in washingMachineStartSlots)
                {
                    if (startSlot.washingMachine != null && startSlot.washingMachine.isOpenCloseType)
                    {
                        // Nếu toggle sẽ gây TH tất cả hàng đầu đều đóng,
                        // thì ko cho đóng hộp đang ở hàng dưới sắp lên hàng đầu
                        if (wouldAllTopBeClosed && boxesMovingToTop.Contains(startSlot.washingMachine))
                        {
                            continue;
                        }
                        startSlot.washingMachine.isOpen = !startSlot.washingMachine.isOpen;
                    }
                }
            }

            CheckNoAntBoxLeftOnStartSlots();
        }

        public void StartLevel(AntsFlow.LevelItem levelItem)
        {
            this.levelItem = levelItem;

            if (washingMachineSlots != null)
            {
                for (int i = 0; i < washingMachineSlots.Count; i++)
                {
                    if (i < 5)
                    {
                        washingMachineSlots[i].SetLocked(false);
                    }

                    if (levelItem.specialElementConfig != null &&
                        levelItem.specialElementConfig.lockedSlotConfig.lockedSlotIndices != null &&
                        levelItem.specialElementConfig.lockedSlotConfig.lockedSlotIndices.Contains(i))
                    {
                        washingMachineSlots[i].SetLocked(true, showUnlockButton: false);
                    }
                }
            }


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

            // TileManager.Ins.tiles = TransformUtil.GetComponentsFromAllChildren<BaseTile>(levelItem.transform);
            TileManager.Ins.UpdateTileCount();
            TileManager.Ins.BuildMap(TileManager.Ins.tiles);

            SpecialElementManager.Ins.Setup(levelItem);
            if (HardPixelManager.Ins != null)
            {
                HardPixelManager.Ins.Setup(levelItem);

            }

            if (GamePersistentVariable.levelItem.specialElementConfig.isContainsHiddenTiles)
            {
                int manualStart = currentLevel.Value == 40 ? 0 : -1;

                SpecialElementManager.Ins.FillHiddenTiles(TileManager.Ins.tilemap, GamePersistentVariable.numColumn, GamePersistentVariable.numRow, manualStart);
            }

            if (GamePersistentVariable.levelItem.specialElementConfig.isContainsWallTiles)
            {
                SpecialElementManager.Ins.FillWallTiles(TileManager.Ins.tilemap, GamePersistentVariable.numColumn, GamePersistentVariable.numRow);
            }

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

            CheckLoseLevel();
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
            washingMachineItem.washingMachineStartSlot.washingMachine = null;

            if (washingMachineItem.skeletonAnimation != null)
            {
                var localPos = washingMachineItem.skeletonAnimation.transform.localPosition;
                washingMachineItem.skeletonAnimation.transform.localPosition = new Vector3(localPos.x, localPos.y, -1.5f);
            }

            // Tween.Scale(washingMachineItem.transform, 1.3f * washingMachineItem.transform.localScale.x, duration: 0.3f, cycles: 2, cycleMode: CycleMode.Yoyo);
            Vector3 initialScale = washingMachineItem.transform.localScale;
            Tween.Position(washingMachineItem.transform, washingMachineSlot.transform.position.AddY(2.2f).AddZ(-3f), duration: 0.2f, ease: Ease.Linear);
            Tween.ScaleY(washingMachineItem.transform, 1.3f * washingMachineItem.transform.localScale.y, duration: 0.2f)
            .OnComplete(() =>
            {
                Tween.Scale(washingMachineItem.transform, initialScale, duration: 0.2f);

                Tween.Position(washingMachineItem.transform, washingMachineSlot.transform.position.AddZ(-0.1f), duration: 0.2f, ease: Ease.Linear)
                .OnComplete(() =>
                {
                    washingMachineItem.isMoving = false;

                    if (washingMachineItem.skeletonAnimation != null)
                    {
                        var localPos = washingMachineItem.skeletonAnimation.transform.localPosition;
                        washingMachineItem.skeletonAnimation.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
                    }

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
                    SpecialElementManager.Ins.SpawnMagnetAntBoxLinkFirstAntBoxes(index);
                });
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
                            washingMachineItem.isMoving = true;
                            washingMachineItem.Setup(washingMachineStartSlot);
                            bool isTut = (levelItem != null && levelItem.isTutorialLevel);
                            if (!isTut && currentLevel != null && BoosterTutorial.Ins != null && BoosterTutorial.Ins.boosterTutorialLevels != null)
                            {
                                isTut = BoosterTutorial.Ins.boosterTutorialLevels.Contains(currentLevel.Value);
                            }
                            float rowZ = isTut ? 0 : WashingMachineItem.GetRowZOffset(washingMachineStartSlot.row);
                            Tween.LocalPosition(washingMachineItem.transform, Vector3.zero.AddZ(rowZ), duration: 0.3f)
                            .OnComplete(() =>
                            {
                                washingMachineItem.isMoving = false;
                            });

                            washingMachineItem.washingMachineStartSlot = washingMachineStartSlot;
                            washingMachineStartSlot.washingMachine = washingMachineItem;
                            otherWashingMachineStartSlot.washingMachine = null;
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

            CheckLoseLevel();
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

                    SpecialElementManager.Ins.SpawnAntBoxLink();
                    SpecialElementManager.Ins.SpawnMagnetAntBoxLink();

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
            else if (levelElement == LevelElement.OpenCloseAntBox)
            {
                key = GameConstants.openCloseAntBox;
            }
            else if (levelElement == LevelElement.KeyAntBox)
            {
                key = GameConstants.keyAntBox;
            }
            else if (levelElement == LevelElement.SnakeAntBox)
            {
                key = GameConstants.snakeAntBox;
            }

            WashingMachineItem antBox = ObjectPoolingEverything.GetFromPool<WashingMachineItem>(key);

            if (levelElement == LevelElement.OpenCloseAntBox)
            {
                antBox.isOpenCloseType = true;
                antBox.isOpen = SpecialElementManager.Ins.nextOpenCloseAntBoxInitialState;
            }
            else
            {
                antBox.isOpenCloseType = false;
                antBox.isOpen = true;
            }

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

        public void UnlockOneLockedSlot()
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

                    if (washingMachine.isDouble || washingMachine.isMagnet || washingMachine is HiddenAntBox || washingMachine is FreezedAntBox)
                    {
                        continue;
                    }

                    Color nextFaction;
                    int numAnt;

                    washingMachine.isShuffled = false;

                    GetNextAntSpawnByDifficulty(difficultyLevel: 0, out nextFaction, out numAnt, isIncludeStartSlot: false);

                    washingMachine.Init(washingMachineStartSlot, nextFaction, numAnt);

                    washingMachine.isShuffled = true;
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
                .Where(tile => !tile.isDone && tile.Faction == selectedFaction && !tile.isAntApproaching).ToList();

            for (int i = 0; i < tilesToClear.Count; i++)
            {
                bool isLast = i == tilesToClear.Count - 1;

                tilesToClear[i].isDone = true;
                tilesToClear[i].Hide();
                tilesToClear[i].HideObstacle();

                SpawnFakeTile(tilesToClear[i], isLast);
            }

            TileManager.Ins.CheckWinLevel();

            AdjustAntBoxAfterClearingOneColor(tilesToClear);
            HardPixelManager.Ins.OnBoosterClearTiles(tilesToClear.Count);
            HardPixelManager.Ins.CheckBoosterClear();

            try
            {
                SpecialElementManager.Ins.spawningTileBox.ClearSpawningTileBoxesOnUsingClearOneColorBooster(tilesToClear);
            }
            catch (System.Exception)
            {

            }

            void SpawnFakeTile(BaseTile tile, bool isLast)
            {
                FakeTile fakeTile = ObjectPoolingEverything.GetFromPool<FakeTile>(GameConstants.fakeTile);

                fakeTile.Init(tile);

                Vector3 initialPosition = fakeTile.transform.position;
                Vector3 initialScale = fakeTile.transform.localScale;

                Tween.ScaleY(fakeTile.transform, 1.5f * initialScale.y, duration: 0.3f);
                Tween.PositionY(fakeTile.transform, fakeTile.transform.position.y + 1.6f, duration: 0.3f)
                .OnComplete(() =>
                {
                    Tween.ScaleY(fakeTile.transform, initialScale.y, duration: 0.15f);
                    Tween.Position(fakeTile.transform, initialPosition, duration: 0.15f)
                    .OnComplete(() =>
                    {
                        SaferioTween.ScaleDownWithPunch(fakeTile.transform, onCompleteAction: () =>
                        {
                            ObjectPoolingEverything.ReturnToPool(GameConstants.fakeTile, fakeTile.gameObject);
                        });

                        if (isLast)
                        {
                            SoundManager.Instance.PlaySound(GameConstants.tileHitGroundSound);
                        }
                    });
                });
            }
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
                    int numToRemove = Mathf.Min(antBox.number, numToClear) - antBox.tilesToMove.Count;
                    antBox.number -= numToRemove;
                    numToClear -= numToRemove;

                    if (numToRemove >= antBox.number)
                    {
                        if (antBox.isInSlot)
                        {
                            antBox.washingMachineSlot.washingMachineItem = null;
                        }
                        else
                        {
                            antBox.washingMachineStartSlot.washingMachine = null;
                        }

                        antBox.washingMachineSlot = null;
                        antBox.washingMachineStartSlot = null;
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

                    if (antBox != null && antBox.IsActuallyStuck())
                    {
                        numAntBoxLeft--;
                    }
                }
            }

            bool isLose = numAntBoxLeft == 0;
            if (!isLose)
            {
                int numAvailableSlots = GetAvailableWashingMachineSlots().Count;

                bool anyStartBoxCanMove = false;
                for (int i = 0; i < washingMachineStartSlots.Count; i++)
                {
                    WashingMachineStartSlot startSlot = washingMachineStartSlots[i];
                    if (startSlot.row == 0 && startSlot.washingMachine != null)
                    {
                        WashingMachineItem box = startSlot.washingMachine;

                        if (box.isInSlot || box.isDisable) continue;

                        if (box.isOpenCloseType && !box.isOpen) continue;

                        if (box.isMagnet)
                        {
                            // Magnet
                            if (HasAdjacentAvailableSlotPair() &&
                                box.magnetPartner != null && !box.magnetPartner.isDisable)
                            {
                                anyStartBoxCanMove = true;
                                break;
                            }
                        }
                        else
                        {
                            //Normal
                            if (numAvailableSlots >= box.numSlotRequired)
                            {
                                anyStartBoxCanMove = true;
                                break;
                            }
                        }
                    }
                }

                if (!anyStartBoxCanMove)
                {
                    bool allActiveSlotsStuck = true;
                    for (int i = 0; i < washingMachineSlots.Count; i++)
                    {
                        WashingMachineSlot slot = washingMachineSlots[i];
                        if (!slot.isLocked && slot.washingMachineItem != null)
                        {
                            WashingMachineItem item = slot.washingMachineItem;
                            if (!item.IsActuallyStuck())
                            {
                                allActiveSlotsStuck = false;
                                break;
                            }
                        }
                    }

                    if (allActiveSlotsStuck)
                    {
                        isLose = true;
                    }
                }
            }

            return isLose;
        }

        public bool CheckLoseLevel()
        {
            bool isLose = IsLose();

            if (isLose)
            {
                List<WashingMachineItem> stuckAntBoxes = washingMachineSlots.Where(item => item.washingMachineItem != null).Select(item => item.washingMachineItem).ToList();

                // Only turn on warning lose animation if all occupied slots are actually stuck
                bool allActuallyStuck = true;
                for (int i = 0; i < stuckAntBoxes.Count; i++)
                {
                    if (!stuckAntBoxes[i].IsActuallyStuck())
                    {
                        allActuallyStuck = false;
                        break;
                    }
                }

                if (allActuallyStuck)
                {
                    for (int i = 0; i < stuckAntBoxes.Count; i++)
                    {
                        WashingMachineItem antBox = stuckAntBoxes[i];

                        antBox.TurnOnWarningLoseAnimation();
                    }
                }

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

            while (GamePersistentVariable.numActiveAnt > 0)
            {
                yield return waitForSeconds;
            }

            // yield return new WaitForSeconds(2f);

            // List<WashingMachineItem> stuckAntBoxes = washingMachineSlots.Where(item => item.washingMachineItem != null).Select(item => item.washingMachineItem).ToList();

            // for (int i = 0; i < stuckAntBoxes.Count; i++)
            // {
            //     WashingMachineItem antBox = stuckAntBoxes[i];

            //     antBox.CheckAntsPath(isAlsoCheckLose: false);
            // }

            if (IsLose() && !GamePersistentVariable.isLevelLost)
            {
                loseLevelEvent?.Invoke();

                GamePersistentVariable.isLevelLost = true;
            }
            else
            {
                List<WashingMachineItem> antBoxes = washingMachineSlots.Where(item => item.washingMachineItem != null).Select(item => item.washingMachineItem).ToList();

                for (int i = 0; i < antBoxes.Count; i++)
                {
                    WashingMachineItem antBox = antBoxes[i];

                    antBox.TurnOffWarningLoseAnimation();
                }
            }

            GamePersistentVariable.isPendingLoseLevel = false;
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

            // Check if top row had no pickable boxes but there was a closed openclose box
            bool hasClosedOpenCloseBoxAtTop = false;
            bool anyTopBoxCanMove = false;
            int numAvailableSlots = GetAvailableWashingMachineSlots().Count;

            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot startSlot = washingMachineStartSlots[i];
                if (startSlot.row == 0 && startSlot.washingMachine != null)
                {
                    WashingMachineItem box = startSlot.washingMachine;
                    if (box.isInSlot || box.isDisable) continue;

                    if (box.isOpenCloseType && !box.isOpen)
                    {
                        hasClosedOpenCloseBoxAtTop = true;
                        continue;
                    }

                    if (box.isMagnet)
                    {
                        if (HasAdjacentAvailableSlotPair() &&
                            box.magnetPartner != null && !box.magnetPartner.isDisable)
                        {
                            anyTopBoxCanMove = true;
                        }
                    }
                    else
                    {
                        if (numAvailableSlots >= box.numSlotRequired)
                        {
                            anyTopBoxCanMove = true;
                        }
                    }
                }
            }

            if (!anyTopBoxCanMove && hasClosedOpenCloseBoxAtTop)
            {
                for (int i = 0; i < washingMachineStartSlots.Count; i++)
                {
                    WashingMachineStartSlot startSlot = washingMachineStartSlots[i];
                    if (startSlot.row == 0 && startSlot.washingMachine != null)
                    {
                        WashingMachineItem box = startSlot.washingMachine;
                        if (box.isOpenCloseType && !box.isOpen)
                        {
                            box.isOpen = true;
                        }
                    }
                }
            }

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

        public bool HasAdjacentAvailableSlotPair()
        {
            for (int i = 0; i < washingMachineSlots.Count - 1; i++)
            {
                var a = washingMachineSlots[i];
                var b = washingMachineSlots[i + 1];

                if (!a.isLocked && a.washingMachineItem == null &&
                    !b.isLocked && b.washingMachineItem == null)
                {
                    return true;
                }
            }

            return false;
        }

        public (WashingMachineSlot, WashingMachineSlot) GetAdjacentAvailableSlotPair()
        {
            for (int i = 0; i < washingMachineSlots.Count - 1; i++)
            {
                var a = washingMachineSlots[i];
                var b = washingMachineSlots[i + 1];

                if (!a.isLocked && a.washingMachineItem == null &&
                    !b.isLocked && b.washingMachineItem == null)
                {
                    return (a, b);
                }
            }

            return (null, null);
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

            for (int i = 0; i < TileManager.Ins.tiles.Count; i++)
            {
                BaseTile tile = TileManager.Ins.tiles[i];

                if (tile.isDone || tile.isAntApproaching)
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

                        if (antBox.isShuffled || antBox.isDouble || antBox.isMagnet || antBox is HiddenAntBox || antBox is FreezedAntBox)
                        {
                            CommonUtil.SumValueOfDictionary(result, washingMachineStartSlot.washingMachine.faction, -washingMachineStartSlot.washingMachine.number);
                        }
                    }
                }
            }

            for (int i = 0; i < washingMachineSlots.Count; i++)
            {
                WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

                if (washingMachineSlot.washingMachineItem != null)
                {
                    CommonUtil.SumValueOfDictionary(result, washingMachineSlot.washingMachineItem.faction, -washingMachineSlot.washingMachineItem.remainingNumber);
                }
            }

            return result;
        }

        public Dictionary<Color, int> GetNumNotBlockedTilePerFaction(bool isIncludeStartSlot = true)
        {
            Dictionary<Color, int> sortedPoolFaction = new Dictionary<Color, int>();

            int multiplier = currentLevel.Value <= 10 ? 50 : 1;

            for (int i = 0; i < TileManager.Ins.tiles.Count; i++)
            {
                BaseTile tile = TileManager.Ins.tiles[i];

                if (tile.isDone || tile.isAntApproaching)
                {
                    continue;
                }

                CommonUtil.SumValueOfDictionary(sortedPoolFaction, tile.Faction, tile.IsBlocked() ? 0 : multiplier);
            }

            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

                if (washingMachineStartSlot.washingMachine != null)
                {
                    if (isIncludeStartSlot)
                    {
                        CommonUtil.SumValueOfDictionary(sortedPoolFaction, washingMachineStartSlot.washingMachine.faction, -washingMachineStartSlot.washingMachine.number);
                    }
                    else
                    {
                        WashingMachineItem antBox = washingMachineStartSlot.washingMachine;

                        if (antBox.isShuffled || antBox.isDouble || antBox.isMagnet || antBox is HiddenAntBox || antBox is FreezedAntBox)
                        {
                            CommonUtil.SumValueOfDictionary(sortedPoolFaction, washingMachineStartSlot.washingMachine.faction, -washingMachineStartSlot.washingMachine.number);
                        }
                    }
                }
            }

            for (int i = 0; i < washingMachineSlots.Count; i++)
            {
                WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

                if (washingMachineSlot.washingMachineItem != null)
                {
                    CommonUtil.SumValueOfDictionary(sortedPoolFaction, washingMachineSlot.washingMachineItem.faction, -washingMachineSlot.washingMachineItem.remainingNumber);
                }
            }

            sortedPoolFaction = sortedPoolFaction.OrderByDescending(item => item.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // foreach (var item in sortedPoolFaction)
            // {
            //     Debug.Log($"GetNextAntSpawnByDifficulty {ColorUtil.ColorToHex(item.Key)} {item.Value}");
            // }

            return sortedPoolFaction;
        }

        public Dictionary<Color, int> GetNumAntSpawnPerFactionOrderByDifficulty(bool isIncludeStartSlot = true)
        {
            Dictionary<Color, int> result = new Dictionary<Color, int>();
            Dictionary<Color, int> numNotBlockedTilePerFaction = GetNumNotBlockedTilePerFaction(isIncludeStartSlot);
            Dictionary<Color, int> maxAntRequiredPerFaction = GetMaxAntRequiredPerFaction(isIncludeStartSlot);

            foreach (var faction in numNotBlockedTilePerFaction.Keys)
            {
                int maxValue = maxAntRequiredPerFaction[faction];
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
