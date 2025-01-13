using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class RevivePopup : BasePopup
{
    [SerializeField] private Button reviveByDiamondButton;
    [SerializeField] private Image reviveByDiamondButtonForeground;
    [SerializeField] private Image boosterImage;
    [SerializeField] private Button reviveByAdsButton;
    [SerializeField] private TMP_Text costText;

    [Header("SPRITE")]
    [SerializeField] private Sprite unlockBoxSprite;
    [SerializeField] private Sprite clearHolesSprite;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private BoosterDataObserver boosterDataObserver;

    #region PRIVATE FIELD
    private BoosterType _currentBoosterOnRevive;
    #endregion

    public static event Action<BoosterType> reviveEvent;
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

    private void ShowRevivePopup(BoosterType boosterType)
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

        if (boosterType == BoosterType.UnlockScrewBox)
        {
            boosterImage.sprite = unlockBoxSprite;
        }
        else if (boosterType == BoosterType.ClearScrewPorts)
        {
            boosterImage.sprite = clearHolesSprite;
        }

        UIUtil.SetSizeKeepRatioY(boosterImage, 0.2f * container.sizeDelta.x);

        _currentBoosterOnRevive = boosterType;
    }

    private void ReviveByDiamond()
    {
        base.Hide();

        reviveEvent?.Invoke(_currentBoosterOnRevive);
    }

    private void ReviveByAds()
    {
        base.Hide();

        reviveEvent?.Invoke(_currentBoosterOnRevive);
    }
}
