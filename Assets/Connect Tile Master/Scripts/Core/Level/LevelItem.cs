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

    [Space(10)]
    [Header("Tile Spacing (0 = dùng mặc định)")]
    [Tooltip("Khoảng cách ngang giữa các ô tile. 0 = dùng mặc định (210). " +
             "Tăng lên khi số cột < 5 để tiles trải rộng hơn.")]
    public float tileSpacingWidth = 0f;

    [Tooltip("Khoảng cách dọc giữa các hàng tile. 0 = dùng mặc định (266). " +
             "Tăng lên khi số hàng < 4 để tiles trải rộng hơn.")]
    public float tileSpacingHeight = 0f;

    /// <summary>Spacing ngang thực sự sẽ dùng (fallback về 234 nếu = 0).</summary>
    public float ActualSpacingWidth
    {
        get
        {
            if (tileSpacingWidth > 0) return tileSpacingWidth;
            if ((initRow == 2 && initColumn == 3) || (initRow == 3 && initColumn == 3)) return 300f;
            return 234f;
        }
    }

    /// <summary>Spacing dọc thực sự sẽ dùng (fallback về 275 nếu = 0).</summary>
    public float ActualSpacingHeight
    {
        get
        {
            if (tileSpacingHeight > 0) return tileSpacingHeight;
            if (initRow == 2 && initColumn == 4) return 350.5f;
            if (initRow == 3 && initColumn == 4) return 353f;
            if ((initRow == 2 && initColumn == 3) || (initRow == 3 && initColumn == 3)) return 353f;
            return 287.5f;
        }
    }

    public float ActualTileContainerYOffset
    {
        get
        {
            if (initRow == 2 && initColumn == 4) return -30f;
            return tileContainerYOffset;
        }
    }

    [Space(20)]
    [Header("Tutorial")]
    public bool isTutorial;

    [Space(20)]
    [Header("Tile Container Config")]
    [Tooltip("Điều chỉnh vị trí Y của Tile Container lúc khởi tạo")]
    private float tileContainerYOffset = -52f;
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

        // ── Spacing helper section ───────────────────────────────────────────
        GUILayout.Space(10);
        GUILayout.Label("Tile Spacing Helper", EditorStyles.boldLabel);

        bool isSmallGrid = levelItem.initRow < 4 || levelItem.initColumn < 5;

        if (isSmallGrid)
        {
            EditorGUILayout.HelpBox(
                $"Grid {levelItem.initRow} hàng × {levelItem.initColumn} cột " +
                $"(nhỏ hơn 4×5). Hãy tăng spacing để tiles không bị chen chật.",
                MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox(
                $"Grid {levelItem.initRow} hàng × {levelItem.initColumn} cột. " +
                "Để spacing = 0 sẽ dùng mặc định (210 × 266).",
                MessageType.Info);
        }

        // Show effective spacing
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUILayout.Label("Spacing hiệu lực:", GUILayout.Width(120));
        GUILayout.Label($"W = {levelItem.ActualSpacingWidth:F0}", EditorStyles.boldLabel, GUILayout.Width(80));
        GUILayout.Label($"H = {levelItem.ActualSpacingHeight:F0}", EditorStyles.boldLabel, GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();

        // Auto-suggest button
        if (GUILayout.Button("Auto-suggest Spacing theo Grid", GUILayout.Height(28)))
        {
            Undo.RecordObject(levelItem, "Auto Suggest Spacing");

            // Base spacing: scale up proportionally when fewer tiles
            // 4×5 = 20 tiles → base 210×266
            // Fewer cols → wider, fewer rows → taller
            float baseW = 210f;
            float baseH = 266f;
            float suggestW = baseW * (5f / Mathf.Max(1, levelItem.initColumn));
            float suggestH = baseH * (4f / Mathf.Max(1, levelItem.initRow));

            // Cap at 2× base (don't over-expand)
            suggestW = Mathf.Min(suggestW, baseW * 2f);
            suggestH = Mathf.Min(suggestH, baseH * 2f);

            // Round to nearest 10
            levelItem.tileSpacingWidth = Mathf.Round(suggestW / 10f) * 10f;
            levelItem.tileSpacingHeight = Mathf.Round(suggestH / 10f) * 10f;

            EditorUtility.SetDirty(levelItem);
            Debug.Log($"[LevelItem] Auto-suggest spacing: W={levelItem.tileSpacingWidth}, H={levelItem.tileSpacingHeight}");
        }

        if (GUILayout.Button("Reset về mặc định (210 × 266)", GUILayout.Height(24)))
        {
            Undo.RecordObject(levelItem, "Reset Spacing");
            levelItem.tileSpacingWidth = 0f;
            levelItem.tileSpacingHeight = 0f;
            EditorUtility.SetDirty(levelItem);
        }

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
                            int spritesCount = (iconSet.iconCdnKeys != null && iconSet.iconCdnKeys.Length > 0) ? iconSet.iconCdnKeys.Length : (iconSet.icons != null ? iconSet.icons.Length : 0);
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
                            int spritesCount = (iconSet.iconCdnKeys != null && iconSet.iconCdnKeys.Length > 0) ? iconSet.iconCdnKeys.Length : (iconSet.icons != null ? iconSet.icons.Length : 0);
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
