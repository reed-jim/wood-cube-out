using System;
using UnityEngine;

[Serializable]
public class SpaceRaceData
{
    public bool isStarted;
    public SpaceRacePlayerData[] spaceRacePlayerGroup;
    [SerializeField] public long endTimestamp;
    [SerializeField] public long nextTimestampCheatEnemyScore;
}

[Serializable]
public class SpaceRacePlayerData
{
    public string id;
    public string name;
    public int scoreSinceRaceBegin;
    public bool isSelf;
}
