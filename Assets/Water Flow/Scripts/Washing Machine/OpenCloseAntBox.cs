using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class OpenCloseAntBox : WashingMachineItem
{
    [Header("Box Animations (skeletonAnimation)")]
    [SpineAnimation(dataField = "skeletonAnimation")] public string closedIdleAnimation = "idle2";
    [SpineAnimation(dataField = "skeletonAnimation")] public string appearAnimation = "appear";

    [Header("Door Animations (skeletonOpenClose)")]
    public SkeletonAnimation skeletonOpenClose;
    [SpineAnimation(dataField = "skeletonOpenClose")] public string doorBeginAnimation = "begin";
    [SpineAnimation(dataField = "skeletonOpenClose")] public string doorOpenAnimation = "action_open";
    [SpineAnimation(dataField = "skeletonOpenClose")] public string doorCloseAnimation = "action_close";

    private bool lastOpenState;
    private TrackEntry doorOpenEntry;
    private TrackEntry boxAppearOpenEntry;
    private TrackEntry boxAppearCloseEntry;
    private TrackEntry doorCloseEntry;

    protected override void ExtendAwake()
    {
        base.ExtendAwake();
        isOpenCloseType = true;
    }

    public override void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number, Action onCompleteAction = null)
    {
        bool initialOpenState = isOpen;
        base.Init(washingMachineStartSlot, faction, number, onCompleteAction);
        isOpenCloseType = true;
        isOpen = initialOpenState;
        lastOpenState = isOpen;

        PlayOpenCloseAnimation(isOpen, instant: true);
    }

    private void Update()
    {
        if (isInSlot)
        {
            if (skeletonOpenClose != null && skeletonOpenClose.gameObject.activeSelf)
            {
                skeletonOpenClose.gameObject.SetActive(false);
            }
            ClearAnimationListeners();
            return;
        }

        if (boxAppearCloseEntry != null && boxAppearCloseEntry.TimeScale < 0f)
        {
            if (boxAppearCloseEntry.TrackTime <= 0f)
            {
                var entry = boxAppearCloseEntry;
                boxAppearCloseEntry = null;
                OnBoxAppearCloseComplete(entry);
            }
        }

        if (isOpen != lastOpenState)
        {
            lastOpenState = isOpen;
            PlayOpenCloseAnimation(isOpen, instant: false);
        }
    }

    private void ClearAnimationListeners()
    {
        if (doorOpenEntry != null)
        {
            doorOpenEntry.Complete -= OnDoorOpenComplete;
            doorOpenEntry = null;
        }
        if (boxAppearOpenEntry != null)
        {
            boxAppearOpenEntry.Complete -= OnBoxAppearOpenComplete;
            boxAppearOpenEntry = null;
        }
        if (boxAppearCloseEntry != null)
        {
            boxAppearCloseEntry.Complete -= OnBoxAppearCloseComplete;
            boxAppearCloseEntry = null;
        }
        if (doorCloseEntry != null)
        {
            doorCloseEntry.Complete -= OnDoorCloseComplete;
            doorCloseEntry = null;
        }
    }

    private void PlayOpenCloseAnimation(bool open, bool instant)
    {
        ClearAnimationListeners();

        if (isInSlot)
        {
            if (skeletonOpenClose != null)
            {
                skeletonOpenClose.gameObject.SetActive(false);
            }
            if (skeletonAnimation != null && !string.IsNullOrEmpty(idleAnimation))
            {
                skeletonAnimation.AnimationState.SetAnimation(0, idleAnimation, true);
            }
            return;
        }

        if (instant)
        {
            if (open)
            {
                if (skeletonAnimation != null && !string.IsNullOrEmpty(beginAnimation))
                {
                    skeletonAnimation.AnimationState.SetAnimation(0, beginAnimation, true);
                }
                if (skeletonOpenClose != null)
                {
                    skeletonOpenClose.gameObject.SetActive(false);
                }
            }
            else
            {
                if (skeletonAnimation != null && !string.IsNullOrEmpty(closedIdleAnimation))
                {
                    skeletonAnimation.AnimationState.SetAnimation(0, closedIdleAnimation, true);
                }
                if (skeletonOpenClose != null)
                {
                    skeletonOpenClose.gameObject.SetActive(true);
                    if (!string.IsNullOrEmpty(doorBeginAnimation))
                    {
                        skeletonOpenClose.AnimationState.SetAnimation(0, doorBeginAnimation, true);
                    }
                }
            }
        }
        else
        {
            if (open)
            {
                StartAnimateOpen();
            }
            else
            {
                StartAnimateClose();
            }
        }
    }

    private void StartAnimateOpen()
    {
        if (skeletonAnimation != null && !string.IsNullOrEmpty(closedIdleAnimation))
        {
            skeletonAnimation.AnimationState.SetAnimation(0, closedIdleAnimation, true);
        }

        if (skeletonOpenClose != null)
        {
            skeletonOpenClose.gameObject.SetActive(true);
            if (!string.IsNullOrEmpty(doorOpenAnimation))
            {
                doorOpenEntry = skeletonOpenClose.AnimationState.SetAnimation(0, doorOpenAnimation, false);
                if (doorOpenEntry != null)
                {
                    doorOpenEntry.Complete += OnDoorOpenComplete;
                }
                else
                {
                    OnDoorOpenComplete(null);
                }
            }
            else
            {
                OnDoorOpenComplete(null);
            }
        }
        else
        {
            OnDoorOpenComplete(null);
        }
    }

    private void OnDoorOpenComplete(TrackEntry trackEntry)
    {
        if (doorOpenEntry != null)
        {
            doorOpenEntry.Complete -= OnDoorOpenComplete;
            doorOpenEntry = null;
        }

        if (skeletonOpenClose != null)
        {
            skeletonOpenClose.gameObject.SetActive(false);
        }

        if (skeletonAnimation != null && !string.IsNullOrEmpty(appearAnimation))
        {
            boxAppearOpenEntry = skeletonAnimation.AnimationState.SetAnimation(0, appearAnimation, false);
            if (boxAppearOpenEntry != null)
            {
                boxAppearOpenEntry.Complete += OnBoxAppearOpenComplete;
            }
            else
            {
                OnBoxAppearOpenComplete(null);
            }
        }
        else
        {
            OnBoxAppearOpenComplete(null);
        }
    }

    private void OnBoxAppearOpenComplete(TrackEntry trackEntry)
    {
        if (boxAppearOpenEntry != null)
        {
            boxAppearOpenEntry.Complete -= OnBoxAppearOpenComplete;
            boxAppearOpenEntry = null;
        }

        if (skeletonAnimation != null && !string.IsNullOrEmpty(beginAnimation))
        {
            skeletonAnimation.AnimationState.SetAnimation(0, beginAnimation, true);
        }
    }

    private void StartAnimateClose()
    {
        if (skeletonAnimation != null && !string.IsNullOrEmpty(appearAnimation))
        {
            boxAppearCloseEntry = skeletonAnimation.AnimationState.SetAnimation(0, appearAnimation, false);
            if (boxAppearCloseEntry != null && boxAppearCloseEntry.Animation != null)
            {
                boxAppearCloseEntry.TimeScale = -1f;
                boxAppearCloseEntry.TrackTime = boxAppearCloseEntry.Animation.Duration;
                boxAppearCloseEntry.Complete += OnBoxAppearCloseComplete;
            }
            else
            {
                OnBoxAppearCloseComplete(null);
            }
        }
        else
        {
            OnBoxAppearCloseComplete(null);
        }
    }

    private void OnBoxAppearCloseComplete(TrackEntry trackEntry)
    {
        if (boxAppearCloseEntry != null)
        {
            boxAppearCloseEntry.Complete -= OnBoxAppearCloseComplete;
            boxAppearCloseEntry = null;
        }

        if (skeletonAnimation != null && !string.IsNullOrEmpty(closedIdleAnimation))
        {
            skeletonAnimation.AnimationState.SetAnimation(0, closedIdleAnimation, true);
        }

        if (skeletonOpenClose != null)
        {
            skeletonOpenClose.gameObject.SetActive(true);
            if (!string.IsNullOrEmpty(doorCloseAnimation))
            {
                doorCloseEntry = skeletonOpenClose.AnimationState.SetAnimation(0, doorCloseAnimation, false);
                if (doorCloseEntry != null)
                {
                    doorCloseEntry.Complete += OnDoorCloseComplete;
                }
                else
                {
                    OnDoorCloseComplete(null);
                }
            }
            else
            {
                OnDoorCloseComplete(null);
            }
        }
        else
        {
            OnDoorCloseComplete(null);
        }
    }

    private void OnDoorCloseComplete(TrackEntry trackEntry)
    {
        if (doorCloseEntry != null)
        {
            doorCloseEntry.Complete -= OnDoorCloseComplete;
            doorCloseEntry = null;
        }

        if (skeletonOpenClose != null && !string.IsNullOrEmpty(doorBeginAnimation))
        {
            skeletonOpenClose.AnimationState.SetAnimation(0, doorBeginAnimation, true);
        }
    }
}
