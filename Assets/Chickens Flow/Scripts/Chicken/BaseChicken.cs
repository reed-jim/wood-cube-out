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

    public Color faction;
    public int column;
    public int row;

    [Header("State")]
    public bool isEating;
    public bool isDone;
    public bool isStuck;

    public WaitingChickenSlot waitingChickenSlot;

    public float Size => transform.localScale.x;

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

    public void Feed(GrainPile grainPile, bool isLastChickenInTheWave)
    {
        Rotate(grainPile.pileCenter - transform.position);

        Tween.Position(transform, grainPile.pileCenter.AddZ(-1), duration: 0.6f, ease: Ease.Linear)
        .OnComplete(() =>
        {
            grainPile.BeEaten(this, onCompleteAction: () =>
            {
                Vector3 outPosition = transform.position;

                if (outPosition.x > 0)
                {
                    outPosition.x += 10;
                }
                else
                {
                    outPosition.x -= 10;
                }

                Tween.Position(transform, outPosition, duration: 0.8f, ease: Ease.Linear).OnComplete(() =>
                {
                    gameObject.SetActive(false);

                    isDone = true;
                });
            });

            if (isLastChickenInTheWave)
            {
                ChickenManager.Ins.MoveChickensDown();
                // WashingMachineManager.Ins.grainPool.CheckFeedChickens();
            }
        });

        isEating = true;

        waitingChickenSlot.chicken = null;
    }

    public void PlayIdleAnim()
    {
        PlayForCycles(skeletonAnimation, idleAnimation, 3, null, false);
    }

    public static void PlayForCycles(
        SkeletonAnimation skeleton,
        string animationName,
        int cycles,
        string endAnimation,
        bool endLoop = true)
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

                skeleton.enabled = false;
                // state.SetAnimation(0, endAnimation, endLoop);
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
}
