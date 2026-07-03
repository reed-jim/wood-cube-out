using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using PrimeTween;
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
    private IAppleExtensions m_AppleExtensions;

    // True once OnInitialized has fired with a valid controller
    private bool _isInitialized;
    // Prevents multiple simultaneous internet-monitor coroutines
    private bool _isMonitoringInternet;

    #region EVENT
    public static event Action iapProductPurchasedCompletedEvent;
    public static event Action removeAdPurchasedCompletedEvent;
    public static event Action unlockScrewBoxesPurchasedCompletedEvent;
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
        IAPReviewPopup.buyIAPEvent += BuyProduct;
        //IAPSuggestionPopup.buyIAPEvent += BuyProduct;
        UnlockAllScrewBoxesForeverPopup.buyIAPEvent += BuyProduct;
        DailyOfferPopup.buyIAPEvent += BuyProduct;
        UnderPopupSaleIAPPackUI.buyIAPEvent += BuyProduct;
        IAPShopScreen.fetchLocalizedPriceIAPEvent += FetchLocalizedPrice;
        IAPShopPopup.fetchLocalizedPriceIAPEvent += FetchLocalizedPrice;
        IAPReviewPopup.requestFetchIAPPrice += FetchLocalizedPrice;
        //IAPSuggestionPopup.requestFetchIAPPrice += FetchLocalizedPrice;
        UnlockAllScrewBoxesForeverPopup.requestFetchIAPPrice += FetchLocalizedPrice;
        DailyOfferPopup.requestFetchIAPPrice += FetchLocalizedPrice;
        PackageIAPUnderPopupSwiper.requestFetchIAPPrice += FetchLocalizedPrice;

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

        StartCoroutine(DelayInit());

        DontDestroyOnLoad(gameObject);

        GamePersistentVariable.iapInitializeFailReason = "IAP Init FAILED";
    }

    private void OnDestroy()
    {
        IAPPackageUI.buyIAPEvent -= BuyProduct;
        IAPCoinItemUI.buyIAPEvent -= BuyProduct;
        IAPShopItem.buyIAPEvent -= BuyProduct;
        RemoveAdPopup.buyIAPEvent -= BuyProduct;
        IAPReviewPopup.buyIAPEvent -= BuyProduct;
        //IAPSuggestionPopup.buyIAPEvent -= BuyProduct;
        UnlockAllScrewBoxesForeverPopup.buyIAPEvent -= BuyProduct;
        DailyOfferPopup.buyIAPEvent -= BuyProduct;
        UnderPopupSaleIAPPackUI.buyIAPEvent -= BuyProduct;
        IAPShopScreen.fetchLocalizedPriceIAPEvent -= FetchLocalizedPrice;
        IAPShopPopup.fetchLocalizedPriceIAPEvent -= FetchLocalizedPrice;
        IAPReviewPopup.requestFetchIAPPrice -= FetchLocalizedPrice;
        //IAPSuggestionPopup.requestFetchIAPPrice -= FetchLocalizedPrice;
        UnlockAllScrewBoxesForeverPopup.requestFetchIAPPrice -= FetchLocalizedPrice;
        DailyOfferPopup.requestFetchIAPPrice -= FetchLocalizedPrice;
        PackageIAPUnderPopupSwiper.requestFetchIAPPrice -= FetchLocalizedPrice;
    }

    private IEnumerator DelayInit()
    {
        yield return new WaitForSeconds(6);

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            Init();
        }
        else
        {
            // No internet at startup — wait for connection then init
            Debug.Log("[SaferioIAP] No internet at startup. Will retry IAP init when connection is available.");
            StartInternetMonitor();
        }
    }

    private void Init()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.Configure<IAppleConfiguration>().
            SetApplePromotionalPurchaseInterceptorCallback(OnPromotionalPurchase);

        foreach (var product in googlePlayStoreProducts)
        {
            builder.AddProduct(product.Id, product.ProductType);
        }

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

    // PROMOTIONAL
    private void OnPromotionalPurchase(Product item)
    {
        Debug.Log("Attempted promotional purchase: " + item.definition.id);
        // Promotional purchase has been detected.
        // Handle this event by, e.g. presenting a parental gate.
        // Here, for demonstration purposes only, we will wait five seconds before continuing
        // the purchase.
        StartCoroutine(ContinuePromotionalPurchases());
    }

    private IEnumerator ContinuePromotionalPurchases()
    {
        Debug.Log("Continuing promotional purchases in 5 seconds");
        yield return new WaitForSeconds(5);
        Debug.Log("Continuing promotional purchases now");
        m_AppleExtensions.ContinuePromotionalPurchases(); // iOS and tvOS only
    }
    //

    public void BuyProduct(string productId)
    {
        if (controller != null)
        {
            SaferioTracking.TrackStartIAPEvent(productId);

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
        _isInitialized = true;
        _isMonitoringInternet = false; // stop any pending monitor

        m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();

        // Restore transactions
        m_AppleExtensions.RestoreTransactions((result, error) =>
        {
            if (result)
            {
                // This does not mean anything was restored,
                // merely that the restoration process succeeded.
            }
            else
            {
                // Restoration failed. `error` contains the failure reason.
            }
        });

        foreach (var item in controller.products.all)
        {
            if (item.availableToPurchase)
            {
                // Set all these products to be visible in the user's App Store
                m_AppleExtensions.SetStorePromotionVisibility(item, AppleStorePromotionVisibility.Show);
            }
        }

        FetchLocalizedPrice();

        CheckRestoredRemoveAds();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        GamePersistentVariable.iapInitializeFailReason = error.ToString();
        Debug.LogWarning($"[SaferioIAP] IAP init failed: {error}. Will retry when internet is available.");
        // Init failed (possibly no internet) — monitor and retry automatically
        StartInternetMonitor();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        bool validPurchase = true; // Presume valid for platforms with no R.V.

#if !UNITY_EDITOR
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
        // Choose one Apple certificate. NOTE AppleStoreKitTestTangle requires
        // the active Xcode Scheme set to use a StoreKit Configuration file.
        // Here we use a symbol, defined either in code or Project Settings >
        // Player > Scripting Define Symbols, to choose which Apple IAP system
        // we intend to test with in Xcode, next.

#if !DEBUG_STOREKIT_TEST
        var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
            AppleTangle.Data(), Application.identifier);
#else
            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
                AppleStoreKitTestTangle.Data(), Application.identifier);
