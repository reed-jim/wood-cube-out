using System;
using System.Collections.Generic;

public class SaveSlotManager : Singleton<SaveSlotManager>
{
    private const int TOTAL_SLOTS = 3;

    // Existing field preserved from your code
    public SaveSlotData currentSaveSlotData;

    private readonly List<SaveSlotData> allSaveSlots = new List<SaveSlotData>();
    public IReadOnlyList<SaveSlotData> AllSaveSlots => allSaveSlots;

    // Events matching ResearchManager style
    public static event Action OnSaveSlotsUpdated;
    public static event Action<SaveSlotData> OnSlotSelected;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();
        LoadAllSlots();
    }

    #region Slot Management

    public void LoadAllSlots()
    {
        allSaveSlots.Clear();

        for (int i = 1; i <= TOTAL_SLOTS; i++)
        {
            string slotID = $"Slot_{i}";
            SaveSlotData slotData = DataUtility.Load(
                GetSlotMetadataKey(slotID),
                GetDefaultSlotData(slotID, $"Save Slot {i}")
            );

            allSaveSlots.Add(slotData);
        }

        // Initialize currentSaveSlotData if unassigned
        if (currentSaveSlotData == null || string.IsNullOrEmpty(currentSaveSlotData.slotID))
        {
            currentSaveSlotData = allSaveSlots[0];
        }

        OnSaveSlotsUpdated?.Invoke();
    }

    public void SelectSlot(SaveSlotData slotData)
    {
        if (slotData == null) return;

        currentSaveSlotData = slotData;

        if (slotData.isEmpty)
        {
            CreateNewSaveSlot(slotData.slotID, slotData.slotName);
        }

        OnSlotSelected?.Invoke(currentSaveSlotData);
        OnSaveSlotsUpdated?.Invoke();
    }

    public void CreateNewSaveSlot(string slotID, string name)
    {
        SaveSlotData targetSlot = allSaveSlots.Find(s => s.slotID == slotID);
        if (targetSlot == null) return;

        targetSlot.slotName = name;
        targetSlot.isEmpty = false;
        targetSlot.lastModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        targetSlot.totalPlayTime = 0f;
        targetSlot.currentLevel = 1;

        currentSaveSlotData = targetSlot;
        SaveSlotMetadata(targetSlot);

        OnSaveSlotsUpdated?.Invoke();
    }

    public void DeleteSlot(string slotID)
    {
        SaveSlotData targetSlot = allSaveSlots.Find(s => s.slotID == slotID);
        if (targetSlot == null) return;

        SaveSlotData resetData = GetDefaultSlotData(slotID, targetSlot.slotName);
        int index = allSaveSlots.IndexOf(targetSlot);
        allSaveSlots[index] = resetData;

        DataUtility.Save(GetSlotMetadataKey(slotID), resetData);

        OnSaveSlotsUpdated?.Invoke();
    }

    public void SaveSlotMetadata(SaveSlotData slotData)
    {
        if (slotData == null) return;

        slotData.lastModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        DataUtility.Save(GetSlotMetadataKey(slotData.slotID), slotData);
    }

    #endregion

    #region Save Key Helpers

    // Preserves your original signature and format
    public string GetSaveKey(string name)
    {
        string prefix = (currentSaveSlotData != null && !string.IsNullOrEmpty(currentSaveSlotData.slotID))
            ? currentSaveSlotData.slotID
            : "Slot_1";

        return $"{prefix}_{name}";
    }

    private string GetSlotMetadataKey(string slotID)
    {
        return $"{slotID}_Metadata";
    }

    private SaveSlotData GetDefaultSlotData(string slotID, string defaultName)
    {
        return new SaveSlotData
        {
            slotID = slotID,
            slotName = defaultName,
            isEmpty = true,
            lastModifiedDate = string.Empty,
            totalPlayTime = 0f,
            currentLevel = 1
        };
    }

    #endregion
}
