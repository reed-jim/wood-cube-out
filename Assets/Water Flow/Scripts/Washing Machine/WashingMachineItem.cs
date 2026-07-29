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
    #region  Chicken Feed
    public Vector3 pileCenter;
    public SpriteRenderer colorPreview;
    #endregion

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
    public bool isGhostMode;
    public bool isStucking;
    public bool isPlayingWarningLoseAnimation;
    public bool isShuffled;

    public WashingMachineStartSlot washingMachineStartSlot;
    public WashingMachineSlot washingMachineSlot;

    private Vector3 numberTextInitialPosition;
    protected Coroutine spawningAntCoroutine;

    public GameSetting gameSetting;

    public static event Action<WashingMachineItem> selectWashingMachineEvent;
    public static event Action<WashingMachineItem> antBoxDoneEvent;
    public static event Action<WashingMachineItem> moveUpEvent;
    public static event Action spawnAntEvent;
    public static event Action loseLevelEvent;

    public List<AntsFlow.BaseTile> tilesToMove;
    private int walkableMask;

    void Awake()
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

    void OnDestroy()
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
        transform.rotation = Quaternion.identity;
        numberText.transform.localPosition = numberTextInitialPosition;

        Tween.Scale(transform, 2.3f, duration: 0.3f)
        .OnComplete(() =>
        {
            isAnimating = false;

            onCompleteAction?.Invoke();
        });

        isAnimating = true;

        shadow.sprite = shadows[0];

        skeletonAnimation.Skeleton.SetSkin(FactionMapper.Ins.GetAntBoxSkin(faction));
        skeletonAnimation.Skeleton.SetSlotsToSetupPose();
        skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
        skeletonAnimation.AnimationState.SetAnimation(0, beginAnimation, false);

        colorPreview.color = faction;

        tilesToMove = new List<AntsFlow.BaseTile>();

        Setup(washingMachineStartSlot);

        isMoving = false;
        isInSlot = false;
        isGhostMode = false;
        isDouble = false;
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
        isStucking = false;
        isPlayingWarningLoseAnimation = false;
        isShuffled = false;

        washingMachineStartSlot = null;
        washingMachineSlot = null;

        tilesToMove = new List<AntsFlow.BaseTile>();
    }

    public virtual void Setup(WashingMachineStartSlot washingMachineStartSlot)
    {
        isDisable = !(washingMachineStartSlot.row == 0);

        numberText.alpha = isDisable ? 0.2f : 1;
    }

    #region Chicken Feed
    public void PourGrainOur(out bool isPouringAll, Action onCompleteAction)
    {
        int maxAddNumber = Mathf.Min(number, WashingMachineManager.Ins.grainPool.AmountLeft);

        isPouringAll = maxAddNumber == number;

        List<GrainPoolSlot> grainPoolSlots = WashingMachineManager.Ins.grainPool.GetGrainPoolSlots(maxAddNumber);

        StartCoroutine(PouringOut(grainPoolSlots, isBucketMoveBack: !isPouringAll, onCompleteAction));

        WashingMachineManager.Ins.grainPool.AddGrain(maxAddNumber);
    }

    private IEnumerator PouringOut(List<GrainPoolSlot> grainPoolSlots, bool isBucketMoveBack, Action onCompleteAction)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);

        bool isPouring = false;
        bool isRotated = false;

        while (grainPoolSlots.Count > 0)
        {
            if (!isPouring)
            {
                GrainPoolSlot grainPoolSlot = grainPoolSlots[0];
                grainPoolSlots.RemoveAt(0);

                bool isLast = grainPoolSlots.Count == 0;

                Vector3 bucketDestination;

                bucketDestination.x = 0;
                bucketDestination.y = WashingMachineManager.Ins.grainPool.transform.position.y + 7;
                bucketDestination.z = -9;

                pileCenter.x = grainPoolSlot.transform.position.x;
                pileCenter.y = WashingMachineManager.Ins.grainPool.transform.position.y;
                pileCenter.z = -1;

                transform.SetParent(grainPoolSlot.transform);

                Tween.LocalPosition(transform, bucketDestination, duration: 0.3f).OnComplete(() =>
                {
                    GrainPile grainPile = ObjectPoolingEverything.GetFromPool<GrainPile>(GameConstants.grainPile);

                    grainPile.transform.SetParent(grainPoolSlot.transform);
                    grainPile.transform.localPosition = bucketDestination.ChangeX(0);
                    WashingMachineManager.Ins.grainPool.AddGrainPileToSlot(grainPile, grainPoolSlot);

                    grainPile.Setup(this, 1);

                    if (!isRotated)
                    {
                        Tween.EulerAngles(transform, transform.eulerAngles, new Vector3(0, 0, 180), duration: 0.3f)
                        .OnComplete(() =>
                        {
                            PourGrainPile(grainPile, isLast, () =>
                            {
                                isPouring = false;
                            });

                            isRotated = true;
                        });
                    }
                    else
                    {
                        PourGrainPile(grainPile, isLast, () =>
                        {
                            isPouring = false;
                        });
                    }

                    number--;

                    UpdateNumber();

                    WashingMachineManager.Ins.grainPool.AddGrainPile(grainPile);
                });

                isPouring = true;
            }

            yield return waitForSeconds;

            void PourGrainPile(GrainPile grainPile, bool isLast, Action onComplete)
            {
                grainPile.Pour(pileCenter, onFirstGrainHitGround: () =>
                {
                    if (isLast && !isBucketMoveBack)
                    {
                        SaferioTween.ScaleDownWithPunch(transform);
                    }

                    onComplete?.Invoke();
                });
            }
        }

        onCompleteAction?.Invoke();
    }
    #endregion

    public void UpdateNumber()
    {
        numberText.text = $"{number}";

        // if (number <= 0)
        // {
        //     SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
        //     {
        //         antBoxDoneEvent?.Invoke(this);
        //     });

        //     SoundManager.Instance.PlaySoundSlotDone();
        // }
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
        Enable();

        SwitchShadow();

        WashingMachineSlot washingMachineSlot = WashingMachineManager.Ins.MoveItemToSlot(this);

        SoundManager.Instance.PlaySoundBoxTap();

        this.washingMachineSlot = washingMachineSlot;

        selectWashingMachineEvent?.Invoke(this);
    }

    protected virtual void PlayInvalidSelectAnimation()
    {
        if (isInSlot)
        {
            return;
        }

        if (isDouble)
        {
            Tween.ShakeScale(transform, strength: 0.2f * Vector3.one, duration: 0.3f);
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
    }

    public void TurnOnWarningLoseAnimation()
    {
        skeletonAnimation.AnimationState.SetAnimation(0, selectFalseAnimation, true);

        isPlayingWarningLoseAnimation = true;
    }

    public void TurnOffWarningLoseAnimation()
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

    public void TemporarilyEnable()
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
        return !isInSlot && !isDisable &&
            WashingMachineManager.Ins.GetAvailableWashingMachineSlots().Count >= numSlotRequired &&
            !WashingMachineManager.Ins.IsAnyGhostAntBox() &&
            !GamePersistentVariable.isPreventSelectAntBox;
    }

    public virtual bool IsValidToSelect(int currentNumSlotRequired)
    {
        if (WashingMachineManager.Ins.grainPool.IsMaxCapacity())
        {
            return false;
        }

        return !isInSlot && !isDisable &&
            WashingMachineManager.Ins.GetAvailableWashingMachineSlots().Count >= currentNumSlotRequired &&
            !WashingMachineManager.Ins.IsAnyGhostAntBox() &&
            !GamePersistentVariable.isPreventSelectAntBox;
    }

    public void OpenTheBox()
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

    public void CheckAntsPathGhostMode()
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

        spawningAntCoroutine = null;
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

        ObjectPoolingEverything.ReturnToPool(GameConstants.washingMachine, gameObject);
    }

    private void OnLevelReplayed()
    {
        Reset();

        ObjectPoolingEverything.ReturnToPool(GameConstants.washingMachine, gameObject);
    }

    private void OnLevelWon()
    {
        Reset();

        ObjectPoolingEverything.ReturnToPool(GameConstants.washingMachine, gameObject);
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

    private void OnSelectTrueCompleted()
    {
        Vector3 initialPosition = transform.position;

        bool isPouringAll = true;

        PourGrainOur(out isPouringAll, onCompleteAction: () =>
        {
            if (!isPouringAll)
            {
                MoveBack();
            }
        });

        if (isPouringAll)
        {
            if (washingMachineStartSlot != null)
            {
                washingMachineStartSlot.washingMachine = null;
            }

            WashingMachineManager.Ins.MoveAntBoxTop();
        }

        SoundManager.Instance.PlaySoundBoxTap();

        selectWashingMachineEvent?.Invoke(this);

        void MoveBack()
        {
            Tween.Position(transform, initialPosition, duration: 0.3f)
            .OnComplete(() =>
            {
                isInSlot = false;
            });
            Tween.EulerAngles(transform, transform.eulerAngles, Vector3.zero, duration: 0.3f);
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

    private void OnRevive(BoosterType boosterType)
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
