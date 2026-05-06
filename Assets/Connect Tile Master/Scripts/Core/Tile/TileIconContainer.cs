using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TileIconContainer", menuName = "ScriptableObject/TileIconContainer")]
public class TileIconContainer : ScriptableObject
{

    public Sprite[] allSprites;

    public CategoryIconSet[] categoryIconSets;

    public Sprite GetRandomIconOfCategory(ItemCategory itemCategory)
    {
        CategoryIconSet matchCategoryIconSet = categoryIconSets.First(item => item.itemCategory == itemCategory);

        return CommonUtil.GetRandomValueFromArray(matchCategoryIconSet.icons);
    }
}

[Serializable]
public class CategoryIconSet
{
    public ItemCategory itemCategory;
    public Sprite[] icons;
}
