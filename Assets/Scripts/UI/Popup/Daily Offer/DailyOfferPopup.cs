using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyOfferPopup : BasePopup
{
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text[] rewardQuantityTexts;
    [SerializeField] private TMP_Text localizedPriceText;
    [SerializeField] private TMP_Text timeCounterText;

    [SerializeField] private IAPPackageData packageData;

    private SaleIAPData _saleIAPData;

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

    protected override void MoreActionInAwake()
    {
        LoadData();
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        ShowRewards();

        requestFetchIAPPrice?.Invoke();

        StartCoroutine(Counting());
    }

    private void ShowRewards()
    {
        rewardQuantityTexts[0].text = $"{packageData.CoinQuantity}";
        rewardQuantityTexts[1].text = $"{packageData.AddHoleBoosterQuantity}";
        rewardQuantityTexts[2].text = $"{packageData.BreakObjectBoosterQuantity}";
        rewardQuantityTexts[3].text = $"{packageData.ClearHolesBoosterQuantity}";
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

    private void LoadData()
    {
        _saleIAPData = DataUtility.Load(GameConstants.SALE_IAP_DATA, new SaleIAPData());
    }

    private IEnumerator Counting()
    {
        WaitForSeconds waitOneSecond = new WaitForSeconds(1);

        while (true)
        {
            UpdateRemainingTime();

            yield return waitOneSecond;
        }
    }

    private void UpdateRemainingTime()
    {
        if (_saleIAPData.dailyOfferSaleTimestampEnd == 0)
        {
            _saleIAPData.dailyOfferSaleTimestampEnd =
                new DateTimeOffset(DateTime.Now).AddDays(3).ToUnixTimeSeconds();

            DataUtility.Save(GameConstants.SALE_IAP_DATA, _saleIAPData);
        }

        double remainingTime = (DateTimeOffset.FromUnixTimeSeconds(_saleIAPData.dailyOfferSaleTimestampEnd).DateTime - DateTime.Now).TotalSeconds;

        timeCounterText.text = $"{CommonUtil.ConvertTimeFormat(remainingTime)}";
    }

    private void OnResourceEarnPopupShown(bool isShow)
    {
        if (isShow)
        {
            Hide();
        }
    }
}
