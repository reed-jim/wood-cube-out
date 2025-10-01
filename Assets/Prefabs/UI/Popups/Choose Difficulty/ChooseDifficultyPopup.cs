using System;
using UnityEngine;
using UnityEngine.UI;

public class ChooseDifficultyPopup : BasePopup
{
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button refuseButton;

    public static event Action hardModeSuggestionClosedEvent;

    protected override void MoreActionInAwake()
    {
        acceptButton.onClick.AddListener(Accept);
        refuseButton.onClick.AddListener(Refuse);
    }

    protected override void Hide(Action onCompletedAction = null)
    {
        base.Hide(onCompletedAction: () =>
        {
            hardModeSuggestionClosedEvent?.Invoke();
        });
    }

    private void Accept()
    {
        GamePersistentVariable.isHardMode = true;

        GameplayScreen.Ins.SwitchToHardMode();

        Hide();
    }

    private void Refuse()
    {
        GamePersistentVariable.isHardMode = false;

        Hide();
    }
}
