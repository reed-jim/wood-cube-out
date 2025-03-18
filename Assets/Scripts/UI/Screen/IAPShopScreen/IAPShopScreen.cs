using System;
using UnityEngine;
using UnityEngine.UI;

public class IAPShopScreen : UIScreen
{
    public static event Action fetchLocalizedPriceIAPEvent;

    // [SerializeField] private IAPDataContainer IAPDataContainer;
    // [SerializeField] private UserResourcesObserver userResourcesObserver;

    // public static event Action<int> collectCoinEvent;

    // private void OnIAPPurchasedCompleted(string productId)
    // {
    //     foreach (var productData in IAPDataContainer.ProductsData)
    //     {
    //         if (productData.ProductId == productId)
    //         {
    //             collectCoinEvent?.Invoke(productData.CoinQuantity);

    //             break;
    //         }
    //     }
    // }

    protected override void Show()
    {
        base.Show();

        fetchLocalizedPriceIAPEvent?.Invoke();
    }

    // void Start()
    // {
    //     fetchLocalizedPriceIAPEvent?.Invoke();
    // }

    // protected override void MoreActionInAwake()
    // {
    //     fetchLocalizedPriceIAPEvent?.Invoke();
    // }
}
