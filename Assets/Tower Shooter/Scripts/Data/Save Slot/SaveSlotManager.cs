using System;
using UnityEngine;

public class SaveSlotManager : Singleton<SaveSlotManager>
{
    public SaveSlotData currentSaveSlotData;
}

[Serializable]
public class SaveSlotData
{
    public string slotName;
}
