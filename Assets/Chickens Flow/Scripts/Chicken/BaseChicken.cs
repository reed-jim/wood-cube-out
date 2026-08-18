using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Spine.AnimationState;

public class BaseChicken : MonoBehaviour
{
    public SkeletonAnimation[] skeletonAnimations;
    public SkeletonAnimation skeletonAnimation;
    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string moveAnimation;
    [SpineAnimation] public string[] eatAnimations;
    [SpineAnimation] public string[] flyAnimations;
    [SpineAnimation] public string[] idleAnimations;

    #region Circle Progress
    public Image circleProgress;
    public Image circleProgressFill;
    #endregion

    #region Fill Progress
    public MeshRenderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;
    // Property ID caches for performance (avoids string hash overhead)
    private static readonly int MinYProperty = Shader.PropertyToID("_MinY");
    private static readonly int MaxYProperty = Shader.PropertyToID("_MaxY");
    private static readonly int DissolveProgressProperty = Shader.PropertyToID("_DissolveProgress");
    #endregion

    public LazyHealthBar eatingProgress;
    public TMP_Text eatingProgressText;

    public Color faction;
    public int column;
    public int row;
    public int numColumnTaken;
    public int numRowTaken;
    public int numGrainPileRequired;
    public int numGrainPileRequiredLeft;

    [Header("Customize")]
    public Vector3 completeFxOffset;

    [Header("State")]
    public bool isMoving;
    public bool isEating;
    public bool isDone;
    public bool isStuck;
    public bool isForceFlyOut;
    public bool isGhost;
    public string currentAnimation;
    public int currentIdleIndex;

    public float prevEatingProgress;
    public float currentEatingProgress;

    public List<WaitingChickenSlot> waitingChickenSlots;

    private Coroutine chickenEatingGrainsCoroutine;
    public int remaingEatingGrainCount;
    public int totalGrainEaten;

    private Spine.TrackEntry _waitingEntry;
    private Spine.TrackEntry smokeAnimEntry;
    private Spine.TrackEntry smokeAnimForceFlyOutEntry;
    private Spine.TrackEntry playIdleSequentiallyTrackEntry;
    private string _pendingAnimation;
    private SkeletonAnimation smokeAnimation;
    private bool isLastChickenForcedFlyOut;
    private float flyAnimDuration;
    private Tween updateProgressTween;

    public bool IsInSide(Vector2 position)
    {
        Vector2 chickenPos = transform.position;

        return position.x > chickenPos.x - 0.8f &&
                position.x < chickenPos.x + 0.8f &&
                position.y > chickenPos.y - 0.2f &&
                position.y < chickenPos.y + 2f;
    }

    void Awake()
    {
        SettingPopup.replayLevelEvent += Reset;
        LosePopup.replayLevelEvent += Reset;
        DebugPopup.switchLevelEvent += Reset;
        WinPopup.resetLevelEvent += Reset;

        propertyBlock = new MaterialPropertyBlock();

        SetDissolveProgress(0);

        flyAnimDuration = skeletonAnimation.Skeleton.Data.FindAnimation(flyAnimations[0]).Duration;

        currentIdleIndex = UnityEngine.Random.Range(0, idleAnimations.Length);
    }

    void OnDestroy()
    {
        SettingPopup.replayLevelEvent -= Reset;
        LosePopup.replayLevelEvent -= Reset;
        DebugPopup.switchLevelEvent -= Reset;
        WinPopup.resetLevelEvent -= Reset;
    }

    // void Awake()
    // {
    //     Debug.Log($"Chicken Bones: {skeletonAnimation.Skeleton.Data.Bones.Count}");
    //     Debug.Log($"Chicken Slots: {skeletonAnimation.Skeleton.Data.Slots.Count}");
    //     Debug.Log($"Chicken Animations: {skeletonAnimation.Skeleton.Data.Animations.Count}");
    //     Debug.Log($"Chicken Skins: {skeletonAnimation.Skeleton.Data.Skins.Count}");

    //     Mesh mesh = skeletonAnimation.GetComponent<MeshFilter>().sharedMesh;

