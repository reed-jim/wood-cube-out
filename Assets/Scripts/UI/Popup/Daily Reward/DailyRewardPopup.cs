using System;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardPopup : BasePopup
{
    [SerializeField] private Button claimButton;

    public static event Action claimDailyRewardEvent;

    protected override void RegisterMoreEvent()
    {
        claimButton.onClick.AddListener(Claim);
    }

    private void Claim()
    {
        claimDailyRewardEvent?.Invoke();
    }
}
