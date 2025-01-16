using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IAPPackageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinQuantityText;
    [SerializeField] private TMP_Text addHoleQuantityText;
    [SerializeField] private TMP_Text breakObjectQuantityText;
    [SerializeField] private TMP_Text clearHolesQuantityText;
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
