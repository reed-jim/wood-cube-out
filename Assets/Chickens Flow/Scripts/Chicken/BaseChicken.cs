using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine;
using Spine.Unity;
using UnityEngine;
using static Spine.AnimationState;

public class BaseChicken : MonoBehaviour
{
    public SkeletonAnimation skeletonAnimation;
    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string moveAnimation;
    [SpineAnimation] public string[] eatAnimations;
    [SpineAnimation] public string flyAnimation;
    public GameObject eyes;

    public LazyHealthBar eatingProgress;

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
    public string currentAnimation;

    public List<WaitingChickenSlot> waitingChickenSlots;

    public float Size => transform.localScale.x;

    void Awake()
    {
        Debug.Log($"Chicken Bones: {skeletonAnimation.Skeleton.Data.Bones.Count}");
        Debug.Log($"Chicken Slots: {skeletonAnimation.Skeleton.Data.Slots.Count}");
        Debug.Log($"Chicken Animations: {skeletonAnimation.Skeleton.Data.Animations.Count}");
        Debug.Log($"Chicken Skins: {skeletonAnimation.Skeleton.Data.Skins.Count}");

        Mesh mesh = skeletonAnimation.GetComponent<MeshFilter>().sharedMesh;

        Debug.Log($"Chicken  {mesh.vertexCount}");
        Debug.Log($"Chicken  {mesh.triangles.Length / 3}");
    }

    public void SetFaction(Color faction)
    {
        skeletonAnimation.Skeleton.SetSkin(FactionMapper.Ins.GetAntBoxSkin(faction));
        skeletonAnimation.Skeleton.SetSlotsToSetupPose();
        skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);

        Tween.Delay(0.02f).OnComplete(() =>
        {
            skeletonAnimation.enabled = false;
        });

        this.faction = faction;
    }

    public void FlyOut()
    {
        transform.SetZ(-9);

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

        numGrainPileRequiredLeft--;

        if (numGrainPileRequiredLeft == 0)
        {
            PlayFlyAnim();

            Tween.Position(transform, outPosition, duration: flyOutDuration, ease: Ease.Linear).OnComplete(() =>
            {
                gameObject.SetActive(false);
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
            isEating = false;
        }

        if (numGrainPileRequired > 1)
        {
            if (!eatingProgress.gameObject.activeSelf)
            {
                SaferioTween.ShowWithPunch(eatingProgress.transform, 1, onCompleteAction: () =>
                {
                    eatingProgress.SetProgress((float)numGrainPileRequiredLeft / numGrainPileRequired);
                });
            }
            else
            {
                eatingProgress.SetProgress((float)numGrainPileRequiredLeft / numGrainPileRequired);
            }
        }
    }

    public void Feed(GrainPile grainPile, bool isLastChickenInTheWave)
    {
        grainPile.BeEaten(this, onCompleteAction: () =>
        {
            transform.SetZ(-9);

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

            numGrainPileRequiredLeft--;

            if (numGrainPileRequiredLeft == 0)
            {
                PlayFlyAnim();

                Tween.Position(transform, outPosition, duration: flyOutDuration, ease: Ease.Linear).OnComplete(() =>
                {
                    gameObject.SetActive(false);
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
                isEating = false;
            }

            if (numGrainPileRequired > 1)
            {
                if (!eatingProgress.gameObject.activeSelf)
                {
                    SaferioTween.ShowWithPunch(eatingProgress.transform, 1, onCompleteAction: () =>
                    {
                        eatingProgress.SetProgress((float)numGrainPileRequiredLeft / numGrainPileRequired);
                    });
                }
                else
                {
                    eatingProgress.SetProgress((float)numGrainPileRequiredLeft / numGrainPileRequired);
                }
            }
        });

        isEating = true;
    }

    public void Complete()
    {
        isDone = true;
    }

    public void PlayIdleAnim(int loopTime, bool isDisableSpineOnComplete = true)
    {
        PlayForCycles(skeletonAnimation, idleAnimation, loopTime, null, false, onCompleteAction: () =>
        {
            if (isDisableSpineOnComplete)
            {
                skeletonAnimation.enabled = false;
            }
        });
    }

    public void PlayIdleAnimNoLoop(bool isDisableSpineOnComplete = true)
    {
        PlayIdleAnim(1, isDisableSpineOnComplete);
    }

    public void PlayIdleAnim()
    {
        PlayAnim(idleAnimation);
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

    public void PlayMoveAnim()
    {
        PlayAnim(moveAnimation);
    }

    public void PlayEatAnim()
    {
        string eatAnimation = eatAnimations[UnityEngine.Random.Range(0, eatAnimations.Length)];

        PlayAnim(eatAnimation);
    }

    public void PlayFlyAnim()
    {
        PlayAnim(flyAnimation);
    }

    public IEnumerator PlayEyesAnim()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1f));

        yield return new WaitForSeconds(UnityEngine.Random.Range(0, 1f));

        while (true)
        {
            eyes.gameObject.SetActive(!eyes.gameObject.activeSelf);

            yield return waitForSeconds;
        }
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
