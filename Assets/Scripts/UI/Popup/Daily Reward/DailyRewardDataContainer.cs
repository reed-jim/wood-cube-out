using UnityEngine;

[CreateAssetMenu(fileName = "DailyRewardDataContainer", menuName = "Scriptable Objects/DailyRewardDataContainer")]
public class DailyRewardDataContainer : ScriptableObject
{
    [SerializeField] private DailyRewardData[] items;

    public DailyRewardData[] Items
    {
        get => items;
    }
}
