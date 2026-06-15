using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Spine.Unity;

public class SpineAtlasFixer : Editor
{
    [MenuItem("Tools/Spine/Fix Selected Atlas Assets or Folders %&z")]
    public static void FixSelected()
    {
        string[] guids = Selection.assetGUIDs;
        if (guids.Length == 0)
        {
            Debug.LogWarning("Please select at least one Spine Atlas asset or folder in the Project view.");
            return;
        }

        // Try to find the MonoScript for SpineAtlasAsset
        MonoScript spineAtlasAssetScript = null;
        string[] scriptGuids = AssetDatabase.FindAssets("SpineAtlasAsset t:MonoScript");
        if (scriptGuids.Length > 0)
        {
            spineAtlasAssetScript = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(scriptGuids[0]));
        }

        if (spineAtlasAssetScript == null)
        {
            Debug.LogError("Could not find SpineAtlasAsset MonoScript in the project!");
            return;
        }

        List<string> assetPaths = new List<string>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path))
            {
                string[] files = Directory.GetFiles(path, "*.asset", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    assetPaths.Add(file.Replace("\\", "/"));
                }
            }
            else if (path.EndsWith(".asset"))
            {
                assetPaths.Add(path);
            }
        }

        int fixedCount = 0;
        int alreadyFixedCount = 0;

        Debug.Log($"[SpineAtlasFixer] Shortcut pressed. Checking {assetPaths.Count} .asset files...");

        foreach (string path in assetPaths)
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null) continue;

            SpineAtlasAsset atlasAsset = asset as SpineAtlasAsset;
            
            // If the script is missing or null, asset will NOT cast to SpineAtlasAsset
            // Unity often loads it as a DefaultAsset, making SerializedObject fail to modify m_Script.
            // The most robust way is to edit the YAML directly!
            bool nameContainsAtlas = path.ToLower().Contains("atlas");
            if (atlasAsset == null && nameContainsAtlas)
            {
                string spineAtlasGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spineAtlasAssetScript));
                string fullPath = Path.GetFullPath(path);
                
                if (File.Exists(fullPath))
                {
                    string[] lines = File.ReadAllLines(fullPath);
                    bool modified = false;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].StartsWith("  m_Script:"))
                        {
                            lines[i] = $"  m_Script: {{fileID: 11500000, guid: {spineAtlasGuid}, type: 3}}";
                            modified = true;
                            break;
                        }
                    }

                    if (modified)
                    {
                        File.WriteAllLines(fullPath, lines);
                        // Force reimport so Unity updates the object type in memory
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                        // Re-load the asset
                        asset = AssetDatabase.LoadMainAssetAtPath(path);
                        atlasAsset = asset as SpineAtlasAsset;
                    }
                }
            }

            if (atlasAsset != null)
            {
                bool changed = false;
                string directory = Path.GetDirectoryName(path);
                string fileName = Path.GetFileNameWithoutExtension(path);
                
                // Usually the format is "Name_Atlas"
                string baseName = fileName;
                if (fileName.EndsWith("_Atlas"))
                {
                    baseName = fileName.Substring(0, fileName.Length - "_Atlas".Length);
                }
                else if (fileName.EndsWith("Atlas"))
                {
                    baseName = fileName.Substring(0, fileName.Length - "Atlas".Length);
                }

                // 1. Assign Atlas File
                if (atlasAsset.atlasFile == null)
                {
                    // Try different possible extensions for the atlas file
                    string[] possibleAtlasNames = new string[] 
                    { 
                        $"{baseName}.atlas.txt",
                        $"{baseName}.atlas", 
                        $"{baseName}.txt"
                    };

                    TextAsset atlasTextAsset = null;
                    foreach (var name in possibleAtlasNames)
                    {
                        string atlasPath = Path.Combine(directory, name).Replace("\\", "/");
                        atlasTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(atlasPath);
                        if (atlasTextAsset != null) break;
                    }

                    if (atlasTextAsset != null)
                    {
                        atlasAsset.atlasFile = atlasTextAsset;
                        changed = true;
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find .atlas file for {fileName} in {directory}");
                    }
                }

                // 2. Assign Material
                if (atlasAsset.materials == null || atlasAsset.materials.Length == 0 || atlasAsset.materials[0] == null)
                {
                    string[] possibleMatNames = new string[] 
                    { 
                        $"{baseName}_Material.mat",
                        $"{baseName} Material.mat",
                        $"{baseName}.mat"
                    };

                    Material material = null;
                    foreach (var name in possibleMatNames)
                    {
                        string matPath = Path.Combine(directory, name).Replace("\\", "/");
                        material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                        if (material != null) break;
                    }

                    if (material != null)
                    {
                        atlasAsset.materials = new Material[] { material };
                        changed = true;
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find Material for {fileName} in {directory}");
                    }
                }

                if (changed)
                {
                    EditorUtility.SetDirty(atlasAsset);
                    fixedCount++;
                }
                else
                {
                    alreadyFixedCount++;
                }
            }
        }

        if (fixedCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"<color=green>[SpineAtlasFixer] Successfully fixed {fixedCount} Spine Atlas Assets!</color>");
        }
        else
        {
            Debug.Log($"[SpineAtlasFixer] Tool ran, but found nothing to fix. {alreadyFixedCount} assets were already fully assigned.");
        }
    }
}
