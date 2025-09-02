using UnityEngine;
using UnityEngine.UI;

public class DecisionPopup : BasePopup
{
    [SerializeField] private Button decideButton;

    protected override void RegisterMoreEvent()
    {
        decideButton.onClick.AddListener(Decide);
    }

    private void Decide()
    {
        SuspectManager.Instance.suspects = new Suspect[4];
        SuspectManager.Instance.currentSuspectIndex = 0;
        
        for (int i = 0; i < SuspectManager.Instance.suspects.Length; i++)
        {
            SuspectManager.Instance.suspects[i] = new Suspect();
        }

        SwitchRoute(GameEnum.ScreenRoute.Suspect);
    }
}
