using UnityEngine;
using UnityEngine.UI;

public class CheckCameraPopup : BasePopup
{
    [SerializeField] private Button backButton;

    protected override void RegisterMoreEvent()
    {
        backButton.onClick.AddListener(Back);
    }

    private void Back()
    {
        SwitchRoute(GameEnum.ScreenRoute.Decision);
    }
}
