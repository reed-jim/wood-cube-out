using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PictureItem : MonoBehaviour
{
    public Image image;
    public Image icon;

    public ItemCategory[] categories;
    public ItemCategory specificCategoryForLevel;

    void OnValidate()
    {
        if (categories.Length > 0)
        {
            if (!categories.Contains(specificCategoryForLevel))
            {
                specificCategoryForLevel = categories[0];
            }
        }
    }
}
