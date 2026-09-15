using System;
using UnityEngine;

public class SaveSlotManager : Singleton<SaveSlotManager>
{
    public SaveSlotData currentSaveSlotData;

    public string GetSaveKey(string name)
    {
        return $"{currentSaveSlotData.slotName}_{name}";
    }
}

[Serializable]
public class SaveSlotData
{
    public string slotName;
}
