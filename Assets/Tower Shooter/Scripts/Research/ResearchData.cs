using System;
using System.Collections.Generic;

[Serializable]
public class ActiveResearchData
{
    public string researchID;
    public float remainingTime;
    public float totalDuration;
    public int targetLevel;

    public ActiveResearchData(string id, float remaining, float total, int level)
    {
        researchID = id;
        remainingTime = remaining;
        totalDuration = total;
        targetLevel = level;
    }
}

[Serializable]
public class ResearchSaveData
{
    public List<ResearchProgressData> unlockedResearches = new List<ResearchProgressData>();
    public ActiveResearchData activeResearch;
}

[Serializable]
public class ResearchProgressData
{
    public string researchID;
    public int currentLevel;

    public ResearchProgressData(string id, int level)
    {
        researchID = id;
        currentLevel = level;
    }
}
