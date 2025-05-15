using System;
using System.Collections.Generic;
using Lean.Localization;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IAPSuggestionPopup : BasePopup
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text localizedTitle;
    [SerializeField] private TMP_Text localizedPriceText;
    [SerializeField] private RectTransform[] resourceGroups;
    private ResourceGroupUI[] _resourceGroupsUI;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button noThanksButton;

    [Header("CUSTOMIZE")]
    [SerializeField] private IAPDataContainer IAPDataContainer;
    [SerializeField] private IAPPackageData packageData;

    private int _coinQuantity;
    private bool _isRemoveAds;

    public static event Action<string> buyIAPEvent;
    public static event Action requestFetchIAPPrice;

    protected override void MoreActionInAwake()
    {
        _resourceGroupsUI = new ResourceGroupUI[resourceGroups.Length];

        for (int i = 0; i < resourceGroups.Length; i++)
        {
            _resourceGroupsUI[i] = resourceGroups[i].GetComponent<ResourceGroupUI>();
        }
    }

    protected override void RegisterMoreEvent()
    {
        SaferioIAPManager.updatePriceEvent += FetchLocalizedPrice;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourceEarnPopupShown;

        buyButton.onClick.AddListener(Buy);
        noThanksButton.onClick.AddListener(Hide);
    }

    protected override void UnregisterMoreEvent()
    {
        SaferioIAPManager.updatePriceEvent -= FetchLocalizedPrice;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourceEarnPopupShown;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        if (CommonUtil.GetParentActiveChildCount(transform.parent) > 1)
        {
            return;
        }

        int packageIndex = RemoteConfigController.GetIntConfig(FirebaseConfig.IAP_SUGGESTION_PACKAGE_INDEX, 1);

        packageData = IAPDataContainer.ProductsData[packageIndex];

        if (packageData == null)
        {
            return;
        }

        requestFetchIAPPrice?.Invoke();

        int coinQuantity = packageData.CoinQuantity;

        int[] boosterQuantites = new int[3] {
            packageData.AddHoleBoosterQuantity,
            packageData.BreakObjectBoosterQuantity,
            packageData.ClearHolesBoosterQuantity
        };

        bool isRemoveAds = packageData.IsRemoveAd;

        ShowRewards(coinQuantity, boosterQuantites, isRemoveAds);

        // localizedTitle.text = packageData.ProductName;

        // delay enable buttons
        closeButton.interactable = false;
        buyButton.interactable = false;
        noThanksButton.interactable = false;

        closeButton.transform.localScale = Vector3.zero;
        buyButton.transform.localScale = Vector3.zero;
        noThanksButton.transform.localScale = Vector3.zero;

        base.Show(ShowButtons);

        void ShowButtons()
        {
            _tweens.Add(Tween.Scale(closeButton.transform, 1, duration: 0.3f));
            _tweens.Add(Tween.Scale(buyButton.transform, 1, duration: 0.3f));
            _tweens.Add(Tween.Scale(noThanksButton.transform, 1, duration: 0.3f)
                .OnComplete(() =>
                {
                    closeButton.interactable = true;
                    buyButton.interactable = true;
                    noThanksButton.interactable = true;
                })
            );
        }
    }

    private void ShowRewards(int coinQuantity, int[] boosterQuantities, bool isRemoveAd)
    {
        List<RectTransform> validResourceGroups = new List<RectTransform>();

        if (isRemoveAd && !UserData.IsRemoveAds)
        {
            validResourceGroups.Add(resourceGroups[0]);
        }

        if (coinQuantity > 0)
        {
            _resourceGroupsUI[1].SetQuantity(coinQuantity);
            _resourceGroupsUI[1].SetMultiplyQuantity(3 * coinQuantity);

            validResourceGroups.Add(resourceGroups[1]);
        }

        for (int i = 0; i < boosterQuantities.Length; i++)
        {
            if (boosterQuantities[i] > 0)
            {
                _resourceGroupsUI[2 + i].SetQuantity(boosterQuantities[i]);
                _resourceGroupsUI[2 + i].SetMultiplyQuantity(3 * boosterQuantities[i]);

                validResourceGroups.Add(resourceGroups[2 + i]);
            }
        }

        Vector3 position = Vector3.zero;

        position.y = validResourceGroups[0].localPosition.y;

        for (int i = 0; i < resourceGroups.Length; i++)
        {
            resourceGroups[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < validResourceGroups.Count; i++)
        {
            position.x = (-(validResourceGroups.Count - 1) / 2f + i) * 0.17f * container.sizeDelta.x;

            validResourceGroups[i].gameObject.SetActive(true);
            validResourceGroups[i].localPosition = position;
        }

        _coinQuantity = coinQuantity;
    }

    private void Buy()
    {
        GamePersistentVariable.IAPLocation = "IAP Suggestion";
        GamePersistentVariable.IAPAddition = 2;

        buyIAPEvent?.Invoke(packageData.ProductId);
    }

    private void FetchLocalizedPrice(string productId, string localizedPrice)
    {
        if (productId == packageData.ProductId)
        {
            localizedPriceText.text = $"{localizedPrice}";
        }
    }

    private void OnResourceEarnPopupShown(bool isShow)
    {
        if (isShow)
        {
            Hide();
        }
    }
}
