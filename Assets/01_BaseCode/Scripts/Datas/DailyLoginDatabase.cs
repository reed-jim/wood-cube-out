using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/DailyLoginDatabase", fileName = "DailyLoginDatabase.asset")]
public class DailyLoginDatabase : SerializedScriptableObject
{
    public DailyLoginItem[] rewardDays;
}

[System.Serializable]
public class DailyLoginItem
{
    public int id;
    public RewardRandom[] rewards;
    public bool isChest;
}