using UnityEngine;

public enum SpaceObjectType
{
    Earth,
    Mars,
    Neptune,
    InnerOortGate,
    OuterOortCloud,
    ProximaCentauri,
    Moon,
    TerrestrialPlanet,
    GasGiant,
    IceGiant,
    DwarfPlanet,
    YellowDwarfStar,
    RedDwarfStar,
    OrangeDwarfStar,
    WhiteDwarfStar,
    BlueGigantStar,
    BlueSupergiantStar,
    RedSupergiantStar,
    NeutronStar,
    BlackHole,
    GlobularCluster,
    SpiralGalaxy,
    EllipticalGalaxy,
    Quasar,
    CosmicHorizon
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

        voyageData.remaningKm = GameConstants.KmPerLightYear * CurveManager.Ins.GetVoyageDistance(voyageData.starPathConfigIndex);

        return voyageData;
    }

    public void SetStarPathConfig(int index)
    {
        if (index < starVoyageContainer.starPathConfigs.Count)
        {
            currentStarPathConfig = starVoyageContainer.starPathConfigs[index];
        }

        if (index + 1 < starVoyageContainer.starPathConfigs.Count)
        {
            nextStarPathConfig = starVoyageContainer.starPathConfigs[index + 1];
        }
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

        return nextStarPathConfig;
    }

    public bool IsLastVoyage(int index)
    {
        return index == starVoyageContainer.starPathConfigs.Count - 1;
    }
}
