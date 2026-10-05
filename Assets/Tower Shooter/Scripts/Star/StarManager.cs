using UnityEngine;

public enum SpaceObjectType
{
    Earth,
    Mars,
    Neptune,
    InnerOortGate,
    OuterOortCloud,
    ProximaCentauri
}

public class StarManager : Singleton<StarManager>
{
    public BaseCharacter Mars;

    public StarVoyageContainer starVoyageContainer;

    public StarPathConfig currentStarPathConfig;
    public StarPathConfig nextStarPathConfig;

    public VoyageData GetFirstVoyage()
    {
        VoyageData voyageData = new VoyageData();

        voyageData.starPathConfigIndex = 0;

        voyageData.remaningKm = GameConstants.KmPerLightYear * GetVoyageConfig(voyageData.starPathConfigIndex).distanceInLightyear;

        return voyageData;
    }

    public StarPathConfig GetVoyageConfig(int index)
    {
        if (index < starVoyageContainer.starPathConfigs.Count)
        {
            currentStarPathConfig = starVoyageContainer.starPathConfigs[index];
        }

        if (index + 1 < starVoyageContainer.starPathConfigs.Count)
        {
            nextStarPathConfig = starVoyageContainer.starPathConfigs[index + 1];
        }

        return currentStarPathConfig;
    }

    public bool IsLastVoyage(int index)
    {
        return index == starVoyageContainer.starPathConfigs.Count - 1;
    }
}
