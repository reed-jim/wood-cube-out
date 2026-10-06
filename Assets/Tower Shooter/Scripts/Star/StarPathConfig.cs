using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "StarPathConfig", menuName = "Star/StarPathConfig")]
public class StarPathConfig : ScriptableObject
{
    public int id;
    public string starName;
    public double distanceInLightyear;
    public SpaceObjectType spaceObjectType;
    public Sprite picture;
    public Material planetMaterial;
    public string description;

    [Header("Milstone")]
    public VoyageMilestoneConfig voyageMilestoneConfig;

    [Header("Reward")]
    public List<ResourceDataItem> rewards;
    public List<Augment> unlockedAugments;
}
