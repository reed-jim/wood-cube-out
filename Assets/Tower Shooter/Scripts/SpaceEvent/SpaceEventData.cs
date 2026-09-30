using System;

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
    public System.Collections.Generic.List<SpaceEventData> activeEvents;
}
