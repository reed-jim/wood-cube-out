using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StarVoyageContainer", menuName = "Star/StarVoyageContainer")]
public class StarVoyageContainer : ScriptableObject
{
    public List<StarPathConfig> starPathConfigs;

    public Material GetPlanetTagMaterial(SpaceObjectType spaceObjectType)
    {
        for (int i = 0; i < starPathConfigs.Count; i++)
        {
            StarPathConfig starPathConfig = starPathConfigs[i];

            if (starPathConfig.spaceObjectType == spaceObjectType)
            {
                return starPathConfig.planetTagMaterial;
            }
        }

        return null;
    }
}
