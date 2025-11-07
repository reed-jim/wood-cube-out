using UnityEngine;

[CreateAssetMenu(menuName = "WoodCube/IAPPackageData")]
public class IAPPackageData : ScriptableObject
{
    [SerializeField] private string productId;
    [SerializeField] private string productName;
    [SerializeField] private int coinQuantity;
    [SerializeField] private int addHoleBoosterQuantity;
    [SerializeField] private int breakObjectBoosterQuantity;
    [SerializeField] private int clearHolesBoosterQuantity;
    [SerializeField] private bool isRemoveAd;
    [SerializeField] private bool isUnlockAllScrewBoxes;
    [SerializeField] private float price;

    public string ProductId
    {
        get => productId;
    }

    public string ProductName
    {
        get => productName;
    }

    public int CoinQuantity
    {
        get => coinQuantity;
    }

    public int AddHoleBoosterQuantity
    {
        get => addHoleBoosterQuantity;
    }

    public int BreakObjectBoosterQuantity
    {
        get => breakObjectBoosterQuantity;
    }

    public int ClearHolesBoosterQuantity
    {
        get => clearHolesBoosterQuantity;
    }

    public bool IsRemoveAd
    {
        get => isRemoveAd;
    }

    public bool IsUnlockAllScrewBoxes
    {
        get => isUnlockAllScrewBoxes;
    }

    public float Price
    {
        get => price;
    }
}
