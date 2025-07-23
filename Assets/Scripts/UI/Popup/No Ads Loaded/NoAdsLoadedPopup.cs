using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class NoAdsLoadedPopup : BasePopup
{
    [SerializeField] private Button okButton;
    [SerializeField] private Image blackBackground;

    protected override void RegisterMoreEvent()
    {
        showAdNotLoadedPopupEvent += Show;

        okButton.onClick.AddListener(Hide);
    }

    protected override void UnregisterMoreEvent()
    {
        showAdNotLoadedPopupEvent -= Show;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        blackBackground.color = ColorUtil.WithAlpha(Color.black, 0);

        _tweens.Add(Tween.Alpha(blackBackground, 0.95f, duration: 0.3f));

        base.Show(onCompletedAction);
    }

    protected override void Hide()
    {
        _tweens.Add(Tween.Alpha(blackBackground, 0, duration: 0.3f));

        base.Hide();
    }
}
