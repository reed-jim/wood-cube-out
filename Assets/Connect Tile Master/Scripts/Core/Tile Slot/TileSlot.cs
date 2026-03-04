using System.Collections.Generic;
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

    public List<BaseTile> tiles;

    public void AddTile(BaseTile tile)
    {
        tiles.Add(tile);
    }
}
