using System;
using Spine.Unity;
using UnityEngine;

public class OpenCloseAntBox : WashingMachineItem
{
    [SpineAnimation] public string actionAnimation;
    public SkeletonAnimation skeletonOpenClose;

    private bool lastOpenState;

    protected override void ExtendAwake()
    {
        base.ExtendAwake();
        isOpenCloseType = true;
    }

    public override void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number, Action onCompleteAction = null)
    {
        base.Init(washingMachineStartSlot, faction, number, onCompleteAction);
        isOpenCloseType = true;
        isOpen = true;
        lastOpenState = isOpen;

        PlayOpenCloseAnimation(isOpen, instant: true);
    }

    private void Update()
    {
        if (isOpen != lastOpenState)
        {
            lastOpenState = isOpen;
            PlayOpenCloseAnimation(isOpen, instant: false);
        }
    }

    private void PlayOpenCloseAnimation(bool open, bool instant)
    {

        var entry = skeletonAnimation.AnimationState.SetAnimation(0, actionAnimation, false);
        if (entry == null) return;

        if (open)
        {
            if (instant)
            {
                entry.TrackTime = entry.Animation.Duration;
            }
            else
            {
                entry.TimeScale = 1f;
                entry.TrackTime = 0f;
            }
        }
        else
        {
            if (instant)
            {
                entry.TrackTime = 0f;
            }
            else
            {
                entry.TimeScale = -1f;
                entry.TrackTime = entry.Animation.Duration;
            }
        }
    }
}
