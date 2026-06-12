using System;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;



#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class ThemeData
{
    public ItemCategory category;
    public int numSprite;
    public int numString;
}

public class LevelItem : MonoBehaviour
{
    public PictureItem pictureItemPrefab;
    public Canvas previewCanvas;

    [Header("Level Data")]
    public List<ThemeData> themeDataList = new List<ThemeData>();
    public TileIconContainer tileIconContainer;

    public int initRow = 4;
    public int initColumn = 5;

    public float timeLimit = 60f;

    public float priorityCardRatio = 0.5f;
    public bool isHard = false;

    [Space(20)]
    [Header("Tutorial")]
    public bool isTutorial;

}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelItem))]
public class LevelItemEditor : Editor
{
    // Change these to match your enum values
    //private int randomCount = 3;
    private ItemCategory selectedCategory;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelItem levelItem = (LevelItem)target;

        GUILayout.Space(10);

        //randomCount = EditorGUILayout.IntField("Random Count", randomCount);

        //if (GUILayout.Button("Generate Random Categories"))
        //{
        //    GenerateRandomCategories(levelItem, randomCount);
        //}

        GUILayout.Space(10);
        GUILayout.Label("Theme Data Helper", EditorStyles.boldLabel);

        selectedCategory = (ItemCategory)EditorGUILayout.EnumPopup("Select Theme", selectedCategory);

        if (GUILayout.Button("Fill Selected Theme"))
        {
            if (levelItem.tileIconContainer != null && levelItem.tileIconContainer.categoryIconSets != null)
            {
                Undo.RecordObject(levelItem, "Fill Selected Theme Data");
                bool modified = false;

                foreach (var themeData in levelItem.themeDataList)
                {
                    if (themeData.category != selectedCategory) continue;

                    foreach (var iconSet in levelItem.tileIconContainer.categoryIconSets)
                    {
                        if (iconSet.itemCategory == selectedCategory)
                        {
                            int spritesCount = iconSet.icons != null ? iconSet.icons.Length : 0;
                            int stringsCount = iconSet.text != null ? iconSet.text.Length : 0;

                            if (themeData.numSprite != spritesCount || themeData.numString != stringsCount)
                            {
                                themeData.numSprite = spritesCount;
                                themeData.numString = stringsCount;
                                modified = true;
                            }
                            break;
                        }
                    }
                }

                if (modified)
                {
                    EditorUtility.SetDirty(levelItem);
                    Debug.Log($"Successfully updated numSprite and numString for theme {selectedCategory}.");
                }
                else
                {
                    Debug.Log($"Theme data for {selectedCategory} is already up to date or not found in ThemeDataList.");
                }
            }
            else
            {
                Debug.LogWarning("TileIconContainer is missing or empty! Please assign it first.");
            }
        }

        if (GUILayout.Button("Auto Fill All Themes"))
        {
            if (levelItem.tileIconContainer != null && levelItem.tileIconContainer.categoryIconSets != null)
            {
                Undo.RecordObject(levelItem, "Auto Fill Theme Data");
                bool modified = false;

                foreach (var themeData in levelItem.themeDataList)
                {
                    foreach (var iconSet in levelItem.tileIconContainer.categoryIconSets)
                    {
                        if (iconSet.itemCategory == themeData.category)
                        {
                            int spritesCount = iconSet.icons != null ? iconSet.icons.Length : 0;
                            int stringsCount = iconSet.text != null ? iconSet.text.Length : 0;

                            if (themeData.numSprite != spritesCount || themeData.numString != stringsCount)
                            {
                                themeData.numSprite = spritesCount;
                                themeData.numString = stringsCount;
                                modified = true;
                            }
                            break;
                        }
                    }
                }

                if (modified)
                {
                    EditorUtility.SetDirty(levelItem);
                    Debug.Log("Successfully updated numSprite and numString for all themes in LevelItem.");
                }
                else
                {
                    Debug.Log("Theme data is already up to date.");
                }
            }
            else
            {
                Debug.LogWarning("TileIconContainer is missing or empty! Please assign it first.");
            }
        }

        if (GUILayout.Button("Generate Preview Tiles"))
        {
            GeneratePreviewTiles(levelItem);
        }
    }

    private void GenerateRandomCategories(LevelItem levelItem, int count)
    {
        levelItem.themeDataList.Clear();

        for (int i = 0; i < count; i++)
        {
            ThemeData newTheme = new ThemeData()
            {
                category = CommonUtil.GetRandomEnumValue<ItemCategory>(numLastValuesToSkip: 1),
                numSprite = 4,
                numString = 0
            };
            levelItem.themeDataList.Add(newTheme);
        }

        EditorUtility.SetDirty(levelItem);
    }

    private void GeneratePreviewTiles(LevelItem levelItem)
    {
        List<GameObject> toRemoveObjects = new List<GameObject>();

        for (int i = 0; i < levelItem.previewCanvas.transform.childCount; i++)
        {
            toRemoveObjects.Add(levelItem.previewCanvas.transform.GetChild(i).gameObject);
        }

        foreach (var item in toRemoveObjects)
        {
            DestroyImmediate(item, true);
        }

        List<ItemCategory> flatCategories = new List<ItemCategory>();
        foreach (var theme in levelItem.themeDataList)
        {
            int count = theme.numSprite + theme.numString;
            for (int i = 0; i < count; i++)
            {
                flatCategories.Add(theme.category);
            }
        }

        int totalTiles = flatCategories.Count;
        if (totalTiles == 0) return;

        int col = 4;
        int row = Mathf.CeilToInt((float)totalTiles / col);

        float distance = 1.05f * levelItem.pictureItemPrefab.image.rectTransform.sizeDelta.x;

        float totalWidth = col * distance;
        float totalHeight = row * distance;

        Vector2 position;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                int index = j + col * i;

                if (index >= totalTiles) break;

                ItemCategory currentCategory = flatCategories[index];

                position.x = totalWidth / 2 + j * distance;
                position.y = totalHeight / 2 - i * distance;

                PictureItem pictureItem = Instantiate(levelItem.pictureItemPrefab, levelItem.previewCanvas.transform);

                pictureItem.name = $"Preview Picture {index}";

                pictureItem.transform.position = position;
                pictureItem.icon.sprite = levelItem.tileIconContainer.GetRandomIconOfCategory(currentCategory);
            }
        }

        EditorUtility.SetDirty(levelItem);
    }

    private void GenerateFullLevel()
    {

    }
}
#endif
