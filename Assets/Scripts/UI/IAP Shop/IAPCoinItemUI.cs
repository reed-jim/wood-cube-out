using TMPro;
using UnityEngine;

public class IAPCoinItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinQuantityText;
    [SerializeField] private TMP_Text priceText;

    [SerializeField] private IAPPackageData _packageData;

    private void Awake()
    {
        Setup(_packageData);
    }

    public void Setup(IAPPackageData data)
    {
        coinQuantityText.text = $"{data.CoinQuantity}";

        priceText.text = $"{data.Price}";
    }
}
