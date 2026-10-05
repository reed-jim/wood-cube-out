using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class SaveSlotPanelUI : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform slotContainer;

    [Header("Switchable Screen")]
    public SwitchableScreenType switchableScreenType;

    private readonly List<SaveSlotItemUI> spawnedSlotItems = new List<SaveSlotItemUI>();

    private void Awake()
    {
        SaveSlotManager.OnSaveSlotsUpdated += RefreshSlots;
        SaveSlotItemUI.onSelectSlotClicked += OnSlotSelected;
        SaveSlotItemUI.onDeleteSlotClicked += OnSlotDeleted;
        SwitchableScreenButton.switchScreenEvent += OnScreenSwitch;
    }

    private void OnDestroy()
    {
        SaveSlotManager.OnSaveSlotsUpdated -= RefreshSlots;
        SaveSlotItemUI.onSelectSlotClicked -= OnSlotSelected;
        SaveSlotItemUI.onDeleteSlotClicked -= OnSlotDeleted;
        SwitchableScreenButton.switchScreenEvent -= OnScreenSwitch;
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

    private void OnScreenSwitch(SwitchableScreenType type)
    {
        if (type == this.switchableScreenType)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void Show()
    {
        container.gameObject.SetActive(true);
        container.localPosition = new Vector3(0, -1.5f * container.sizeDelta.y);

        Tween.LocalPositionY(container, 0f, duration: 0.3f);

        GenerateSlots();
    }

    public void Hide()
    {
        Tween.LocalPositionY(container, -1.5f * container.sizeDelta.y, duration: 0.3f)
            .OnComplete(() =>
            {
                container.gameObject.SetActive(false);
            });
    }
}
