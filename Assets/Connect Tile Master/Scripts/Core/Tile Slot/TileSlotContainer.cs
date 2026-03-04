using TMPro;
using UnityEngine;

public class TileSlotContainer : MonoBehaviour
{
    [SerializeField] private TMP_Text category;



    [SerializeField] private ItemCategory itemCategory;

    public ItemCategory ItemCategory
    {
        get => itemCategory;
        set => itemCategory = value;
    }

    [SerializeField] private TileSlot[] tileSlots;

    public TileSlot[] TileSlots
    {
        get => tileSlots;
    }

    void Awake()
    {
        Init();
    }

    public void Init()
    {
        itemCategory = CommonUtil.GetRandomEnumValue<ItemCategory>();
        category.text = $"{itemCategory.ToString()[0]}";
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
}
