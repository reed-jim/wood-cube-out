using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IAPCoinItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinQuantityText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    [SerializeField] private IAPPackageData _packageData;

    #region EVENT
    public static event Action<string> buyIAPEvent;
    #endregion

    private void Awake()
    {
        SaferioIAPManager.updatePriceEvent += UpdatePrice;

        buyButton.onClick.AddListener(Buy);

        Setup(_packageData);
    }

    void OnDestroy()
    {
        SaferioIAPManager.updatePriceEvent -= UpdatePrice;
    }

    public void Setup(IAPPackageData data)
    {
        coinQuantityText.text = $"{data.CoinQuantity}";

        priceText.text = $"{data.Price}$";
    }

    private void Buy()
    {
        buyIAPEvent?.Invoke(_packageData.ProductId);
    }

    private void UpdatePrice(string productId, string price)
    {
        if (productId == _packageData.ProductId)
        {
            priceText.text = price;
        }
    }
}
