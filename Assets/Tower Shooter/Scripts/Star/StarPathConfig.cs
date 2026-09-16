using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StarPathConfig", menuName = "Star/StarPathConfig")]
public class StarPathConfig : ScriptableObject
{
    public string starName;
    public double distanceInLightyear;
    public SpaceObjectType spaceObjectType;

    [Header("Milstone")]
    public VoyageMilestoneConfig voyageMilestoneConfig;

    [Header("Reward")]
    public List<ResourceDataItem> rewards;
    public List<Augment> unlockedAugments;
}
