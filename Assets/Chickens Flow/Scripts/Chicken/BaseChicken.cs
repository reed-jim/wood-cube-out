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
    [SpineAnimation] public string eatAnimation;
    public GameObject eyes;

    public Color faction;
    public int column;
    public int row;
    public int numColumnTaken;
    public int numRowTaken;

    [Header("Customize")]
    public float eatingRange;

    [Header("State")]
    public bool isEating;
    public bool isDone;
    public bool isStuck;

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

    // void Start()
    // {
    //     StartCoroutine(PlayEyesAnim());
    // }

    public void SetFaction(Color faction)
    {
        // skeletonAnimation.Skeleton.SetSkin(FactionMapper.Ins.GetAntBoxSkin(faction));
        // skeletonAnimation.Skeleton.SetSlotsToSetupPose();
        // skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);

        Tween.Delay(0.02f).OnComplete(() =>
        {
            skeletonAnimation.enabled = false;
        });

        this.faction = faction;
    }

    public void Feed(GrainPile grainPile, bool isLastChickenInTheWave)
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

                PlayMoveAnim();

                Rotate(outPosition - transform.position);

                Tween.Position(transform, outPosition, duration: 0.8f, ease: Ease.Linear).OnComplete(() =>
                {
                    gameObject.SetActive(false);

                    isDone = true;

                    foreach (var waitingChickenSlot in waitingChickenSlots)
                    {
                        waitingChickenSlot.chicken = null;
                    }

                    ChickenManager.Ins.MoveChickensDown();

                    // if (isLastChickenInTheWave)
                    // {
                    //     ChickenManager.Ins.MoveChickensDown();
                    // }
                });
            });

        isEating = true;
    }

    public void Complete()
    {
        isDone = true;
    }

    public void PlayIdleAnim(int loopTime)
    {
        PlayForCycles(skeletonAnimation, idleAnimation, loopTime, null, false);
    }

    public void PlayAnim(string anim)
    {
        if (!skeletonAnimation.enabled)
        {
            skeletonAnimation.enabled = true;
        }

        skeletonAnimation.AnimationState.SetAnimation(0, anim, true);
    }

    public void PlayIdleAnim()
    {
        PlayIdleAnim(1);
        // PlayAnim(idleAnimation);
    }

    public void PlayMoveAnim()
    {
        PlayAnim(moveAnimation);
    }

    public void PlayEatAnim()
    {
        PlayAnim(eatAnimation);
    }

    public IEnumerator PlayEyesAnim()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Random.Range(0.5f, 1f));

        yield return new WaitForSeconds(Random.Range(0, 1f));

        while (true)
        {
            eyes.gameObject.SetActive(!eyes.gameObject.activeSelf);

            yield return waitForSeconds;
        }
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

    public int GetFirstRow()
    {
        return waitingChickenSlots.Min(item => item.row);
    }
}
