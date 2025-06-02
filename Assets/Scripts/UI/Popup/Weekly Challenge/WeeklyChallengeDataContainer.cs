using UnityEngine;

[CreateAssetMenu(fileName = "WeeklyChallengeDataContainer", menuName = "Scriptable Objects/WeeklyChallengeDataContainer")]
public class WeeklyChallengeDataContainer : ScriptableObject
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
