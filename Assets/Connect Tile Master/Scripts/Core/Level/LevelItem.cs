using System;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelItem : MonoBehaviour
{
    public PictureItem pictureItemPrefab;
    public Canvas previewCanvas;

    public ItemCategory[] poolCategory;
    public TileIconContainer tileIconContainer;
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelItem))]
public class LevelItemEditor : Editor
{
    // Change these to match your enum values
    private int randomCount = 3;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelItem levelItem = (LevelItem)target;

        GUILayout.Space(10);

        randomCount = EditorGUILayout.IntField("Random Count", randomCount);

        if (GUILayout.Button("Generate Random Categories"))
        {
            GenerateRandomCategories(levelItem, randomCount);
        }

        if (GUILayout.Button("Generate Preview Tiles"))
        {
            GeneratePreviewTiles(levelItem);
        }
    }

    private void GenerateRandomCategories(LevelItem levelItem, int count)
    {
        levelItem.poolCategory = new ItemCategory[count];

        for (int i = 0; i < count; i++)
        {
            levelItem.poolCategory[i] = CommonUtil.GetRandomEnumValue<ItemCategory>(numLastValuesToSkip: 1);
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

        int totalTiles = 4 * levelItem.poolCategory.Length;

        ItemCategory currentCategory = ItemCategory.Vegetable;

        int col = 4;
        int row = totalTiles / col;

        float distance = 1.05f * levelItem.pictureItemPrefab.image.rectTransform.sizeDelta.x;

        float totalWidth = col * distance;
        float totalHeight = row * distance;

        Vector2 position;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                int index = j + col * i;

                if (index % 4 == 0)
                {
                    currentCategory = levelItem.poolCategory[index / 4];
                }

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
}
#endif
