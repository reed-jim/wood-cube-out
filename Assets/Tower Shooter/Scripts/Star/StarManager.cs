using System.Collections.Generic;
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
    public StarVoyageContainer starVoyageContainer;

    public StarPathConfig currentStarPathConfig;
    public StarPathConfig nextStarPathConfig;

    private Dictionary<int, StarPathConfig> starPathConfigIds = new Dictionary<int, StarPathConfig>();

    public VoyageData GetFirstVoyage()
    {
        VoyageData voyageData = new VoyageData();

        voyageData.starPathConfigIndex = 0;

        voyageData.remaningKm = GameConstants.KmPerLightYear * CurveManager.Ins.GetVoyageDistance(voyageData.starPathConfigIndex);

        return voyageData;
    }

    public void SetStarPathConfig(int index)
    {
        if (starPathConfigIds.Count == 0)
        {
            BuildStarPathConfigIdLookup();
        }

        if (index < starPathConfigIds.Count)
        {
            currentStarPathConfig = starPathConfigIds[index];
        }

        if (index + 1 < starPathConfigIds.Count)
        {
            nextStarPathConfig = starPathConfigIds[index + 1];
        }
    }

    public StarPathConfig GetVoyageConfig(int index)
    {
        if (index < starPathConfigIds.Count)
        {
            currentStarPathConfig = starPathConfigIds[index];
        }

        if (index + 1 < starPathConfigIds.Count)
        {
            nextStarPathConfig = starPathConfigIds[index + 1];
        }

        return nextStarPathConfig;
    }

    public bool IsLastVoyage(int index)
    {
        return index == starVoyageContainer.starPathConfigs.Count - 1;
    }

    private void BuildStarPathConfigIdLookup()
    {
        for (int i = 0; i < starVoyageContainer.starPathConfigs.Count; i++)
        {
            StarPathConfig starPathConfig = starVoyageContainer.starPathConfigs[i];
            int id = starPathConfig.id;

            starPathConfigIds.Add(id, starPathConfig);
        }
    }

    #region Util
    public StarPathConfig GetNextStarPathConfig()
    {
        return nextStarPathConfig;
    }
    #endregion
}
