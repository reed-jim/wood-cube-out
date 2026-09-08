using UnityEngine;

public class StarManager : Singleton<StarManager>
{
    public BaseCharacter Mars;

    public StarVoyageContainer starVoyageContainer;

    public VoyageData GetFirstVoyage()
    {
        VoyageData voyageData = new VoyageData();

        voyageData.starPathConfigIndex = 0;

        voyageData.remaningKm = GetVoyageConfig(voyageData.starPathConfigIndex).distance;

        return voyageData;
    }

    public StarPathConfig GetVoyageConfig(int index)
    {
        return starVoyageContainer.starPathConfigs[index];
    }
}
