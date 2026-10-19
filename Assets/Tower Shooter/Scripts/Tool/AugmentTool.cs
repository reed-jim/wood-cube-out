using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class AugmentTool : EditorWindow
{
    private string searchString = "";
    private AugmentQuality? qualityFilter = null;
    private SpaceObjectType? typeFilter = null;
    private Vector2 scrollPos;

    [SerializeField] private AugmentContainer augmentContainer;

    // Generator Settings
    private bool showGeneratorPanel = false;
    private int generationCount = 20;
    private string saveFolderPath = "Assets/ScriptableObjects/Augments";
    private AugmentQuality defaultGeneratedQuality = AugmentQuality.Common;
    private SpaceObjectType defaultGeneratedType = SpaceObjectType.Earth;

    // Distribution Weights (Percentages adding up to 100%)
    private int weight1Mod = 25;
    private int weight2Mod = 25;
    private int weight3Mod = 25;
    private int weight4Mod = 25;

    // UI Styles Cache
    private GUIStyle headerStyle;
    private GUIStyle subHeaderStyle;
    private GUIStyle cardStyle;
    private GUIStyle countButtonStyle;
    private GUIStyle generatorBoxStyle;
    private GUIStyle mainHeaderStyle;
    private bool stylesInitialized = false;

    // Fixed display order for sorting stats: Damage -> AttackSpeed -> CritChance -> CritMultiplier
    private static readonly Dictionary<StatType, int> StatDisplayOrder = new Dictionary<StatType, int>
    {
        { StatType.Damage, 0 },
        { StatType.AttackSpeed, 1 },
        { StatType.CritChance, 2 },
        { StatType.CritMultiplier, 3 }
    };

    [MenuItem("Tools/Augment Dashboard &A")]
    public static void ShowWindow() => GetWindow<AugmentTool>("🌌 Augment Command Center").minSize = new Vector2(760, 520);

    private void InitStyles()
    {
        if (stylesInitialized) return;

        mainHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            richText = true,
            normal = { textColor = new Color(0.4f, 0.85f, 1.0f) }
        };

        headerStyle = new GUIStyle(EditorStyles.boldLabel) 
        { 
            fontSize = 12, 
            richText = true, 
            normal = { textColor = new Color(0.9f, 0.95f, 1.0f) } 
        };
        
        subHeaderStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 10,
            wordWrap = true,
            richText = true,
            normal = { textColor = new Color(0.65f, 0.75f, 0.85f) }
        };

        cardStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(12, 12, 10, 10),
            margin = new RectOffset(6, 6, 4, 4)
        };

        countButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
        {
            richText = true,
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter
        };

        generatorBoxStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(14, 14, 12, 12),
            margin = new RectOffset(6, 6, 6, 6)
        };

        stylesInitialized = true;
    }

    private void OnGUI()
    {
        InitStyles();
        DrawSpaceBackground();
        DrawToolbar();

        if (augmentContainer == null)
        {
            EditorGUILayout.Space(25);
            EditorGUILayout.HelpBox("⚠️ NO AUGMENT CONTAINER DETECTED.\nPlease assign your AugmentContainer asset above to initialize radar scanning.", MessageType.Warning);
            return;
        }

        DrawGeneratorPanel();
        DrawSpaceCountersBar();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        if (augmentContainer.augments != null)
        {
            var filteredList = augmentContainer.augments.Where(a => a != null &&
                (string.IsNullOrEmpty(searchString) || 
                 a.augmentName.Contains(searchString, System.StringComparison.OrdinalIgnoreCase) || 
                 a.name.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)) &&
                (!qualityFilter.HasValue || a.augmentQuality == qualityFilter.Value) &&
                (!typeFilter.HasValue || a.spaceObjectType == typeFilter.Value)
            ).ToList();

            if (filteredList.Count == 0)
            {
                EditorGUILayout.Space(25);
                EditorGUILayout.HelpBox("🛰️ No matching augments found in sector database.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.Space(4);
                foreach (var aug in filteredList)
                {
                    DrawAugmentCard(aug);
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawSpaceBackground()
    {
        Rect bgRect = new Rect(0, 0, position.width, position.height);
        EditorGUI.DrawRect(bgRect, new Color(0.10f, 0.12f, 0.16f, 1f));
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(28));
        
        augmentContainer = (AugmentContainer)EditorGUILayout.ObjectField(augmentContainer, typeof(AugmentContainer), false, GUILayout.Width(180));
        
        searchString = EditorGUILayout.TextField(searchString, EditorStyles.toolbarSearchField, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
        if (GUILayout.Button("", EditorStyles.toolbarSearchField, GUILayout.Width(20))) { searchString = ""; GUI.FocusControl(null); }

        string qLabel = qualityFilter.HasValue ? qualityFilter.Value.ToString() : "All Qualities";
        if (EditorGUILayout.DropdownButton(new GUIContent($"Quality: {qLabel}"), FocusType.Passive, GUILayout.Width(125)))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("All Qualities"), !qualityFilter.HasValue, () => qualityFilter = null);
            foreach (AugmentQuality q in System.Enum.GetValues(typeof(AugmentQuality)))
                menu.AddItem(new GUIContent(q.ToString()), qualityFilter == q, () => qualityFilter = q);
            menu.ShowAsContext();
        }

        string tLabel = typeFilter.HasValue ? typeFilter.Value.ToString() : "All Types";
        if (EditorGUILayout.DropdownButton(new GUIContent($"Type: {tLabel}"), FocusType.Passive, GUILayout.Width(110)))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("All Types"), !typeFilter.HasValue, () => typeFilter = null);
            foreach (SpaceObjectType t in System.Enum.GetValues(typeof(SpaceObjectType)))
                menu.AddItem(new GUIContent(t.ToString()), typeFilter == t, () => typeFilter = t);
            menu.ShowAsContext();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawGeneratorPanel()
    {
        showGeneratorPanel = EditorGUILayout.Foldout(showGeneratorPanel, "⚙️ Procedural Unique Augment Generator & Distribution", true, EditorStyles.foldoutHeader);
        if (!showGeneratorPanel) return;

        EditorGUILayout.BeginVertical(generatorBoxStyle);
        {
            EditorGUILayout.LabelField("Generator Configuration", mainHeaderStyle);
            EditorGUILayout.Space(4);

            generationCount = EditorGUILayout.IntField("Generate Count (n max 200)", generationCount);
            generationCount = Mathf.Clamp(generationCount, 1, 200);
            
            EditorGUILayout.BeginHorizontal();
            saveFolderPath = EditorGUILayout.TextField("Base Save Folder", saveFolderPath);
            if (GUILayout.Button("Browse", GUILayout.Width(70), GUILayout.Height(18)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Augment Save Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath) && selectedPath.StartsWith(Application.dataPath))
                {
                    saveFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();

            defaultGeneratedQuality = (AugmentQuality)EditorGUILayout.EnumPopup("Default Quality", defaultGeneratedQuality);
            defaultGeneratedType = (SpaceObjectType)EditorGUILayout.EnumPopup("Default Sector Type", defaultGeneratedType);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Modifier Count Percentage Distribution (%)", mainHeaderStyle);
            
            EditorGUILayout.BeginHorizontal();
            weight1Mod = EditorGUILayout.IntField("1 Mod", weight1Mod);
            weight2Mod = EditorGUILayout.IntField("2 Mod", weight2Mod);
            weight3Mod = EditorGUILayout.IntField("3 Mod", weight3Mod);
            weight4Mod = EditorGUILayout.IntField("4 Mod", weight4Mod);
            EditorGUILayout.EndHorizontal();

            int totalWeight = weight1Mod + weight2Mod + weight3Mod + weight4Mod;
            if (totalWeight != 100)
            {
                EditorGUILayout.HelpBox($"⚠️ Total distribution weights equal {totalWeight}%. They will be normalized automatically.", MessageType.Warning);
            }

            EditorGUILayout.Space(8);
            if (GUILayout.Button("🚀 Generate & Organize Unique Augments", GUILayout.Height(32)))
            {
                GenerateUniqueAugments();
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(6);
    }

    private void GenerateUniqueAugments()
    {
        if (augmentContainer == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign an AugmentContainer first.", "OK");
            return;
        }

        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
        }

        int startingId = 0;
        if (augmentContainer.augments != null && augmentContainer.augments.Count > 0)
        {
            startingId = augmentContainer.augments.Max(a => a != null ? a.id : 0) + 1;
        }

        StatType[] allStats = { StatType.Damage, StatType.AttackSpeed, StatType.CritChance, StatType.CritMultiplier };
        ModifierType[] allModTypes = System.Enum.GetValues(typeof(ModifierType)).Cast<ModifierType>().ToArray();
        float[] allValues = { 1f, -1f };

        // Build pools categorized by modifier count (1, 2, 3, 4) to ensure sorting by modifier count (smaller modifiers first = smaller IDs/names)
        Dictionary<int, List<List<StatModifierData>>> modifierPools = new Dictionary<int, List<List<StatModifierData>>>
        {
            { 1, new List<List<StatModifierData>>() },
            { 2, new List<List<StatModifierData>>() },
            { 3, new List<List<StatModifierData>>() },
            { 4, new List<List<StatModifierData>>() }
        };

        for (int k = 1; k <= 4; k++)
        {
            foreach (var statComb in GetCombinations(allStats, k))
            {
                var variations = GenerateModVariations(statComb, allModTypes, allValues);
                modifierPools[k].AddRange(variations);
            }
            // Shuffle each individual pool
            modifierPools[k] = modifierPools[k].OrderBy(_ => Random.value).ToList();
        }

        // Normalize weights
        float totalW = Mathf.Max(1, weight1Mod + weight2Mod + weight3Mod + weight4Mod);
        int target1 = Mathf.RoundToInt(generationCount * (weight1Mod / totalW));
        int target2 = Mathf.RoundToInt(generationCount * (weight2Mod / totalW));
        int target3 = Mathf.RoundToInt(generationCount * (weight3Mod / totalW));
        int target4 = Mathf.Max(0, generationCount - (target1 + target2 + target3));

        List<List<StatModifierData>> selectedCombinations = new List<List<StatModifierData>>();
        
        // Gather in exact ascending order of modifier count so smaller modifier counts get lower IDs/names
        if (target1 > 0) selectedCombinations.AddRange(modifierPools[1].Take(target1));
        if (target2 > 0) selectedCombinations.AddRange(modifierPools[2].Take(target2));
        if (target3 > 0) selectedCombinations.AddRange(modifierPools[3].Take(target3));
        if (target4 > 0) selectedCombinations.AddRange(modifierPools[4].Take(target4));

        int createdCount = 0;

        for (int i = 0; i < selectedCombinations.Count; i++)
        {
            int currentId = startingId + i;
            
            Augment newAugment = CreateInstance<Augment>();
            newAugment.id = currentId;
            newAugment.augmentName = $"Ag {currentId}";
            newAugment.augmentQuality = defaultGeneratedQuality;
            newAugment.spaceObjectType = defaultGeneratedType;

            // Sort stat modifiers: Damage -> AttackSpeed -> CritChance -> CritMultiplier
            newAugment.statModifiers = selectedCombinations[i]
                .OrderBy(m => StatDisplayOrder.ContainsKey(m.statType) ? StatDisplayOrder[m.statType] : 99)
                .ToList();

            // Organize assets into sub-folders based on modifier count
            int modCount = newAugment.statModifiers.Count;
            string subfolderName = $"{modCount}_Modifier{(modCount > 1 ? "s" : "")}";
            string targetFolder = Path.Combine(saveFolderPath, subfolderName);
            
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            string assetPath = Path.Combine(targetFolder, $"Ag_{currentId}.asset");
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            
            AssetDatabase.CreateAsset(newAugment, assetPath);

            if (augmentContainer.augments == null)
            {
                augmentContainer.augments = new List<Augment>();
            }
            augmentContainer.augments.Add(newAugment);
            createdCount++;
        }

        EditorUtility.SetDirty(augmentContainer);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Successfully generated {createdCount} unique sorted augments based on your distribution profile!");
    }

    private IEnumerable<IEnumerable<StatType>> GetCombinations(StatType[] list, int length)
    {
        if (length == 1) return list.Select(t => new StatType[] { t });
        return GetCombinations(list, length - 1)
            .SelectMany(t => list.Where(o => !t.Contains(o)), (t1, t2) => t1.Append(t2));
    }

    private List<List<StatModifierData>> GenerateModVariations(IEnumerable<StatType> stats, ModifierType[] modTypes, float[] values)
    {
        var statList = stats.ToList();
        List<List<StatModifierData>> results = new List<List<StatModifierData>> { new List<StatModifierData>() };

        foreach (var stat in statList)
        {
            List<List<StatModifierData>> nextResults = new List<List<StatModifierData>>();
            foreach (var existing in results)
            {
                foreach (var mType in modTypes)
                {
                    foreach (var val in values)
                    {
                        var copy = new List<StatModifierData>(existing);
                        copy.Add(new StatModifierData { statType = stat, modType = mType, value = val });
                        nextResults.Add(copy);
                    }
                }
            }
            results = nextResults;
        }

        return results;
    }

    private void DrawSpaceCountersBar()
    {
        if (augmentContainer.augments == null || augmentContainer.augments.Count == 0) return;

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(28));
        GUILayout.Label($"<b>⚡ TOTAL: {augmentContainer.augments.Count}</b>", headerStyle, GUILayout.Width(95));

        foreach (AugmentQuality q in System.Enum.GetValues(typeof(AugmentQuality)))
        {
            int count = augmentContainer.augments.Count(a => a != null && a.augmentQuality == q);
            if (count > 0)
            {
                Color c = GetQualityColor(q);
                string hexColor = ColorUtility.ToHtmlStringRGB(c);
                
                if (GUILayout.Button($"<color=#{hexColor}><b>{q}: {count}</b></color>", countButtonStyle))
                {
                    qualityFilter = (qualityFilter == q) ? null : q; 
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
    }

    private void DrawAugmentCard(Augment aug)
    {
        Color originalBg = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.15f, 0.18f, 0.24f, 1f);

        EditorGUILayout.BeginHorizontal(cardStyle);
        {
            Color qColor = GetQualityColor(aug.augmentQuality);
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(5, 30, GUILayout.ExpandWidth(false)), qColor);

            EditorGUILayout.BeginVertical();
            {
                string displayName = string.IsNullOrEmpty(aug.augmentName) ? aug.name : aug.augmentName;
                EditorGUILayout.LabelField(displayName, headerStyle);

                string qHex = ColorUtility.ToHtmlStringRGB(qColor);
                int modCount = aug.statModifiers != null ? aug.statModifiers.Count : 0;
                EditorGUILayout.LabelField($"Quality: <color=#{qHex}><b>{aug.augmentQuality}</b></color>  |  Type: <color=#00E5FF><b>{aug.spaceObjectType}</b></color>  |  Modifiers: <b>{modCount}</b>", subHeaderStyle);
            }
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("📡 Ping", GUILayout.Width(65), GUILayout.Height(26)))
            {
                EditorGUIUtility.PingObject(aug);
                Selection.activeObject = aug;
            }
        }
        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = originalBg;
    }

    private Color GetQualityColor(AugmentQuality q) => q switch
    {
        AugmentQuality.Common => new Color(0.65f, 0.65f, 0.65f),
        AugmentQuality.Uncommon => new Color(0.15f, 0.9f, 0.4f),
        AugmentQuality.Rare => new Color(0.1f, 0.6f, 1.0f),
        AugmentQuality.Epic => new Color(0.7f, 0.3f, 1.0f),
        AugmentQuality.Legendary => new Color(1.0f, 0.6f, 0.1f),
        AugmentQuality.Mythic => new Color(1.0f, 0.2f, 0.3f),
        _ => Color.white
    };
}