using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class TileSlotContainer : MonoBehaviour
{
    [SerializeField] private TMP_Text category;
    [SerializeField] private Button unlockButton;
    public Image lockImage;

    [SerializeField] private ItemCategory itemCategory;
    [SerializeField] private bool isActive;
    public bool isLocked;

    public ItemCategory ItemCategory
    {
        get => itemCategory;
        set => itemCategory = value;
    }

    public bool IsActive
    {
        get => isActive;
        set => isActive = value;
    }

    [SerializeField] private TileSlot[] tileSlots;

    public TileSlot[] TileSlots
    {
        get => tileSlots;
    }

    public static event Action<BoosterType> showBuyBoosterPopupEvent;

    void Awake()
    {
        unlockButton.onClick.AddListener(ClickButtonUnlockByAds);
    }

    public void Init(ItemCategory itemCategory)
    {
        this.itemCategory = itemCategory;
        category.text = $"{itemCategory}";

        lockImage.gameObject.SetActive(isLocked);
        category.gameObject.SetActive(!isLocked);
    }

    public TileSlot GetAvailableTileSlot()
    {
        for (int i = 0; i < tileSlots.Length; i++)
        {
            TileSlot tileSlot = tileSlots[i];

            if (tileSlot.Tile == null)
            {
                return tileSlot;
            }
        }

        return null;
    }

    public bool IsLastSlot(TileSlot slot)
    {
        return slot == tileSlots[tileSlots.Length - 1];
    }

    public void ClearTiles()
    {
        foreach (var tileSlot in tileSlots)
        {
            tileSlot.ClearTile();
        }
    }

    private void ClickButtonUnlockByAds()
    {
        if (isLocked)
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.UnlockScrewBox);
        }
    }
}
