using UnityEngine;

[CreateAssetMenu(fileName = "CoinChallengeConfigContainer", menuName = "Scriptable Objects/CoinChallengeConfigContainer")]
public class CoinChallengeConfigContainer : ScriptableObject
{
    [SerializeField] private int[] requiredItems;

    [SerializeField] private DailyRewardData[] rewards;

    public int[] RequiredItems
    {
        get => requiredItems;
    }

    public DailyRewardData[] Rewards
    {
        get => rewards;
    }
}
