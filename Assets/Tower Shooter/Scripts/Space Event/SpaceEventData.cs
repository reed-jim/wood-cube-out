using System;
using System.Collections.Generic;

[Serializable]
public class SpaceEventData
{
    public string eventID;
    public float remainingTime;

    public SpaceEventData(string id, float remaining)
    {
        eventID = id;
        remainingTime = remaining;
    }
}

[Serializable]
public class SpaceEventSaveData
{
    public List<SpaceEventData> activeEvents;
}
