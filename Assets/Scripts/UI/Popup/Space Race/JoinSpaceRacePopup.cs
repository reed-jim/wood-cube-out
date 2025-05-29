using UnityEngine;
using UnityEngine.UI;

public class JoinSpaceRacePopup : BasePopup
{
    [SerializeField] private Button joinButton;

    protected override void MoreActionInAwake()
    {
        joinButton.onClick.AddListener(Join);
    }

    private void Join()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }

        SwitchRoute(GameEnum.ScreenRoute.SpaceRace);
    }
}
