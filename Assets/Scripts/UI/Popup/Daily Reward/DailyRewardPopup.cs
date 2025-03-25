using System;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardPopup : BasePopup
{
    [SerializeField] private Button claimButton;

    public static event Action claimDailyRewardEvent;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();
    }

    private void Claim()
    {
        claimDailyRewardEvent?.Invoke();
    }
}
