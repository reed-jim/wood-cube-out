using System;
using UnityEngine;

[Serializable]
public class SpaceRaceData
{
    public bool isStarted;
    public LeaderboardUserData[] spaceRacePlayerGroup;
    [SerializeField] public long endTimestamp;
}

[Serializable]
public class SpaceRacePlayer
{

}
