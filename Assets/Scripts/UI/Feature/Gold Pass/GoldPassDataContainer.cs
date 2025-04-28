using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GoldPassDataContainer", menuName = "ScriptableObjects/GoldPassDataContainer")]
public class GoldPassDataContainer : ScriptableObject
{
    [SerializeField] public GoldPassMilestoneData[] goldPassMilestones;
    [SerializeField] public RewardBundleData[] goldPassRewards;

    [SerializeField] public int numDays;

    [SerializeField] public GoldPassData goldPassData;

    public GoldPassMilestoneData CurrentMilestone
    {
        get => goldPassMilestones[goldPassData.passLevel];
    }

    public void Load(GoldPassData savedData)
    {
        goldPassData = savedData;
    }
}

[Serializable]
public class GoldPassData
{
    [SerializeField] public int passLevel;
    [SerializeField] public int keyCollected;

    [SerializeField] public long endTimestamp;
}
