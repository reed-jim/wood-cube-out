using System;
using UnityEngine;

[Serializable]
public class SpaceRaceData
{
    public bool isStarted;
    public bool isEnded;
    public SpaceRacePlayerData[] spaceRacePlayerGroup;
    [SerializeField] public long startTimestamp;
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
