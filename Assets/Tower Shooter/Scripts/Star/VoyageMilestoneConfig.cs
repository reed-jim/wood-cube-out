using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VoyageMilestoneConfig", menuName = "Star/VoyageMilestoneConfig")]
public class VoyageMilestoneConfig : ScriptableObject
{
    public string startLocation;
    public string endLocation;
    public List<VoyageMilestone> voyageMilestones;
}

[Serializable]
public class VoyageMilestone
{
    public float percent;
    public bool isStart;
    public bool isEnd;

    [Header("Reward")]
    public List<ResourceDataItem> rewards;
}
