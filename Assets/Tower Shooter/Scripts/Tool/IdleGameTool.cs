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

    // Augment Effect Creation fields
    private string effectFolderPath = "Assets/Tower Shooter/Data/Augment/Effects";

    // Augment Folder & Container Selection fields
    private string augmentFolderPath = "Assets/Tower Shooter/Data/Augment";
    private string augmentContainerPath = "Assets/Tower Shooter/Data/Augment/Augment Container.asset";

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
        GUILayout.Label("Generic Augment Effect Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        effectFolderPath = EditorGUILayout.TextField("Target Folder Path", effectFolderPath);

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

        // Find all Augment assets in the target folder
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

                // Assign sequential ID based on index
                augment.id = i;
                EditorUtility.SetDirty(augment);

                // Add to AugmentContainer list
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

        // Ensure target folder exists
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        // 1. Create a new instance of GenericAugmentEffect
        GenericAugmentEffect newEffect = ScriptableObject.CreateInstance<GenericAugmentEffect>();
        newEffect.config = new GenericAugmentData();

        // 2. Set default effect color (#C85AFF)
        if (ColorUtility.TryParseHtmlString("#C85AFF", out Color defaultColor))
        {
            newEffect.effectColor = defaultColor;
        }

        // 3. Randomize config properties
        newEffect.config.trigger = GetRandomEnumValue<EffectTrigger>();
        newEffect.config.scope = GetRandomEnumValue<EffectScope>();
        newEffect.config.cycleFrequency = Random.Range(1, 10);
        newEffect.config.sourceStat = GetRandomEnumValue<StatType>();
        newEffect.config.targetStat = GetRandomEnumValue<StatType>();
        newEffect.config.operation = GetRandomEnumValue<EffectOperation>();
        newEffect.config.scalingFunction = GetRandomEnumValue<ScalingFunction>();
        newEffect.config.baseValue = Mathf.Round(Random.Range(1f, 50f) * 10f) / 10f;
        newEffect.config.scalingFactor = Mathf.Round(Random.Range(0.05f, 0.5f) * 100f) / 100f;
        newEffect.config.condition = GetRandomEnumValue<ConditionType>();
        newEffect.config.comparisonStat = GetRandomEnumValue<StatType>();
        newEffect.config.thresholdValue = Mathf.Round(Random.Range(10f, 100f) * 10f) / 10f;
        newEffect.config.durationType = GetRandomEnumValue<EffectDuration>();
        newEffect.config.durationInSeconds = Mathf.Round(Random.Range(2f, 15f) * 10f) / 10f;
        newEffect.config.decayRatePerSecond = Mathf.Round(Random.Range(0.1f, 2f) * 10f) / 10f;
        newEffect.config.maxStacks = Random.Range(1, 10);

        // 4. Auto-assign effectName based on generated config
        newEffect.effectName = newEffect.GenerateAutoEffectName(newEffect.config);

        // 5. Generate asset path using the effectName (ensures unique filename)
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{newEffect.effectName}.asset");

        // 6. Save asset to disk
        AssetDatabase.CreateAsset(newEffect, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 7. Ping and select created asset in Project view
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newEffect;

        Debug.Log($"Created new randomized GenericAugmentEffect '{newEffect.effectName}' at '{assetPath}'.");
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

        // Retrieve all RectTransform components including those on child GameObjects
        RectTransform[] rectTransforms = Selection.activeTransform.GetComponentsInChildren<RectTransform>(true);

        if (rectTransforms.Length == 0)
        {
            EditorUtility.DisplayDialog("No RectTransforms Found", "The selected object and its children do not contain any RectTransform components.", "OK");
            return;
        }

        // Register action in Undo history so changes can be reverted with Ctrl+Z / Cmd+Z
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

            // Mark object dirty so Unity detects the change for saving
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
