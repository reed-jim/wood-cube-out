using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class SaveSlotManager : Singleton<SaveSlotManager>
{
    private const int TOTAL_SLOTS = 3;
    private const string CURRENT_SLOT_ID_KEY = "CurrentSaveSlotID";
    private const string DEFAULT_SLOT_ID = "Slot_1";

    public SaveSlotData currentSaveSlotData;

    private readonly List<SaveSlotData> allSaveSlots = new List<SaveSlotData>();
    public IReadOnlyList<SaveSlotData> AllSaveSlots => allSaveSlots;

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

        string savedSlotID = DataUtility.Load(CURRENT_SLOT_ID_KEY, DEFAULT_SLOT_ID);
        currentSaveSlotData = FindSlot(savedSlotID) ?? allSaveSlots[0];

        OnSaveSlotsUpdated?.Invoke();
    }

    public void SelectSlot(SaveSlotData slotData)
    {
        if (slotData == null) return;

        string previousSlotID = currentSaveSlotData?.slotID;
        bool wasEmpty = slotData.isEmpty;

        if (wasEmpty)
        {
            CreateNewSaveSlot(slotData.slotID, slotData.slotName);
        }
        else
        {
            currentSaveSlotData = FindSlot(slotData.slotID) ?? slotData;
        }

        PersistSelectedSlot(currentSaveSlotData.slotID);

        OnSlotSelected?.Invoke(currentSaveSlotData);
        OnSaveSlotsUpdated?.Invoke();

        bool isSameFilledSlot = !wasEmpty && previousSlotID == currentSaveSlotData.slotID;
        if (!isSameFilledSlot)
        {
            RestartGame();
        }
    }

    public void CreateNewSaveSlot(string slotID, string name)
    {
        SaveSlotData targetSlot = FindSlot(slotID);
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
        SaveSlotData targetSlot = FindSlot(slotID);
        if (targetSlot == null) return;

        SaveSlotData resetData = GetDefaultSlotData(slotID, targetSlot.slotName);
        int index = allSaveSlots.IndexOf(targetSlot);
        allSaveSlots[index] = resetData;

        DataUtility.Save(GetSlotMetadataKey(slotID), resetData);

        if (currentSaveSlotData != null && currentSaveSlotData.slotID == slotID)
        {
            currentSaveSlotData = resetData;
        }

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

    public string GetSaveKey(string name)
    {
        string prefix = (currentSaveSlotData != null && !string.IsNullOrEmpty(currentSaveSlotData.slotID))
            ? currentSaveSlotData.slotID
            : DEFAULT_SLOT_ID;

        return $"{prefix}_{name}";
    }

    private void PersistSelectedSlot(string slotID)
    {
        DataUtility.Save(CURRENT_SLOT_ID_KEY, slotID);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private SaveSlotData FindSlot(string slotID)
    {
        return allSaveSlots.Find(s => s.slotID == slotID);
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
