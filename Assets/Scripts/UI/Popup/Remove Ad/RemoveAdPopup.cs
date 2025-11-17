using System;
using NamPhuThuy.Data;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class RemoveAdPopup : BasePopup
{
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI priceText;

    #region EVENT
    public static event Action<string> buyIAPEvent;
    #endregion

    protected override void MoreActionInAwake()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            priceText.text = $"6.99$";
        }
    }

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
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Hide();

            ShowNotification(GameConstants.NO_INTERNET, GameConstants.PLEASE_CHECK_YOUR_INTERNET_CONNECTION);
        }
        else
        {
            buyIAPEvent?.Invoke(DataManager.Ins.RemoveAdsPackIAP.ProductId);

            buyButton.interactable = false;

            _tweens.Add(Tween.Delay(1f).OnComplete(() =>
            {
                buyButton.interactable = true;
            }));
        }
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
        IAPPackageData iapData = DataManager.Ins.RemoveAdsPackIAP;
        if (productId == iapData.ProductId)
        {
            priceText.text = price;
        }
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);
    }

    protected override void Hide(Action onCompletedAction = null)
    {
        base.Hide(onCompletedAction);
    }
}
