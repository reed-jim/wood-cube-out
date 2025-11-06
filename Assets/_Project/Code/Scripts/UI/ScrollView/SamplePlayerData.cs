using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "SamplePlayerData", menuName = "ScriptableObjects/SamplePlayerData", order = 4)]
public class SamplePlayerData : ScriptableObject
{
    public List<PlayerData1> PlayerDatas;
}

[Serializable]
public class PlayerData1
{
    public string name;
    public Rank rank;
    public Sprite avatar;
    public int level;
}

public enum Rank
{
    COPPER,
    SILVER,
    GOLD,
    PLATINUM,
    DIAMOND,
    MASTER,
    CHALLENGER
}
