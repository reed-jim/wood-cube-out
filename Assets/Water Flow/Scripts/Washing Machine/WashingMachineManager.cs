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
        public bool isLevelContainingNewSpecialElements;

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
            if (isLevelContainingNewSpecialElements)
            {
                if (Time.frameCount != lastToggleFrame)
                {
                    lastToggleFrame = Time.frameCount;


                    List<WashingMachineItem> predictedRow0Boxes = new List<WashingMachineItem>();
                    foreach (var slot in washingMachineStartSlots)
                    {
                        if (slot.row == 0)
                        {
                            if (slot.washingMachine != null)
                            {
                                predictedRow0Boxes.Add(slot.washingMachine);
                            }
                            else
                            {
                                int x = (int)slot.transform.position.x;
                                for (int j = 0; j < washingMachineStartSlots.Count; j++)
                                {
                                    var otherSlot = washingMachineStartSlots[j];
                                    if (otherSlot.row > 0 && otherSlot.washingMachine != null && (int)otherSlot.transform.position.x == x)
                                    {
                                        predictedRow0Boxes.Add(otherSlot.washingMachine);
                                        break;
                                    }
                                }
                            }
                        }
                    }


                    bool wouldBeDeadlocked = true;
                    if (predictedRow0Boxes.Count == 0)
                    {
                        wouldBeDeadlocked = false;
                    }
                    else
                    {
                        foreach (var box in predictedRow0Boxes)
                        {
                            if (box.isMagnet)
                            {
                                if (box.IsValidToSelect())
                                {
                                    wouldBeDeadlocked = false;
                                    break;
                                }
                            }
                            else if (box.isOpenCloseType)
                            {
                                bool nextOpenState = !box.isOpen;
                                if (nextOpenState)
                                {
                                    wouldBeDeadlocked = false;
                                    break;
                                }
                            }
                            else
                            {

                                wouldBeDeadlocked = false;
                                break;
                            }
                        }
                    }


                    foreach (var startSlot in washingMachineStartSlots)
                    {
                        var box = startSlot.washingMachine;
                        if (box != null && box.isOpenCloseType)
                        {

                            if (wouldBeDeadlocked && predictedRow0Boxes.Contains(box))
                            {
                                box.isOpen = true;
                            }
                            else
                            {
                                box.isOpen = !box.isOpen;
                            }
                        }
                    }
                }
            }


            CheckNoAntBoxLeftOnStartSlots();
        }

        public void StartLevel(AntsFlow.LevelItem levelItem)
        {
            this.levelItem = levelItem;
            isLevelContainingNewSpecialElements = IsLevelContainingNewSpecialElements();

            if (washingMachineSlots != null &&
                levelItem.specialElementConfig != null &&
                levelItem.specialElementConfig.lockedSlotConfig != null &&
                levelItem.specialElementConfig.lockedSlotConfig.lockedSlotIndices != null)
            {
                List<int> lockedSlotIndices = levelItem.specialElementConfig.lockedSlotConfig.lockedSlotIndices;

                for (int i = 0; i < washingMachineSlots.Count; i++)
                {
                    bool shouldLock = lockedSlotIndices.Contains(i);

                    washingMachineSlots[i].SetLocked(shouldLock, showUnlockButton: false);
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

            // Level mới đẩy shadowTilemap ra sau (z=4f); level cũ giữ z gốc như ants-flow
            TileManager.Ins.ApplyShadowTilemapDepth(isLevelContainingNewSpecialElements);

            SpecialElementManager.Ins.Setup(levelItem);
            if (isLevelContainingNewSpecialElements)
            {
                if (HardPixelManager.Ins != null)
                {
                    HardPixelManager.Ins.Setup(levelItem);
                }
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
            if (isLevelContainingNewSpecialElements)
            {
                CheckLoseLevel();
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
            washingMachineItem.washingMachineStartSlot.washingMachine = null;
            if (isLevelContainingNewSpecialElements)
            {
                if (washingMachineItem.skeletonAnimation != null)
                {
                    var localPos = washingMachineItem.skeletonAnimation.transform.localPosition;
                    washingMachineItem.skeletonAnimation.transform.localPosition = new Vector3(localPos.x, localPos.y, -1.5f);
                }
            }

            // Tween.Scale(washingMachineItem.transform, 1.3f * washingMachineItem.transform.localScale.x, duration: 0.3f, cycles: 2, cycleMode: CycleMode.Yoyo);
            Vector3 initialScale = washingMachineItem.transform.localScale;
            if (isLevelContainingNewSpecialElements)
            {
                Tween.Position(washingMachineItem.transform, washingMachineSlot.transform.position.AddY(2.2f).AddZ(-3f), duration: 0.2f, ease: Ease.Linear);

            }
            else
            {
                Tween.Position(washingMachineItem.transform, washingMachineSlot.transform.position.AddY(2.2f).AddZ(-0.1f), duration: 0.2f, ease: Ease.Linear);

            }
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

                // GD control độ khó: nếu index của hộp này nằm trong easySpawnOrder thì ép hộp
                // sinh ra với faction trùng tile đang ăn được ngay trên map (difficulty 0 = faction
                // có nhiều tile không bị chặn nhất), ghi đè faction/số lượng ở trên.
                if (levelItem.easySpawnOrder != null &&
                    levelItem.easySpawnOrder.Count > 0 &&
                    levelItem.easySpawnOrder.Contains(i))
                {
                    Color easyFaction;
                    int easyNumAnt;

                    if (GetNextAntSpawnByDifficulty(difficultyLevel: 0, out easyFaction, out easyNumAnt))
                    {
                        nextFaction = easyFaction;
                        numAnt = easyNumAnt;
                    }
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

                            if (washingMachineStartSlot.row == 0 && washingMachineItem.isOpenCloseType)
                            {
                                List<WashingMachineItem> unselectableRow0Magnets = new List<WashingMachineItem>();
                                foreach (var startSlot in washingMachineStartSlots)
                                {
                                    if (startSlot.row == 0 && startSlot.washingMachine != null && startSlot.washingMachine.isMagnet)
                                    {
                                        if (!startSlot.washingMachine.IsValidToSelect())
                                        {
                                            unselectableRow0Magnets.Add(startSlot.washingMachine);
                                        }
                                    }
                                }

                                if (unselectableRow0Magnets.Count >= 2)
                                {
                                    washingMachineItem.isOpen = true;
                                }
                            }


                            washingMachineItem.transform.SetParent(washingMachineStartSlot.transform);
                            washingMachineItem.isMoving = true;

                            if (isLevelContainingNewSpecialElements)
                            {
                                // Logic mới: Setup trước tween để set z-offset theo row, tween về rowZ
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
                            }
                            else
                            {
                                // Giữ nguyên hành vi nhánh ants-flow: z = -1, Setup gọi trong OnComplete
                                Tween.LocalPosition(washingMachineItem.transform, Vector3.zero.AddZ(-1), duration: 0.3f)
                                .OnComplete(() =>
                                {
                                    washingMachineItem.Setup(washingMachineStartSlot);

                                    washingMachineItem.isMoving = false;
                                });
                            }

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

            if (isLevelContainingNewSpecialElements)
            {
                List<WashingMachineItem> closedOpenCloseBoxesOnRow0 = new List<WashingMachineItem>();
                List<WashingMachineItem> stuckMagnetsOnRow0 = new List<WashingMachineItem>();
                int totalBoxesOnRow0 = 0;

                foreach (var slot in washingMachineStartSlots)
                {
                    if (slot.row == 0 && slot.washingMachine != null)
                    {
                        totalBoxesOnRow0++;
                        var box = slot.washingMachine;
                        if (box.isOpenCloseType && !box.isOpen)
                        {
                            closedOpenCloseBoxesOnRow0.Add(box);
                        }
                        else if (box.isMagnet && !box.IsValidToSelect())
                        {
                            stuckMagnetsOnRow0.Add(box);
                        }
                    }
                }

                if (closedOpenCloseBoxesOnRow0.Count > 0 && (closedOpenCloseBoxesOnRow0.Count + stuckMagnetsOnRow0.Count == totalBoxesOnRow0))
                {
                    foreach (var box in closedOpenCloseBoxesOnRow0)
                    {
                        box.isOpen = true;
                    }
                }

                CheckLoseLevel();
            }

        }

        public void SpawnNewWashingMachine()
        {
            Color nextFaction;
            int numAnt;

            bool forceSpecialBox = false;

            if (!GetNextAntSpawnByDifficulty(difficultyLevel: GetCurrentLevelDifficulty(), out nextFaction, out numAnt))
            {
                // Pool faction thường đã hết ants để sinh. Bình thường thì dừng ở đây. NHƯNG nếu boss/rắn
                // vẫn còn box chờ theo spawnOrder thì phải ép sinh nốt: khi clear hết tile (vd booster3)
                // numAntBoxSpawned không tăng tới các mốc spawnOrder lớn, khiến warrior/snake box "sinh
                // thiếu" và boss/rắn không bao giờ bị hạ (bug khi spawnOrder lớn hơn số hộp sinh ban đầu).
                if (!SpecialElementManager.Ins.HasPendingSpecialBossBox())
                {
                    return;
                }

                forceSpecialBox = true;

                // Faction chỉ để hiển thị màu (số lượng do boss/rắn quyết định trong Init).
                if (poolFaction != null && poolFaction.Count > 0)
                {
                    nextFaction = poolFaction[0];
                }
            }

            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

                if (washingMachineStartSlot.washingMachine == null)
                {
                    numAntBoxSpawned++;

                    WashingMachineItem washingMachineItem = GetAntBoxFromPool(washingMachineStartSlot.row, washingMachineStartSlot.column, forceSpecialBox);

                    washingMachineItem.gameObject.SetActive(true);
                    washingMachineItem.transform.SetParent(washingMachineStartSlot.transform);

                    washingMachineItem.transform.localPosition = new Vector3(0, -5, 0);
                    Tween.LocalPositionY(washingMachineItem.transform, 0, duration: 0.3f).OnComplete(() =>
                    {
                        SpecialElementManager.Ins.SpawnAntBoxLink();
                        SpecialElementManager.Ins.SpawnMagnetAntBoxLink();
                    });

                    washingMachineStartSlot.washingMachine = washingMachineItem;

                    washingMachineItem.Init(washingMachineStartSlot, nextFaction, numAnt);



                    break;
                }
            }
        }

        private WashingMachineItem GetAntBoxFromPool(int rowIndex, int columnIndex, bool forceSpecialBox = false)
        {
            string key = GameConstants.washingMachine;

            LevelElement levelElement = forceSpecialBox
                ? SpecialElementManager.Ins.GetNextForcedSpecialElement()
                : SpecialElementManager.Ins.GetNextLevelElement(rowIndex, columnIndex);

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
            // 1. Shuffle normal boxes
            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

                if (washingMachineStartSlot.washingMachine != null)
                {
                    WashingMachineItem washingMachine = washingMachineStartSlot.washingMachine;

                    if (washingMachine.isDouble || washingMachine.isMagnet ||
                        washingMachine is HiddenAntBox || washingMachine is FreezedAntBox ||
                        washingMachine is KeyAntBox
                        || washingMachine is WarriorAntBox || washingMachine is SnakeAntBox)
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

            // 2. Shuffle Warrior Ant Boxes separately
            List<WarriorAntBox> warriorBoxes = new List<WarriorAntBox>();
            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                if (washingMachineStartSlots[i].washingMachine is WarriorAntBox warriorBox)
                {
                    warriorBoxes.Add(warriorBox);
                }
            }

            if (warriorBoxes.Count > 0)
            {
                int totalWarriorAnts = warriorBoxes.Sum(b => b.number);
                List<int> newWarriorCounts = DistributeSpecialAntBoxCounts(warriorBoxes.Count, totalWarriorAnts);

                for (int i = 0; i < warriorBoxes.Count; i++)
                {
                    warriorBoxes[i].ShuffleBox(newWarriorCounts[i]);
                }
            }

            // 3. Shuffle Snake Ant Boxes separately
            List<SnakeAntBox> snakeBoxes = new List<SnakeAntBox>();
            for (int i = 0; i < washingMachineStartSlots.Count; i++)
            {
                if (washingMachineStartSlots[i].washingMachine is SnakeAntBox snakeBox)
                {
                    snakeBoxes.Add(snakeBox);
                }
            }

            if (snakeBoxes.Count > 0)
            {
                int totalSnakeAnts = snakeBoxes.Sum(b => b.number);
                List<int> newSnakeCounts = DistributeSpecialAntBoxCounts(snakeBoxes.Count, totalSnakeAnts);

                for (int i = 0; i < snakeBoxes.Count; i++)
                {
                    snakeBoxes[i].ShuffleBox(newSnakeCounts[i]);
                }
            }
        }

        public static List<int> DistributeSpecialAntBoxCounts(int totalBox, int totalSum)
        {
            List<int> result = new List<int>();

            if (totalBox <= 0)
            {
                return result;
            }

            if (totalBox == 1)
            {
                result.Add(totalSum);
                return result;
            }

            const int minAllowed = 5;
            const int maxAllowed = 95;

            int remainingHealth = totalSum;

            for (int i = 0; i < totalBox; i++)
            {
                int remainingBoxes = totalBox - i;

                if (remainingBoxes == 1)
                {
                    result.Add(Mathf.Clamp(remainingHealth, 1, maxAllowed));
                    break;
                }

                if (remainingHealth >= remainingBoxes * minAllowed)
                {
                    int requiredMin = remainingHealth - (remainingBoxes - 1) * maxAllowed;
                    int minUnits = Mathf.Max(minAllowed, requiredMin);
                    minUnits = Mathf.CeilToInt(minUnits / 5f) * 5;

                    int maxUnits = remainingHealth - (remainingBoxes - 1) * minAllowed;
                    maxUnits = Mathf.Min(maxUnits, maxAllowed);
                    int maxUnitsMultipleOf5 = (maxUnits / 5) * 5;

                    if (maxUnitsMultipleOf5 < minUnits)
                    {
                        int units = Mathf.Clamp(minUnits, minAllowed, maxAllowed);
                        result.Add(units);
                        remainingHealth -= units;
                    }
                    else
                    {
                        int stepCount = ((maxUnitsMultipleOf5 - minUnits) / 5) + 1;
                        int units = minUnits + UnityEngine.Random.Range(0, stepCount) * 5;
                        result.Add(units);
                        remainingHealth -= units;
                    }
                }
                else
                {
                    int units = Mathf.Max(1, remainingHealth / remainingBoxes);
                    result.Add(units);
                    remainingHealth -= units;
                }
            }

            for (int i = result.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                int temp = result[i];
                result[i] = result[randomIndex];
                result[randomIndex] = temp;
            }

            return result;
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

            if (SpecialElementManager.Ins != null)
            {
                SpecialElementManager.Ins.OnBoosterClearOneColor(selectedFaction, tilesToClear);
            }

            TileManager.Ins.CheckWinLevel();

            AdjustAntBoxAfterClearingOneColor(tilesToClear);

            if (isLevelContainingNewSpecialElements)
            {
                HardPixelManager.Ins.OnBoosterClearTiles(tilesToClear.Count);
                HardPixelManager.Ins.CheckBoosterClear();
                for (int i = 0; i < washingMachineSlots.Count; i++)
                {
                    if (washingMachineSlots[i].washingMachineItem != null)
                    {
                        washingMachineSlots[i].washingMachineItem.CheckAntsPath();
                    }
                }
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

                // Box warrior/snake có number = phần máu boss/rắn, KHÔNG liên quan tới màu tile.
                // Booster clear-one-color chỉ dọn tile màu thường nên không được trừ count của chúng
                // (trừ nhầm sẽ làm thiếu sát thương -> boss/rắn không hạ hết -> kẹt/lỗi game).
                if (antBox is WarriorAntBox || antBox is SnakeAntBox)
                {
                    continue;
                }

                if (antBox.number > 0 && antBox.faction == tilesToClear[0].Faction)
                {
                    int numToRemove = Mathf.Min(antBox.remainingNumber, numToClear);
                    if (numToRemove <= 0)
                    {
                        continue;
                    }

                    antBox.number -= numToRemove;
                    antBox.remainingNumber -= numToRemove;
                    numToClear -= numToRemove;

                    if (antBox.number <= 0)
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

            // Cập nhật poolFaction TRƯỚC khi MoveAntBoxTop: ở lần clear màu cuối, pool cạn ngay tại đây
            // để SpawnNewWashingMachine (gọi trong MoveAntBoxTop) ép sinh nốt boss/snake box còn lại,
            // thay vì phải chờ người chơi tương tác thêm mới "đẩy" chúng lên.
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

            MoveAntBoxTop();

            for (int i = 0; i < antBoxes.Count; i++)
            {
                WashingMachineItem antBox = antBoxes[i];

                if (antBox.faction == tilesToClear[0].Faction)
                {
                    antBox.UpdateNumber();
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

            // Level có boss/rắn: chưa hạ hết thì chưa win, dù ant box màu thường đã hết.
            if (isWin && SpecialElementManager.Ins != null && !SpecialElementManager.Ins.AreSpecialBossElementsCleared())
            {
                isWin = false;
            }

            if (isWin)
            {
                OnLevelReplayed();

                winLevelEvent?.Invoke();
            }
        }
        #endregion

        #region Check Lose
        public bool IsLevelContainingNewSpecialElements()
        {
            if (levelItem == null || levelItem.specialElementConfig == null)
            {
                return false;
            }

            var spec = levelItem.specialElementConfig;

            if (spec.magnetAntBoxConfig != null &&
                spec.magnetAntBoxConfig.spawnOrder != null &&
                spec.magnetAntBoxConfig.spawnOrder.Count > 0)
            {
                return true;
            }
            if (spec.openCloseAntBoxConfig != null &&
                spec.openCloseAntBoxConfig.spawnOrder != null &&
                spec.openCloseAntBoxConfig.spawnOrder.Count > 0)
            {
                return true;
            }

            if (spec.hardPixelConfigs != null &&
                spec.hardPixelConfigs.Count > 0)
            {
                return true;
            }

            if (spec.spawningTileBoxConfig != null &&
                spec.spawningTileBoxConfig.items != null &&
                spec.spawningTileBoxConfig.items.Count > 0)
            {
                return true;
            }

            if (spec.bossAreaConfig != null &&
                spec.bossAreaConfig.spawnOrder != null &&
                spec.bossAreaConfig.spawnOrder.Count > 0)
            {
                return true;
            }

            if (spec.snakeConfig != null &&
                spec.snakeConfig.spawnOrder != null &&
                spec.snakeConfig.spawnOrder.Count > 0)
            {
                return true;
            }

            if (spec.lockedSlotConfig != null &&
                spec.lockedSlotConfig.lockedSlotIndices != null &&
                spec.lockedSlotConfig.lockedSlotIndices.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool IsLose()
        {
            int numAntBoxLeft = washingMachineSlots.Count;

            bool isNewSpecLevel = isLevelContainingNewSpecialElements;

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

                    bool isStuck = false;
                    if (antBox != null)
                    {
                        isStuck = isNewSpecLevel ? antBox.IsActuallyStuck() : antBox.isStucking;
                    }

                    if (antBox != null && isStuck)
                    {
                        numAntBoxLeft--;
                    }
                }
            }

            bool isLose = numAntBoxLeft == 0;
            if (isNewSpecLevel && !isLose)
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
                            // Hộp băng (FreezedAntBox) chưa được phá vỡ thì không thể select lên slot
                            // (chỉ tan băng khi hộp khác hoàn thành), nên không được tính là còn nước đi.
                            // Nhờ vậy TH hàng đầu gồm 2 magnet đang kẹt + 1 ô băng chưa vỡ sẽ được cho thua.
                            if (box is FreezedAntBox frozenBox && frozenBox.isFreezed)
                            {
                                continue;
                            }

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
                if (isLevelContainingNewSpecialElements)
                {
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
                }
                else
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

            if (mainCamera != null)
            {
                Tween.StopAll(mainCamera.transform);
                var pos = mainCamera.transform.position;
                pos.y = 0;
                mainCamera.transform.position = pos;
            }

            GamePersistentVariable.boosterMode = BoosterMode.None;

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
                    WashingMachineItem antBox = washingMachineStartSlot.washingMachine;

                    if (antBox is WarriorAntBox || antBox is SnakeAntBox || antBox is KeyAntBox)
                    {
                        continue;
                    }

                    if (isIncludeStartSlot)
                    {
                        CommonUtil.SumValueOfDictionary(result, antBox.faction, -antBox.number);
                    }
                    else
                    {
                        if (antBox.isShuffled || antBox.isDouble || antBox.isMagnet || antBox is HiddenAntBox || antBox is FreezedAntBox)
                        {
                            CommonUtil.SumValueOfDictionary(result, antBox.faction, -antBox.number);
                        }
                    }
                }
            }

            for (int i = 0; i < washingMachineSlots.Count; i++)
            {
                WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

                if (washingMachineSlot.washingMachineItem != null)
                {
                    WashingMachineItem antBox = washingMachineSlot.washingMachineItem;

                    if (antBox is WarriorAntBox || antBox is SnakeAntBox || antBox is KeyAntBox)
                    {
                        continue;
                    }

                    CommonUtil.SumValueOfDictionary(result, antBox.faction, -antBox.remainingNumber);
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
                    WashingMachineItem antBox = washingMachineStartSlot.washingMachine;

                    if (antBox is WarriorAntBox || antBox is SnakeAntBox || antBox is KeyAntBox)
                    {
                        continue;
                    }

                    if (isIncludeStartSlot)
                    {
                        CommonUtil.SumValueOfDictionary(sortedPoolFaction, antBox.faction, -antBox.number);
                    }
                    else
                    {
                        if (antBox.isShuffled || antBox.isDouble || antBox.isMagnet || antBox is HiddenAntBox || antBox is FreezedAntBox)
                        {
                            CommonUtil.SumValueOfDictionary(sortedPoolFaction, antBox.faction, -antBox.number);
                        }
                    }
                }
            }

            for (int i = 0; i < washingMachineSlots.Count; i++)
            {
                WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

                if (washingMachineSlot.washingMachineItem != null)
                {
                    WashingMachineItem antBox = washingMachineSlot.washingMachineItem;

                    if (antBox is WarriorAntBox || antBox is SnakeAntBox || antBox is KeyAntBox)
                    {
                        continue;
                    }

                    CommonUtil.SumValueOfDictionary(sortedPoolFaction, antBox.faction, -antBox.remainingNumber);
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
