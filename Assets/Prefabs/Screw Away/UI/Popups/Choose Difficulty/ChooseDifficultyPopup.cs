using UnityEngine;
using UnityEngine.UI;

public class ChooseDifficultyPopup : BasePopup
{
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button refuseButton;

    protected override void MoreActionInAwake()
    {
        acceptButton.onClick.AddListener(Accept);
        refuseButton.onClick.AddListener(Refuse);
    }

    private void Accept()
    {
        GamePersistentVariable.isHardMode = true;

        GameplayScreen.Instance.SwitchToHardMode();

        Hide();
    }

    private void Refuse()
    {
        GamePersistentVariable.isHardMode = false;

        Hide();
    }
}
