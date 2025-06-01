using System;
using UnityEngine;

[Serializable]
public class WeeklyChallengeItemData
{
    [SerializeField] private bool isDone;
    [SerializeField] private bool isClaimed;

    public bool IsDone
    {
        get => isDone;
        set => isDone = value;
    }

    public bool IsClaimed
    {
        get => isClaimed;
        set => isClaimed = value;
    }
}

[Serializable]
public class WeeklyChallengeData
{
    [SerializeField] private WeeklyChallengeItemData[] items;
    [SerializeField] private int currentChallengeIndex;
    [SerializeField] private int eventItemCollected;
    public long startTimestamp;

    public WeeklyChallengeItemData[] Items
    {
        get => items;
        set => items = value;
    }

    public int CurrentChallengeIndex
    {
        get => currentChallengeIndex;
        set => currentChallengeIndex = value;
    }

    public int EventItemCollected
    {
        get => eventItemCollected;
        set => eventItemCollected = value;
    }

    public bool IsMilestoneClaimable(int milestoneIndex)
    {
        return milestoneIndex < currentChallengeIndex && !items[milestoneIndex].IsClaimed;
    }
}
