using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "StarVoyageContainer", menuName = "Star/StarVoyageContainer")]
public class StarVoyageContainer : ScriptableObject
{
    public List<StarPathConfig> starPathConfigs;

    public Material GetPlanetTagMaterial(SpaceObjectType spaceObjectType)
    {
        for (int i = 0; i < starPathConfigs.Count; i++)
        {
            StarPathConfig starPathConfig = starPathConfigs[i];

            if (starPathConfig.spaceObjectType == spaceObjectType)
            {
                return starPathConfig.planetTagMaterial;
            }
        }

        return null;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(StarVoyageContainer))]
public class StarVoyageContainerEditor : Editor
{
    private string planetMaterialFolder = "Assets/Materials/Planets";
    private string planetTagMaterialFolder = "Assets/Materials/PlanetTags";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        StarVoyageContainer container = (StarVoyageContainer)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Automation Tools", EditorStyles.boldLabel);

        // Folder path overrides in inspector
        planetMaterialFolder = EditorGUILayout.TextField("Planet Mat Folder", planetMaterialFolder);
        planetTagMaterialFolder = EditorGUILayout.TextField("Planet Tag Mat Folder", planetTagMaterialFolder);

        if (GUILayout.Button("Auto Assign Materials by Order"))
        {
            AutoAssignMaterialsByOrder(container);
        }

        if (GUILayout.Button("Increment IDs by 1"))
        {
            IncrementIds(container);
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Export Names to JSON"))
        {
            ExportToJson(container);
        }
    }

    private void IncrementIds(StarVoyageContainer container)
    {
        if (container.starPathConfigs == null || container.starPathConfigs.Count == 0)
        {
            Debug.LogWarning("No StarPathConfigs found in the container.");
            return;
        }

        int updatedCount = 0;

        foreach (var config in container.starPathConfigs)
        {
            if (config == null) continue;

            Undo.RecordObject(config, "Increment StarPath ID");
            
            // Increase ID by 1
            config.id += 1;
            updatedCount++;

            EditorUtility.SetDirty(config);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Successfully incremented IDs by 1 for {updatedCount} StarPathConfig(s).");
    }

    private void AutoAssignMaterialsByOrder(StarVoyageContainer container)
    {
        if (container.starPathConfigs == null || container.starPathConfigs.Count == 0)
        {
            Debug.LogWarning("No StarPathConfigs found in the container.");
            return;
        }

        List<Material> planetMaterials = LoadSortedMaterialsFromFolder(planetMaterialFolder);
        List<Material> planetTagMaterials = LoadSortedMaterialsFromFolder(planetTagMaterialFolder);

        int assignedPlanetMats = 0;
        int assignedTagMats = 0;

        for (int i = 0; i < container.starPathConfigs.Count; i++)
        {
            StarPathConfig config = container.starPathConfigs[i];
            if (config == null) continue;

            Undo.RecordObject(config, "Auto Assign Materials by Order");

            if (i < planetMaterials.Count)
            {
                config.planetMaterial = planetMaterials[i];
                assignedPlanetMats++;
            }

            if (i < planetTagMaterials.Count)
            {
                config.planetTagMaterial = planetTagMaterials[i];
                assignedTagMats++;
            }

            EditorUtility.SetDirty(config);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Sequential assignment complete! Assigned {assignedPlanetMats} Planet Materials and {assignedTagMats} Planet Tag Materials by order.");
    }

    private List<Material> LoadSortedMaterialsFromFolder(string folderPath)
    {
        List<Material> materials = new List<Material>();

        string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), folderPath);
        if (!Directory.Exists(absolutePath))
        {
            Debug.LogWarning($"Folder path does not exist: {folderPath}");
            return materials;
        }

        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folderPath });
        
        var sortedGuids = guids
            .Select(guid => new { Guid = guid, Path = AssetDatabase.GUIDToAssetPath(guid) })
            .OrderBy(x => x.Path)
            .Select(x => x.Guid);

        foreach (string guid in sortedGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                materials.Add(mat);
            }
        }

        return materials;
    }

    private void ExportToJson(StarVoyageContainer container)
    {
        StarPathNamesWrapper wrapper = new StarPathNamesWrapper();
        wrapper.names = new List<string>();

        if (container.starPathConfigs != null)
        {
            foreach (var config in container.starPathConfigs)
            {
                if (config != null)
                {
                    wrapper.names.Add(config.starName);
                }
            }
        }

        string jsonString = JsonUtility.ToJson(wrapper, true);
        string path = EditorUtility.SaveFilePanel("Save StarPath Names JSON", Application.dataPath, "StarPathConfigNames", "json");

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, jsonString);
            AssetDatabase.Refresh();
            Debug.Log($"Successfully exported JSON to: {path}");
        }
    }
}

[System.Serializable]
public class StarPathNamesWrapper
{
    public List<string> names;
}
#endif
