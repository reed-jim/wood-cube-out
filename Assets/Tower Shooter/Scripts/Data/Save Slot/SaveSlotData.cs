using System;

[Serializable]
public class SaveSlotData
{
    public string slotID;
    public string slotName;
    public string lastModifiedDate;
    public float totalPlayTime;
    public int currentLevel;
    public bool isEmpty = true;
}
