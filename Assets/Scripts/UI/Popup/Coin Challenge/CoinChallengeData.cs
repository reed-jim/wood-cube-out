using System;
using UnityEngine;

[Serializable]
public class CoinChallengeData
{
    public long startTimestamp;
    [SerializeField] private int currentMilestone;
    [SerializeField] private int[] eventItemCollectedOfMilstones;
    [SerializeField] private bool[] isMilestonesClaimed;
    public int newGainedItem;
    public bool isCompleted;

    public int TotalMilestone
    {
        get => eventItemCollectedOfMilstones.Length;
    }

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

    public bool IsCompleted()
    {
        // for (int i = 0; i < isMilestonesClaimed.Length; i++)
        // {
        //     if (!isMilestonesClaimed[i])
        //     {
        //         return false;
        //     }
        // }

        // return true;

        return isCompleted;
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
    // [SerializeField] public int prevStarCollected;
    public int newGainedItem;
}
