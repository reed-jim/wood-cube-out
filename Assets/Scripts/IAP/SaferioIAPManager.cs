using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class SaferioIAPManager : MonoBehaviour, IDetailedStoreListener
{
    [SerializeField] private GooglePlayStoreProduct[] googlePlayStoreProducts;

    private IStoreController controller;
    private IExtensionProvider extensions;

    #region EVENT
    public static event Action iapProductPurchasedCompletedEvent;
    public static event Action removeAdPurchasedCompletedEvent;
    #endregion

    private void Awake()
    {
        IAPShopItem.buyIAPEvent += BuyProduct;
        RemoveAdPopup.buyRemoveAdEvent += BuyProduct;

        Init();

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        IAPShopItem.buyIAPEvent -= BuyProduct;
        RemoveAdPopup.buyRemoveAdEvent -= BuyProduct;
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

        UnityPurchasing.Initialize(this, builder);
    }

    public void BuyProduct(string productId)
    {
        controller.InitiatePurchase(productId);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
        this.extensions = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log(error);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        if (e.purchasedProduct.definition.id == GameConstants.REMOVE_AD_ID)
        {
            removeAdPurchasedCompletedEvent?.Invoke();
        }
        else
        {
            iapProductPurchasedCompletedEvent?.Invoke();
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
        Debug.Log("SAFERIO " + error);
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
    }

    public ProductType ProductType
    {
        get => productType;
    }
}
