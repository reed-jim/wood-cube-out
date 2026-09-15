using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VoyageMilestoneConfig", menuName = "Star/VoyageMilestoneConfig")]
public class VoyageMilestoneConfig : ScriptableObject
{
    public List<VoyageMilestone> voyageMilestones;
}

[Serializable]
public class VoyageMilestone
{
    public float percent;

    [Header("Reward")]
    public List<ResourceDataItem> rewards;
}
