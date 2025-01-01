using System;
using UnityEngine;
using UnityEngine.UI;

public class RevivePopup : BasePopup
{
    [SerializeField] private Button reviveByDiamondButton;
    [SerializeField] private Button reviveByAdsButton;

    public static event Action reviveEvent;
    public static event Action reviveRefusedEvent;

    protected override void RegisterMoreEvent()
    {
        GameStateLose.loseLevelEvent += OnLevelLose;

        reviveByDiamondButton.onClick.AddListener(ReviveByDiamond);
    }

    protected override void MoreActionOnDestroy()
    {
        GameStateLose.loseLevelEvent -= OnLevelLose;

        reviveByAdsButton.onClick.AddListener(ReviveByAds);
    }

    protected override void Hide()
    {
        base.Hide();

        reviveRefusedEvent?.Invoke();
    }

    private void OnLevelLose()
    {
        Show();

        AudioSource loseSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.LOSE_SOUND);

        loseSound.Play();
    }

    private void ReviveByDiamond()
    {
        base.Hide();

        reviveEvent?.Invoke();
    }

    private void ReviveByAds()
    {
        base.Hide();

        reviveEvent?.Invoke();
    }
}
