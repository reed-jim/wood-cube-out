using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StarPathConfig", menuName = "Star/StarPathConfig")]
public class StarPathConfig : ScriptableObject
{
    public string starName;
    public float distance;

    [Header("Reward")]
    public float coinReward;
    public List<Augment> unlockedAugments;
}
