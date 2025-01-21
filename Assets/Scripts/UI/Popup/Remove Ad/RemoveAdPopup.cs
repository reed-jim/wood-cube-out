using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RemoveAdPopup : BasePopup
{
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text priceText;

    [SerializeField] private IAPPackageData iapData;

    #region EVENT
    public static event Action<string> buyIAPEvent;
    #endregion

    protected override void RegisterMoreEvent()
    {
        SaferioIAPManager.updatePriceEvent += UpdatePrice;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourceEarnPopupShow;

        buyButton.onClick.AddListener(BuyRemoveAd);
    }

    protected override void UnregisterMoreEvent()
    {
        SaferioIAPManager.updatePriceEvent -= UpdatePrice;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourceEarnPopupShow;
    }

    private void BuyRemoveAd()
    {
        buyIAPEvent?.Invoke(iapData.ProductId);
    }

    private void OnResourceEarnPopupShow(bool isShow)
    {
        if (isShow)
        {
            Hide();
        }
    }

    private void UpdatePrice(string productId, string price)
    {
        if (productId == iapData.ProductId)
        {
            priceText.text = price;
        }
    }
}
