#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class IdleGameTool : EditorWindow
{
    private float scaleFactor = 1.0f;
    private bool scalePosition = true;
    private bool scaleSizeDelta = true;

    // Material JSON modification fields
    private Material targetMaterial;
    private TextAsset jsonFile;
    private string rawJsonInput = "";

    // Augment JSON Creation fields
    private AugmentContainer customAugmentContainer;
    private string newAugmentSaveFolder = "Assets/Tower Shooter/Data/Augment";
    private string effectFolderPath = "Assets/Tower Shooter/Data/Augment/Effects";
    private TextAsset augmentJsonFile;
    private string rawAugmentJsonInput = "";

    // Effect Picker Dropdown selection index
    private int selectedEffectIndex = 0;

    // Augment Folder & Container Selection fields (for Auto-Assigner section)
    private string augmentFolderPath = "Assets/Tower Shooter/Data/Augment";
    private string augmentContainerPath = "Assets/Tower Shooter/Data/Augment/Augment Container.asset";

    // Allowed 4 stats restriction
    private static readonly StatType[] AllowedStats = new StatType[]
    {
        StatType.Damage,
        StatType.AttackSpeed,
        StatType.CritChance,
        StatType.CritMultiplier
    };

    [System.Serializable]
    public class ColorProperty
    {
        public string name;
        public Color value;
    }

    [System.Serializable]
    public class FloatProperty
    {
        public string name;
        public float value;
    }

    [System.Serializable]
    public class MaterialPropertiesData
    {
        public ColorProperty[] colors;
        public FloatProperty[] floats;
    }

    // DTO for JSON import to create Augments
    [System.Serializable]
    public class AugmentJsonData
    {
        public string augmentName;
        public string customNotesTemplate;
        public AugmentQuality augmentQuality;
        public SpaceObjectType spaceObjectType;
        public bool isNeedCharacterToApply = true;
        public bool isApplyImmediatelyAfterPick = true;
        public bool isForNodeGroup;
        public List<StatModifierData> statModifiers;
        public GenericAugmentData effectConfig; // Optional embedded effect data
        public string effectColorHex = "#C85AFF";
    }

    [MenuItem("Saferio/Tools/IdleGameTool")]
    public static void ShowWindow()
    {
        GetWindow<IdleGameTool>("IdleGameTool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Augment Auto-Assigner", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        augmentFolderPath = EditorGUILayout.TextField("Augment Folder Path", augmentFolderPath);

        if (GUILayout.Button("Select Augment Folder"))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Folder with Augments", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    augmentFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Folder", "Please select a folder inside the Assets directory.", "OK");
                }
            }
        }

        augmentContainerPath = EditorGUILayout.TextField("Container Asset Path", augmentContainerPath);

        if (GUILayout.Button("Auto Assign Augments & IDs"))
        {
            AutoAssignIdForAugment();
        }

        EditorGUILayout.Space(10);
        GUILayout.Label("Read JSON to Create Augment", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // 1. Input Augment Container
        customAugmentContainer = (AugmentContainer)EditorGUILayout.ObjectField("Target Augment Container", customAugmentContainer, typeof(AugmentContainer), false);

        // 2. Input Save Folder for Newly Created Augments
        EditorGUILayout.BeginHorizontal();
        newAugmentSaveFolder = EditorGUILayout.TextField("Augment Save Folder", newAugmentSaveFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Augment Save Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            {
                newAugmentSaveFolder = "Assets" + path.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();

        // 3. Input Augment Effect Folder
        EditorGUILayout.BeginHorizontal();
        effectFolderPath = EditorGUILayout.TextField("Augment Effect Folder", effectFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Effect Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            {
                effectFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();

        // Pick existing AugmentEffect from specified effect folder
        List<AugmentEffect> availableEffects = GetAvailableEffectsFromFolder(effectFolderPath);
        string[] effectOptions = new string[availableEffects.Count + 1];
        effectOptions[0] = "-- None (Use JSON embedded effect if present) --";
        for (int i = 0; i < availableEffects.Count; i++)
        {
            effectOptions[i + 1] = availableEffects[i].name;
        }

        selectedEffectIndex = EditorGUILayout.Popup("Pick Existing Effect", selectedEffectIndex, effectOptions);
        if (selectedEffectIndex > availableEffects.Count) selectedEffectIndex = 0;

        EditorGUILayout.Space(5);
        augmentJsonFile = (TextAsset)EditorGUILayout.ObjectField("Augment JSON Asset", augmentJsonFile, typeof(TextAsset), false);
        EditorGUILayout.LabelField("Augment Raw JSON Input:");
        rawAugmentJsonInput = EditorGUILayout.TextArea(rawAugmentJsonInput, GUILayout.Height(80));

        if (GUILayout.Button("Create Augment from JSON"))
        {
            AugmentEffect pickedEffect = (selectedEffectIndex > 0 && selectedEffectIndex <= availableEffects.Count)
                ? availableEffects[selectedEffectIndex - 1]
                : null;

            CreateAugmentFromJSON(pickedEffect);
        }

        EditorGUILayout.Space(10);
        GUILayout.Label("Generic Augment Effect Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Create & Randomize New Generic Augment Effect"))
        {
            CreateAndRandomizeGenericAugmentEffect();
        }

        EditorGUILayout.Space(10);
        GUILayout.Label("Scale RectTransform Hierarchy", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        scaleFactor = EditorGUILayout.FloatField("Scale Factor", scaleFactor);
        scalePosition = EditorGUILayout.Toggle("Scale Anchored Position", scalePosition);
        scaleSizeDelta = EditorGUILayout.Toggle("Scale Size Delta", scaleSizeDelta);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Scale Selected & Children"))
        {
            ScaleSelectedHierarchy();
        }

        EditorGUILayout.Space(15);
        GUILayout.Label("Modify Material Properties from JSON", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        targetMaterial = (Material)EditorGUILayout.ObjectField("Target Material", targetMaterial, typeof(Material), false);
        jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON Asset (Optional)", jsonFile, typeof(TextAsset), false);

        EditorGUILayout.LabelField("JSON String Input:");
        rawJsonInput = EditorGUILayout.TextArea(rawJsonInput, GUILayout.Height(100));

        if (GUILayout.Button("Apply Material Properties JSON"))
        {
            ApplyMaterialPropertiesFromJSON();
        }
    }

    private List<AugmentEffect> GetAvailableEffectsFromFolder(string folderPath)
    {
        List<AugmentEffect> effects = new List<AugmentEffect>();
        if (!AssetDatabase.IsValidFolder(folderPath)) return effects;

        string[] guids = AssetDatabase.FindAssets("t:AugmentEffect", new[] { folderPath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AugmentEffect eff = AssetDatabase.LoadAssetAtPath<AugmentEffect>(path);
            if (eff != null)
            {
                effects.Add(eff);
            }
        }
        return effects;
    }

    private void CreateAugmentFromJSON(AugmentEffect selectedEffect)
    {
        string jsonToParse = rawAugmentJsonInput;
        if (augmentJsonFile != null && !string.IsNullOrEmpty(augmentJsonFile.text))
        {
            jsonToParse = augmentJsonFile.text;
        }

        if (string.IsNullOrWhiteSpace(jsonToParse))
        {
            EditorUtility.DisplayDialog("Empty JSON", "Please paste JSON string or assign an Augment JSON TextAsset.", "OK");
            return;
        }

        try
        {
            AugmentJsonData data = JsonUtility.FromJson<AugmentJsonData>(jsonToParse);
            if (data == null)
            {
                EditorUtility.DisplayDialog("Parsing Error", "Could not parse Augment JSON.", "OK");
                return;
            }

            string targetFolder = string.IsNullOrWhiteSpace(newAugmentSaveFolder) ? "Assets" : newAugmentSaveFolder.TrimEnd('/', '\\');
            if (!AssetDatabase.IsValidFolder(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
                AssetDatabase.Refresh();
            }

            // Create new Augment ScriptableObject
            Augment newAugment = ScriptableObject.CreateInstance<Augment>();
            newAugment.augmentName = string.IsNullOrEmpty(data.augmentName) ? "New Augment" : data.augmentName;
            newAugment.augmentQuality = data.augmentQuality;
            newAugment.spaceObjectType = data.spaceObjectType;
            newAugment.isNeedCharacterToApply = data.isNeedCharacterToApply;
            newAugment.isApplyImmediatelyAfterPick = data.isApplyImmediatelyAfterPick;
            newAugment.isForNodeGroup = data.isForNodeGroup;

            // Enforce the 4 allowed stats for direct stat modifiers
            if (data.statModifiers != null)
            {
                foreach (var mod in data.statModifiers)
                {
                    if (System.Array.Exists(AllowedStats, s => s == mod.statType))
                    {
                        newAugment.statModifiers.Add(mod);
                    }
                    else
                    {
                        Debug.LogWarning($"Stat '{mod.statType}' is not in the allowed 4 stats list. Skipping modifier.");
                    }
                }
            }

            // Priority 1: Attach picked existing effect from folder dropdown
            if (selectedEffect != null)
            {
                newAugment.dynamicEffects.Add(selectedEffect);
            }
            // Priority 2: Fallback to creating effect from embedded JSON config if present
            else if (data.effectConfig != null)
            {
                data.effectConfig.sourceStat = ValidateStat(data.effectConfig.sourceStat);
                data.effectConfig.targetStat = ValidateStat(data.effectConfig.targetStat);
                data.effectConfig.comparisonStat = ValidateStat(data.effectConfig.comparisonStat);

                string effFolderPath = string.IsNullOrWhiteSpace(effectFolderPath) ? targetFolder : effectFolderPath.TrimEnd('/', '\\');
                if (!AssetDatabase.IsValidFolder(effFolderPath))
                {
                    Directory.CreateDirectory(effFolderPath);
                    AssetDatabase.Refresh();
                }

                GenericAugmentEffect effectAsset = ScriptableObject.CreateInstance<GenericAugmentEffect>();
                effectAsset.config = data.effectConfig;

                if (ColorUtility.TryParseHtmlString(data.effectColorHex, out Color effColor))
                {
                    effectAsset.effectColor = effColor;
                }

                effectAsset.effectName = effectAsset.GenerateAutoEffectName(effectAsset.config);

                string effectPath = AssetDatabase.GenerateUniqueAssetPath($"{effFolderPath}/{effectAsset.effectName}.asset");
                AssetDatabase.CreateAsset(effectAsset, effectPath);

                newAugment.dynamicEffects.Add(effectAsset);
            }

            string augmentPath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/{newAugment.augmentName}.asset");
            AssetDatabase.CreateAsset(newAugment, augmentPath);

            // Register into target AugmentContainer if assigned
            if (customAugmentContainer != null)
            {
                Undo.RegisterCompleteObjectUndo(customAugmentContainer, "Add New Augment to Container");
                if (customAugmentContainer.augments == null)
                {
                    customAugmentContainer.augments = new List<Augment>();
                }

                newAugment.id = customAugmentContainer.augments.Count;
                customAugmentContainer.augments.Add(newAugment);
                EditorUtility.SetDirty(customAugmentContainer);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = newAugment;
            Debug.Log($"Successfully created Augment asset '{newAugment.augmentName}' at '{augmentPath}'.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error creating Augment from JSON: {ex.Message}");
        }
    }

    private void AutoAssignIdForAugment()
    {
        AugmentContainer augmentContainer = AssetDatabase.LoadAssetAtPath<AugmentContainer>(augmentContainerPath);

        if (augmentContainer == null)
        {
            EditorUtility.DisplayDialog("Error", $"AugmentContainer asset not found at path: {augmentContainerPath}", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(augmentFolderPath))
        {
            EditorUtility.DisplayDialog("Error", $"Invalid folder path: {augmentFolderPath}", "OK");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Augment", new[] { augmentFolderPath });

        if (augmentContainer.augments == null)
        {
            augmentContainer.augments = new List<Augment>();
        }
        else
        {
            augmentContainer.augments.Clear();
        }

        Undo.RegisterCompleteObjectUndo(augmentContainer, "Auto Assign Augments to Container");

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            Augment augment = AssetDatabase.LoadAssetAtPath<Augment>(assetPath);

            if (augment != null)
            {
                Undo.RegisterCompleteObjectUndo(augment, "Auto Assign ID");

                augment.id = i;
                EditorUtility.SetDirty(augment);

                augmentContainer.augments.Add(augment);
            }
        }

        EditorUtility.SetDirty(augmentContainer);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Successfully auto-assigned {augmentContainer.augments.Count} Augment(s) to container and assigned IDs from 0 to {augmentContainer.augments.Count - 1}.");
    }

    private void CreateAndRandomizeGenericAugmentEffect()
    {
        string folderPath = string.IsNullOrWhiteSpace(effectFolderPath) ? "Assets" : effectFolderPath.TrimEnd('/', '\\');

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        GenericAugmentEffect newEffect = ScriptableObject.CreateInstance<GenericAugmentEffect>();
        newEffect.config = new GenericAugmentData();

        if (ColorUtility.TryParseHtmlString("#C85AFF", out Color defaultColor))
        {
            newEffect.effectColor = defaultColor;
        }

        // Randomize restricted to the 4 allowed stats
        newEffect.config.trigger = GetRandomEnumValue<EffectTrigger>();
        newEffect.config.scope = GetRandomEnumValue<EffectScope>();
        newEffect.config.cycleFrequency = Random.Range(1, 10);
        newEffect.config.sourceStat = GetRandomAllowedStat();
        newEffect.config.targetStat = GetRandomAllowedStat();
        newEffect.config.operation = GetRandomEnumValue<EffectOperation>();
        newEffect.config.scalingFunction = GetRandomEnumValue<ScalingFunction>();
        newEffect.config.baseValue = Mathf.Round(Random.Range(1f, 50f) * 10f) / 10f;
        newEffect.config.scalingFactor = Mathf.Round(Random.Range(0.05f, 0.5f) * 100f) / 100f;
        newEffect.config.condition = GetRandomEnumValue<ConditionType>();
        newEffect.config.comparisonStat = GetRandomAllowedStat();
        newEffect.config.thresholdValue = Mathf.Round(Random.Range(10f, 100f) * 10f) / 10f;
        newEffect.config.durationType = GetRandomEnumValue<EffectDuration>();
        newEffect.config.durationInSeconds = Mathf.Round(Random.Range(2f, 15f) * 10f) / 10f;
        newEffect.config.decayRatePerSecond = Mathf.Round(Random.Range(0.1f, 2f) * 10f) / 10f;
        newEffect.config.maxStacks = Random.Range(1, 10);

        newEffect.effectName = newEffect.GenerateAutoEffectName(newEffect.config);

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{newEffect.effectName}.asset");

        AssetDatabase.CreateAsset(newEffect, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newEffect;

        Debug.Log($"Created new randomized GenericAugmentEffect '{newEffect.effectName}' at '{assetPath}'.");
    }

    private StatType GetRandomAllowedStat()
    {
        return AllowedStats[Random.Range(0, AllowedStats.Length)];
    }

    private StatType ValidateStat(StatType type)
    {
        if (System.Array.Exists(AllowedStats, s => s == type))
        {
            return type;
        }
        return StatType.Damage;
    }

    private T GetRandomEnumValue<T>() where T : System.Enum
    {
        System.Array values = System.Enum.GetValues(typeof(T));
        return (T)values.GetValue(Random.Range(0, values.Length));
    }

    private void ScaleSelectedHierarchy()
    {
        if (Selection.activeTransform == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select a GameObject in the Hierarchy.", "OK");
            return;
        }

        if (scaleFactor <= 0)
        {
            EditorUtility.DisplayDialog("Invalid Scale", "Scale factor should be greater than 0.", "OK");
            return;
        }

        RectTransform[] rectTransforms = Selection.activeTransform.GetComponentsInChildren<RectTransform>(true);

        if (rectTransforms.Length == 0)
        {
            EditorUtility.DisplayDialog("No RectTransforms Found", "The selected object and its children do not contain any RectTransform components.", "OK");
            return;
        }

        Undo.RegisterCompleteObjectUndo(rectTransforms, "Scale RectTransforms");

        int modifiedCount = 0;
        foreach (RectTransform rect in rectTransforms)
        {
            if (scalePosition)
            {
                rect.anchoredPosition *= scaleFactor;
            }

            if (scaleSizeDelta)
            {
                rect.sizeDelta *= scaleFactor;
            }

            EditorUtility.SetDirty(rect);
            modifiedCount++;
        }

        Debug.Log($"Successfully scaled {modifiedCount} RectTransform(s) by factor of {scaleFactor}.");
    }

    private void ApplyMaterialPropertiesFromJSON()
    {
        if (targetMaterial == null)
        {
            EditorUtility.DisplayDialog("Missing Material", "Please assign a Target Material.", "OK");
            return;
        }

        string jsonToParse = rawJsonInput;

        if (jsonFile != null && !string.IsNullOrEmpty(jsonFile.text))
        {
            jsonToParse = jsonFile.text;
        }

        if (string.IsNullOrWhiteSpace(jsonToParse))
        {
            EditorUtility.DisplayDialog("Empty JSON", "Please paste a JSON string or assign a valid JSON TextAsset.", "OK");
            return;
        }

        try
        {
            MaterialPropertiesData data = JsonUtility.FromJson<MaterialPropertiesData>(jsonToParse);

            if (data == null)
            {
                EditorUtility.DisplayDialog("Parsing Error", "Could not parse JSON. Check format.", "OK");
                return;
            }

            Undo.RegisterCompleteObjectUndo(targetMaterial, "Apply JSON to Material Properties");

            if (data.colors != null)
            {
                foreach (var prop in data.colors)
                {
                    if (targetMaterial.HasProperty(prop.name))
                    {
                        targetMaterial.SetColor(prop.name, prop.value);
                    }
                }
            }

            if (data.floats != null)
            {
                foreach (var prop in data.floats)
                {
                    if (targetMaterial.HasProperty(prop.name))
                    {
                        targetMaterial.SetFloat(prop.name, prop.value);
                    }
                }
            }

            EditorUtility.SetDirty(targetMaterial);
            AssetDatabase.SaveAssets();
            Debug.Log($"Successfully updated material properties on '{targetMaterial.name}'.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing material JSON: {ex.Message}");
        }
    }
}
#endif
