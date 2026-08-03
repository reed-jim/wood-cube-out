using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using CandyCoded.HapticFeedback;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static AntsFlow.TileManager;
using static GameEnum;

public class WashingMachineItem : MonoBehaviour
{
    public SpriteRenderer lid;
    public SpriteRenderer shadow;
    public SpriteRenderer doubleAntBoxHole;
    public SkeletonAnimation skeletonAnimation;

    [SpineAnimation] public string beginAnimation;
    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string selectTrueAnimation;
    [SpineAnimation] public string selectFalseAnimation;
    [SpineAnimation] public string doubleBoxAnimation;

    public Sprite[] shadows;

    public TMP_Text numberText;

    public Color faction;
    public bool isDisable;
    public bool isTemporarilyEnabled;
    public int numSlotRequired = 1;
    public int number;
    public int remainingNumber;
    public bool isMoving;
    public bool isAnimating;
    public bool isInSlot;
    public bool isDouble;
    public bool isMagnet;
    public WashingMachineItem magnetPartner;
    public bool isGhostMode;
    public bool isStucking;
    public bool isPlayingWarningLoseAnimation;
    public bool isShuffled;
    public bool isOpen = true;
    public bool isOpenCloseType = false;

    public WashingMachineStartSlot washingMachineStartSlot;
    public WashingMachineSlot washingMachineSlot;

    private Vector3 numberTextInitialPosition;
    protected Coroutine spawningAntCoroutine;

    public GameSetting gameSetting;

    public static event Action<WashingMachineItem> selectWashingMachineEvent;
    public static event Action<WashingMachineItem> antBoxDoneEvent;
    public static event Action<WashingMachineItem> magnetBoxCountZeroEvent;
    public static event Action<WashingMachineItem> moveUpEvent;
    public static event Action spawnAntEvent;
    public static event Action loseLevelEvent;

    public List<AntsFlow.BaseTile> tilesToMove;
    private int walkableMask;

    protected virtual void Awake()
    {
        selectWashingMachineEvent += OnAntBoxSelected;
        antBoxDoneEvent += OnAntBoxCompleted;
        WashingMachineManager.loseLevelEvent += OnLevelLose;
        AntsFlow.BaseTile.tileMoveToAntEvent += OnTileMoveToAnt;
        LevelLoader.startLevelEvent += OnLevelStarted;
        SettingPopup.replayLevelEvent += OnLevelReplayed;
        LosePopup.replayLevelEvent += OnLevelReplayed;
        DebugPopup.switchLevelEvent += OnLevelReplayed;
        TileManager.winLevelEvent += OnLevelWon;
        RevivePopup.reviveEvent += OnRevive;

        spawnAntEvent += OnAntSpawned;
        WashingMachineManager.ghostAntBoxesClearedEvent += OnAllGhostAntBoxesCompleted;

        washingMachineStartSlot = transform.parent.GetComponent<WashingMachineStartSlot>();

        ExtendAwake();

        numberTextInitialPosition = numberText.transform.localPosition;

        walkableMask = 1 << NavMesh.GetAreaFromName("Walkable");
    }



    protected virtual void OnDestroy()
    {
        selectWashingMachineEvent -= OnAntBoxSelected;

        antBoxDoneEvent -= OnAntBoxCompleted;
        WashingMachineManager.loseLevelEvent -= OnLevelLose;
        AntsFlow.BaseTile.tileMoveToAntEvent -= OnTileMoveToAnt;
        LevelLoader.startLevelEvent -= OnLevelStarted;
        SettingPopup.replayLevelEvent -= OnLevelReplayed;
        LosePopup.replayLevelEvent -= OnLevelReplayed;
        DebugPopup.switchLevelEvent -= OnLevelReplayed;
        TileManager.winLevelEvent -= OnLevelWon;
        RevivePopup.reviveEvent -= OnRevive;
        spawnAntEvent -= OnAntSpawned;
        WashingMachineManager.ghostAntBoxesClearedEvent -= OnAllGhostAntBoxesCompleted;

        ExtendOnDestroy();
    }

