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
    public Transform bucket;
    public Transform bucketTop;
    public SpriteRenderer grainSprite;
    public Transform[] grainSpriteRanges;
    public BucketGrainFill grainFill;

    [SpineAnimation] public string jumpAnimation;
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

    [Header("Customize")]
    public float pourRange = 6;
    public float pourOffsetY = 7;
    public Vector3 grainsOffsetWhenPouring;
    public float durationUpdateGrainTroughProgress = 0.3f;
    public float settleBufferBeforeFeed = 0.15f;
    public float pourTiltAngle = -140f;
    public float pourTiltDuration = 0.3f;

    public TMP_Text numberText;

    [Header("State")]
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
    public bool isPouring;

    public WashingMachineStartSlot washingMachineStartSlot;
    public WashingMachineSlot washingMachineSlot;

    private Vector3 numberTextInitialPosition;
    protected Coroutine spawningAntCoroutine;

    private Vector3 bucketTopInitialPosition;
    private Vector3 bucketTextInitialPosition;

    public static event Action<WashingMachineItem> selectWashingMachineEvent;
    public static event Action<WashingMachineItem> antBoxDoneEvent;
    public static event Action<WashingMachineItem> moveUpEvent;
    public static event Action spawnAntEvent;
    public static event Action loseLevelEvent;

    private Tween disableBucketTween;


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
        // skeletonAnimation.AnimationState.Event += OnSpineEvent;

        washingMachineStartSlot = transform.parent.GetComponent<WashingMachineStartSlot>();

        ExtendAwake();

        numberTextInitialPosition = numberText.transform.localPosition;

        bucketTopInitialPosition = bucketTop.transform.localPosition;
        bucketTextInitialPosition = numberText.transform.localPosition;
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
        // skeletonAnimation.AnimationState.Event -= OnSpineEvent;

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
        numberText.text = $"{(int)((float)number / GrainPool.Ins.initialMaxCapacity * 100)}%";

        this.washingMachineStartSlot = washingMachineStartSlot;
        washingMachineSlot = null;

        doubleAntBoxHole.gameObject.SetActive(false);
        shadow.color = Color.white;

        transform.SetParent(washingMachineStartSlot.transform);
        transform.localPosition = new Vector3(0, 0, -1);
        transform.localScale = Vector3.zero;
        transform.rotation = Quaternion.identity;
        bucketTop.transform.localPosition = bucketTopInitialPosition;
        numberText.transform.localPosition = bucketTextInitialPosition;

        Tween.Scale(transform, 2.3f, duration: 0.3f)
        .OnComplete(() =>
        {
            isAnimating = false;

            grainFill?.Fill(faction, number * GameConstants.grainMultiplier, transform);

            onCompleteAction?.Invoke();
        });

        isAnimating = true;

        skeletonAnimation.Skeleton.SetSkin(FactionMapper.Ins.GetAntBoxSkin(faction));
        skeletonAnimation.Skeleton.SetSlotsToSetupPose();
        skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
        skeletonAnimation.AnimationState.SetAnimation(0, beginAnimation, false);

        grainSprite.gameObject.SetActive(false);

        Setup(washingMachineStartSlot);

        isMoving = false;
        isInSlot = false;
        isGhostMode = false;
        isDouble = false;
        isStucking = false;
        isPlayingWarningLoseAnimation = false;
        isPouring = false;
        remainingNumber = number;
        spawningAntCoroutine = null;

        numSlotRequired = 1;
    }

    public void Reset()
    {
        doubleAntBoxHole.gameObject.SetActive(false);
        grainFill?.Clear();
        ObjectPoolingEverything.ReturnToPool(GameConstants.washingMachine, gameObject);

        isMoving = false;
        isAnimating = false;
        isInSlot = false;
        isGhostMode = false;
        isDouble = false;
        isStucking = false;
        isPlayingWarningLoseAnimation = false;
        isShuffled = false;
        isPouring = false;

        washingMachineStartSlot = null;
        washingMachineSlot = null;
    }

    public virtual void Setup(WashingMachineStartSlot washingMachineStartSlot)
    {
        if (disableBucketTween.isAlive)
        {
            disableBucketTween.Stop();
        }

        isDisable = !(washingMachineStartSlot.row == 0);

        numberText.alpha = isDisable ? 0.2f : 1;
    }

    #region Chicken Feed
    public void PourGrainOur(out bool isPouringAll, Action onCompleteAction)
    {
        // int maxAddNumber = Mathf.Min(number, WashingMachineManager.Ins.grainPool.AmountLeft);
        int maxAddNumber = number;

        isPouringAll = maxAddNumber == number;

        PouringOut(maxAddNumber, isBucketMoveBack: !isPouringAll, onCompleteAction);

        isPouring = true;
    }

    public float GetEventTime(SkeletonAnimation skeletonAnimation, string animationName, string eventName)
    {
        var animation = skeletonAnimation.Skeleton.Data.FindAnimation(animationName);
        if (animation == null)
            return -1f;

        foreach (var timeline in animation.Timelines)
        {
            if (timeline is EventTimeline eventTimeline)
            {
                for (int i = 0; i < eventTimeline.Events.Length; i++)
                {
                    if (eventTimeline.Events[i].Data.Name == eventName)
                        return eventTimeline.Events[i].Time;
                }
            }
        }

        return -1f;
    }

    private void PouringOut(int grainCount, bool isBucketMoveBack, Action onCompleteAction)
    {
        GrainPile grainPile = ObjectPoolingEverything.GetFromPool<GrainPile>(GameConstants.grainPile);

        Vector3 bucketDestination;

        bucketDestination.x = WashingMachineManager.Ins.GetPourPositionX();
        bucketDestination.y = pourOffsetY;
        bucketDestination.z = LayerManager.GetBucketZ(GamePersistentVariable.bucketOrder);

        bucketTop.SetZ(LayerManager.GetBucketTopZ(GamePersistentVariable.bucketOrder));
        numberText.transform.SetZ(LayerManager.GetBucketTextZ(GamePersistentVariable.bucketOrder));

        GamePersistentVariable.bucketOrder++;

        pileCenter.x = 0;
        pileCenter.y = WashingMachineManager.Ins.grainPool.transform.position.y;
        pileCenter.z = -1;

        Vector3 initialScale = transform.localScale;

        GrainPool.Ins.ChangePendingAmount(grainCount);

        GrainPool.Ins.AddGrain(grainCount, durationUpdateGrainTroughProgress);

        TweenManager.Ins.AddTween(Tween.ScaleY(transform, 1.5f * initialScale.y, duration: 0.2f)
            .OnComplete(() =>
            {
                TweenManager.Ins.AddTween(Tween.Scale(transform, initialScale, duration: 0.2f));
            })
        );

        grainFill.StartPouring(
            onFirstGrainInjected: () =>
            {
                WashingMachineManager.Ins.grainPool.AddGrainPile(grainPile);
            },
            onAllGrainsInjected: () =>
            {
                if (!isBucketMoveBack)
                {
                    float duration = 0.5f;

                    Tween.Rotation(transform, Quaternion.identity, duration: duration, startDelay: 0.1f);
                    DOTweenUtil.MoveParabol(transform, destination: transform.position.ChangeX(-20), duration: duration, delay: 0.1f, onComplete: () =>
                    {
                        Reset();

                        GamePersistentVariable.bucketOrder--;
                    });
                }

                TweenManager.Ins.AddTween(Tween.Delay(settleBufferBeforeFeed).OnComplete(() =>
                {
                    ChickenManager.Ins.FeedTheChicken(grainPile);
                    WashingMachineManager.Ins.CheckLoseLevel();
                }));
            });

        Tween firstHalfTween = Tween.Position(transform, bucketDestination, duration: 0.2f).OnComplete(() =>
        {
            Tween secondHalfTween = Tween.Position(transform, bucketDestination, duration: 0.2f).OnComplete(() =>
            {
                grainPile.gameObject.SetActive(true);

                grainPile.transform.position = bucketDestination + grainsOffsetWhenPouring;

                grainPile.Setup(this, grainCount);

                grainFill.SetPhysicsPaused(false);

                Tween.EulerAngles(transform, transform.eulerAngles, new Vector3(0, 0, pourTiltAngle), duration: pourTiltDuration);
            });

            TweenManager.Ins.AddTween(secondHalfTween);
        });

        TweenManager.Ins.AddTween(firstHalfTween);

        onCompleteAction?.Invoke();
    }
    #endregion

    public void UpdateNumber()
    {
        numberText.text = $"{number}";

        // if (number <= 0)
        // {
        // SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
        // {
        //     antBoxDoneEvent?.Invoke(this);
        // });

        // SoundManager.Instance.PlaySoundSlotDone();
        // }
    }

    public void Done()
    {
        SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
            {
                antBoxDoneEvent?.Invoke(this);
            });

        SoundManager.Instance.PlaySoundSlotDone();
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

            WashingMachineManager.Ins.CheckLoseLevel();

            return;
        }

        Enable();

        // skeletonAnimation.AnimationState.SetAnimation(0, selectTrueAnimation, false);

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

        // if (isDouble)
        // {
        //     Tween.ShakeScale(transform, strength: 0.2f * Vector3.one, duration: 0.3f);
        // }
        // else
        // {
        //     skeletonAnimation.AnimationState.SetAnimation(0, selectFalseAnimation, false);
        // }

        skeletonAnimation.AnimationState.SetAnimation(0, selectFalseAnimation, false);

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
        // shadow.sprite = shadows[1];

        // shadow.gameObject.SetActive(false);

        // Tween.Delay(0.5f).OnComplete(() =>
        // {
        //     SaferioTween.AlphaShow(shadow);
        // });

        SaferioTween.AlphaHide(shadow);
    }

    public virtual void Enable()
    {
        isTemporarilyEnabled = false;
        isDisable = false;

        numberText.alpha = 1;
    }

    public virtual void Disable()
    {
        disableBucketTween = Tween.Alpha(numberText, 0.2f, duration: 0.3f)
        .OnComplete(() =>
        {
            isDisable = true;
        });
    }

    public void TemporarilyEnable()
    {
        GamePersistentVariable.isPreventSelectAntBox = true;

        if (isDisable)
        {
            Tween.Alpha(numberText, 1, duration: 0.3f)
            .OnComplete(() =>
            {
                isTemporarilyEnabled = true;
                isDisable = false;
                GamePersistentVariable.isPreventSelectAntBox = false;
            });
        }
    }

    public virtual bool IsValidToSelect()
    {
        return !GamePersistentVariable.isPreventSelectAntBox && !GrainPool.Ins.IsMaxCapacity() && !isDisable && !isPouring;

        return !isInSlot && !isDisable &&
            WashingMachineManager.Ins.GetAvailableWashingMachineSlots().Count >= numSlotRequired &&
            !WashingMachineManager.Ins.IsAnyGhostAntBox() &&
            !GamePersistentVariable.isPreventSelectAntBox;
    }

    public virtual bool IsValidToSelect(int currentNumSlotRequired)
    {
        return !GamePersistentVariable.isPreventSelectAntBox && !GrainPool.Ins.IsMaxCapacity() && !isDisable && !isPouring;

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

            isTemporarilyEnabled = false;
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

        Transform initialParent = transform.parent;

        TweenManager.Ins.AddTween(Tween.Alpha(shadow, 0, duration: 0.3f));

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

            GamePersistentVariable.pouringBucket = this;

            WashingMachineManager.Ins.MoveAntBoxTop();
        }

        SoundManager.Instance.PlaySoundBoxTap();

        selectWashingMachineEvent?.Invoke(this);

        void MoveBack()
        {
            transform.SetParent(initialParent);

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

    }
}
