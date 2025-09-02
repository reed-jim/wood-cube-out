using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuspectDetailPopup : BasePopup
{
    [SerializeField] private TMP_Text suspectNameText;
    [SerializeField] private Button innocentButton;
    [SerializeField] private Button warnButton;
    [SerializeField] private Button fineButton;
    [SerializeField] private Button arrestButton;

    public static event Action updateSuspectStatusEvent;

    protected override void RegisterMoreEvent()
    {
        innocentButton.onClick.AddListener(() => IssueVerdict(Decision.Innocent));
        warnButton.onClick.AddListener(() => IssueVerdict(Decision.Warning));
        fineButton.onClick.AddListener(() => IssueVerdict(Decision.Fine));
        arrestButton.onClick.AddListener(() => IssueVerdict(Decision.Arrest));
    }

    protected override void Show(Action onCompletedAction = null)
    {
        suspectNameText.text = $"{SuspectManager.Instance.CurrentSuspect.suspectName}";

        base.Show(onCompletedAction);
    }

    private void IssueVerdict(Decision decision)
    {
        SuspectManager.Instance.CurrentSuspect.decision = decision;

        // updateSuspectStatusEvent?.Invoke();

        SwitchRoute(GameEnum.ScreenRoute.Suspect);
    }
}
