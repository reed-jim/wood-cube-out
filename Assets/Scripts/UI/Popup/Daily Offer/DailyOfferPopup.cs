using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyOfferPopup : BasePopup
{
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text[] rewardQuantityTexts;
    [SerializeField] private TMP_Text localizedPriceText;
    [SerializeField] private TMP_Text timeCounterText;

    [SerializeField] private DailyRewardData rewardData;
    [SerializeField] private IAPPackageData packageData;

    public static event Action<string> buyIAPEvent;
    public static event Action requestFetchIAPPrice;

    protected override void RegisterMoreEvent()
    {
        SaferioIAPManager.updatePriceEvent += FetchLocalizedPrice;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourceEarnPopupShown;

        buyButton.onClick.AddListener(Buy);
    }

    protected override void UnregisterMoreEvent()
    {
        SaferioIAPManager.updatePriceEvent -= FetchLocalizedPrice;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourceEarnPopupShown;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        ShowRewards();

        requestFetchIAPPrice?.Invoke();
    }

    private void ShowRewards()
    {
        rewardQuantityTexts[0].text = $"{rewardData.BoosterQuantities[0]}";
        rewardQuantityTexts[1].text = $"{rewardData.BoosterQuantities[1]}";
        rewardQuantityTexts[2].text = $"{rewardData.BoosterQuantities[2]}";
        rewardQuantityTexts[3].text = $"{rewardData.CoinQuantity}";
    }

    private void FetchLocalizedPrice(string productId, string localizedPrice)
    {
        if (productId == packageData.ProductId)
        {
            localizedPriceText.text = $"{localizedPrice}";
        }
    }

    private void Buy()
    {
        buyIAPEvent?.Invoke(packageData.ProductId);
    }

    private void OnResourceEarnPopupShown(bool isShow)
    {
        if (isShow)
        {
            Hide();
        }
    }
}
