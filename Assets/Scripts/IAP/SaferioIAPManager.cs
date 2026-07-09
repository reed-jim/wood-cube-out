using System;
using UnityEngine;

public delegate void CollectCoinDelegate(int amount);
public delegate void UpdatePriceDelegate(string id, string price);
public delegate void ShowResourcesEarnPopupDelegate(int arg1, int[] arg2, bool arg3);
public delegate void ShowNotificationDelegate(string title, string body);

public class SaferioIAPManager : MonoBehaviour
{
    #region EVENT
    public static event Action iapProductPurchasedCompletedEvent;
    public static event Action removeAdPurchasedCompletedEvent;
    public static event Action unlockScrewBoxesPurchasedCompletedEvent;
    public static event CollectCoinDelegate collectCoinEvent;
    public static event UpdatePriceDelegate updatePriceEvent;
    public static event ShowResourcesEarnPopupDelegate showResourcesEarnPopupEvent;
    public static event Action removeAdsEvent;
    public static event ShowNotificationDelegate showNotificationEvent;
    #endregion

    public static string LastPurchasedRemoveAdsProductId { get; private set; }

    private void Awake()
    {
    }
}
