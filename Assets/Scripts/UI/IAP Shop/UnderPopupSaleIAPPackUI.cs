using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class UnderPopupSaleIAPPackUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinQuantityText;
    [SerializeField] private TMP_Text addHoleQuantityText;
    [SerializeField] private TMP_Text breakObjectQuantityText;
    [SerializeField] private TMP_Text clearHolesQuantityText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    [Header("CUSTOMIZE")]
    [SerializeField] private bool isContainRemoveAd;

    [SerializeField] private IAPPackageData packageData;
    [SerializeField] private SalePackData salePackData;

    private bool _isProcessingPayment;

    #region EVENT
    public static event Action<string> buyIAPEvent;
    public static event Action<string, string> showNotificationEvent;
    public static event Action refreshSuggestSaleIAPPackEvent;
    #endregion

    private void Awake()
    {
        SaferioIAPManager.updatePriceEvent += UpdatePrice;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourceEarnPopupShown;

        buyButton.onClick.AddListener(Buy);

        Setup(packageData);
    }

    void OnDestroy()
    {
        SaferioIAPManager.updatePriceEvent -= UpdatePrice;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourceEarnPopupShown;
    }

    public void Setup(IAPPackageData data)
    {
        if (isContainRemoveAd && UserData.IsRemoveAds)
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
            _isProcessingPayment = true;

            buyIAPEvent?.Invoke(packageData.ProductId);
        }
    }

    private void UpdatePrice(string productId, string price)
    {
        if (productId == packageData.ProductId)
        {
            priceText.text = price;
        }
    }

    private void OnResourceEarnPopupShown(bool isShow)
    {
        if (isShow)
        {
            try
            {
                if (gameObject.activeInHierarchy && _isProcessingPayment)
                {
                    int currentBought = DataUtility.Load($"Time Bought {salePackData.name}", 0);

                    DataUtility.Save($"Time Bought {salePackData.name}", currentBought + 1);
                    DataUtility.Save($"Bought {salePackData.name}", true);

                    refreshSuggestSaleIAPPackEvent?.Invoke();

                    _isProcessingPayment = false;
                }
            }
            catch (System.Exception)
            {

            }
        }
    }
}
