using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

public class SaferioIAPManager : MonoBehaviour, IStoreListener
{
    [SerializeField] private GooglePlayStoreProduct[] googlePlayStoreProducts;
    [SerializeField] private IAPDataContainer IAPDataContainer;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private IntVariable currentLevel;

    [SerializeField] private string removeAdsProductId;

    private IStoreController controller;
    private IAppleExtensions m_AppleExtensions;
    private ConfigurationBuilder builder;

    #region EVENT
    public static event Action iapProductPurchasedCompletedEvent;
    public static event Action removeAdPurchasedCompletedEvent;
    public static event Action unlockScrewBoxesPurchasedCompletedEvent;
    public static event Action<int> collectCoinEvent;
    public static event Action<string, string> updatePriceEvent;
    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;
    public static event Action<string> buyIAPPackageEvent;
    public static event Action removeAdsEvent;
    public static event Action<string, string> showNotificationEvent;
    #endregion

    WaitForSeconds wait = new WaitForSeconds(5);

    private void Awake()
    {
        IAPPackageUI.buyIAPEvent += BuyProduct;
        IAPCoinItemUI.buyIAPEvent += BuyProduct;
        IAPShopItem.buyIAPEvent += BuyProduct;
        RemoveAdPopup.buyIAPEvent += BuyProduct;
        IAPReviewPopup.buyIAPEvent += BuyProduct;
        IAPSuggestionPopup.buyIAPEvent += BuyProduct;
        UnlockAllScrewBoxesForeverPopup.buyIAPEvent += BuyProduct;
        DailyOfferPopup.buyIAPEvent += BuyProduct;
        UnderPopupSaleIAPPackUI.buyIAPEvent += BuyProduct;
        IAPShopScreen.fetchLocalizedPriceIAPEvent += FetchLocalizedPrice;
        IAPShopPopup.fetchLocalizedPriceIAPEvent += FetchLocalizedPrice;
        WinPopup.fetchLocalizedPriceIAPEvent += FetchLocalizedPrice;
        IAPReviewPopup.requestFetchIAPPrice += FetchLocalizedPrice;
        IAPSuggestionPopup.requestFetchIAPPrice += FetchLocalizedPrice;
        UnlockAllScrewBoxesForeverPopup.requestFetchIAPPrice += FetchLocalizedPrice;
        DailyOfferPopup.requestFetchIAPPrice += FetchLocalizedPrice;
        PackageIAPUnderPopupSwiper.requestFetchIAPPrice += FetchLocalizedPrice;
        SettingPopup.restorePurchasesEvent += RestorePurchases;

        for (int i = 0; i < IAPDataContainer.ProductsData.Length; i++)
        {
            googlePlayStoreProducts[i].Id = IAPDataContainer.ProductsData[i].ProductId;
            googlePlayStoreProducts[i].ProductType = ProductType.Consumable;
        }

        //StartCoroutine(DelayInit());

        DontDestroyOnLoad(gameObject);

        GamePersistentVariable.iapInitializeFailReason = "IAP Init FAILED";
        Init();
    }

    private void OnDestroy()
    {
        IAPPackageUI.buyIAPEvent -= BuyProduct;
        IAPCoinItemUI.buyIAPEvent -= BuyProduct;
        IAPShopItem.buyIAPEvent -= BuyProduct;
        RemoveAdPopup.buyIAPEvent -= BuyProduct;
        IAPReviewPopup.buyIAPEvent -= BuyProduct;
        IAPSuggestionPopup.buyIAPEvent -= BuyProduct;
        UnlockAllScrewBoxesForeverPopup.buyIAPEvent -= BuyProduct;
        DailyOfferPopup.buyIAPEvent -= BuyProduct;
        UnderPopupSaleIAPPackUI.buyIAPEvent -= BuyProduct;
        IAPShopScreen.fetchLocalizedPriceIAPEvent -= FetchLocalizedPrice;
        IAPShopPopup.fetchLocalizedPriceIAPEvent -= FetchLocalizedPrice;
        WinPopup.fetchLocalizedPriceIAPEvent -= FetchLocalizedPrice;
        IAPReviewPopup.requestFetchIAPPrice -= FetchLocalizedPrice;
        IAPSuggestionPopup.requestFetchIAPPrice -= FetchLocalizedPrice;
        UnlockAllScrewBoxesForeverPopup.requestFetchIAPPrice -= FetchLocalizedPrice;
        DailyOfferPopup.requestFetchIAPPrice -= FetchLocalizedPrice;
        PackageIAPUnderPopupSwiper.requestFetchIAPPrice -= FetchLocalizedPrice;
        SettingPopup.restorePurchasesEvent -= RestorePurchases;
    }

