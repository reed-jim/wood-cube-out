using UnityEngine;
using UnityEngine.UI;

public class SpaceRaceTutorialPopup : BasePopup
{
    [SerializeField] private Button continueButton;

    protected override void RegisterMoreEvent()
    {
        continueButton.onClick.AddListener(Hide);
    }

    protected override void Hide()
    {
        base.Hide(onCompletedAction: () =>
        {
            SwitchRoute(GameEnum.ScreenRoute.SpaceRace);
        });
    }
}