    protected virtual void ExtendAwake()
    {

    }

    protected virtual void ExtendOnDestroy()
    {

    }

    public virtual void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number, Action onCompleteAction = null)
    {
        this.faction = faction;
        this.number = number;

        lid.color = faction;
        numberText.text = $"{number}";

        this.washingMachineStartSlot = washingMachineStartSlot;
        washingMachineSlot = null;

        doubleAntBoxHole.gameObject.SetActive(false);

        transform.SetParent(washingMachineStartSlot.transform);
        transform.localPosition = new Vector3(0, 0, -1);
        transform.localScale = Vector3.zero;
        numberText.transform.localPosition = numberTextInitialPosition;

        Tween.Scale(transform, 2.3f, duration: 0.3f)
        .OnComplete(() =>
        {
            isAnimating = false;

            onCompleteAction?.Invoke();
        });

        isAnimating = true;

        shadow.sprite = shadows[0];

        ApplyShadowDepth();

        skeletonAnimation.Skeleton.SetSkin(FactionMapper.Ins.GetAntBoxSkin(faction));
        skeletonAnimation.Skeleton.SetSlotsToSetupPose();
        skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
        skeletonAnimation.AnimationState.SetAnimation(0, beginAnimation, false);

        tilesToMove = new List<AntsFlow.BaseTile>();

        Setup(washingMachineStartSlot);

        isMoving = false;
        isInSlot = false;
        isGhostMode = false;
        isDouble = false;
        isMagnet = false;
        magnetPartner = null;
        isStucking = false;
        isPlayingWarningLoseAnimation = false;
        remainingNumber = number;
        spawningAntCoroutine = null;

        numSlotRequired = 1;
    }

    private void Reset()
    {
        doubleAntBoxHole.gameObject.SetActive(false);

        isMoving = false;
        isAnimating = false;
        isInSlot = false;
        isGhostMode = false;
        isDouble = false;
        isMagnet = false;
        magnetPartner = null;
        isStucking = false;
        isPlayingWarningLoseAnimation = false;
        isShuffled = false;
        isOpenCloseType = false;
        isOpen = true;

        washingMachineStartSlot = null;
        washingMachineSlot = null;

        tilesToMove = new List<AntsFlow.BaseTile>();
    }

    public virtual void Setup(WashingMachineStartSlot washingMachineStartSlot)
    {
        isDisable = !(washingMachineStartSlot.row == 0);

        numberText.alpha = isDisable ? 0.2f : 1;
        if (WashingMachineManager.Ins.isLevelContainingNewSpecialElements)
        {
            bool isTut = false;
            if (WashingMachineManager.Ins != null)
            {
                if (WashingMachineManager.Ins.levelItem != null && WashingMachineManager.Ins.levelItem.isTutorialLevel)
                {
                    isTut = true;
                }
                else if (WashingMachineManager.Ins.currentLevel != null && BoosterTutorial.Ins != null && BoosterTutorial.Ins.boosterTutorialLevels != null)
                {
                    isTut = BoosterTutorial.Ins.boosterTutorialLevels.Contains(WashingMachineManager.Ins.currentLevel.Value);
                }
            }

            if (!isTut)
            {
                float zOffset = GetRowZOffset(washingMachineStartSlot.row);
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, zOffset);
            }
        }


    }

    public const float rowZSeparation = 1.8f;

    public static float GetRowZOffset(int row)
    {
        return -row * rowZSeparation;
    }

    // z shadow theo hành vi ants-flow của level cũ. Prefab Ant Box đã bị đổi 0.1 -> 4 (dùng chung mọi level),
    // nên phải set tường minh: level mới = 4f, level cũ = 0.1f (giá trị prefab gốc nhánh ants-flow).
    public const float shadowZNewSpecialLevel = 4f;
    public const float shadowZLegacyLevel = 0.1f;

    private void ApplyShadowDepth()
    {
        if (shadow == null)
        {
            return;
        }

        var localPos = shadow.transform.localPosition;
        float z = WashingMachineManager.Ins.isLevelContainingNewSpecialElements ? shadowZNewSpecialLevel : shadowZLegacyLevel;
        shadow.transform.localPosition = new Vector3(localPos.x, localPos.y, z);
    }

    public void MinusClothingPiece()
    {
        number--;
    }

    public void ReceiveClothes(Action onCompleteAction)
    {
        numberText.text = $"{number}";

        if (number <= 0)
        {
            SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
            {
                antBoxDoneEvent?.Invoke(this);
                onCompleteAction?.Invoke();
            });
        }
    }

    public void UpdateNumber()
    {
        numberText.text = $"{number}";

        if (number <= 0)
        {
            SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
            {
                antBoxDoneEvent?.Invoke(this);
            });

            SoundManager.Instance.PlaySoundSlotDone();
        }
    }

    public virtual void Select()
    {
        Select(numSlotRequired);
    }

    public void Select(int numSlotRequired)
    {
        if (isAnimating)
        {
            return;
        }

        if (!IsValidToSelect(numSlotRequired))
        {
            PlayInvalidSelectAnimation();

            return;
        }

        Enable();

        skeletonAnimation.AnimationState.SetAnimation(0, selectTrueAnimation, false);

        OnSelectTrueCompleted();

        if (GamePersistentVariable.IsVibrate)
        {
            HapticFeedback.MediumFeedback();
        }

        // entry = skeletonAnimation.AnimationState.SetAnimation(0, selectTrueAnimation, false);
        // entry.Complete += OnSelectTrueCompleted;
    }

    public void ForceSelect()
    {
        if (isMagnet)
        {
            return;
        }

        Enable();

        SwitchShadow();

        WashingMachineSlot washingMachineSlot = WashingMachineManager.Ins.MoveItemToSlot(this);

        SoundManager.Instance.PlaySoundBoxTap();

        this.washingMachineSlot = washingMachineSlot;

        selectWashingMachineEvent?.Invoke(this);
    }

    public void ForceSelectToSlot(WashingMachineSlot slot)
    {
        Enable();

        SwitchShadow();

        MoveToSlotDirect(slot);

        SoundManager.Instance.PlaySoundBoxTap();

        this.washingMachineSlot = slot;

        selectWashingMachineEvent?.Invoke(this);
    }

    public void MoveToSlotDirect(WashingMachineSlot targetSlot)
    {
        isMoving = true;
        Vector3 initialScale = transform.localScale;
        if (skeletonAnimation != null)
        {
            var localPos = skeletonAnimation.transform.localPosition;
            skeletonAnimation.transform.localPosition = new Vector3(localPos.x, localPos.y, -1.5f);
        }
        Tween.Position(transform, targetSlot.transform.position.AddY(2.2f).AddZ(-0.5f), duration: 0.2f, ease: Ease.Linear);
        Tween.ScaleY(transform, 1.3f * transform.localScale.y, duration: 0.2f)
        .OnComplete(() =>
        {
            Tween.Scale(transform, initialScale, duration: 0.2f);

            Tween.Position(transform, targetSlot.transform.position.AddZ(-0.1f), duration: 0.2f, ease: Ease.Linear)
            .OnComplete(() =>
            {
                isMoving = false;
                washingMachineStartSlot.washingMachine = null;

                if (skeletonAnimation != null)
                {
                    var localPos = skeletonAnimation.transform.localPosition;
                    skeletonAnimation.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
                }

                OpenTheBox();
                CheckAntsPath();

                WashingMachineManager.Ins.MoveAntBoxTop();
            });
        });
    }

    protected virtual void PlayInvalidSelectAnimation()
    {
        if (isInSlot)
        {
            return;
        }

        if (isOpenCloseType && !isOpen)
        {
            Tween.ShakeScale(transform, strength: 0.2f * Vector3.one, duration: 0.3f);
        }
        else if (isDouble)
        {
            Tween.ShakeScale(transform, strength: 0.2f * Vector3.one, duration: 0.3f);
        }
        else if (isMagnet)
        {
            Tween.ShakeScale(transform, strength: 0.2f * Vector3.one, duration: 0.3f);
            if (magnetPartner != null && !magnetPartner.isInSlot)
            {
                Tween.ShakeScale(magnetPartner.transform, strength: 0.2f * Vector3.one, duration: 0.3f);
            }
        }
        else
        {
            skeletonAnimation.AnimationState.SetAnimation(0, selectFalseAnimation, false);
        }

        // SaferioTween.Punch(transform, 1.1f, initScale: transform.localScale.x);

        if (GamePersistentVariable.IsVibrate)
        {
            HapticFeedback.MediumFeedback();
        }
        if (WashingMachineManager.Ins.isLevelContainingNewSpecialElements)
        {
            WashingMachineManager.Ins.CheckLoseLevel();

        }
    }

    public virtual void TurnOnWarningLoseAnimation()
    {
        skeletonAnimation.AnimationState.SetAnimation(0, selectFalseAnimation, true);

        isPlayingWarningLoseAnimation = true;
    }

    public virtual void TurnOffWarningLoseAnimation()
    {
        if (isInSlot)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, idleAnimation, false);
        }
        else
        {
            skeletonAnimation.AnimationState.SetAnimation(0, beginAnimation, false);
        }

        isPlayingWarningLoseAnimation = false;
    }

    public bool IsActuallyStuck()
    {
        if (isDisable) return false;
        if (isMagnet)
        {
            return isStucking || number <= 0;
        }
        else
        {
            return remainingNumber > 0 && isStucking;
        }
    }

    private void SwitchShadow()
    {
        shadow.sprite = shadows[1];

        shadow.gameObject.SetActive(false);

        Tween.Delay(0.5f).OnComplete(() =>
        {
            SaferioTween.AlphaShow(shadow);
        });
    }

    public virtual void Enable()
    {
        isTemporarilyEnabled = false;
        isDisable = false;

        numberText.alpha = 1;
    }

    public virtual void Disable()
    {
        Tween.Alpha(numberText, 0.2f, duration: 0.3f)
        .OnComplete(() =>
        {
            isTemporarilyEnabled = false;
            isDisable = true;
        });
    }

    public virtual void TemporarilyEnable()
    {
        if (isDisable)
        {
            Tween.Alpha(numberText, 1, duration: 0.3f)
            .OnComplete(() =>
            {
                isTemporarilyEnabled = true;
                isDisable = false;
            });
        }
    }

    public virtual bool IsValidToSelect()
    {
        bool isValid = !isInSlot && !isDisable &&
            WashingMachineManager.Ins.GetAvailableWashingMachineSlots().Count >= numSlotRequired &&
            !WashingMachineManager.Ins.IsAnyGhostAntBox() &&
            !GamePersistentVariable.isPreventSelectAntBox;
        if (isOpenCloseType)
        {
            isValid = isValid && isOpen;
        }
        if (isMagnet)
        {
            isValid = isValid &&
                magnetPartner != null &&
                !magnetPartner.isDisable &&
                WashingMachineManager.Ins.HasAdjacentAvailableSlotPair();
        }
        return isValid;
    }

    public virtual bool IsValidToSelect(int currentNumSlotRequired)
    {
        bool isValid = !isInSlot && !isDisable &&
            WashingMachineManager.Ins.GetAvailableWashingMachineSlots().Count >= currentNumSlotRequired &&
            !WashingMachineManager.Ins.IsAnyGhostAntBox() &&
            !GamePersistentVariable.isPreventSelectAntBox;
        if (isOpenCloseType)
        {
            isValid = isValid && isOpen;
        }
        if (isMagnet)
        {
            isValid = isValid &&
                magnetPartner != null &&
                !magnetPartner.isDisable &&
                WashingMachineManager.Ins.HasAdjacentAvailableSlotPair();
        }
        return isValid;
    }

    public virtual void OpenTheBox()
    {
        Tween.LocalPositionY(numberText.transform, numberText.transform.localPosition.y - 1f, duration: 0.3f);

        doubleAntBoxHole.gameObject.SetActive(true);
        doubleAntBoxHole.sprite = FactionMapper.Ins.GetDoubleAntHoleSprite(faction);

        if (isDouble)
        {
            // doubleAntBoxHole.gameObject.SetActive(true);
            // doubleAntBoxHole.sprite = FactionMapper.Ins.GetDoubleAntHoleSprite(faction);

            skeletonAnimation.AnimationState.SetAnimation(0, doubleBoxAnimation, false);
        }
        else
        {
            skeletonAnimation.AnimationState.SetAnimation(0, idleAnimation, false);
        }
    }

    public virtual void CheckAntsPath(bool isAlsoCheckLose = true)
    {
        if (remainingNumber <= 0)
        {
            // Chỉ áp dụng cho level có special element mới; IsActuallyStuck (nhánh mới) đã bỏ qua
            // TH remainingNumber<=0. Level cũ (ants-flow) đọc isStucking thô nên không được set ở đây,
            // tránh bị tính thua nhầm khi hộp đã nhả hết kiến.
            if (isInSlot && WashingMachineManager.Ins.isLevelContainingNewSpecialElements)
            {
                isStucking = true;
            }
            return;
        }

        if (isMoving || GamePersistentVariable.isLevelLost || isGhostMode || WashingMachineManager.Ins.IsAnyGhostAntBox())
        {
            return;
        }

        int addedTile = 0;

        int max = AntsFlow.TileManager.Ins.tiles.Count(tile => !tile.isAntApproaching && !tile.isDone && tile.Faction == faction);

        bool isCenter = Math.Abs(transform.position.x) < 0.1f;
        Vector3 antBoxPosition = transform.position;

        if (isCenter)
        {
            antBoxPosition = antBoxPosition.AddX(3);
        }

        List<AntsFlow.BaseTile> foundTiles = AntsFlow.TileManager.Ins.GetFirstNMovableTiles(faction, max, antBoxPosition);

        foundTiles = foundTiles
            // .Where(item => item.numStepToReach < 5)
            .OrderBy(item => item.position.y)
            .ThenBy(item => item.position.x)
            .Take(remainingNumber)
            .ToList();

        for (int i = 0; i < foundTiles.Count; i++)
        {
            AntsFlow.BaseTile tile = foundTiles[i];

            if (!tilesToMove.Contains(tile))
            {
                tilesToMove.Add(tile);

                tile.isAntApproaching = true;

                addedTile++;
            }
        }

        // Debug.Log($"CheckAntsPath {name} {max} {remainingNumber} {tilesToMove.Count}");

        // for (int i = 0; i < remainingNumber; i++)
        // {
        //     AntsFlow.BaseTile tile;

        //     bool isFound = AntsFlow.TileManager.Ins.GetAvailableTile(faction, out tile);

        //     if (!isFound)
        //     {
        //         break;
        //     }

        //     if (!tilesToMove.Contains(tile))
        //     {
        //         tilesToMove.Add(tile);

        //         tile.isAntApproaching = true;
        //         // tile.InvokeRemoveTileAsBlockageEvent();

        //         addedTile++;
        //     }
        // }

        remainingNumber -= addedTile;

        // var left = tilesToMove
        //     .Where(t => t.position.x < 0)
        //     .OrderBy(t => t.position.y)
        //     .ThenBy(t => t.position.x);

        // var right = tilesToMove
        //     .Where(t => t.position.x >= 0)
        //     .OrderBy(t => t.position.y)
        //     .ThenByDescending(t => t.position.x);

        // tilesToMove = left.Concat(right).ToList();

        // tilesToMove = tilesToMove.OrderBy(t => t.GetTotalPathDistance(transform.position)).ToList();

        if (!GamePersistentVariable.isPendingLoseLevel && isPlayingWarningLoseAnimation)
        {
            TurnOffWarningLoseAnimation();
        }

        if (tilesToMove.Count > 0)
        {
            isStucking = false;

            if (spawningAntCoroutine == null)
            {
                spawningAntCoroutine = StartCoroutine(SpawningAnts(antType: GameConstants.ant));
            }
        }
        else if (isAlsoCheckLose)
        {
            isStucking = true;

            WashingMachineManager.Ins.CheckLoseLevel();
        }
    }

    public virtual void CheckAntsPathGhostMode()
    {
        if (isMoving)
        {
            return;
        }

        List<AntsFlow.BaseTile> tilesForGhostAnt = AntsFlow.TileManager.Ins.GetAvailableTileForGhostModeAnt(faction).Take(number).ToList();

        for (int i = 0; i < tilesForGhostAnt.Count; i++)
        {
            tilesForGhostAnt[i].isAntApproaching = true;
        }

        StartCoroutine(SpawningGhostAnts(tilesForGhostAnt));

        isGhostMode = true;
    }

    protected IEnumerator SpawningAnts(string antType)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.35f);

        int currentAntIndex = 0;

        while (tilesToMove.Count > 0)
        {
            if (GamePersistentVariable.isLevelLost)
            {
                for (int i = 0; i < tilesToMove.Count; i++)
                {
                    tilesToMove[i].isAntApproaching = false;
                }

                remainingNumber += tilesToMove.Count;

                tilesToMove.Clear();

                break;
            }

            if (GamePersistentVariable.isPauseGame)
            {
                yield return waitForSeconds;

                continue;
            }

            BaseAnt ant = ObjectPoolingEverything.GetFromPool<BaseAnt>(antType);

            ant.transform.position = transform.position.AddY(1.2f).ChangeZ(-0.1f);
            ant.transform.eulerAngles = Vector3.zero;

            ant.Init(faction);
            // ant.MoveToTile(tilesToMove[currentAntIndex]);
            // BFS
            // tilesToMove[currentAntIndex].path = AntsFlow.TileManager.Ins.FindPath(tilesToMove[currentAntIndex], transform.position);
            ant.MoveTo(tilesToMove[currentAntIndex], tilesToMove.Count);

            SoundManager.Instance.PlaySoundAntPop();

            number--;
            numberText.text = $"{number}";

            if (!isAnimating)
            {
                SaferioTween.Punch(numberText.transform, 1.1f, onCompleteAction: () =>
                {
                    isAnimating = false;
                });

                isAnimating = true;
            }

            tilesToMove.RemoveAt(currentAntIndex);

            currentAntIndex = 0;

            GamePersistentVariable.numActiveAnt++;

            spawnAntEvent?.Invoke();

            yield return waitForSeconds;
        }

        if (number == 0)
        {
            if (isMagnet)
            {
                magnetBoxCountZeroEvent?.Invoke(this);
            }
            else
            {
                SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
                {
                    if (washingMachineSlot != null)
                    {
                        washingMachineSlot.washingMachineItem = null;
                    }

                    antBoxDoneEvent?.Invoke(this);
                });

                isInSlot = false;
            }
        }

        spawningAntCoroutine = null;

        OnFinishSpawningAntsBatch();
    }


    protected virtual void OnFinishSpawningAntsBatch()
    {

    }

    private IEnumerator SpawningGhostAnts(List<AntsFlow.BaseTile> tilesForGhostAnts)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.25f);

        int currentAntIndex = 0;

        List<AntsFlow.BaseTile> tilesToMove = new List<AntsFlow.BaseTile>();

        tilesToMove.AddRange(tilesForGhostAnts);

        isStucking = false;

        while (tilesToMove.Count > 0)
        {
            BaseAnt ant = ObjectPoolingEverything.GetFromPool<BaseAnt>(GameConstants.ant);

            ant.transform.position = transform.position.AddY(1.2f).ChangeZ(-0.1f);
            ant.transform.eulerAngles = Vector3.zero;

            ant.Init(faction);
            ant.SetGhostMode();
            ant.MoveToTileGhostMode(tilesToMove[0]);

            SoundManager.Instance.PlaySoundAntPop();

            number--;
            numberText.text = $"{number}";

            if (!isAnimating)
            {
                SaferioTween.Punch(numberText.transform, 1.1f, onCompleteAction: () =>
                {
                    isAnimating = false;
                });

                isAnimating = true;
            }

            tilesToMove.RemoveAt(0);

            GamePersistentVariable.numActiveAnt++;

            yield return waitForSeconds;
        }

        if (number == 0)
        {
            SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
            {
                if (washingMachineSlot.washingMachineItem != null)
                {
                    washingMachineSlot.washingMachineItem = null;
                }

                antBoxDoneEvent?.Invoke(this);
            });

            isInSlot = false;

            WashingMachineManager.Ins.CompleteGhostAntBox(this);
        }
    }

    private void LoseLevel()
    {
        loseLevelEvent?.Invoke();

        GamePersistentVariable.isLevelLost = true;
    }

    private void OnLevelStarted()
    {
        Reset();

        string key = isOpenCloseType ? GameConstants.openCloseAntBox : GameConstants.washingMachine;
        ObjectPoolingEverything.ReturnToPool(key, gameObject);
    }

    private void OnLevelReplayed()
    {
        Reset();

        string key = isOpenCloseType ? GameConstants.openCloseAntBox : GameConstants.washingMachine;
        ObjectPoolingEverything.ReturnToPool(key, gameObject);
    }

    private void OnLevelWon()
    {
        Reset();

        string key = isOpenCloseType ? GameConstants.openCloseAntBox : GameConstants.washingMachine;
        ObjectPoolingEverything.ReturnToPool(key, gameObject);
    }

    private void OnAntBoxSelected(WashingMachineItem antBox)
    {
        if (isTemporarilyEnabled)
        {
            Disable();
        }

        if (antBox != this)
        {
            if (isInSlot)
            {
                CheckAntsPath();
            }
        }
        else
        {
            isInSlot = true;
        }
    }

    protected virtual void OnAntBoxCompleted(WashingMachineItem antBox)
    {

    }

    public void InvokeMoveUpEvent()
    {
        moveUpEvent?.Invoke(this);
    }

    public static void InvokeAntBoxDoneEvent(WashingMachineItem antBox)
    {
        antBoxDoneEvent?.Invoke(antBox);
    }

    private void OnSelectTrueCompleted()
    {
        SwitchShadow();

        WashingMachineSlot washingMachineSlot = null;
        if (isMagnet && magnetPartner != null && !magnetPartner.isInSlot)
        {
            var pair = WashingMachineManager.Ins.GetAdjacentAvailableSlotPair();
            if (pair.Item1 != null && pair.Item2 != null)
            {

                pair.Item1.washingMachineItem = this;
                pair.Item2.washingMachineItem = magnetPartner;


                magnetPartner.ForceSelectToSlot(pair.Item2);
                washingMachineSlot = pair.Item1;
                MoveToSlotDirect(washingMachineSlot);
            }
        }

        if (washingMachineSlot == null)
        {
            washingMachineSlot = WashingMachineManager.Ins.MoveItemToSlot(this);
        }

        SoundManager.Instance.PlaySoundBoxTap();

        this.washingMachineSlot = washingMachineSlot;

        selectWashingMachineEvent?.Invoke(this);
    }

    // private void OnTileCompleted(AntsFlow.BaseTile tile)
    // {
    //     if (isInSlot)
    //     {
    //         CheckAntsPath();
    //     }
    // }

    private void OnTileCompleted(AntsFlow.BaseTile tile)
    {
        if (isInSlot)
        {
            CheckAntsPath();
        }
    }

    private void OnTileMoveToAnt(AntsFlow.BaseTile tile)
    {
        if (isInSlot)
        {
            CheckAntsPath();
        }
    }

    private bool IsMovable(AntsFlow.BaseTile tile)
    {
        // // check using navmesh
        // float tileSize = GamePersistentVariable.tileMapScaleFactor;
        // Vector3 tilePosition = AntsFlow.TileManager.Ins.GetTilePosition(tile);
        // NavMeshPath path = new NavMeshPath();
        // Vector3 nearestMovableStartPoint = GetNearestMovableNavMeshPoint(transform.position, 1f * tileSize);
        // Vector3 nearestMovablePoint = GetNearestMovableNavMeshPoint(tilePosition, 2f * tileSize);

        // NavMesh.CalculatePath(nearestMovableStartPoint, nearestMovablePoint, walkableMask, path);

        // return path.status == NavMeshPathStatus.PathComplete;

        // Vector3 nearestMovablePoint = GetNearestMovableNavMeshPoint(tilePosition, 0.6f * tileSize).ChangeZ(0);
        // Vector3 furthestPoint = GetFurthestReachablePoint(transform.position.ChangeZ(0), nearestMovablePoint);

        // return Vector2.Distance(furthestPoint, nearestMovablePoint) < 1f * tileSize;
        // return !AntsFlow.TileManager.Ins.IsBlocked(tile);

        // return tile.IsBlocked();


        // BFS
        return true;
    }

    public Vector3 GetNearestMovableNavMeshPoint(Vector3 targetPosition, float maxDistance)
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            targetPosition,
            out hit,
            maxDistance,
            walkableMask))
        {
            return hit.position;
        }

        return targetPosition;
    }

    public Vector3 GetFurthestReachablePoint(Vector3 startPosition, Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();

        NavMesh.CalculatePath(startPosition, targetPosition, walkableMask, path);

        if (path.corners == null || path.corners.Length == 0)
        {
            return startPosition;
        }

        // Fully reachable
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            return targetPosition;
        }

        // Partial path → last corner is furthest reachable point
        return path.corners[path.corners.Length - 1];
    }

    private void OnLevelLose()
    {
        // if (spawningAntCoroutine != null)
        // {
        //     StopCoroutine(spawningAntCoroutine);
        // }
    }

    protected virtual void OnRevive(BoosterType boosterType)
    {
        if (isInSlot)
        {
            CheckAntsPath();

            skeletonAnimation.AnimationState.SetAnimation(0, idleAnimation, false);
        }
    }

    private void OnAntSpawned()
    {
        if (isPlayingWarningLoseAnimation)
        {
            TurnOffWarningLoseAnimation();
        }
    }

    private void OnAllGhostAntBoxesCompleted()
    {
        if (isInSlot)
        {
            CheckAntsPath();
        }
    }

    // private void OnSelectTrueCompleted(TrackEntry trackEntry)
    // {
    //     entry.Complete -= OnSelectTrueCompleted;

    //     SwitchShadow();

    //     WashingMachineSlot washingMachineSlot = WashingMachineManager.Ins.MoveItemToSlot(this, onCompleteAction: () =>
    //     {

    //     });

    //     SoundManager.Instance.PlaySoundBoxTap();

    //     this.washingMachineSlot = washingMachineSlot;

    //     selectWashingMachineEvent?.Invoke(this);
    // }
}
