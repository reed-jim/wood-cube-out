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
using static GameEnum;

public class WashingMachineItem : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
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

    public WashingMachineStartSlot washingMachineStartSlot;
    public WashingMachineSlot washingMachineSlot;

    private Vector3 numberTextInitialPosition;
    private Coroutine spawningAntCoroutine;

    public GameSetting gameSetting;

    public static event Action<WashingMachineItem> selectWashingMachineEvent;
    public static event Action<WashingMachineItem> antBoxDoneEvent;
    public static event Action<WashingMachineItem> moveUpEvent;
    public static event Action spawnAntEvent;
    public static event Action loseLevelEvent;

    public List<AntsFlow.BaseTile> tilesToMove;

    void Awake()
    {
        selectWashingMachineEvent += OnAntBoxSelected;
        antBoxDoneEvent += OnAntBoxCompleted;
        WashingMachineManager.loseLevelEvent += OnLevelLose;
        AntsFlow.BaseTile.tileCompleteEvent += OnTileCompleted;
        LevelLoader.startLevelEvent += OnLevelStarted;
        SettingPopup.replayLevelEvent += OnLevelReplayed;
        LosePopup.replayLevelEvent += OnLevelReplayed;
        WashingMachineManager.winLevelEvent += OnLevelWon;
        RevivePopup.reviveEvent += OnRevive;
        spawnAntEvent += OnAntSpawned;
        WashingMachineManager.ghostAntBoxesClearedEvent += OnAllGhostAntBoxesCompleted;

        washingMachineStartSlot = transform.parent.GetComponent<WashingMachineStartSlot>();

        ExtendAwake();

        numberTextInitialPosition = numberText.transform.localPosition;
    }

    void OnDestroy()
    {
        selectWashingMachineEvent -= OnAntBoxSelected;
        antBoxDoneEvent -= OnAntBoxCompleted;
        WashingMachineManager.loseLevelEvent -= OnLevelLose;
        AntsFlow.BaseTile.tileCompleteEvent -= OnTileCompleted;
        LevelLoader.startLevelEvent -= OnLevelStarted;
        SettingPopup.replayLevelEvent -= OnLevelReplayed;
        LosePopup.replayLevelEvent -= OnLevelReplayed;
        WashingMachineManager.winLevelEvent -= OnLevelWon;
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

    public virtual void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number)
    {
        this.faction = faction;
        this.number = number;

        spriteRenderer.color = faction;
        lid.color = faction;
        numberText.text = $"{number}";

        this.washingMachineStartSlot = washingMachineStartSlot;

        doubleAntBoxHole.gameObject.SetActive(false);

        transform.SetParent(washingMachineStartSlot.transform);
        transform.localPosition = new Vector3(0, 0, -1);
        transform.localScale = Vector3.zero;
        numberText.transform.localPosition = numberTextInitialPosition;

        Tween.Scale(transform, 2.3f, duration: 0.3f);
        // SaferioTween.AlphaShow(lid);

        // spriteRenderer.sprite = FactionMapper.Ins.GetAntBoxSprite(faction);
        // lid.sprite = FactionMapper.Ins.GetAntBoxLidSprite(faction);
        shadow.sprite = shadows[0];

        skeletonAnimation.Skeleton.SetSkin(FactionMapper.Ins.GetAntBoxSkin(faction));
        skeletonAnimation.Skeleton.SetSlotsToSetupPose();
        skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
        skeletonAnimation.AnimationState.SetAnimation(0, beginAnimation, false);

        tilesToMove = new List<AntsFlow.BaseTile>();

        Setup(washingMachineStartSlot);

        isInSlot = false;
        isGhostMode = false;
        isDouble = false;
        remainingNumber = number;
        spawningAntCoroutine = null;
    }

    private void Reset()
    {
        doubleAntBoxHole.gameObject.SetActive(false);
    }

    public virtual void Setup(WashingMachineStartSlot washingMachineStartSlot)
    {
        isDisable = !(washingMachineStartSlot.row == 0);

        numberText.alpha = isDisable ? 0.2f : 1;
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

    public void ReceiveTile()
    {
        number--;
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
        if (!IsValidToSelect(numSlotRequired))
        {
            PlayInvalidSelectAnimation();

            return;
        }

        Enable();

        skeletonAnimation.AnimationState.SetAnimation(0, selectTrueAnimation, false);

        OnSelectTrueCompleted();

        if (gameSetting.IsVibrate)
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

        skeletonAnimation.AnimationState.SetAnimation(0, selectFalseAnimation, false);
        // SaferioTween.Punch(transform, 1.1f, initScale: transform.localScale.x);

        if (gameSetting.IsVibrate)
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
        skeletonAnimation.AnimationState.SetAnimation(0, idleAnimation, false);

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

    public void CheckAntsPath()
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

        for (int i = 0; i < remainingNumber; i++)
        {
            AntsFlow.BaseTile tile = AntsFlow.TileManager.Ins.GetAvailableTile(faction);

            if (tile == null)
            {
                break;
            }
            else if (!tilesToMove.Contains(tile))
            {
                tilesToMove.Add(tile);

                tile.isAntApproaching = true;
                tile.InvokeRemoveTileAsBlockageEvent();

                addedTile++;
            }
        }

        remainingNumber -= addedTile;

        tilesToMove = tilesToMove.OrderBy(tile => Vector2.Distance(transform.position, (Vector2)tile.transform.position)).ToList();

        if (!GamePersistentVariable.isPendingLoseLevel && isPlayingWarningLoseAnimation)
        {
            TurnOffWarningLoseAnimation();
        }

        if (tilesToMove.Count > 0)
        {
            if (spawningAntCoroutine == null)
            {
                spawningAntCoroutine = StartCoroutine(SpawningAnts());
            }
        }
        else
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

    private IEnumerator SpawningAnts()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.45f);

        int currentAntIndex = 0;

        isStucking = false;

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

            if (IsMovable(tilesToMove[currentAntIndex]))
            {
                BaseAnt ant = ObjectPoolingEverything.GetFromPool<BaseAnt>(GameConstants.ant);

                ant.transform.position = transform.position.AddY(1.2f).ChangeZ(-0.1f);
                ant.transform.eulerAngles = Vector3.zero;

                ant.Init(faction);
                ant.MoveToTile(tilesToMove[currentAntIndex]);

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
            else
            {
                bool isAnyMovableAnt = false;

                for (int i = 1; i < tilesToMove.Count; i++)
                {
                    if (IsMovable(tilesToMove[i]))
                    {
                        currentAntIndex = i;

                        isAnyMovableAnt = true;

                        break;
                    }
                }

                if (!isAnyMovableAnt)
                {
                    isStucking = true;

                    WashingMachineManager.Ins.CheckLoseLevel();
                }

                yield return waitForSeconds;
            }
        }

        if (number == 0)
        {
            SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
            {
                washingMachineSlot.washingMachineItem = null;

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

            yield return waitForSeconds;
        }

        if (number == 0)
        {
            SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
            {
                washingMachineSlot.washingMachineItem = null;

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

    private void OnSelectTrueCompleted()
    {
        SwitchShadow();

        WashingMachineSlot washingMachineSlot = WashingMachineManager.Ins.MoveItemToSlot(this, onCompleteAction: () =>
        {

        });

        SoundManager.Instance.PlaySoundBoxTap();

        this.washingMachineSlot = washingMachineSlot;

        selectWashingMachineEvent?.Invoke(this);
    }

    private void OnTileCompleted(AntsFlow.BaseTile tile)
    {
        if (isInSlot)
        {
            CheckAntsPath();
        }
    }

    private bool IsMovable(AntsFlow.BaseTile tile)
    {
        float tileSize = tile.GetWorldSize();

        Vector3 nearestMovablePoint = GetNearestMovableNavMeshPoint(tile.coordinate.transform.position.ChangeZ(0), 0.6f * tileSize);
        Vector3 furthestPoint = GetFurthestReachablePoint(transform.position.ChangeZ(0), nearestMovablePoint, tile);

        return Vector2.Distance(furthestPoint, nearestMovablePoint) < 0.6f * tileSize;

        // return tile.actualBlockages.Count < 4;
    }

    public static Vector3 GetNearestMovableNavMeshPoint(Vector3 targetPosition, float maxDistance)
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            targetPosition,
            out hit,
            maxDistance,
            NavMesh.AllAreas))
        {
            return hit.position;
        }

        return targetPosition;
    }

    public static Vector3 GetFurthestReachablePoint(Vector3 startPosition, Vector3 targetPosition, AntsFlow.BaseTile tile)
    {
        NavMeshPath path = new NavMeshPath();

        NavMesh.CalculatePath(startPosition, targetPosition, NavMesh.AllAreas, path);

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
