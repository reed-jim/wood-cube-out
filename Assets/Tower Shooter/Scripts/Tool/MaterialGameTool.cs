#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class MaterialCreatorWindow : EditorWindow
{
    private string rawJsonInput = @"{
  ""materials"": [
    {
      ""name"": ""Earth_Default"",
      ""floats"": [
        { ""key"": ""_PlanetRadius"", ""value"": 0.28 },
        { ""key"": ""_RotationSpeed"", ""value"": 0.4 }
      ],
      ""colors"": [
        { ""key"": ""_OceanColor"", ""r"": 0.01, ""g"": 0.08, ""b"": 0.28, ""a"": 1.0 }
      ]
    }
  ]
}";

    private Shader targetShader;
    private string saveFolderPath = "Assets/Materials/Generated";
    private Vector2 scrollPosition;

    [MenuItem("Tools/Material Creator from Text Area")]
    public static void ShowWindow()
    {
        GetWindow<MaterialCreatorWindow>("Material Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Bulk Material Creator from JSON Text", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Target Shader Selection
        targetShader = (Shader)EditorGUILayout.ObjectField("Target Shader", targetShader, typeof(Shader), false);

        // Save Folder Path Selection
        EditorGUILayout.BeginHorizontal();
        saveFolderPath = EditorGUILayout.TextField("Save Folder", saveFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Save Folder", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    saveFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Folder", "Please select a folder inside the Unity 'Assets' directory.", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Paste / Edit JSON Content Below:", EditorStyles.boldLabel);

        // Text Area for direct JSON editing/pasting
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        rawJsonInput = EditorGUILayout.TextArea(rawJsonInput, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // Generate Button
        GUI.enabled = !string.IsNullOrEmpty(rawJsonInput) && targetShader != null;
        if (GUILayout.Button("Generate Materials", GUILayout.Height(35)))
        {
            GenerateMaterials();
        }
        GUI.enabled = true;
    }

    private void GenerateMaterials()
    {
        if (string.IsNullOrEmpty(rawJsonInput) || targetShader == null) return;

        // Ensure save folder exists
        if (!AssetDatabase.IsValidFolder(saveFolderPath))
        {
            string[] folders = saveFolderPath.Split('/');
            string currentPath = folders[0]; // "Assets"
            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = nextPath;
            }
        }

        MaterialListWrapper data;
        try
        {
            data = JsonUtility.FromJson<MaterialListWrapper>(rawJsonInput);
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("JSON Error", $"Failed to parse JSON:\n{e.Message}", "OK");
            return;
        }

        if (data == null || data.materials == null || data.materials.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "Parsed JSON successfully, but no materials list was found.", "OK");
            return;
        }

        int createdCount = 0;

        foreach (var matData in data.materials)
        {
            if (string.IsNullOrEmpty(matData.name)) continue;

            Material newMat = new Material(targetShader);

            // Apply Float Properties
            if (matData.floats != null)
            {
                foreach (var prop in matData.floats)
                {
                    if (newMat.HasProperty(prop.key))
                    {
                        newMat.SetFloat(prop.key, prop.value);
                    }
                }
            }

            // Apply Color Properties
            if (matData.colors != null)
            {
                foreach (var prop in matData.colors)
                {
                    if (newMat.HasProperty(prop.key))
                    {
                        newMat.SetColor(prop.key, new Color(prop.r, prop.g, prop.b, prop.a));
                    }
                }
            }

            string assetPath = Path.Combine(saveFolderPath, matData.name + ".mat");
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            
            AssetDatabase.CreateAsset(newMat, assetPath);
            createdCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"Successfully created {createdCount} materials in {saveFolderPath}!", "OK");
    }

    [System.Serializable]
    public class MaterialListWrapper
    {
        public List<MaterialJsonData> materials;
    }

    [System.Serializable]
    public class MaterialJsonData
    {
        public string name;
        public List<FloatProperty> floats;
        public List<ColorProperty> colors;
    }

    [System.Serializable]
    public struct FloatProperty
    {
        public string key;
        public float value;
    }

    [System.Serializable]
    public struct ColorProperty
    {
        public string key;
        public float r;
        public float g;
        public float b;
        public float a;
    }
}
#endif