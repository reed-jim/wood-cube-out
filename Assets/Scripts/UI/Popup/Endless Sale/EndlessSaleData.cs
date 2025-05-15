using System;
using UnityEngine;

[Serializable]
public class EndlessSaleData
{
    [SerializeField] private int rewardLevel;

    public int RewardLevel
    {
        get => rewardLevel;
        set => rewardLevel = value;
    }
}
