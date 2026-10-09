using UnityEngine;

public enum SpaceObjectType
{
    Earth,
    TheMoon,
    Venus,
    Mars,
    Ceres,
    Jupiter,
    Saturn,
    Uranus,
    Neptune,
    Pluto,
    Eris,
    ProximaCentauri,
    ProximaCentaurib,
    AlphaCentauriA,
    AlphaCentauriB,
    BarnardsStar,
    Wolf359,
    Lalande21185,
    SiriusA,
    SiriusB,
    Ross154,
    EpsilonEridani,
    ProcyonA,
    SixtyOneCygniA,
    TauCeti,
    TauCetie,
    KapteynsStar,
    Groombridge34A,
    EpsilonIndiA,
    TeegardensStar,
    Teegardenb,
    TZArietis,
    Gl832,
    Gl832c,
    Altair,
    Vega,
    Pollux,
    Arcturus,
    CapellaA,
    Aldebaran,
    Regulus,
    Spica,
    Antares,
    Betelgeuse,
    Rigel,
    Deneb,
    CrabPulsar,
    VXSgr,
    UYScuti,
    Stephenson218,
    Earendel
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
