using UnityEngine;
using UnityEngine.UI;

public class DecisionPopup : BasePopup
{
    [SerializeField] private Button checkCameraButton;
    [SerializeField] private Button decideButton;

    protected override void RegisterMoreEvent()
    {
        checkCameraButton.onClick.AddListener(CheckCamera);
        decideButton.onClick.AddListener(Decide);
    }

    private void CheckCamera()
    {
        SwitchRoute(GameEnum.ScreenRoute.CheckCamera);
    }

    private void Decide()
    {
        SuspectManager.Instance.suspects = new Suspect[3];
        SuspectManager.Instance.currentSuspectIndex = 0;

        for (int i = 0; i < SuspectManager.Instance.suspects.Length; i++)
        {
            SuspectManager.Instance.suspects[i] = new Suspect();
        }

        SwitchRoute(GameEnum.ScreenRoute.Suspect);
    }
}
