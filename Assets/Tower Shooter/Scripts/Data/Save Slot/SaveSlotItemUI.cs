using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI slotNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button deleteButton;

    [Header("Overlays")]
    [SerializeField] private RectTransform emptySlotOverlay;
    [SerializeField] private RectTransform activeSlotOverlay;

    public SaveSlotData BoundData { get; private set; }

    public static event Action<SaveSlotData> onSelectSlotClicked;
    public static event Action<SaveSlotData> onDeleteSlotClicked;

    private void Awake()
    {
        if (selectButton) selectButton.onClick.AddListener(OnSelectClicked);
        if (deleteButton) deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    public void Bind(SaveSlotData slotData)
    {
        BoundData = slotData;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (BoundData == null) return;

        bool isCurrentSlot = SaveSlotManager.Ins.currentSaveSlotData?.slotID == BoundData.slotID;

        if (emptySlotOverlay) emptySlotOverlay.gameObject.SetActive(BoundData.isEmpty);
        if (activeSlotOverlay) activeSlotOverlay.gameObject.SetActive(isCurrentSlot && !BoundData.isEmpty);

        if (!BoundData.isEmpty)
        {
            if (slotNameText) slotNameText.text = BoundData.slotName;
            if (levelText) levelText.text = $"Level {BoundData.currentLevel}";
            if (dateText) dateText.text = BoundData.lastModifiedDate;
        }
        else
        {
            if (slotNameText) slotNameText.text = "Empty Slot";
        }
    }

    private void OnSelectClicked()
    {
        onSelectSlotClicked?.Invoke(BoundData);
    }

    private void OnDeleteClicked()
    {
        onDeleteSlotClicked?.Invoke(BoundData);
    }
}
