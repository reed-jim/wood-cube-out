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
    public SkeletonAnimation skeletonAnimation;
    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string moveAnimation;
    [SpineAnimation] public string[] eatAnimations;
    [SpineAnimation] public string[] flyAnimations;

    #region Circle Progress
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
    public float eatingRange;
    public float flyOutDuration;

    [Header("State")]
    public bool isMoving;
    public bool isEating;
    public bool isDone;
    public bool isStuck;
    public bool isForceFlyOut;
    public string currentAnimation;

    public float prevEatingProgress;
    public float currentEatingProgress;

    public List<WaitingChickenSlot> waitingChickenSlots;

    void Awake()
    {
        SettingPopup.replayLevelEvent += Reset;
        LosePopup.replayLevelEvent += Reset;
        DebugPopup.switchLevelEvent += Reset;

        propertyBlock = new MaterialPropertyBlock();

        SetDissolveProgress(0);
    }

    void OnDestroy()
    {
        SettingPopup.replayLevelEvent -= Reset;
        LosePopup.replayLevelEvent -= Reset;
        DebugPopup.switchLevelEvent -= Reset;
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
        // State
        isMoving = false;
        isEating = false;
        isDone = false;
        isStuck = false;

        currentAnimation = idleAnimation;

        prevEatingProgress = 0f;
        currentEatingProgress = 0f;

        // Gameplay
        numGrainPileRequiredLeft = numGrainPileRequired;
        skeletonAnimation.gameObject.SetActive(false);
        eatingProgress.gameObject.SetActive(false);
        eatingProgressText.gameObject.SetActive(false);
        transform.rotation = Quaternion.identity;

        // Waiting slots
        waitingChickenSlots.Clear();

        ObjectPoolingEverything.ReturnToPool(GameConstants.chicken, gameObject);
    }

    public void SetFaction(Color faction)
    {
        try
        {
            string skin = FactionMapper.Ins.GetAntBoxSkin(faction);
            SkeletonDataAsset skeletonDataAsset = FactionMapper.Ins.GetSkeletonDataAsset(skin);

            skeletonAnimation.skeletonDataAsset = skeletonDataAsset;
            skeletonAnimation.Skeleton.SetSkin(skin);
        }
        catch (System.Exception)
        {

        }

        skeletonAnimation.Skeleton.SetSlotsToSetupPose();
        skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);

        Tween.Delay(0.02f).OnComplete(() =>
        {
            skeletonAnimation.enabled = false;
        });

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
        // prevEatingProgress = 1 - (float)numGrainPileRequiredLeft / numGrainPileRequired;

        // numGrainPileRequiredLeft -= numGrain;

        // currentEatingProgress = 1 - (float)numGrainPileRequiredLeft / numGrainPileRequired;

        grainPile.BeEaten(this, numGrain, isLastChickenInTheWave, onCompleteAction: () =>
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

                // SaferioTween.ScaleDownWithPunch(eatingProgressText.transform);
                SaferioTween.ScaleDownWithPunch(circleProgressFill.transform);

                PlayFlyAnim();

                Tween.PositionX(transform, outPosition.x, duration: flyOutDuration, ease: Ease.Linear).OnComplete(() =>
                {
                    gameObject.SetActive(false);

                    ChickenManager.Ins.CheckWinLevel();

                    onChickenFlyOut?.Invoke();
                });

                foreach (var waitingChickenSlot in waitingChickenSlots)
                {
                    waitingChickenSlot.chicken = null;
                }

                isDone = true;

                ChickenManager.Ins.MoveChickensDown();
            }
            else
            {
                PlayIdleAnim();

                isEating = false;
            }

            if (numGrainPileRequired > 1)
            {
                // if (!eatingProgress.gameObject.activeSelf)
                // {
                //     SaferioTween.ShowWithPunch(eatingProgress.transform, 1, onCompleteAction: () =>
                //     {
                //         eatingProgress.SetProgress((float)numGrainPileRequiredLeft / numGrainPileRequired);
                //     });
                // }
                // else
                // {
                //     eatingProgress.SetProgress((float)numGrainPileRequiredLeft / numGrainPileRequired);
                // }
            }
        });

        float prevProgress = 1 - (numGrainPileRequiredLeft) / (float)numGrainPileRequired;
        float progress = 1 - (numGrainPileRequiredLeft - numGrain) / (float)numGrainPileRequired;

        isEating = true;
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
        SaferioTween.ScaleDownWithPunch(circleProgressFill.transform);

        PlayFlyAnim();

        Tween.PositionX(transform, outPosition.x, duration: flyOutDuration, ease: Ease.Linear).OnComplete(() =>
        {
            gameObject.SetActive(false);

            if (isLast)
            {
                ChickenManager.Ins.MoveChickensDown();
            }

            ChickenManager.Ins.CheckWinLevel();
        });

        foreach (var waitingChickenSlot in waitingChickenSlots)
        {
            waitingChickenSlot.chicken = null;
        }

        isForceFlyOut = true;
        isDone = true;
    }

    #region Progress
    public void UpdateEatingProgress(float progress, float prevProgress, float duration = 1f)
    {
        // if (!eatingProgressText.gameObject.activeSelf)
        // {
        //     SaferioTween.ShowWithPunch(eatingProgressText.transform, 1, onCompleteAction: () =>
        //     {

        //     });
        // }

        TweenManager.Ins.AddTween(Tween.Custom(prevProgress, progress, duration, onValueChange: newVal =>
        {
            // eatingProgressText.text = $"{(int)(newVal * 100)}%";
            circleProgressFill.fillAmount = newVal;
        }));
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
        skeletonAnimation.gameObject.SetActive(isActive);
        skeletonAnimation.enabled = true;
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
        PlayAnimWhenFinished(idleAnimation);
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

    public void PlayAnimWhenFinished(string anim)
    {
        var state = skeletonAnimation.AnimationState;
        var current = state.GetCurrent(0);

        if (current == null || current.Loop)
        {
            state.SetAnimation(0, anim, true);
            currentAnimation = anim;
            return;
        }

        void OnComplete(Spine.TrackEntry entry)
        {
            if (entry != current)
                return;

            state.Complete -= OnComplete;

            state.SetAnimation(0, anim, true);
            currentAnimation = anim;
        }

        state.Complete += OnComplete;
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
        PlayAnimWhenFinished(flyAnimations[flyAnimationIndex]);
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

    void Rotate(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);

        transform.rotation = targetRotation;
    }

    public int GetFirstRow()
    {
        return waitingChickenSlots.Min(item => item.row);
    }
}
