using UnityEngine;

public enum SpaceObjectType
{
    Earth,
    Mars,
    ProximaCentauri
}

public class StarManager : Singleton<StarManager>
{
    public BaseCharacter Mars;

    public StarVoyageContainer starVoyageContainer;

    public StarPathConfig currentStarPathConfig;

    public VoyageData GetFirstVoyage()
    {
        VoyageData voyageData = new VoyageData();

        voyageData.starPathConfigIndex = 0;

        voyageData.remaningKm = GetVoyageConfig(voyageData.starPathConfigIndex).distance;

        return voyageData;
    }

    public StarPathConfig GetVoyageConfig(int index)
    {
        currentStarPathConfig = starVoyageContainer.starPathConfigs[index];

        return currentStarPathConfig;
    }

    public bool IsLastVoyage(int index)
    {
        return index == starVoyageContainer.starPathConfigs.Count - 1;
    }
}
