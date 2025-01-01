using System;
using UnityEngine;
using UnityEngine.UI;

public class RatePopup : BasePopup
{
    [SerializeField] private Button rateButton;

    public static event Action launchInAppReviewEvent;

    protected override void RegisterMoreEvent()
    {
        rateButton.onClick.AddListener(Rate);
    }

    private void Rate()
    {
        launchInAppReviewEvent?.Invoke();
    }
}
