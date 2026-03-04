using UnityEngine;

public enum ItemCategory
{
    Ball,
    Food,
    Animal
}

public class TileSlot : MonoBehaviour
{
    [SerializeField] private ItemCategory itemCategory;

    public ItemCategory ItemCategory
    {
        get => itemCategory;
        set => itemCategory = value;
    }
}
