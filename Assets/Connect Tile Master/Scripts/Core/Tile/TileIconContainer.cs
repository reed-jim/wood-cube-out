using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "TileIconContainer", menuName = "ScriptableObject/TileIconContainer")]
public class TileIconContainer : ScriptableObject
{

    public Sprite[] allSprites;

    public CategoryIconSet[] categoryIconSets;



    public Sprite GetRandomIconOfCategory(ItemCategory itemCategory)
    {
        CategoryIconSet matchCategoryIconSet = categoryIconSets.First(item => item.itemCategory == itemCategory);

        if (matchCategoryIconSet.icons == null || matchCategoryIconSet.icons.Length == 0)
        {
            return null;
        }

        return CommonUtil.GetRandomValueFromArray(matchCategoryIconSet.icons);
    }

    public string GetRandomTextOfCategory(ItemCategory itemCategory)
    {
        CategoryIconSet matchCategoryIconSet = categoryIconSets.First(item => item.itemCategory == itemCategory);

        if (matchCategoryIconSet.text == null || matchCategoryIconSet.text.Length == 0)
        {
            return itemCategory.ToString();
        }

        return CommonUtil.GetRandomValueFromArray(matchCategoryIconSet.text);
    }
}

[Serializable]
public class CategoryIconSet
{
    public ItemCategory itemCategory;
    public Sprite[] icons;
    public string[] text;
    public string textOrder;
    public Sprite spriteOrder;
}

#if UNITY_EDITOR
[CustomEditor(typeof(TileIconContainer))]
public class TileIconContainerEditor : UnityEditor.Editor
{
    private DefaultAsset spriteFolder;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TileIconContainer container = (TileIconContainer)target;

        GUILayout.Space(15);
        GUILayout.Label("Auto Fill Sprite Order Helper", EditorStyles.boldLabel);

        spriteFolder = (DefaultAsset)EditorGUILayout.ObjectField("Sprite Folder", spriteFolder, typeof(DefaultAsset), false);

        if (GUILayout.Button("Fill Sprite Order From Folder"))
        {
            if (spriteFolder != null)
            {
                string folderPath = AssetDatabase.GetAssetPath(spriteFolder);
                string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

                Undo.RecordObject(container, "Fill Sprite Order");
                int matchCount = 0;

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

                    if (sprite != null)
                    {
                        foreach (var categorySet in container.categoryIconSets)
                        {
                            string categoryName1 = categorySet.itemCategory.ToString();
                            string categoryName2 = categoryName1.Replace("_", " ");
                            string categoryName3 = categoryName1.Replace("_", "");

                            if (string.Equals(sprite.name, categoryName1, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(sprite.name, categoryName2, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(sprite.name, categoryName3, StringComparison.OrdinalIgnoreCase))
                            {
                                categorySet.spriteOrder = sprite;
                                matchCount++;
                                break;
                            }
                        }
                    }
                }

                if (matchCount > 0)
                {
                    EditorUtility.SetDirty(container);
                    Debug.Log($"Successfully updated {matchCount} sprite orders from folder '{folderPath}'.");
                }
                else
                {
                    Debug.LogWarning($"No matching sprites found for any ItemCategory in folder '{folderPath}'. Ensure sprite names match Category names exactly.");
                }
            }
            else
            {
                Debug.LogWarning("Please assign a Sprite Folder first!");
            }
        }
    }
}
#endif
