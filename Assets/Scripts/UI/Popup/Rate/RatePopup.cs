using System;
using UnityEngine;
using UnityEngine.UI;

public class RatePopup : BasePopup
{
    [SerializeField] private Button okButton;

    public static event Action launchInAppReviewEvent;

    protected override void RegisterMoreEvent()
    {
        RateStarItemUI.selectRateStarEvent += SelectRateStar;

        okButton.onClick.AddListener(Hide);
    }

    protected override void UnregisterMoreEvent()
    {
        RateStarItemUI.selectRateStarEvent -= SelectRateStar;
    }

    private void Rate()
    {
        OpenReviewPage();

        // launchInAppReviewEvent?.Invoke();
    }

    private void SelectRateStar(int index)
    {
        if (index == 4)
        {
            OpenReviewPage();
        }
        else
        {
            Hide();
        }
    }

    public void OpenReviewPage()
    {
        string url = "https://play.google.com/store/apps/details?id=com.screw.away.pin.puzzle.and";

        Application.OpenURL(url);
    }
}