    //     Debug.Log($"Chicken  {mesh.vertexCount}");
    //     Debug.Log($"Chicken  {mesh.triangles.Length / 3}");
    // }

    public void Reset()
    {
        if (chickenEatingGrainsCoroutine != null)
        {
            StopCoroutine(chickenEatingGrainsCoroutine);
        }

        if (smokeAnimEntry != null)
        {
            smokeAnimEntry.Complete -= OnSmokeAnimComplete;
            smokeAnimEntry = null;
        }

        if (smokeAnimForceFlyOutEntry != null)
        {
            smokeAnimForceFlyOutEntry.Complete -= OnSmokeAnimCompleteForceFlyOut;
            smokeAnimForceFlyOutEntry = null;
        }

        // State
        isMoving = false;
        isEating = false;
        isDone = false;
        isStuck = false;
        isForceFlyOut = false;
        isLastChickenForcedFlyOut = false;
        isGhost = false;

        prevEatingProgress = 0f;
        currentEatingProgress = 0f;
        remaingEatingGrainCount = 0;
        totalGrainEaten = 0;

        // Gameplay
        numGrainPileRequiredLeft = numGrainPileRequired;
        skeletonAnimation.gameObject.SetActive(false);
        eatingProgress.gameObject.SetActive(false);
        eatingProgressText.gameObject.SetActive(false);
        circleProgress.gameObject.SetActive(false);
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;

        // Waiting slots
        waitingChickenSlots.Clear();

        ObjectPoolingEverything.ReturnToPool(GameConstants.chicken, gameObject);
    }

    public void SetFaction(Color faction)
    {
        string skin = FactionMapper.Ins.GetAntBoxSkin(faction);

        int skeletonAnimationIndex = FactionMapper.Ins.GetSkeletonAnimationIndex(skin);

        foreach (var item in skeletonAnimations)
        {
            item.gameObject.SetActive(false);
        }

        skeletonAnimation = skeletonAnimations[skeletonAnimationIndex];

        skeletonAnimation.Skeleton.SetSkin(skin);
        skeletonAnimation.Skeleton.SetSlotsToSetupPose();
        skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);

        meshRenderer = skeletonAnimation.GetComponent<MeshRenderer>();

