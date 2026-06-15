using UnityEditor;
using UnityEngine;
using Spine.Unity;

public class TestScript {
    public static void Run() {
        var guids = AssetDatabase.FindAssets("SpineAtlasAsset t:MonoScript");
        Debug.Log("SpineAtlasAsset guids found: " + guids.Length);
        
        string path = "Assets/Ants Flow/Spines/Booster2-ani/Booster2_Atlas.asset"; // Wait, I don't know the exact path. I'll search for Booster2_Atlas.asset
        
        var atlasGuids = AssetDatabase.FindAssets("Booster2_Atlas");
        if (atlasGuids.Length > 0) {
            path = AssetDatabase.GUIDToAssetPath(atlasGuids[0]);
            Debug.Log("Found Booster2_Atlas at: " + path);
            Object asset = AssetDatabase.LoadMainAssetAtPath(path);
            Debug.Log("Asset type: " + asset.GetType());
            
            SerializedObject so = new SerializedObject(asset);
            SerializedProperty sp = so.FindProperty("m_Script");
            if (sp != null) {
                Debug.Log("m_Script exists. Is null? " + (sp.objectReferenceValue == null));
            } else {
                Debug.Log("m_Script not found");
            }
        }
    }
}
