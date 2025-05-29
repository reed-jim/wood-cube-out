using System;
using UnityEngine;

[Serializable]
public class CoinChallengeData
{
    [SerializeField] public int gemCollected;
    public long endTimestamp;
}

[Serializable]
public class StarLevelData
{
    [SerializeField] public int starCollected;
}
