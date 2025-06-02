using System;
using UnityEngine;

[Serializable]
public class CoinChallengeData
{
    public long endTimestamp;
    [SerializeField] private int currentMilestone;
    [SerializeField] private int[] eventItemCollectedOfMilstones;
    [SerializeField] private bool[] isMilestonesClaimed;

    public int CurrentMilestone
    {
        get => currentMilestone;
        set => currentMilestone = value;
    }

    public int[] EventItemCollectedOfMilstones
    {
        get => eventItemCollectedOfMilstones;
        set => eventItemCollectedOfMilstones = value;
    }

    public bool[] IsMilestonesClaimed
    {
        get => isMilestonesClaimed;
        set => isMilestonesClaimed = value;
    }

    public CoinChallengeData()
    {
        eventItemCollectedOfMilstones = new int[9];
        isMilestonesClaimed = new bool[9];
    }
}

[Serializable]
public class StarLevelData
{
    [SerializeField] public int starCollected;
    [SerializeField] public int prevStarCollected;
}
