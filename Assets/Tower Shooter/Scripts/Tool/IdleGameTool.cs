#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class IdleGameTool : EditorWindow
{
    // Scroll position
    private Vector2 mainScrollPosition;

    // Foldout visibility states
    private bool showAugmentAssignerGroup = true;
    private bool showAugmentCreatorGroup = true;
    private bool showMaterialCreatorGroup = true;
    private bool showEffectCreatorGroup = true;
    private bool showHierarchyScalerGroup = true;
    private bool showMaterialModifierGroup = true;

    // Scale RectTransform fields
    private float scaleFactor = 1.0f;
    private bool scalePosition = true;
    private bool scaleSizeDelta = true;

    // Material JSON modification fields
    private Material targetMaterial;
    private TextAsset jsonFile;
    private string rawJsonInput = "";

    // Material Creation fields
    private Shader planetShader;
    private TextAsset createMaterialJsonFile;
    private string rawCreateMaterialJsonInput = "";
    private string materialSaveFolder = "Assets/Materials";

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

    public enum PlanetTypeMode
    {
        Terrestrial = 0,
        GasGiant = 1,
        CrateredMoon = 2,
        LavaVolcanic = 3
    }

    [System.Serializable]
    public class PlanetBadgeMaterialData
    {
        public string materialName = "NewPlanetMat";
        public PlanetTypeMode planetType = PlanetTypeMode.Terrestrial;

        // Colors
        public string baseColorHex = "#0D3380";
        public string detailColorHex = "#1A9933";
        public string secondaryColorHex = "#E6F2FF";
        public string atmosphereColorHex = "#33B3FF";
        public string cloudColorHex = "#FFFFFFCC"; // Hex with Alpha
        public string emissiveColorHex = "#FF4500";

        // Terrain & Band Settings
        public float planetRadius = 0.38f;
        public float rotationSpeed = 0.2f;
        public float noiseScale = 3.5f;
        public float landThreshold = 0.5f;
        public float bandStretch = 10.0f;
        public float craterDensity = 6.0f;

        // Clouds & Glow
        public float cloudSpeed = 1.3f;
        public float cloudDensity = 0.45f;
        public float emissionPower = 0.0f;
        public float glowThickness = 0.08f;
        public float glowPower = 3.0f;

        // Lighting
        public Vector4 lightDir = new Vector4(0.5f, 0.5f, 1.0f, 0.0f);
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

    private void OnEnable()
    {
        if (planetShader == null)
        {
            planetShader = Shader.Find("UI/URP/PlanetGlow");
        }
    }

    private void OnGUI()
    {
        mainScrollPosition = EditorGUILayout.BeginScrollView(mainScrollPosition);

        EditorGUILayout.Space(8);

        // -------------------------------------------------------------
        // SECTION 1: Augment Auto-Assigner
        // -------------------------------------------------------------
        BeginCardSection("Augment Auto-Assigner", ref showAugmentAssignerGroup);
        if (showAugmentAssignerGroup)
        {
            augmentFolderPath = EditorGUILayout.TextField("Augment Folder Path", augmentFolderPath);

            if (GUILayout.Button("Browse Augment Folder", GUILayout.Height(22)))
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

            EditorGUILayout.Space(4);
            augmentContainerPath = EditorGUILayout.TextField("Container Asset Path", augmentContainerPath);

            EditorGUILayout.Space(6);
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("Auto Assign Augments & IDs", GUILayout.Height(26)))
            {
                AutoAssignIdForAugment();
            }
            GUI.backgroundColor = Color.white;
        }
        EndCardSection();

        // -------------------------------------------------------------
        // SECTION 2: Read JSON to Create Augment
        // -------------------------------------------------------------
        BeginCardSection("Read JSON to Create Augment", ref showAugmentCreatorGroup);
        if (showAugmentCreatorGroup)
        {
            customAugmentContainer = (AugmentContainer)EditorGUILayout.ObjectField("Target Container", customAugmentContainer, typeof(AugmentContainer), false);

            EditorGUILayout.BeginHorizontal();
            newAugmentSaveFolder = EditorGUILayout.TextField("Augment Save Folder", newAugmentSaveFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(65)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Augment Save Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    newAugmentSaveFolder = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            effectFolderPath = EditorGUILayout.TextField("Augment Effect Folder", effectFolderPath);
            if (GUILayout.Button("Browse", GUILayout.Width(65)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Effect Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    effectFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();

            List<AugmentEffect> availableEffects = GetAvailableEffectsFromFolder(effectFolderPath);
            string[] effectOptions = new string[availableEffects.Count + 1];
            effectOptions[0] = "-- None (Use JSON embedded effect) --";
            for (int i = 0; i < availableEffects.Count; i++)
            {
                effectOptions[i + 1] = availableEffects[i].name;
            }

            selectedEffectIndex = EditorGUILayout.Popup("Pick Existing Effect", selectedEffectIndex, effectOptions);
            if (selectedEffectIndex > availableEffects.Count) selectedEffectIndex = 0;

            EditorGUILayout.Space(4);
            augmentJsonFile = (TextAsset)EditorGUILayout.ObjectField("Augment JSON Asset", augmentJsonFile, typeof(TextAsset), false);

            EditorGUILayout.LabelField("Augment Raw JSON Input:");
            rawAugmentJsonInput = EditorGUILayout.TextArea(rawAugmentJsonInput, GUILayout.Height(70));

            EditorGUILayout.Space(6);
            GUI.backgroundColor = new Color(0.35f, 0.65f, 1.0f);
            if (GUILayout.Button("Create Augment from JSON", GUILayout.Height(26)))
            {
                AugmentEffect pickedEffect = (selectedEffectIndex > 0 && selectedEffectIndex <= availableEffects.Count)
                    ? availableEffects[selectedEffectIndex - 1]
                    : null;

                CreateAugmentFromJSON(pickedEffect);
            }
            GUI.backgroundColor = Color.white;
        }
        EndCardSection();

        // -------------------------------------------------------------
        // SECTION 3: Create New Material from JSON (Planet Badge)
        // -------------------------------------------------------------
        BeginCardSection("Create New Material from JSON (Planet Badge)", ref showMaterialCreatorGroup);
        if (showMaterialCreatorGroup)
        {
            planetShader = (Shader)EditorGUILayout.ObjectField("Planet Shader", planetShader, typeof(Shader), false);

            EditorGUILayout.BeginHorizontal();
            materialSaveFolder = EditorGUILayout.TextField("Material Save Folder", materialSaveFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(65)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Material Save Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    materialSaveFolder = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();

            createMaterialJsonFile = (TextAsset)EditorGUILayout.ObjectField("Material JSON Asset", createMaterialJsonFile, typeof(TextAsset), false);

            EditorGUILayout.LabelField("Material Raw JSON Input:");
            rawCreateMaterialJsonInput = EditorGUILayout.TextArea(rawCreateMaterialJsonInput, GUILayout.Height(70));

            EditorGUILayout.Space(6);
            GUI.backgroundColor = new Color(0.35f, 0.65f, 1.0f);
            if (GUILayout.Button("Create Material from JSON", GUILayout.Height(26)))
            {
                CreatePlanetMaterialFromJSON();
            }
            GUI.backgroundColor = Color.white;
        }
        EndCardSection();

        // -------------------------------------------------------------
        // SECTION 4: Generic Augment Effect Creator
        // -------------------------------------------------------------
        BeginCardSection("Generic Augment Effect Creator", ref showEffectCreatorGroup);
        if (showEffectCreatorGroup)
        {
            GUI.backgroundColor = new Color(0.9f, 0.6f, 0.3f);
            if (GUILayout.Button("Create & Randomize New Generic Augment Effect", GUILayout.Height(26)))
            {
                CreateAndRandomizeGenericAugmentEffect();
            }
            GUI.backgroundColor = Color.white;
        }
        EndCardSection();

        // -------------------------------------------------------------
        // SECTION 5: Scale RectTransform Hierarchy
        // -------------------------------------------------------------
        BeginCardSection("Scale RectTransform Hierarchy", ref showHierarchyScalerGroup);
        if (showHierarchyScalerGroup)
        {
            scaleFactor = EditorGUILayout.FloatField("Scale Factor", scaleFactor);
            scalePosition = EditorGUILayout.Toggle("Scale Anchored Position", scalePosition);
            scaleSizeDelta = EditorGUILayout.Toggle("Scale Size Delta", scaleSizeDelta);

            EditorGUILayout.Space(6);
            if (GUILayout.Button("Scale Selected & Children", GUILayout.Height(24)))
            {
                ScaleSelectedHierarchy();
            }
        }
        EndCardSection();

        // -------------------------------------------------------------
        // SECTION 6: Modify Material Properties from JSON
        // -------------------------------------------------------------
        BeginCardSection("Modify Material Properties from JSON", ref showMaterialModifierGroup);
        if (showMaterialModifierGroup)
        {
            targetMaterial = (Material)EditorGUILayout.ObjectField("Target Material", targetMaterial, typeof(Material), false);
            jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON Asset (Optional)", jsonFile, typeof(TextAsset), false);

            EditorGUILayout.LabelField("JSON String Input:");
            rawJsonInput = EditorGUILayout.TextArea(rawJsonInput, GUILayout.Height(80));

            EditorGUILayout.Space(6);
            if (GUILayout.Button("Apply Material Properties JSON", GUILayout.Height(24)))
            {
                ApplyMaterialPropertiesFromJSON();
            }
        }
        EndCardSection();

        EditorGUILayout.Space(8);
        EditorGUILayout.EndScrollView();
    }

    private void BeginCardSection(string title, ref bool foldoutState)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        foldoutState = EditorGUILayout.Foldout(foldoutState, title, true, EditorStyles.foldoutHeader);
        if (foldoutState)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);
        }
    }

    private void EndCardSection()
    {
        if (EditorGUI.indentLevel > 0)
        {
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(6);
    }

    private void CreatePlanetMaterialFromJSON()
    {
        if (planetShader == null) planetShader = Shader.Find("UI/URP/PlanetGlow");

        string jsonToParse = rawCreateMaterialJsonInput;
        if (createMaterialJsonFile != null && !string.IsNullOrEmpty(createMaterialJsonFile.text))
            jsonToParse = createMaterialJsonFile.text;

        if (string.IsNullOrWhiteSpace(jsonToParse)) return;

        try
        {
            PlanetBadgeMaterialData data = JsonUtility.FromJson<PlanetBadgeMaterialData>(jsonToParse);
            Material newMaterial = new Material(planetShader);

            // Enable Planet Type Keyword
            newMaterial.DisableKeyword("_PLANET_TYPE_TERRESTRIAL");
            newMaterial.DisableKeyword("_PLANET_TYPE_GASGIANT");
            newMaterial.DisableKeyword("_PLANET_TYPE_CRATEREDMOON");
            newMaterial.DisableKeyword("_PLANET_TYPE_LAVAVOLCANIC");

            switch (data.planetType)
            {
                case PlanetTypeMode.GasGiant:
                    newMaterial.EnableKeyword("_PLANET_TYPE_GASGIANT");
                    newMaterial.SetFloat("_PLANET_TYPE", 1);
                    break;
                case PlanetTypeMode.CrateredMoon:
                    newMaterial.EnableKeyword("_PLANET_TYPE_CRATEREDMOON");
                    newMaterial.SetFloat("_PLANET_TYPE", 2);
                    break;
                case PlanetTypeMode.LavaVolcanic:
                    newMaterial.EnableKeyword("_PLANET_TYPE_LAVAVOLCANIC");
                    newMaterial.SetFloat("_PLANET_TYPE", 3);
                    break;
                default:
                    newMaterial.EnableKeyword("_PLANET_TYPE_TERRESTRIAL");
                    newMaterial.SetFloat("_PLANET_TYPE", 0);
                    break;
            }

            // Set Colors
            if (ColorUtility.TryParseHtmlString(data.baseColorHex, out Color baseCol)) newMaterial.SetColor("_BaseColor", baseCol);
            if (ColorUtility.TryParseHtmlString(data.detailColorHex, out Color detailCol)) newMaterial.SetColor("_DetailColor", detailCol);
            if (ColorUtility.TryParseHtmlString(data.secondaryColorHex, out Color secCol)) newMaterial.SetColor("_SecondaryColor", secCol);
            if (ColorUtility.TryParseHtmlString(data.atmosphereColorHex, out Color atmCol)) newMaterial.SetColor("_AtmosphereColor", atmCol);
            if (ColorUtility.TryParseHtmlString(data.cloudColorHex, out Color cloudCol)) newMaterial.SetColor("_CloudColor", cloudCol);
            if (ColorUtility.TryParseHtmlString(data.emissiveColorHex, out Color emissCol)) newMaterial.SetColor("_EmissiveColor", emissCol);

            // Set Float Properties
            newMaterial.SetFloat("_PlanetRadius", data.planetRadius);
            newMaterial.SetFloat("_RotationSpeed", data.rotationSpeed);
            newMaterial.SetFloat("_NoiseScale", data.noiseScale);
            newMaterial.SetFloat("_LandThreshold", data.landThreshold);
            newMaterial.SetFloat("_BandStretch", data.bandStretch);
            newMaterial.SetFloat("_CraterDensity", data.craterDensity);
            newMaterial.SetFloat("_CloudSpeed", data.cloudSpeed);
            newMaterial.SetFloat("_CloudDensity", data.cloudDensity);
            newMaterial.SetFloat("_EmissionPower", data.emissionPower);
            newMaterial.SetFloat("_GlowThickness", data.glowThickness);
            newMaterial.SetFloat("_GlowPower", data.glowPower);
            newMaterial.SetVector("_LightDir", data.lightDir);

            string targetFolder = string.IsNullOrWhiteSpace(materialSaveFolder) ? "Assets" : materialSaveFolder.TrimEnd('/', '\\');
            if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

            string assetName = string.IsNullOrWhiteSpace(data.materialName) ? "NewPlanetMat" : data.materialName;
            string materialPath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/{assetName}.mat");

            AssetDatabase.CreateAsset(newMaterial, materialPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = newMaterial;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error: {ex.Message}");
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

            Augment newAugment = ScriptableObject.CreateInstance<Augment>();
            newAugment.augmentName = string.IsNullOrEmpty(data.augmentName) ? "New Augment" : data.augmentName;
            newAugment.augmentQuality = data.augmentQuality;
            newAugment.spaceObjectType = data.spaceObjectType;
            newAugment.isNeedCharacterToApply = data.isNeedCharacterToApply;
            newAugment.isApplyImmediatelyAfterPick = data.isApplyImmediatelyAfterPick;
            newAugment.isForNodeGroup = data.isForNodeGroup;

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

            if (selectedEffect != null)
            {
                newAugment.dynamicEffects.Add(selectedEffect);
            }
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
