using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Spine.Unity;

public class SpineSkeletonDataFixer : Editor
{
    [MenuItem("Tools/Spine/Fix Selected SkeletonData Assets %#s")] // Ctrl+Shift+S (or Cmd+Shift+S)
    public static void FixSelected()
    {
        string[] guids = Selection.assetGUIDs;
        if (guids.Length == 0)
        {
            Debug.LogWarning("Please select at least one SkeletonData asset or folder in the Project view.");
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

        Debug.Log($"[SpineSkeletonDataFixer] Shortcut pressed. Checking {assetPaths.Count} .asset files...");

        foreach (string path in assetPaths)
        {
            SkeletonDataAsset skeletonDataAsset = AssetDatabase.LoadMainAssetAtPath(path) as SkeletonDataAsset;
            if (skeletonDataAsset != null)
            {
                bool needsFix = false;

                // Check if atlas array is empty or contains null entries
                if (skeletonDataAsset.atlasAssets == null || skeletonDataAsset.atlasAssets.Length == 0)
                {
                    needsFix = true;
                }
                else
                {
                    foreach (var a in skeletonDataAsset.atlasAssets)
                    {
                        if (a == null) needsFix = true;
                    }
                }

                if (needsFix)
                {
                    string directory = Path.GetDirectoryName(path);
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    
                    // Usually name is "Name_SkeletonData"
                    string baseName = fileName;
                    if (fileName.EndsWith("_SkeletonData"))
                    {
                        baseName = fileName.Substring(0, fileName.Length - "_SkeletonData".Length);
                    }
                    else if (fileName.EndsWith("SkeletonData"))
                    {
                        baseName = fileName.Substring(0, fileName.Length - "SkeletonData".Length);
                    }

                    // Look for SpineAtlasAsset in the same directory
                    string[] possibleAtlasNames = new string[] 
                    { 
                        $"{baseName}_Atlas.asset",
                        $"{baseName}Atlas.asset",
                        $"{baseName}.asset"
                    };

                    SpineAtlasAsset atlasAsset = null;
                    foreach (var name in possibleAtlasNames)
                    {
                        string atlasPath = Path.Combine(directory, name).Replace("\\", "/");
                        atlasAsset = AssetDatabase.LoadAssetAtPath<SpineAtlasAsset>(atlasPath);
                        if (atlasAsset != null) break;
                    }

                    if (atlasAsset != null)
                    {
                        skeletonDataAsset.atlasAssets = new AtlasAssetBase[] { atlasAsset };
                        EditorUtility.SetDirty(skeletonDataAsset);
                        fixedCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"[SpineSkeletonDataFixer] Could not find matching Atlas for {fileName} in {directory}");
                    }
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
            Debug.Log($"<color=green>[SpineSkeletonDataFixer] Successfully fixed {fixedCount} SkeletonData Assets!</color>");
        }
        else
        {
            Debug.Log($"[SpineSkeletonDataFixer] Tool ran, but found nothing to fix. {alreadyFixedCount} SkeletonData assets were already fully assigned.");
        }
    }
}
