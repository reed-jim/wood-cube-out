using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class SaveSlotPanelUI : BasePopup
{
    [Header("Containers")]
    [SerializeField] private RectTransform slotContainer;

    [Header("Switchable Screen")]
    public SwitchableScreenType switchableScreenType;

    private readonly List<SaveSlotItemUI> spawnedSlotItems = new List<SaveSlotItemUI>();

    private void Awake()
    {
        SaveSlotManager.OnSaveSlotsUpdated += RefreshSlots;
        SaveSlotItemUI.onSelectSlotClicked += OnSlotSelected;
        SaveSlotItemUI.onDeleteSlotClicked += OnSlotDeleted;
    }

    private void OnDestroy()
    {
        SaveSlotManager.OnSaveSlotsUpdated -= RefreshSlots;
        SaveSlotItemUI.onSelectSlotClicked -= OnSlotSelected;
        SaveSlotItemUI.onDeleteSlotClicked -= OnSlotDeleted;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        GenerateSlots();
    }

    public void GenerateSlots()
    {
        ClearSlots();

        var slots = SaveSlotManager.Ins.AllSaveSlots;

        foreach (var slotData in slots)
        {
            SaveSlotItemUI itemUI = ObjectPoolingEverything.GetFromPool<SaveSlotItemUI>(GameConstants.saveSlotItemUI);
            CommonUtil.SetParentAndResetScale(itemUI.transform, slotContainer);

            itemUI.Bind(slotData);
            spawnedSlotItems.Add(itemUI);
        }
    }

    private void RefreshSlots()
    {
        foreach (var item in spawnedSlotItems)
        {
            item.UpdateUI();
        }
    }

    private void ClearSlots()
    {
        CommonUtil.ReturnListToPool(spawnedSlotItems, item => item.gameObject);
        spawnedSlotItems.Clear();
    }

    private void OnSlotSelected(SaveSlotData slotData)
    {
        SaveSlotManager.Ins.SelectSlot(slotData);
    }

    private void OnSlotDeleted(SaveSlotData slotData)
    {
        SaveSlotManager.Ins.DeleteSlot(slotData.slotID);
    }
}
