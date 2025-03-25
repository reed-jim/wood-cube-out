using UnityEngine;

[CreateAssetMenu(fileName = "DailyRewardData", menuName = "Scriptable Objects/DailyRewardData")]
public class DailyRewardData : ScriptableObject
{
    [SerializeField] private int coinQuantity;
    [SerializeField] private int[] boosterQuantities;

    public int CoinQuantity
    {
        get => coinQuantity;
    }

    public int[] BoosterQuantities
    {
        get => boosterQuantities;
    }
}
