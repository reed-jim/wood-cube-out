using System;
using AntsFlow;
using Saferio.Util.SaferioTween;
using UnityEngine;
using PrimeTween;

public class KeyAntBox : WashingMachineItem
{
    protected override void Awake()
    {
        washingMachineStartSlot = transform.parent.GetComponent<WashingMachineStartSlot>();
    }

    protected override void OnDestroy()
    {
    }

    public override void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number, Action onCompleteAction = null)
    {
        this.washingMachineStartSlot = washingMachineStartSlot;
        washingMachineSlot = null;

        transform.SetParent(washingMachineStartSlot.transform);
        transform.localPosition = new Vector3(0, 0, -1);
        transform.localScale = Vector3.zero;

        Tween.Scale(transform, 2.3f, duration: 0.3f)
        .OnComplete(() =>
        {
            isAnimating = false;
            onCompleteAction?.Invoke();
        });

        isAnimating = true;
    }

    public override void Setup(WashingMachineStartSlot washingMachineStartSlot)
    {
        this.washingMachineStartSlot = washingMachineStartSlot;
        isDisable = !(washingMachineStartSlot.row == 0);

        if (washingMachineStartSlot.row == 0)
        {
            SaferioTween.Delay(0.3f, () =>
            {
                TriggerUnlockAndDisappear();
            });
        }
    }

    public override bool IsValidToSelect()
    {
        return false;
    }

    public override bool IsValidToSelect(int currentNumSlotRequired)
    {
        return false;
    }

    private void TriggerUnlockAndDisappear()
    {
        if (washingMachineStartSlot != null)
        {
            washingMachineStartSlot.washingMachine = null;
            var currentStartSlot = washingMachineStartSlot;
            washingMachineStartSlot = null;
            WashingMachineManager.Ins.UnlockOneLockedSlot();

            SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
            {
                ObjectPoolingEverything.ReturnToPool(GameConstants.keyAntBox, gameObject);

                WashingMachineManager.Ins.MoveAntBoxTop();
            });
        }
    }
}
