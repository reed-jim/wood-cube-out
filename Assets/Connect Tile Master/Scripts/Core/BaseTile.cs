using UnityEngine;

public class BaseTile : MonoBehaviour
{
    [SerializeField] private ItemCategory itemCategory;

    public ItemCategory ItemCategory
    {
        get => itemCategory;
        set => itemCategory = value;
    }

    private void MoveToSlot(TileSlot tileSlot)
    {
        if (tileSlot.ItemCategory == itemCategory)
        {
            
        }
    }
}
