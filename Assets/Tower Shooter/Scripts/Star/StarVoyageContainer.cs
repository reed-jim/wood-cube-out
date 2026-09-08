using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StarVoyageContainer", menuName = "Star/StarVoyageContainer")]
public class StarVoyageContainer : ScriptableObject
{
    public List<StarPathConfig> starPathConfigs;
}
