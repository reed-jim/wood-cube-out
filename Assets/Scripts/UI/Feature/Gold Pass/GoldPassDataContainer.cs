using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GoldPassDataContainer", menuName = "ScriptableObjects/GoldPassDataContainer")]
public class GoldPassDataContainer : ScriptableObject
{
    [SerializeField] public GoldPassMilestoneData[] goldPassMilestones;
    [SerializeField] public DailyRewardData[] goldPassRewards;

    [SerializeField] public int numDays;

    // [SerializeField] public GoldPassData goldPassData;

    public GoldPassMilestoneData CurrentMilestone
    {
        get => goldPassMilestones[GamePersistentVariable.goldPassData.passLevel];
    }

    // public void Load(GoldPassData savedData)
    // {
    //     goldPassData = savedData;
    // }
}

[Serializable]
public class GoldPassData
{
    public bool isIntroDone;
    [SerializeField] public bool isGoldPass;
    [SerializeField] public int passLevel;
    [SerializeField] public int keyCollected;
    [SerializeField] public long endTimestamp;
    [SerializeField] public GoldPassMilestoneData[] goldPassMilestones;
}
