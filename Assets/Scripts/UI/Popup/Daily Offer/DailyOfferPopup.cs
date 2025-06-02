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
    [SerializeField] private TMP_Text timeCounterContainer;
    [SerializeField] private TMP_Text timeCounterText;

    [SerializeField] private IAPPackageData packageData;
    [SerializeField] private SalePackData salePackData;

    private SaleIAPData _saleIAPData;

    public static event Action<string> buyIAPEvent;
    public static event Action requestFetchIAPPrice;
    public static event Action refreshSaleIAPPackEvent;
    public static event Action countSaleIAPPackLifeTimeEvent;

    protected override void RegisterMoreEvent()
    {
        SaferioIAPManager.updatePriceEvent += FetchLocalizedPrice;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourceEarnPopupShown;
        SalePackIAPManager.updateDailyOfferLifeTimeEvent += UpdateLifeTime;

        buyButton.onClick.AddListener(Buy);
    }

    protected override void UnregisterMoreEvent()
    {
        SaferioIAPManager.updatePriceEvent -= FetchLocalizedPrice;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourceEarnPopupShown;
        SalePackIAPManager.updateDailyOfferLifeTimeEvent -= UpdateLifeTime;
    }

    protected override void MoreActionInAwake()
    {
        LoadData();
        Setup();
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        ShowRewards();

        requestFetchIAPPrice?.Invoke();
    }

    private void Setup()
    {
        if (salePackData.lifetime <= 0)
        {
            timeCounterContainer.gameObject.SetActive(false);

            return;
        }
        else
        {
            timeCounterContainer.gameObject.SetActive(true);
        }

        bool isVisitPack = DataUtility.Load($"Is Visit {salePackData.name}", false);
        bool isPackExpired = DataUtility.Load($"Is Expired {salePackData.name}", false);
        long packEndTimestamp = DataUtility.Load($"{salePackData.name} End Timestamp", 0);

        if (!isVisitPack)
        {
            InitNewPack();
        }
        else
        {
            if (isPackExpired)
            {
                if (CommonUtil.IsNewDay(packEndTimestamp))
                {
                    InitNewPack();
                }
            }
        }

        void InitNewPack()
        {
            DataUtility.Save($"{salePackData.name} End Timestamp", CommonUtil.GetCurrentTimestamp() + salePackData.lifetime);
            DataUtility.Save($"Is Visit {salePackData.name}", true);
            DataUtility.Save($"Is Expired {salePackData.name}", false);

            countSaleIAPPackLifeTimeEvent?.Invoke();
        }
    }

    private void UpdateLifeTime(double seconds)
    {
        try
        {
            timeCounterText.text = $"{CommonUtil.ToHHMMSS(seconds)}";
        }
        catch (System.Exception)
        {

        }
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

        double remainingTime = (DateTimeOffset.FromUnixTimeSeconds(_saleIAPData.dailyOfferSaleTimestampEnd).LocalDateTime - DateTime.Now).TotalSeconds;

        timeCounterText.text = $"{CommonUtil.ConvertTimeFormat(remainingTime)}";
    }

    private void OnResourceEarnPopupShown(bool isShow)
    {
        if (isShow)
        {
            Hide();

            try
            {
                int currentBought = DataUtility.Load($"Time Bought {salePackData.name}", 0);

                DataUtility.Save($"Time Bought {salePackData.name}", currentBought + 1);

                refreshSaleIAPPackEvent?.Invoke();
            }
            catch (System.Exception)
            {

            }
        }
    }
}
