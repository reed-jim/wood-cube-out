using System;
using Lean.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class BuyBoosterPopup : BasePopup
{
    [SerializeField] private RectTransform container;
    [SerializeField] private Button buyWithDiamondButton;
    [SerializeField] private Image buyWithDiamondButtonForeground;
    [SerializeField] private Button getWithAdsButton;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI title;
    [SerializeField] private LeanLocalizedTextMeshProUGUI boosterDescription;

    [Header("BOOSTER SPRITE")]
    [SerializeField] private Sprite[] boosterSprites;
    [SerializeField] private string[] leanTranslationTitle;
    [SerializeField] private string[] leanTranslationNames;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private BoosterDataObserver boosterDataObserver;

    private BoosterType _boosterType;

    public static event Action<BoosterType> useBoosterEvent;
    public static event Action unlockScrewBoxEvent;

    protected override void RegisterMoreEvent()
    {
        BoosterUI.showBuyBoosterPopupEvent += ShowBuyBoosterPopup;
        ScrewBoxUI.showBuyBoosterPopupEvent += ShowBuyBoosterPopup;

        getWithAdsButton.onClick.AddListener(GetWithAds);
    }

    protected override void UnregisterMoreEvent()
    {
        BoosterUI.showBuyBoosterPopupEvent -= ShowBuyBoosterPopup;
        ScrewBoxUI.showBuyBoosterPopupEvent -= ShowBuyBoosterPopup;
    }

    private void ShowBuyBoosterPopup(BoosterType boosterType)
    {
        Show();

        _boosterType = boosterType;

        icon.sprite = boosterSprites[(int)_boosterType];

        UIUtil.SetSizeKeepRatioY(icon, 0.25f * container.sizeDelta.x);

        costText.text = $"{boosterDataObserver.BoosterCosts[(int)_boosterType]}";
        title.TranslationName = leanTranslationTitle[(int)_boosterType];
        boosterDescription.TranslationName = leanTranslationNames[(int)_boosterType];

        if (userResourcesObserver.UserResources.DiamondQuantity == 0)
        {
            buyWithDiamondButton.interactable = false;

            buyWithDiamondButtonForeground.color = ColorUtil.WithAlpha(buyWithDiamondButtonForeground.color, 0.5f);
        }
        else
        {
            buyWithDiamondButton.interactable = true;

            buyWithDiamondButtonForeground.color = ColorUtil.WithAlpha(buyWithDiamondButtonForeground.color, 1f);
        }

        getWithAdsButton.onClick.RemoveAllListeners();

        if (boosterType == BoosterType.UnlockScrewBox)
        {
            getWithAdsButton.onClick.AddListener(UnlockScrewBoxWithAds);
        }
        else
        {
            getWithAdsButton.onClick.AddListener(GetWithAds);
        }
    }

    private void GetWithAds()
    {
        AdmobAdsMax.Instance.ShowVideoReward(OnRewaredAdCompleted, null, null);
    }

    private void OnRewaredAdCompleted()
    {
        userResourcesObserver.UserResources.BoosterQuantities[(int)_boosterType]++;

        useBoosterEvent?.Invoke(_boosterType);

        Hide();
    }

    private void UnlockScrewBoxWithAds()
    {
        AdmobAdsMax.Instance.ShowVideoReward(OnRewaredAdCompletedToUnlockBox, null, null);
    }

    private void OnRewaredAdCompletedToUnlockBox()
    {
        Hide(onCompletedAction: () =>
        {
            unlockScrewBoxEvent?.Invoke();
        });
    }
}
