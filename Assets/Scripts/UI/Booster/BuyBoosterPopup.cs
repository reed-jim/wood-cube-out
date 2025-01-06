using System;
using Lean.Localization;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class BuyBoosterPopup : BasePopup
{
    [SerializeField] private RectTransform container;
    [SerializeField] private Button buyWithDiamondButton;
    [SerializeField] private Button getWithAdsButton;
    [SerializeField] private Image icon;
    [SerializeField] private LeanLocalizedTextMeshProUGUI boosterDescription;

    [Header("BOOSTER SPRITE")]
    [SerializeField] private Sprite[] boosterSprites;
    [SerializeField] private string[] leanTranslationNames;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    private BoosterType _boosterType;

    public static event Action<BoosterType> useBoosterEvent;

    protected override void MoreActionInAwake()
    {
        if (userResourcesObserver.UserResources.DiamondQuantity == 0)
        {
            buyWithDiamondButton.interactable = false;
        }
        else
        {
            buyWithDiamondButton.interactable = true;
        }
    }

    protected override void RegisterMoreEvent()
    {
        BoosterUI.showBuyBoosterPopupEvent += ShowBuyBoosterPopup;

        getWithAdsButton.onClick.AddListener(GetWithAds);
    }

    protected override void UnregisterMoreEvent()
    {
        BoosterUI.showBuyBoosterPopupEvent -= ShowBuyBoosterPopup;
    }

    private void ShowBuyBoosterPopup(BoosterType boosterType)
    {
        Show();

        _boosterType = boosterType;

        icon.sprite = boosterSprites[(int)_boosterType];

        UIUtil.SetSizeKeepRatioY(icon, 0.3f * container.sizeDelta.x);

        boosterDescription.TranslationName = leanTranslationNames[(int)_boosterType];
    }

    private void GetWithAds()
    {
        userResourcesObserver.UserResources.BoosterQuantities[(int)_boosterType]++;

        useBoosterEvent?.Invoke(_boosterType);

        Hide();
    }
}
