using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RevivePopup : BasePopup
{
    [SerializeField] private Button reviveByDiamondButton;
    [SerializeField] private Image reviveByDiamondButtonForeground;
    [SerializeField] private Button reviveByAdsButton;
    [SerializeField] private TMP_Text costText;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private BoosterDataObserver boosterDataObserver;

    public static event Action reviveEvent;
    public static event Action reviveRefusedEvent;

    protected override void MoreActionInAwake()
    {
        costText.text = $"{boosterDataObserver.BoosterCosts.Last()}";
    }

    protected override void RegisterMoreEvent()
    {
        GameStateLose.showRevivePopupEvent += ShowRevivePopup;

        reviveByDiamondButton.onClick.AddListener(ReviveByDiamond);
        reviveByAdsButton.onClick.AddListener(ReviveByAds);
    }

    protected override void MoreActionOnDestroy()
    {
        GameStateLose.showRevivePopupEvent -= ShowRevivePopup;
    }

    protected override void Hide()
    {
        base.Hide(onCompletedAction: () =>
        {
            reviveRefusedEvent?.Invoke();
        });
    }

    private void ShowRevivePopup()
    {
        Show();

        AudioSource loseSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.LOSE_SOUND);

        loseSound.Play();

        if (userResourcesObserver.UserResources.DiamondQuantity == 0)
        {
            reviveByDiamondButton.interactable = false;

            reviveByDiamondButtonForeground.color = ColorUtil.WithAlpha(reviveByDiamondButtonForeground.color, 0.5f);
        }
        else
        {
            reviveByDiamondButton.interactable = true;

            reviveByDiamondButtonForeground.color = ColorUtil.WithAlpha(reviveByDiamondButtonForeground.color, 1f);
        }
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
