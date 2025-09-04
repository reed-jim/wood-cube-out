using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuspectPopup : BasePopup
{
    [SerializeField] private Button prevSuspectButton;
    [SerializeField] private Button nextSuspectButton;
    [SerializeField] private Button decideButton;
    [SerializeField] private Button finishButton;
    [SerializeField] private TMP_Text suspectText;
    [SerializeField] private TMP_Text statusText;

    protected override void RegisterMoreEvent()
    {
        SuspectDetailPopup.updateSuspectStatusEvent += UpdateSuspectStatus;

        prevSuspectButton.onClick.AddListener(PrevSuspect);
        nextSuspectButton.onClick.AddListener(NextSuspect);
        decideButton.onClick.AddListener(Decide);
        finishButton.onClick.AddListener(Finish);
    }

    protected override void UnregisterMoreEvent()
    {
        SuspectDetailPopup.updateSuspectStatusEvent -= UpdateSuspectStatus;
    }

    private void OnEnable()
    {
        UpdateSuspectStatus();
    }

    // protected override void Show(Action onCompletedAction = null)
    // {
    //     UpdateSuspectStatus();

    //     base.Show();
    // }

    private void PrevSuspect()
    {
        if (SuspectManager.Instance.currentSuspectIndex > 0)
        {
            SuspectManager.Instance.currentSuspectIndex--;

            UpdateSuspectStatus();
        }
        else
        {
            ShowNotification("Warning", "No previous suspect");
        }
    }

    private void NextSuspect()
    {
        if (SuspectManager.Instance.currentSuspectIndex < SuspectManager.Instance.suspects.Length - 1)
        {
            SuspectManager.Instance.currentSuspectIndex++;

            UpdateSuspectStatus();
        }
        else
        {
            ShowNotification("Warning", "No more suspect");
        }
    }

    private void UpdateSuspectStatus()
    {
        suspectText.text = $"{SuspectManager.Instance.CurrentSuspect.suspectName}";
        statusText.text = $"{SuspectManager.Instance.CurrentSuspect.decision}";
    }

    private void Decide()
    {
        SwitchRoute(GameEnum.ScreenRoute.SuspectDetail);
    }

    private void Finish()
    {
        int numSuspectNotTakenVerdict = SuspectManager.Instance.NumSuspectNotTakenVerdict();

        if (numSuspectNotTakenVerdict > 0)
        {
            ShowNotificationNoLocalized("Warning", $"{numSuspectNotTakenVerdict} suspects have not received a verdict yet.");
        }
        else
        {

            AdmobAdsMax.Instance.ShowInterstitial(actionIniterClose: () =>
            {
                SwitchRoute(GameEnum.ScreenRoute.SuspectResult);
            });
        }
    }
}
