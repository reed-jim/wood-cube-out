using System;
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class OpenCloseAntBox : WashingMachineItem
{
    [Header("Box Animations (skeletonAnimation)")]
    [SpineAnimation(dataField = "skeletonAnimation")] public string closedIdleAnimation = "idle2";
    [SpineAnimation(dataField = "skeletonAnimation")] public string appearAnimation = "appear";
    [SpineAnimation(dataField = "skeletonAnimation")] public string disappearAnimation = "appear-re";


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
    private Coroutine toggleNumberCoroutine;

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

    public override void Setup(WashingMachineStartSlot washingMachineStartSlot)
    {
        base.Setup(washingMachineStartSlot);
        // Sau khi lên hàng: đồng bộ hiện/ẩn số theo trạng thái đóng/mở hiện tại.
        SetNumberVisible(isOpen);
    }

    // Cửa (Spine pixel_box2) đã rút nông nên không tự che số khi đóng.
    // => Điều khiển ẩn/hiện số bằng code theo đúng yêu cầu:
    //    - BẬT số khi anim mở thực hiện được 1/2 thời gian.
    //    - TẮT số khi anim đóng thực hiện được 1/2 thời gian.
    // numberVisible là nguồn sự thật duy nhất cho việc số có được hiện hay không.
    private bool numberVisible;

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

    // Booster EnableAll gọi hàm này. Nếu hộp đang ĐÓNG thì vẫn cho enable nhưng KHÔNG hiện số
    // (số nằm trước cửa nên sẽ đè lên cửa). Số chỉ hiện khi hộp thực sự mở.
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

        // Chống booster/Enable bật số lên khi hộp đang đóng (số nằm trước cửa nên sẽ đè lên cửa).
        // Khi numberVisible == false thì luôn ép số ẩn, bất kể ai vừa set alpha.
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

            // Instant: trạng thái tức thời => số hiện đúng theo open ngay.
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
        // Theo yêu cầu: KHÔNG bật số ngay khi bắt đầu mở mà đợi anim mở chạy được 1/2 thời gian mới bật.
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

        // Đồng bộ lại alpha số cho chắc chắn
        ApplyNumberVisibility();
    }

    private void StartAnimateClose()
    {
        // Giữ số hiện trong lúc thu box.
        // Tắt số khi anim đóng cửa (doorCloseAnimation) chạy được 1/2 thời gian.

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

        // Đảm bảo số được tắt khi hoàn tất anim đóng.
        SetNumberVisible(false);
    }
}