    private IEnumerator DelayInit()
    {
        yield return new WaitForSeconds(6);

        Init();
    }

    private void Init()
    {
        builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // builder.Configure<IAppleConfiguration>().
        //     SetApplePromotionalPurchaseInterceptorCallback(OnPromotionalPurchase);

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
        StartCoroutine(AutoInit());
    }


    private IEnumerator AutoInit()
    {
        while (true)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                yield return wait;
            }
            else
            {
                if (IsInitialized())
                {
                    yield break;
                }
                //if (!isInited)
                {
                    UnityPurchasing.Initialize(this, builder);
                    //isInited = true;
                }
                yield return wait;
            }
        }
    }

    private bool IsInitialized()
    {
        return controller != null && m_AppleExtensions != null;
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
        if (IsInitialized())
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
        m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();
        m_AppleExtensions.RegisterPurchaseDeferredListener(OnDeferred);
        // Restore transactions
        // m_AppleExtensions.RestoreTransactions((result, error) =>
        // {
        //     if (result)
        //     {
        //         // This does not mean anything was restored,
        //         // merely that the restoration process succeeded.
        //     }
        //     else
        //     {
        //         // Restoration failed. `error` contains the failure reason.
        //     }
        // });

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

    private void OnDeferred(Product item)
    {
        //FunctionHelper.ShowDebug("Purchase deferred: " + item.definition.id);
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        GamePersistentVariable.iapInitializeFailReason = error.ToString();
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            showNotificationEvent?.Invoke(GameConstants.NOTIFICATION, "RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log("RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = m_AppleExtensions;
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
            // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result, str) =>
            {
                // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                // no purchases are available to be restored.
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore. " + str);
                if (!result)
                {
                    showNotificationEvent?.Invoke(GameConstants.NOTIFICATION, "Restore purchases successfully");
                }
                else
                {
                    showNotificationEvent?.Invoke(GameConstants.NOTIFICATION, str);
                }
            });
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            showNotificationEvent?.Invoke(GameConstants.NOTIFICATION, "RestorePurchases FAIL. Not supported on this platform");
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }


       
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        if (e.purchasedProduct.definition.id == GameConstants.REMOVE_AD_ID)
        {
            removeAdPurchasedCompletedEvent?.Invoke();

            UserData.IsRemoveAds = true;
            GameController.Instance.useProfile.IsRemoveAds = true;
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
                            multiplier * productData.AddHoleBoosterQuantity,
                            multiplier * productData.BreakObjectBoosterQuantity,
                            multiplier * productData.ClearHolesBoosterQuantity,
                            multiplier * productData.ClearOneColorBoosterQuantity
                        };

                    userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
                    userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
                    userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);
                    userResourcesObserver.ChangeBoosterQuantity(3, boosterQuantites[3]);

                    userResourcesObserver.ChangeCoin(coinQuantity);

                    if (productData.IsRemoveAd)
                    {
                        //GameController.Instance.admobAds.Hi();
                        GameController.Instance.admobAds.DestroyBanner();

                        UserData.IsRemoveAds = true;
                        GameController.Instance.useProfile.IsRemoveAds = true;
                    }

                    if (productData.IsUnlockAllScrewBoxes)
                    {
                        DataUtility.Save(GameConstants.IS_UNLOCKED_ALL_SCREW_BOXES, true);

                        GamePersistentVariable.isUnlockedAllScrewBoxes = true;

                        GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES = 4;

                        unlockScrewBoxesPurchasedCompletedEvent?.Invoke();
                    }

                    // // Simulate DELAY on iOS    
                    // Tween.Delay(3).OnComplete(() =>
                    // {
                    //     showResourcesEarnPopupEvent?.Invoke(productData.CoinQuantity, boosterQuantites, productData.IsRemoveAd);
                    // });

                    buyIAPPackageEvent?.Invoke(productData.ProductId);
                    showResourcesEarnPopupEvent?.Invoke(coinQuantity, boosterQuantites, productData.IsRemoveAd);

                    SaferioTracking.TrackIAPEvent(productData.ProductId);
                    AnalyticsController.AppsFlyerPurchaseEvent(e.purchasedProduct);

                    try
                    {
                        new FInAppLog(e.purchasedProduct.definition.id, e.purchasedProduct.metadata.localizedPrice, e.purchasedProduct.metadata.isoCurrencyCode, "shop", e.purchasedProduct.transactionID, "");
                    }
                    catch (Exception f)
                    {
                        Debug.LogError(f.Message);
                    }

                    break;
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
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                //Init();
            }

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
