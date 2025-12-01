using UnityEngine;

[CreateAssetMenu(fileName = "DailyRewardData", menuName = "Scriptable Objects/DailyRewardData")]
public class DailyRewardData : ScriptableObject
{
    public ChestType chestType = ChestType.NONE;
    [SerializeField] private int coinQuantity;
    [SerializeField] private int[] boosterQuantities;
    
    
    public int CoinQuantity
    {
        get => coinQuantity;
        set => coinQuantity = value;
    }

    public int[] BoosterQuantities
    {
        get => boosterQuantities;
        set => boosterQuantities = value;
    }

    
}

public enum ChestType
{
    NONE = 0,
    LEVEL_CHEST = 1,
    STAR_CHEST = 2,
}