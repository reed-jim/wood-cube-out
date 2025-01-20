using System;
using UnityEngine;
using UnityEngine.UI;

public class RemoveAdPopup : BasePopup
{
    [SerializeField] private Button buyButton;

    [SerializeField] private IAPPackageData iapData;

    #region EVENT
    public static event Action<string> buyIAPEvent;
    #endregion

    protected override void RegisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourceEarnPopupShow;

        buyButton.onClick.AddListener(BuyRemoveAd);
    }

    protected override void UnregisterMoreEvent()
    {
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
}
