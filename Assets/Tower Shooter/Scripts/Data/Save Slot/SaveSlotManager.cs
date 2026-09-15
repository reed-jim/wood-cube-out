using System;
using UnityEngine;

public class SaveSlotManager : Singleton<SaveSlotManager>
{
    public SaveSlotData currentSaveSlotData;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        currentSaveSlotData.slotName = "TestSaveSlot";
    }

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
