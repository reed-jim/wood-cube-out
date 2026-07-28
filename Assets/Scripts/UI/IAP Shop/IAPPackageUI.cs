using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class IAPPackageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinQuantityText;
    [SerializeField] private TMP_Text addHoleQuantityText;
    [SerializeField] private TMP_Text breakObjectQuantityText;
    [SerializeField] private TMP_Text clearHolesQuantityText;
    [SerializeField] private TMP_Text clearOneColorQuantityText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text timeCounterText;
    [SerializeField] private Button buyButton;

    [Header("CUSTOMIZE")]
    [SerializeField] private bool isHideWhenRemoveAd;
    [SerializeField] private bool isNonConsumable;

    [SerializeField] private IAPPackageData _packageData;
    [SerializeField] private bool isSalePack;
    [SerializeField] private SaleIAPPackType packType;

    #region EVENT
    public static event Action<string> buyIAPEvent;
    public static event Action<string, string> showNotificationEvent;
    #endregion

    private void Awake()
    {
        SaferioIAPManager.updatePriceEvent += UpdatePrice;
        ResourceEarnPopup.removeAdsEvent += OnAdsRemoved;
        SaferioIAPManager.buyIAPPackageEvent += OnIAPPackageBought;
        SalePackIAPManager.updateSalePackVisibilityEvent += UpdateVisibility;
        SalePackIAPManager.updateDailyOfferLifeTimeEvent += UpdateLifeTime;

        buyButton.onClick.AddListener(Buy);

        UpdateVisibility();
    }

    void OnDestroy()
    {
        SaferioIAPManager.updatePriceEvent -= UpdatePrice;
        ResourceEarnPopup.removeAdsEvent -= OnAdsRemoved;
        SaferioIAPManager.buyIAPPackageEvent -= OnIAPPackageBought;
        SalePackIAPManager.updateSalePackVisibilityEvent -= UpdateVisibility;
        SalePackIAPManager.updateDailyOfferLifeTimeEvent -= UpdateLifeTime;
    }

    void OnEnable()
    {
        Setup(_packageData);
    }

    public void Setup(IAPPackageData data)
    {
        if (isHideWhenRemoveAd && UserData.IsRemoveAds)
        {
            gameObject.SetActive(false);

            return;
        }

        if (isNonConsumable && DataUtility.Load($"Is Bought {_packageData.name}", false))
        {
            gameObject.SetActive(false);

            return;
        }

        if (coinQuantityText != null)
        {
            coinQuantityText.text = $"{data.CoinQuantity}";
        }

        if (addHoleQuantityText != null)
        {
            addHoleQuantityText.text = $"{data.AddHoleBoosterQuantity}";
        }

        if (breakObjectQuantityText != null)
        {
            breakObjectQuantityText.text = $"{data.BreakObjectBoosterQuantity}";
        }

        if (clearHolesQuantityText != null)
        {
            clearHolesQuantityText.text = $"{data.ClearHolesBoosterQuantity}";
        }

        if (clearOneColorQuantityText != null)
        {
            clearOneColorQuantityText.text = $"{data.ClearOneColorBoosterQuantity}";
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            priceText.text = $"{data.Price}$";
        }
    }

    private void Buy()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            showNotificationEvent?.Invoke(GameConstants.NO_INTERNET, GameConstants.PLEASE_CHECK_YOUR_INTERNET_CONNECTION);
        }
        else
        {
            buyIAPEvent?.Invoke(_packageData.ProductId);
        }
    }

    private void UpdatePrice(string productId, string price)
    {
        if (productId == _packageData.ProductId)
        {
            priceText.text = price;
        }
    }

    private void OnAdsRemoved()
    {
        if (gameObject.activeInHierarchy)
        {
            if (isHideWhenRemoveAd)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnIAPPackageBought(string packageId)
    {
        if (gameObject.activeInHierarchy && packageId == _packageData.ProductId)
        {
            if (isNonConsumable)
            {
                gameObject.SetActive(false);
            }

            DataUtility.Save($"Is Bought {_packageData.name}", true);
        }
    }

    private void UpdateVisibility()
    {
        if (!isSalePack)
        {
            return;
        }

        try
        {
            gameObject.SetActive(GamePersistentVariable.isShowSalePacks[packType]);
        }
        catch (System.Exception)
        {

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
}
