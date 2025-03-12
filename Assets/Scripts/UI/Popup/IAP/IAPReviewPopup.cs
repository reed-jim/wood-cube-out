using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IAPReviewPopup : BasePopup
{
    [SerializeField] private TMP_Text packNameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image icon;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [Header("DATA")]
    [SerializeField] private IAPPackageData[] packagesIAP;
    [SerializeField] private Sprite[] packIcons;

    #region PRIVATE FIELD
    private int _currentPackIndex;
    private Dictionary<string, string> localizedPrices;
    #endregion

    public static event Action<string> buyIAPEvent;
    public static event Action requestFetchIAPPrice;
    public static event Action<string, string> showNotificationEvent;


    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;

    protected override void MoreActionInAwake()
    {
        localizedPrices = new Dictionary<string, string>();

        for (int i = 0; i < packagesIAP.Length; i++)
        {
            if (!localizedPrices.ContainsKey(packagesIAP[i].ProductId))
            {
                localizedPrices.Add(packagesIAP[i].ProductId, $"${packagesIAP[i].Price}");
            }
        }

        requestFetchIAPPrice?.Invoke();

        ShowPack(0);
    }

    protected override void RegisterMoreEvent()
    {
        SaferioIAPManager.updatePriceEvent += UpdatePrice;

        buyButton.onClick.AddListener(ReviewBuy);
        nextButton.onClick.AddListener(Next);
        prevButton.onClick.AddListener(Prev);
    }

    protected override void UnregisterMoreEvent()
    {
        SaferioIAPManager.updatePriceEvent -= UpdatePrice;
    }

    private void ShowPack(int packIndex)
    {
        packNameText.text = packagesIAP[packIndex].ProductName;
        priceText.text = localizedPrices[packagesIAP[packIndex].ProductId];
        icon.sprite = packIcons[packIndex];

        UIUtil.SetSizeKeepRatioX(icon, 0.3f * container.sizeDelta.x);
    }

    private void ReviewBuy()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            showNotificationEvent?.Invoke(GameConstants.NO_INTERNET, GameConstants.PLEASE_CHECK_YOUR_INTERNET_CONNECTION);
        }
        else
        {
            buyIAPEvent?.Invoke(packagesIAP[_currentPackIndex].ProductId);
        }

        // DEBUG
        // showResourcesEarnPopupEvent?.Invoke(7500, new int[3], false);
    }

    private void Next()
    {
        if (_currentPackIndex + 1 >= packagesIAP.Length)
        {
            _currentPackIndex = 0;
        }
        else
        {
            _currentPackIndex++;
        }

        ShowPack(_currentPackIndex);
    }

    private void Prev()
    {
        if (_currentPackIndex - 1 < 0)
        {
            _currentPackIndex = packagesIAP.Length - 1;
        }
        else
        {
            _currentPackIndex--;
        }

        ShowPack(_currentPackIndex);
    }

    private void UpdatePrice(string productId, string price)
    {
        if (localizedPrices == null)
        {
            localizedPrices = new Dictionary<string, string>();
        }

        if (!localizedPrices.ContainsKey(productId))
        {
            localizedPrices.Add(productId, price);
        }
        else
        {
            localizedPrices[productId] = price;
        }

        if (packagesIAP[_currentPackIndex].ProductId == productId)
        {
            priceText.text = localizedPrices[productId];
        }
    }
}
