using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GoldPassDataContainer", menuName = "ScriptableObjects/GoldPassDataContainer")]
public class GoldPassDataContainer : ScriptableObject
{
    [SerializeField] public GoldPassMilestoneData[] goldPassMilestones;
    [SerializeField] public RewardBundleData[] goldPassRewards;

    [SerializeField] public int numDays;
    [SerializeField] public long endTimestamp;
}