        this.faction = faction;
    }

    public void MinusRequiredGrainNumber(int numGrain)
    {
        prevEatingProgress = 1 - (float)numGrainPileRequiredLeft / numGrainPileRequired;

        numGrainPileRequiredLeft -= numGrain;

        currentEatingProgress = 1 - (float)numGrainPileRequiredLeft / numGrainPileRequired;
    }

    public void Feed(GrainPile grainPile, int numGrain, bool isLastChickenInTheWave, Action onChickenFlyOut = null)
    {
        if (chickenEatingGrainsCoroutine != null)
        {
            StopCoroutine(chickenEatingGrainsCoroutine);

            // GamePersistentVariable.eatingChickens.Remove(this);
        }

        chickenEatingGrainsCoroutine = StartCoroutine(ChickenEatingGrains(grainPile, numGrain, isLastChickenInTheWave, onCompleteAction: () =>
        {
            if (numGrainPileRequiredLeft == 0)
            {
                transform.SetZ(-10f - 0.001f * transform.GetSiblingIndex());

                Vector3 outPosition = transform.position;

                if (outPosition.x > 0)
                {
                    outPosition.x += 15;

                    transform.eulerAngles = new Vector3(0, 180);
                }
                else
                {
                    outPosition.x -= 15;

                    transform.eulerAngles = Vector3.zero;
                }

                SaferioTween.ScaleDownWithPunch(circleProgress.transform);

                PlayFlyAnim(onComplete: () =>
                {
                    PlayCompleteFx();

                    SoundManager.Instance.PlayChickenComplete();

                    gameObject.SetActive(false);

                    onChickenFlyOut?.Invoke();

                    ChickenManager.Ins.MoveChickensDown();
                    ChickenManager.Ins.CheckWinLevel();
                    WashingMachineManager.Ins.CheckLoseLevel();

                    if (GamePersistentVariable.eatingChickens.Count == 0)
                    {
                        GrainPool.Ins.CheckFeedChickens();
                    }
                });

                foreach (var waitingChickenSlot in waitingChickenSlots)
                {
                    waitingChickenSlot.chicken = null;
                }

                isDone = true;
            }
            else
            {
                PlayIdleAnim();

                isEating = false;

                GrainPool.Ins.CheckFeedChickens();
            }
        }));

        isEating = true;
    }

    void OnSmokeAnimComplete(TrackEntry entry)
    {
        ChickenManager.Ins.CheckWinLevel();
        WashingMachineManager.Ins.CheckLoseLevel();

        ObjectPoolingEverything.ReturnToPool(GameConstants.smokeFx, smokeAnimation.gameObject);

        entry.Complete -= OnSmokeAnimComplete;
    }

    void OnSmokeAnimCompleteForceFlyOut(TrackEntry entry)
    {
        ChickenManager.Ins.CheckWinLevel();
        WashingMachineManager.Ins.CheckLoseLevel();

        ObjectPoolingEverything.ReturnToPool(GameConstants.smokeFx, smokeAnimation.gameObject);

        entry.Complete -= OnSmokeAnimCompleteForceFlyOut;

        if (isLastChickenForcedFlyOut)
        {
            ChickenManager.Ins.MoveChickensDown();
        }
    }

    private void PlayCompleteFx()
    {
        BaseFX fx = ObjectPoolingEverything.GetFromPool<BaseFX>(GameConstants.chickenCompleteFx);

        // var main = fx.ripple.main;
        // main.startColor = faction;

        fx.transform.position = transform.position + completeFxOffset;
        fx.containerParticle.Play();
    }

    private IEnumerator ChickenEatingGrains(GrainPile grainPile, int numGrain, bool isLast, Action onCompleteAction)
    {
        WaitForSeconds delayPerGrainWait = new WaitForSeconds(UnityEngine.Random.Range(1, 2f) * grainPile.delayPerGrain);

        float prevChickenEatingProgress = prevEatingProgress;

        if (remaingEatingGrainCount == 0)
        {
            SetActiveSpine(true);
            PlayIdleAnim();

            GamePersistentVariable.numEatingChicken++;
        }

        int numGrainToEat = numGrain * GameConstants.grainMultiplier;

        remaingEatingGrainCount += numGrainToEat;

        ChickenManager.Ins.chickenRenderer.RemoveChickenRendering(this);

        // WashingMachineManager.Ins.grainPool.AddGrain(-numGrain, durationUpdateProgress: GetUpdatingProgressDuration(numGrainToEat));

        int waitingForGrainCountThreshold = 60;
        int waitingForGrainCount = 0;

        int notPlayingEatAnimThreshold = 20;
        int notPlayingEatAnimCount = 0;
        bool isNotPlayingEatAnim = false;

        while (remaingEatingGrainCount > 0 && !isForceFlyOut)
        {
            if (GamePersistentVariable.isNearWinLevel || isGhost || GrainSimulation.Ins.IsAnyGrainNear(transform.position, faction))
            {
                if (!GamePersistentVariable.eatingChickens.Contains(this))
                {
                    GamePersistentVariable.eatingChickens.Add(this);
                }

                if (isNotPlayingEatAnim)
                {
                    PlayEatAnim();

                    isNotPlayingEatAnim = false;
                }
                else
                {
                    if (remaingEatingGrainCount % 10 == 0)
                    {
                        if (remaingEatingGrainCount == 0)
                        {
                            float randomDelay = UnityEngine.Random.Range(0, 0.5f);

                            DG.Tweening.DOVirtual.DelayedCall(randomDelay, () =>
                            {
                                if (isEating)
                                {
                                    PlayEatAnim();
                                }
                            });
                        }
                        else
                        {
                            PlayEatAnim();
                        }

                        PlaySoundAndFx(transform.position);
                    }
                }

                GrainSimulation.Ins.RemoveFirstGrain(faction);

                remaingEatingGrainCount--;
                totalGrainEaten++;

                float currentPercent = (float)totalGrainEaten / (numGrainPileRequired * GameConstants.grainMultiplier);

                UpdateEatingProgress(currentPercent, prevChickenEatingProgress, grainPile.delayPerGrain);

                if (remaingEatingGrainCount % GameConstants.grainMultiplier == 0)
                {
                    WashingMachineManager.Ins.grainPool.AddGrain(-1, durationUpdateProgress: GetUpdatingProgressDuration(numGrainToEat));
                }

                prevChickenEatingProgress = currentPercent;

                waitingForGrainCount = 0;
            }
            else
            {
                PlayIdleAnim();
                // PlayAnimWhenFinished(idleAnimations[UnityEngine.Random.Range(0, idleAnimations.Length)]);

                waitingForGrainCount++;

                if (waitingForGrainCount > waitingForGrainCountThreshold)
                {
                    if (waitingForGrainCount == waitingForGrainCountThreshold + 1)
                    {
                        GamePersistentVariable.numEatingChicken--;

                        GamePersistentVariable.eatingChickens.Remove(this);

                        WashingMachineManager.Ins.CheckLoseLevel();
                    }
                }

                notPlayingEatAnimCount++;

                if (notPlayingEatAnimCount > notPlayingEatAnimThreshold)
                {
                    isNotPlayingEatAnim = true;

                    notPlayingEatAnimCount = 0;
                }
            }

            yield return delayPerGrainWait;
        }

        GamePersistentVariable.numEatingChicken--;

        GamePersistentVariable.eatingChickens.Remove(this);

        WashingMachineManager.Ins.CheckLoseLevel();

        onCompleteAction?.Invoke();

        bool IsGrainNearChicken(Transform grainPileTransform)
        {
            float range = grainPile.grainPileSizeX;

            return transform.position.x > grainPileTransform.position.x - 0.5f * range &&
                transform.position.x < grainPileTransform.position.x + 0.5f * range;
        }

        void PlaySoundAndFx(Vector3 position)
        {
            // ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.LOOSE_SCREW_FX);
            // position.y -= 1;
            // position.z -= 1;
            // fx.transform.position = position;
            // var main = fx.main;
            // main.startColor = faction;
            // fx.Play();

            SoundManager.Instance.PlaySoundChickenPeck();
        }

        float GetUpdatingProgressDuration(int numGrainToEat)
        {
            float baseDuration = 1f;
            float growthRate = 0.5f;

            return baseDuration + growthRate * Mathf.Log(numGrainToEat + 1);
        }
    }

    public void ForceFlyOut(bool isLast)
    {
        transform.SetZ(-10f - 0.001f * transform.GetSiblingIndex());

        Vector3 outPosition = transform.position;

        if (outPosition.x > 0)
        {
            outPosition.x += 15;

            transform.eulerAngles = new Vector3(0, 180);
        }
        else
        {
            outPosition.x -= 15;

            transform.eulerAngles = Vector3.zero;
        }

        // SaferioTween.ScaleDownWithPunch(eatingProgressText.transform);
        SaferioTween.ScaleDownWithPunch(circleProgress.transform);

        PlayFlyAnim(onComplete: () =>
        {
            PlayCompleteFx();

            SoundManager.Instance.PlayChickenComplete();

            gameObject.SetActive(false);

            ChickenManager.Ins.CheckWinLevel();
            WashingMachineManager.Ins.CheckLoseLevel();

            if (isLastChickenForcedFlyOut)
            {
                ChickenManager.Ins.MoveChickensDown();
            }
        });

        foreach (var waitingChickenSlot in waitingChickenSlots)
        {
            waitingChickenSlot.chicken = null;
        }

        isForceFlyOut = true;
        isDone = true;
        isLastChickenForcedFlyOut = isLast;
    }

    public void ForceStopEatingAndFlyOut(bool isLast)
    {
        if (chickenEatingGrainsCoroutine != null)
        {
            StopCoroutine(chickenEatingGrainsCoroutine);

            GamePersistentVariable.eatingChickens.Remove(this);
        }

        ForceFlyOut(isLast);
    }

    #region Progress
    public void UpdateEatingProgress(float progress, float prevProgress, float duration = 1f)
    {
        if (!circleProgress.gameObject.activeSelf)
        {
            float targetLossyScale = Mathf.Min(numColumnTaken, 2);
            float targetLocalScale = targetLossyScale / transform.localScale.x;

            SaferioTween.ShowWithPunch(circleProgress.transform, targetLocalScale, onCompleteAction: () =>
            {

            });
        }

        if (updateProgressTween.isAlive)
        {
            updateProgressTween.Stop();
        }

        updateProgressTween = Tween.Custom(circleProgressFill.fillAmount, progress, duration, onValueChange: newVal =>
        {
            circleProgressFill.fillAmount = newVal;
        });

        TweenManager.Ins.AddTween(updateProgressTween);
    }

    public void UpdateMeshBounds()
    {
        if (meshRenderer == null) return;

        // Get bounds in local space relative to the transform
        Bounds bounds = meshRenderer.bounds;
        Vector3 minLocal = transform.InverseTransformPoint(bounds.min);
        Vector3 maxLocal = transform.InverseTransformPoint(bounds.max);

        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(MinYProperty, minLocal.y);
        propertyBlock.SetFloat(MaxYProperty, maxLocal.y);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    public void SetDissolveProgress(float progress)
    {
        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(DissolveProgressProperty, Mathf.Clamp01(progress));
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
    #endregion

    #region Animation
    public void SetActiveSpine(bool isActive)
    {
        if (skeletonAnimation.gameObject.activeSelf == isActive)
        {
            return;
        }

        skeletonAnimation.gameObject.SetActive(isActive);
        skeletonAnimation.enabled = true;

        if (isActive)
        {
            skeletonAnimation.Skeleton.SetToSetupPose();
            skeletonAnimation.AnimationState.ClearTracks();
        }
    }

    public void SetLayer(int layer)
    {
        return;

        meshRenderer.sortingOrder = layer;
    }

    public void PlayIdleAnim(int loopTime, bool isDisableSpineOnComplete = true)
    {
        if (currentAnimation == idleAnimation)
        {
            return;
        }

        PlayForCycles(skeletonAnimation, idleAnimation, loopTime, null, false, onCompleteAction: () =>
        {
            if (isDisableSpineOnComplete)
            {
                skeletonAnimation.enabled = false;
            }
        });

        currentAnimation = idleAnimation;
    }

    public void PlayIdleAnimNoLoop(bool isDisableSpineOnComplete = true)
    {
        PlayIdleAnim(1, isDisableSpineOnComplete);
    }

    public void PlayIdleAnim()
    {
        PlayIdleSequence();
        // PlayAnim(idleAnimation);
    }

    public void ForcePlayIdleAnim()
    {
        PlayIdleSequence();
        // ForcePlayAnim(idleAnimation);
    }

    public void ForcePlayRandomIdleAnim()
    {
        PlayIdleSequence();
        // ForcePlayAnim(idleAnimations[UnityEngine.Random.Range(0, idleAnimations.Length)]);
    }

    public void PlayIdleSequence()
    {
        Spine.AnimationState state = skeletonAnimation.AnimationState;

        string nextIdleAnim = idleAnimations[currentIdleIndex];

        playIdleSequentiallyTrackEntry = state.SetAnimation(0, nextIdleAnim, false);

        currentIdleIndex++;

        if (currentIdleIndex >= idleAnimations.Length)
        {
            currentIdleIndex = 0;
        }

        if (playIdleSequentiallyTrackEntry != null)
        {
            playIdleSequentiallyTrackEntry.Complete -= OnIdleComplete;
        }

        playIdleSequentiallyTrackEntry.Complete += OnIdleComplete;

        currentAnimation = nextIdleAnim;
    }

    private void OnIdleComplete(TrackEntry entry)
    {
        entry.Complete -= OnIdleComplete;

        playIdleSequentiallyTrackEntry = null;

        PlayIdleSequence();
    }

    public void PlayAnim(string anim)
    {
        if (currentAnimation == anim)
        {
            return;
        }

        if (!skeletonAnimation.enabled)
        {
            skeletonAnimation.enabled = true;
        }

        skeletonAnimation.AnimationState.SetAnimation(0, anim, true);

        currentAnimation = anim;
    }

    public void ForcePlayAnim(string anim)
    {
        if (!skeletonAnimation.enabled)
        {
            skeletonAnimation.enabled = true;
        }

        skeletonAnimation.AnimationState.SetAnimation(0, anim, true);

        currentAnimation = anim;
    }

    public void PlayAnimWhenFinished(string anim)
    {
        var state = skeletonAnimation.AnimationState;
        var current = state.GetCurrent(0);

        // If nothing is playing, or current animation is looping,
        // play immediately.
        if (current == null || current.Loop)
        {
            CancelPendingAnimation();

            state.SetAnimation(0, anim, true);
            currentAnimation = anim;
            return;
        }

        // Replace any previous pending request.
        _pendingAnimation = anim;

        if (_waitingEntry == current)
            return;

        CancelPendingAnimation();

        _waitingEntry = current;
        _waitingEntry.Complete += OnAnimationComplete;
    }

    private void OnAnimationComplete(Spine.TrackEntry entry)
    {
        if (entry != _waitingEntry)
            return;

        var state = skeletonAnimation.AnimationState;

        entry.Complete -= OnAnimationComplete;
        _waitingEntry = null;

        if (string.IsNullOrEmpty(_pendingAnimation))
            return;

        string anim = _pendingAnimation;
        _pendingAnimation = null;

        state.SetAnimation(0, anim, true);
        currentAnimation = anim;
    }

    private void CancelPendingAnimation()
    {
        if (_waitingEntry != null)
        {
            skeletonAnimation.AnimationState.Complete -= OnAnimationComplete;
            _waitingEntry = null;
        }

        _pendingAnimation = null;
    }

    public void PlayMoveAnim()
    {
        PlayAnim(moveAnimation);
    }

    public void PlayEatAnim()
    {
        string eatAnimation = eatAnimations[UnityEngine.Random.Range(0, eatAnimations.Length)];

        PlayAnim(eatAnimation);
    }

    public void PlayEatAnim(int eatAnimIndex)
    {
        eatAnimIndex = Mathf.Min(eatAnimIndex, eatAnimations.Length - 1);

        string eatAnimation = eatAnimations[eatAnimIndex];

        PlayAnim(eatAnimation);
    }

    public void PlayFlyAnim(int flyAnimationIndex = 1)
    {
        PlayAnim(flyAnimations[flyAnimationIndex]);

        SoundManager.Instance.PlayChickenFly();
    }

    public void PlayFlyAnim(Action onComplete)
    {
        string flyAnimation = flyAnimations[0];

        skeletonAnimation.AnimationState.SetAnimation(0, flyAnimation, false);

        TweenManager.Ins.AddTween(Tween.Delay(flyAnimDuration).OnComplete(() =>
        {
            onComplete?.Invoke();
        }));

        // TrackEntry trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, flyAnimation, false);

        // trackEntry.Complete += OnFlyAnimationComplete;

        // void OnFlyAnimationComplete(TrackEntry entry)
        // {
        //     onComplete?.Invoke();

        //     entry.Complete -= OnFlyAnimationComplete;
        // }

        currentAnimation = flyAnimation;

        SoundManager.Instance.PlayChickenFly();
    }

    public static void PlayForCycles
    (
        SkeletonAnimation skeleton,
        string animationName,
        int cycles,
        string endAnimation,
        bool endLoop = true,
        Action onCompleteAction = null
    )
    {
        var state = skeleton.AnimationState;
        int completedLoops = 0;

        skeleton.enabled = true;

        state.SetAnimation(0, animationName, true);

        TrackEntryDelegate handler = null;
        handler = (TrackEntry entry) =>
        {
            if (entry.Animation.Name != animationName)
                return;

            completedLoops++;

            if (completedLoops >= cycles)
            {
                state.Complete -= handler;

                onCompleteAction?.Invoke();
            }
        };

        state.Complete += handler;
    }
    #endregion

    public int GetFirstRow()
    {
        return waitingChickenSlots.Min(item => item.row);
    }

    public bool IsGiantChicken()
    {
        return numColumnTaken > 1 || numRowTaken > 1;
    }
}
