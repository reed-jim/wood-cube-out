using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TileIconContainer", menuName = "ScriptableObject/TileIconContainer")]
public class TileIconContainer : ScriptableObject
{
    public CategoryIconSet[] categoryIconSets;
}

[Serializable]
public class CategoryIconSet
{
    public ItemCategory itemCategory;
    public Sprite[] icons;
}
