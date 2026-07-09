using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class IAPPackageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinQuantityText;
    [SerializeField] private TMP_Text addHintText;
    [SerializeField] private TMP_Text addShuffleText;
    [SerializeField] private TMP_Text addFreezeText;
    [SerializeField] private TMP_Text addUnlockText;

    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text timeCounterText;
    [SerializeField] private Button buyButton;

    [Header("CUSTOMIZE")]
    [SerializeField] private bool isContainRemoveAd;
    [SerializeField] private bool isRemoveAdsPlus;

    [SerializeField] private IAPPackageData _packageData;
    [SerializeField] private bool isSalePack;
    [SerializeField] private SaleIAPPackType packType;

    private string RemoveAdsPlusPurchasedKey => $"Remove Ads Plus Purchased {_packageData.ProductId}";

    #region EVENT
    public static event Action<string> buyIAPEvent;
    public static event Action<string, string> showNotificationEvent;
    #endregion

    private void Awake()
    {
        SaferioIAPManager.updatePriceEvent += UpdatePrice;
        ResourceEarnPopup.removeAdsEvent += OnAdsRemoved;
        SaferioIAPManager.removeAdsEvent += OnAdsRemoved;
        SalePackIAPManager.updateSalePackVisibilityEvent += UpdateVisibility;
        SalePackIAPManager.updateDailyOfferLifeTimeEvent += UpdateLifeTime;

        buyButton.onClick.AddListener(Buy);

        Setup(_packageData);

        UpdateVisibility();
    }

    void OnDestroy()
    {
        SaferioIAPManager.updatePriceEvent -= UpdatePrice;
        ResourceEarnPopup.removeAdsEvent -= OnAdsRemoved;
        SaferioIAPManager.removeAdsEvent -= OnAdsRemoved;
        SalePackIAPManager.updateSalePackVisibilityEvent -= UpdateVisibility;
        SalePackIAPManager.updateDailyOfferLifeTimeEvent -= UpdateLifeTime;
    }

    public void Setup(IAPPackageData data)
    {
        if (ShouldHideAfterRemoveAds())
        {
            gameObject.SetActive(false);

            return;
        }

        if (coinQuantityText != null)
        {
            coinQuantityText.text = $"{data.CoinQuantity}";
        }

        if (addHintText != null)
        {
            addHintText.text = $"{data.AddHintBoosterQuantity}";
        }

        if (addShuffleText != null)
        {
            addShuffleText.text = $"{data.AddShuffleBoosterQuantity}";
        }

        if (addFreezeText != null)
        {
            addFreezeText.text = $"{data.AddFreezeBoosterQuantity}";
        }

        if (addUnlockText != null)
        {
            addUnlockText.text = $"{data.AddUnlockBoosterQuantity}";
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
            GamePersistentVariable.lastPurchasedPackPosition = buyButton.transform.position;
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
        if (isRemoveAdsPlus && IsLastPurchasedPackage())
        {
            DataUtility.Save(RemoveAdsPlusPurchasedKey, true);
        }

        if (ShouldHideAfterRemoveAds())
        {
            gameObject.SetActive(false);
        }
    }

    private bool ShouldHideAfterRemoveAds()
    {
        if (!UserData.IsRemoveAds)
        {
            return false;
        }

        if (isContainRemoveAd)
        {
            return true;
        }

        return isRemoveAdsPlus && (IsRemoveAdsPlusPurchased() || IsLastPurchasedPackage());
    }

    private bool IsRemoveAdsPlusPurchased()
    {
        return _packageData != null && DataUtility.Load(RemoveAdsPlusPurchasedKey, false);
    }

    private bool IsLastPurchasedPackage()
    {
        return _packageData != null &&
               SaferioIAPManager.LastPurchasedRemoveAdsProductId == _packageData.ProductId;
    }

    private void UpdateVisibility()
    {
        if (!isSalePack)
        {
            return;
        }

        try
        {
            gameObject.SetActive(!ShouldHideAfterRemoveAds() && GamePersistentVariable.isShowSalePacks[packType]);
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
