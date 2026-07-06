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

        if (matchCategoryIconSet.isOnServer)
        {
            // ── CDN path (index 21+) ───────────────────────────────────────────────
            // icons[] may have been cleared; check array first, then CDN memory cache.
            if (matchCategoryIconSet.icons != null && matchCategoryIconSet.icons.Length > 0)
            {
                var nonNull = System.Array.FindAll(matchCategoryIconSet.icons, s => s != null);
                if (nonNull.Length > 0)
                    return CommonUtil.GetRandomValueFromArray(nonNull);
            }

#if !UNITY_EDITOR
            if (matchCategoryIconSet.iconCdnKeys != null)
            {
                var downloader = GplayCdn.CdnDownloader.Instance;
                if (downloader != null)
                {
                    var cachedSprites = new System.Collections.Generic.List<Sprite>();
                    foreach (var key in matchCategoryIconSet.iconCdnKeys)
                    {
                        if (string.IsNullOrEmpty(key)) continue;
                        var s = downloader.GetCachedSprite(key);
                        if (s != null) cachedSprites.Add(s);
                    }
                    if (cachedSprites.Count > 0)
                        return CommonUtil.GetRandomValueFromArray(cachedSprites.ToArray());
                }
            }
#endif
            return null;
        }
        else
        {
            // ── Local path (index 0-20): use icons[] directly ──────────────────────
            if (matchCategoryIconSet.icons == null || matchCategoryIconSet.icons.Length == 0)
                return null;

            return CommonUtil.GetRandomValueFromArray(matchCategoryIconSet.icons);
        }
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

    // CDN fields - populated after upload, used for runtime download
    public string[] iconCdnKeys;        // CDN fileId for each icon sprite
    public string spriteOrderCdnKey;    // CDN fileId for spriteOrder sprite
    public bool isOnServer;             // true if sprites are uploaded to CDN
}

#if UNITY_EDITOR
[CustomEditor(typeof(TileIconContainer))]
public class TileIconContainerEditor : UnityEditor.Editor
{
    private DefaultAsset spriteFolder;
    private DefaultAsset parentSpritesFolder;
    private int clearStartIndex = 0;
    private int clearEndIndex = 0;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TileIconContainer container = (TileIconContainer)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Auto Generate All Categories"))
        {
            Undo.RecordObject(container, "Generate Categories");
            
            Array enumValues = Enum.GetValues(typeof(ItemCategory));
            var newCategorySets = new System.Collections.Generic.List<CategoryIconSet>();

            foreach (ItemCategory category in enumValues)
            {
                if (category.ToString() == "None") continue;

                var existing = container.categoryIconSets != null ? container.categoryIconSets.FirstOrDefault(c => c.itemCategory == category) : null;
                if (existing != null)
                {
                    newCategorySets.Add(existing);
                }
                else
                {
                    newCategorySets.Add(new CategoryIconSet { itemCategory = category });
                }
            }

            container.categoryIconSets = newCategorySets.ToArray();
            EditorUtility.SetDirty(container);
            Debug.Log($"Successfully generated {newCategorySets.Count} categories based on ItemCategory Enum.");
        }

        GUILayout.Space(15);
        GUILayout.Label("CDN Clean Helper", EditorStyles.boldLabel);
        clearStartIndex = EditorGUILayout.IntField("Clear Start Index", clearStartIndex);
        clearEndIndex = EditorGUILayout.IntField("Clear End Index", clearEndIndex);

        if (GUILayout.Button("Clear CDN Data For Range"))
        {
            if (container.categoryIconSets == null || container.categoryIconSets.Length == 0)
            {
                Debug.LogWarning("categoryIconSets is empty.");
            }
            else
            {
                int start = Mathf.Clamp(clearStartIndex, 0, container.categoryIconSets.Length - 1);
                int end = Mathf.Clamp(clearEndIndex, 0, container.categoryIconSets.Length - 1);

                if (start > end)
                {
                    int temp = start;
                    start = end;
                    end = temp;
                }

                Undo.RecordObject(container, "Clear CDN Data Range");
                int clearedCount = 0;
                for (int i = start; i <= end; i++)
                {
                    var set = container.categoryIconSets[i];
                    if (set != null)
                    {
                        set.iconCdnKeys = null;
                        set.spriteOrderCdnKey = null;
                        set.isOnServer = false;
                        clearedCount++;
                    }
                }

                EditorUtility.SetDirty(container);
                AssetDatabase.SaveAssets();
                Debug.Log($"Successfully cleared CDN data for {clearedCount} categories (Index {start} to {end}).");
            }
        }

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

        GUILayout.Space(15);
        GUILayout.Label("Auto Fill Category Icons Helper", EditorStyles.boldLabel);

        parentSpritesFolder = (DefaultAsset)EditorGUILayout.ObjectField("Parent Folder", parentSpritesFolder, typeof(DefaultAsset), false);

        if (GUILayout.Button("Fill Icons From Subfolders"))
        {
            if (parentSpritesFolder != null)
            {
                string parentFolderPath = AssetDatabase.GetAssetPath(parentSpritesFolder);
                string[] subFolders = AssetDatabase.GetSubFolders(parentFolderPath);

                Undo.RecordObject(container, "Fill Category Icons");
                int totalCategoriesUpdated = 0;
                int totalSpritesAssigned = 0;

                foreach (string subFolder in subFolders)
                {
                    string folderName = System.IO.Path.GetFileName(subFolder);

                    CategoryIconSet targetCategorySet = null;

                    if (container.categoryIconSets != null)
                    {
                        foreach (var categorySet in container.categoryIconSets)
                        {
                            string categoryName1 = categorySet.itemCategory.ToString();
                            string categoryName2 = categoryName1.Replace("_", " ");
                            string categoryName3 = categoryName1.Replace("_", "");

                            if (string.Equals(folderName, categoryName1, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(folderName, categoryName2, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(folderName, categoryName3, StringComparison.OrdinalIgnoreCase))
                            {
                                targetCategorySet = categorySet;
                                break;
                            }
                        }
                    }

                    if (targetCategorySet != null)
                    {
                        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { subFolder });
                        var sprites = new System.Collections.Generic.List<Sprite>();

                        string categoryName1 = targetCategorySet.itemCategory.ToString();
                        string categoryName2 = categoryName1.Replace("_", " ");
                        string categoryName3 = categoryName1.Replace("_", "");

                        foreach (string guid in spriteGuids)
                        {
                            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                            if (sprite != null)
                            {
                                if (string.Equals(sprite.name, categoryName1, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(sprite.name, categoryName2, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(sprite.name, categoryName3, StringComparison.OrdinalIgnoreCase))
                                {
                                    targetCategorySet.spriteOrder = sprite;
                                    continue;
                                }
                                sprites.Add(sprite);
                            }
                        }

                        if (sprites.Count > 0)
                        {
                            targetCategorySet.icons = sprites.ToArray();
                            totalCategoriesUpdated++;
                            totalSpritesAssigned += sprites.Count;
                        }
                    }
                }

                if (totalCategoriesUpdated > 0)
                {
                    EditorUtility.SetDirty(container);
                    Debug.Log($"Successfully updated {totalCategoriesUpdated} categories with a total of {totalSpritesAssigned} sprites.");
                }
                else
                {
                    Debug.LogWarning("No subfolders matched any ItemCategory, or no sprites were found.");
                }
            }
            else
            {
                Debug.LogWarning("Please assign a Parent Folder first!");
            }
        }
    }
}
#endif
