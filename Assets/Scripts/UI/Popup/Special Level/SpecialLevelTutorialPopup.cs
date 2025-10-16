using System;
using Lean.Localization;
using NamPhuThuy;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SpecialLevelTutorialPopup : BasePopup
{
    [Header("Components")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private LeanLocalizedTextMeshProUGUI descriptionLocalized;
    [SerializeField] private Button okButton;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        okButton.onClick.AddListener(Hide);
    }

    protected override void Show(Action onCompletedAction = null)
    {
        DebugLogger.LogSimple(context:this);
        base.Show(onCompletedAction: () =>
        {
            videoPlayer.Play();
        });
    }

    protected override void Hide()
    {
        base.Hide();
        DebugLogger.LogSimple(context:this);

        videoPlayer.Stop();
    }

    private void PlayTutorialVideo()
    {

    }
}