#endif
#endif

        try
        {
            validator.Validate(e.purchasedProduct.receipt);
        }
        catch (IAPSecurityException exception)
        {
            SaferioTracking.TrackInvalidIAPEvent(exception.Message);

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

                        int multiplier = 1 + GamePersistentVariable.IAPAddition;

                        int coinQuantity = multiplier * productData.CoinQuantity;

                        int[] boosterQuantites = new int[4] {
                            multiplier * productData.AddHintBoosterQuantity,
                            multiplier * productData.AddShuffleBoosterQuantity,
                            multiplier * productData.AddFreezeBoosterQuantity,
                            multiplier * productData.AddUnlockBoosterQuantity
                        };

                        userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
                        userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
                        userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);
                        userResourcesObserver.ChangeBoosterQuantity(3, boosterQuantites[3]);


                        userResourcesObserver.ChangeCoin(coinQuantity);

                        if (productData.IsRemoveAd)
                        {
                            AdmobAdsMax.Instance.HideBanner();
                            AdmobAdsMax.Instance.DestroyBanner();

                            UserData.IsRemoveAds = true;
                        }

                        if (productData.IsUnlockAllScrewBoxes)
                        {
                            DataUtility.Save(GameConstants.IS_UNLOCKED_ALL_SCREW_BOXES, true);

                            GamePersistentVariable.isUnlockedAllScrewBoxes = true;

                            GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES = 4;

                            unlockScrewBoxesPurchasedCompletedEvent?.Invoke();
                        }

                        SaferioTracking.TrackIAPEvent(productData.ProductId);
                        SaferioTracking.TrackIAPEventCostCenter(currentLevel.Value, productData.Price, "$", productData.ProductId);
                        SaferioTracking.TrackIAPCompleteEventAppsflyer($"{productData.Price}");

                        // // Simulate DELAY on iOS    
                        // Tween.Delay(3).OnComplete(() =>
                        // {
                        //     showResourcesEarnPopupEvent?.Invoke(productData.CoinQuantity, boosterQuantites, productData.IsRemoveAd);
                        // });

                        showResourcesEarnPopupEvent?.Invoke(coinQuantity, boosterQuantites, productData.IsRemoveAd);

                        break;
                    }
                }
            }
        }


        if (GamePersistentVariable.IAPAddition > 0)
        {
            DataUtility.Save(GameConstants.IS_BOUGHT_HOT_DEAL, true);

            GamePersistentVariable.IAPAddition = 0;
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {

    }

    public void OnPurchaseFailed(Product i, PurchaseFailureDescription p)
    {
        SaferioTracking.TrackInvalidIAPEvent($"{p.reason}");
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
            // controller is null — IAP hasn't initialized yet
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                // Internet is available now: trigger Init if not already monitoring/initializing
                StartInternetMonitor();
            }

            return new List<Product>();
        }
    }

    /// <summary>
    /// Starts the internet monitor coroutine if it isn't already running.
    /// The monitor waits for internet connectivity then calls Init().
    /// Once Init succeeds (OnInitialized fires), the monitor stops and
    /// FetchLocalizedPrice refreshes all shop UI with localized prices.
    /// </summary>
    private void StartInternetMonitor()
    {
        if (_isInitialized || _isMonitoringInternet) return;
        _isMonitoringInternet = true;
        StartCoroutine(MonitorInternetAndRetryInit());
    }

    private IEnumerator MonitorInternetAndRetryInit()
    {
        Debug.Log("[SaferioIAP] Monitoring internet for IAP re-init...");

        // Wait until internet becomes available
        while (Application.internetReachability == NetworkReachability.NotReachable)
        {
            yield return new WaitForSecondsRealtime(3f);
        }

        if (_isInitialized)
        {
            // Already initialized by another path — just refresh prices
            _isMonitoringInternet = false;
            FetchLocalizedPrice();
            yield break;
        }

        Debug.Log("[SaferioIAP] Internet restored. Retrying IAP initialization...");
        Init();

        // Wait for OnInitialized (or failure) — _isMonitoringInternet is cleared in OnInitialized
        // If Init fails again, OnInitializeFailed will call StartInternetMonitor() to retry
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
        // if (!UserData.IsRemoveAds)
        // {
        //     UserData.IsRemoveAds = IsProductPurchased(removeAdsProductId);
        // }

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
