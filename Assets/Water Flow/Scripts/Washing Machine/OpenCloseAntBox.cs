using System;
using System.Collections;
using AntsFlow;
using Spine;
using Spine.Unity;
using UnityEngine;

public class OpenCloseAntBox : WashingMachineItem
{
    [Header("Box Animations (skeletonAnimation)")]
    [SpineAnimation(dataField = "skeletonAnimation")] public string closedIdleAnimation = "idle2";
    [SpineAnimation(dataField = "skeletonAnimation")] public string appearAnimation = "appear";
    [SpineAnimation(dataField = "skeletonAnimation")] public string disappearAnimation = "appear-re";
    [SpineAnimation(dataField = "skeletonAnimation")] public string selectFailToggleAnimation = "select fail toggle";
    public bool useNewFailAnimation;
    public GameObject selectFailOutLine;


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
    private TrackEntry selectFailEntry;
    private Coroutine toggleNumberCoroutine;

    private bool numberVisible;
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

        if (selectFailOutLine != null)
        {
            selectFailOutLine.SetActive(false);
        }
    }

    public override void Setup(WashingMachineStartSlot washingMachineStartSlot)
    {
        base.Setup(washingMachineStartSlot);
        SetNumberVisible(isOpen);
    }


    private void SetNumberVisible(bool visible)
    {
        numberVisible = visible;
        ApplyNumberVisibility();
    }

    private void ApplyNumberVisibility()
    {
        if (numberText == null)
        {
            return;
        }

        numberText.alpha = (isInSlot || numberVisible) ? (isDisable ? 0.2f : 1f) : 0f;
    }

    public override void TemporarilyEnable()
    {
        if (numberVisible)
        {
            base.TemporarilyEnable();
        }
        else if (isDisable)
        {
            isTemporarilyEnabled = true;
            isDisable = false;
        }
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

        if (isOpen != lastOpenState)
        {
            lastOpenState = isOpen;
            PlayOpenCloseAnimation(isOpen, instant: false);
        }


        if (!numberVisible && numberText != null && numberText.alpha != 0f)
        {
            numberText.alpha = 0f;
        }
    }

    private void ClearAnimationListeners()
    {
        if (toggleNumberCoroutine != null)
        {
            StopCoroutine(toggleNumberCoroutine);
            toggleNumberCoroutine = null;
        }
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
        if (selectFailEntry != null)
        {
            selectFailEntry.Complete -= OnSelectFailComplete;
            selectFailEntry = null;
        }
    }

    private void OnDisable()
    {
        ClearAnimationListeners();
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

            SetNumberVisible(open);
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

        SetNumberVisible(false);

        if (skeletonAnimation != null && !string.IsNullOrEmpty(closedIdleAnimation))
        {
            skeletonAnimation.AnimationState.SetAnimation(0, closedIdleAnimation, true);
        }

        if (skeletonOpenClose != null)
        {
            skeletonOpenClose.gameObject.SetActive(true);
            if (!string.IsNullOrEmpty(doorOpenAnimation))
            {
                SoundManager.Instance.PlaySound(GameConstants.openBoxSound);

                doorOpenEntry = skeletonOpenClose.AnimationState.SetAnimation(0, doorOpenAnimation, false);
                if (doorOpenEntry != null)
                {
                    doorOpenEntry.Complete += OnDoorOpenComplete;
                    float halfDuration = (doorOpenEntry.Animation != null && doorOpenEntry.Animation.Duration > 0f)
                        ? doorOpenEntry.Animation.Duration / 2f
                        : (1f / 6f);
                    toggleNumberCoroutine = StartCoroutine(DelayedSetNumberVisible(true, halfDuration));
                }
                else
                {
                    SetNumberVisible(true);
                    OnDoorOpenComplete(null);
                }
            }
            else
            {
                SetNumberVisible(true);
                OnDoorOpenComplete(null);
            }
        }
        else
        {
            SetNumberVisible(true);
            OnDoorOpenComplete(null);
        }
    }

    private IEnumerator DelayedSetNumberVisible(bool visible, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetNumberVisible(visible);
        toggleNumberCoroutine = null;
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

        ApplyNumberVisibility();
    }

    private void StartAnimateClose()
    {


        if (skeletonAnimation != null && !string.IsNullOrEmpty(disappearAnimation))
        {
            boxAppearCloseEntry = skeletonAnimation.AnimationState.SetAnimation(0, disappearAnimation, false);
            if (boxAppearCloseEntry != null)
            {
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
                    float halfDuration = (doorCloseEntry.Animation != null && doorCloseEntry.Animation.Duration > 0f)
                        ? doorCloseEntry.Animation.Duration / 2f
                        : (1f / 6f);
                    toggleNumberCoroutine = StartCoroutine(DelayedSetNumberVisible(false, halfDuration));
                }
                else
                {
                    SetNumberVisible(false);
                    OnDoorCloseComplete(null);
                }
            }
            else
            {
                SetNumberVisible(false);
                OnDoorCloseComplete(null);
            }
        }
        else
        {
            SetNumberVisible(false);
            OnDoorCloseComplete(null);
        }
    }

    private void OnDoorCloseComplete(TrackEntry trackEntry)
    {
        SoundManager.Instance.PlaySound(GameConstants.closeBoxSound);

        if (doorCloseEntry != null)
        {
            doorCloseEntry.Complete -= OnDoorCloseComplete;
            doorCloseEntry = null;
        }

        if (skeletonOpenClose != null && !string.IsNullOrEmpty(doorBeginAnimation))
        {
            skeletonOpenClose.AnimationState.SetAnimation(0, doorBeginAnimation, true);
        }

        SetNumberVisible(false);
    }

    protected override void PlayInvalidSelectAnimation()
    {
        if (isInSlot)
        {
            return;
        }

        if (selectFailEntry != null)
        {
            selectFailEntry.Complete -= OnSelectFailComplete;
            selectFailEntry = null;
        }

        if (selectFailOutLine != null)
        {
            selectFailOutLine.SetActive(true);
        }

        if (isOpen && useNewFailAnimation)
        {
            if (skeletonAnimation != null && !string.IsNullOrEmpty(selectFailToggleAnimation))
            {
                selectFailEntry = skeletonAnimation.AnimationState.SetAnimation(0, selectFailToggleAnimation, false);
                if (selectFailEntry != null)
                {
                    selectFailEntry.Complete += OnSelectFailComplete;

                    float duration = selectFailEntry.Animation != null ? selectFailEntry.Animation.Duration : 0.5f;
                    if (duration > 0f)
                    {
                        PrimeTween.Tween.Delay(duration).OnComplete(() =>
                        {
                            if (selectFailOutLine != null)
                            {
                                selectFailOutLine.SetActive(false);
                            }
                        });
                    }
                }
            }

            if (GamePersistentVariable.IsVibrate)
            {
                CandyCoded.HapticFeedback.HapticFeedback.MediumFeedback();
            }

            WashingMachineManager.Ins.CheckLoseLevel();
        }
        else
        {
            base.PlayInvalidSelectAnimation();
            if (isOpenCloseType && !isOpen)
            {
                PrimeTween.Tween.Delay(0.3f).OnComplete(() =>
                {
                    if (selectFailOutLine != null)
                    {
                        selectFailOutLine.SetActive(false);
                    }
                });
            }
            else
            {
                if (skeletonAnimation != null && !string.IsNullOrEmpty(selectFalseAnimation))
                {
                    selectFailEntry = skeletonAnimation.AnimationState.GetCurrent(0);
                    if (selectFailEntry != null)
                    {
                        selectFailEntry.Complete += OnSelectFailComplete;

                        float duration = selectFailEntry.Animation != null ? selectFailEntry.Animation.Duration : 0.5f;
                        if (duration > 0f)
                        {
                            PrimeTween.Tween.Delay(duration).OnComplete(() =>
                            {
                                if (selectFailOutLine != null)
                                {
                                    selectFailOutLine.SetActive(false);
                                }
                            });
                        }
                    }
                }
            }
        }
    }

    private void OnSelectFailComplete(TrackEntry trackEntry)
    {
        if (trackEntry != null)
        {
            trackEntry.Complete -= OnSelectFailComplete;
        }
        if (selectFailEntry == trackEntry)
        {
            selectFailEntry = null;
        }
        if (selectFailOutLine != null)
        {
            selectFailOutLine.SetActive(false);
        }
    }

    public override void TurnOnWarningLoseAnimation()
    {
        if (selectFailOutLine != null)
        {
            selectFailOutLine.SetActive(true);
        }

        if (isOpen && useNewFailAnimation && skeletonAnimation != null && !string.IsNullOrEmpty(selectFailToggleAnimation))
        {
            skeletonAnimation.AnimationState.SetAnimation(0, selectFailToggleAnimation, true);
            isPlayingWarningLoseAnimation = true;
        }
        else
        {
            base.TurnOnWarningLoseAnimation();
        }
    }

    public override void TurnOffWarningLoseAnimation()
    {
        base.TurnOffWarningLoseAnimation();
        if (selectFailOutLine != null)
        {
            selectFailOutLine.SetActive(false);
        }
    }

    protected override void OnRevive(GameEnum.BoosterType boosterType)
    {
        base.OnRevive(boosterType);
        if (selectFailEntry != null)
        {
            selectFailEntry.Complete -= OnSelectFailComplete;
            selectFailEntry = null;
        }
        if (selectFailOutLine != null)
        {
            selectFailOutLine.SetActive(false);
        }
    }
}
