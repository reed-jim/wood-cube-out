#if UNITY_EDITOR
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
        if (GUILayout.Button("AutoAssignIdForAugment"))
        {
            AutoAssignIdForAugment();
        }

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
        const string path = "Assets/Tower Shooter/Data/Augment/Augment Container.asset";

        AugmentContainer augmentContainer = AssetDatabase.LoadAssetAtPath<AugmentContainer>(path);

        for (int i = 0; i < augmentContainer.augments.Count; i++)
        {
            Augment augment = augmentContainer.augments[i];

            augment.id = i;

            EditorUtility.SetDirty(augment);
        }

        EditorUtility.SetDirty(augmentContainer);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
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
