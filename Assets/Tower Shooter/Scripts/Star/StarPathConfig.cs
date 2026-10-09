using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "StarPathConfig", menuName = "Star/StarPathConfig")]
public class StarPathConfig : ScriptableObject
{
    public int id;
    public string starName;
    public SpaceObjectType spaceObjectType;
    public Sprite picture;
    public Material planetMaterial;
    public Material planetTagMaterial;
    public string description;

    [Header("Milstone")]
    public VoyageMilestoneConfig voyageMilestoneConfig;

    [Header("Reward")]
    public List<ResourceDataItem> rewards;
    public List<Augment> unlockedAugments;
}
