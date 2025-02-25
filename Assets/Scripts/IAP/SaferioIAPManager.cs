using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

public class SaferioIAPManager : MonoBehaviour, IDetailedStoreListener
{
    [SerializeField] private GooglePlayStoreProduct[] googlePlayStoreProducts;
    [SerializeField] private IAPDataContainer IAPDataContainer;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private IntVariable currentLevel;

    [SerializeField] private string removeAdsProductId;

    private IStoreController controller;
    private IExtensionProvider extensions;

    #region EVENT
    public static event Action iapProductPurchasedCompletedEvent;
    public static event Action removeAdPurchasedCompletedEvent;
    public static event Action<int> collectCoinEvent;
    public static event Action<string, string> updatePriceEvent;
    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;
    public static event Action removeAdsEvent;
    public static event Action<string, string> showNotificationEvent;
    #endregion

    private void Awake()
    {
        IAPPackageUI.buyIAPEvent += BuyProduct;
        IAPCoinItemUI.buyIAPEvent += BuyProduct;
        IAPShopItem.buyIAPEvent += BuyProduct;
        RemoveAdPopup.buyIAPEvent += BuyProduct;
        IAPShopScreen.fetchLocalizedPriceIAPEvent += FetchLocalizedPrice;
        IAPShopPopup.fetchLocalizedPriceIAPEvent += FetchLocalizedPrice;

        for (int i = 0; i < IAPDataContainer.ProductsData.Length; i++)
        {
            googlePlayStoreProducts[i].Id = IAPDataContainer.ProductsData[i].ProductId;

            // if (IAPDataContainer.ProductsData[i].IsRemoveAd)
            // {
            //     googlePlayStoreProducts[i].ProductType = ProductType.NonConsumable;
            // }
            // else
            // {
            //     googlePlayStoreProducts[i].ProductType = ProductType.Consumable;
            // }

            googlePlayStoreProducts[i].ProductType = ProductType.Consumable;
        }

        Init();

        DontDestroyOnLoad(gameObject);

        GamePersistentVariable.iapInitializeFailReason = "IAP Init FAILED";
    }

    private void OnDestroy()
    {
        IAPPackageUI.buyIAPEvent -= BuyProduct;
        IAPCoinItemUI.buyIAPEvent -= BuyProduct;
        IAPShopItem.buyIAPEvent -= BuyProduct;
        RemoveAdPopup.buyIAPEvent -= BuyProduct;
        IAPShopScreen.fetchLocalizedPriceIAPEvent -= FetchLocalizedPrice;
        IAPShopPopup.fetchLocalizedPriceIAPEvent -= FetchLocalizedPrice;
    }

    private void Init()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

#if UNITY_ANDROID
        foreach (var product in googlePlayStoreProducts)
        {
            builder.AddProduct(product.Id, product.ProductType);
        }
#endif

#if UNITY_ANDROID&& !UNITY_EDITOR
        try
        {
            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass client = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
            AndroidJavaObject adInfo = client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", currentActivity);

            var advertisingID = adInfo.Call<string>("getId").ToString();

            builder.Configure<IGooglePlayConfiguration>().SetObfuscatedAccountId(advertisingID);
        }
        catch (Exception e)
        {
            Debug.Log($"Tracking GetIDFA Android: {e.ToString()}");
        }
#endif

        UnityPurchasing.Initialize(this, builder);
    }

    public void BuyProduct(string productId)
    {
        if (controller != null)
        {
            controller.InitiatePurchase(productId);
        }
        else
        {
            showNotificationEvent?.Invoke(GameConstants.ERROR, GameConstants.PLEASE_TRY_AGAIN);
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
        this.extensions = extensions;

        FetchLocalizedPrice();

        CheckRestoredRemoveAds();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        GamePersistentVariable.iapInitializeFailReason = error.ToString();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        bool validPurchase = true; // Presume valid for platforms with no R.V.

        // Unity IAP's validation logic is only included on these platforms.
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
        // Prepare the validator with the secrets we prepared in the Editor
        // obfuscation window.
        var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
            AppleTangle.Data(), Application.identifier);

        try
        {
            // On Google Play, result has a single product ID.
            // On Apple stores, receipts contain multiple products.
            var result = validator.Validate(e.purchasedProduct.receipt);
            // For informational purposes, we list the receipt(s)
        }
        catch (IAPSecurityException)
        {
            validPurchase = false;
        }
#endif

        if (validPurchase)
        {
            if (e.purchasedProduct.definition.id == GameConstants.REMOVE_AD_ID)
            {
                removeAdPurchasedCompletedEvent?.Invoke();

                UserData.IsRemoveAds = true;
            }
            else
            {
                foreach (var productData in IAPDataContainer.ProductsData)
                {
                    if (productData.ProductId == e.purchasedProduct.definition.id)
                    {
                        // collectCoinEvent?.Invoke(productData.CoinQuantity);

                        userResourcesObserver.ChangeBoosterQuantity(0, productData.AddHoleBoosterQuantity);
                        userResourcesObserver.ChangeBoosterQuantity(1, productData.BreakObjectBoosterQuantity);
                        userResourcesObserver.ChangeBoosterQuantity(2, productData.ClearHolesBoosterQuantity);

                        if (productData.IsRemoveAd)
                        {
                            UserData.IsRemoveAds = true;
                        }

                        int[] boosterQuantites = new int[3] {
                        productData.AddHoleBoosterQuantity,
                        productData.BreakObjectBoosterQuantity,
                        productData.ClearHolesBoosterQuantity
                    };

                        SaferioTracking.TrackIAPEvent(productData.ProductId);
                        SaferioTracking.TrackIAPEventCostCenter(currentLevel.Value, productData.Price, "$", productData.ProductId);

                        showResourcesEarnPopupEvent?.Invoke(productData.CoinQuantity, boosterQuantites, productData.IsRemoveAd);

                        break;
                    }
                }
            }
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureDescription p)
    {
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {

    }

    private void FetchLocalizedPrice()
    {
        List<Product> products = GetAvailableProducts();

        // string productId;
        // string localizedPrice;

        for (int i = 0; i < products.Count; i++)
        {
            string productId = products[i].definition.id;

            string localizedPrice = GetLocalizedPrice(products[i]);

            updatePriceEvent?.Invoke(productId, localizedPrice);
        }
    }

    public List<Product> GetAvailableProducts()
    {
        if (controller != null)
        {
            return new List<Product>(controller.products.all);
        }
        else
        {
            return new List<Product>();
        }
    }

    public string GetLocalizedPrice(Product product)
    {
        var price = product.metadata.localizedPriceString;
        var cultureInfo = new CultureInfo("en-US");

        return price;
    }

    private bool IsProductPurchased(string productId)
    {
        if (controller != null)
        {
            Product product = controller.products.WithID(productId);

            if (product != null && product.hasReceipt)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    private void CheckRestoredRemoveAds()
    {
        UserData.IsRemoveAds = IsProductPurchased(removeAdsProductId);

        if (UserData.IsRemoveAds)
        {
            removeAdsEvent?.Invoke();
        }
    }
}

[Serializable]
public class GooglePlayStoreProduct
{
    [SerializeField] private string id;
    [SerializeField] private ProductType productType;

    public string Id
    {
        get => id;
        set => id = value;
    }

    public ProductType ProductType
    {
        get => productType;
        set => productType = value;
    }
}
