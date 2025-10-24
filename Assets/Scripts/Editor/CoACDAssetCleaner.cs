using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;

namespace NamPhuThuy.EditorTools
{
    public class CoACDAssetCleaner : EditorWindow
    {
        private List<string> orphanedAssetPaths = new List<string>();
        private Vector2 scrollPos;
        private bool isScanning = false;
        private bool searchInScenes = false;

        [MenuItem("Tools/TrinhNam/CoACD Asset Cleaner")]
        public static void ShowWindow()
        {
            GetWindow<CoACDAssetCleaner>("CoACD Cleaner");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("CoACD Collider Asset Cleaner", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This tool finds and deletes:\n" +
                "1. CoACD assets with missing script references\n" +
                "2. CoACD assets not referenced by any component",
                MessageType.Info);

            EditorGUILayout.Space();

            // Checkbox for scene searching
            searchInScenes = EditorGUILayout.Toggle("Search in Scenes", searchInScenes);
            if (searchInScenes)
            {
                EditorGUILayout.HelpBox("Warning: Searching in scenes will open all scenes in the project, which may take longer.", MessageType.Warning);
            }

            EditorGUILayout.Space();

            GUI.enabled = !isScanning;
            if (GUILayout.Button("Scan for Orphaned Assets", GUILayout.Height(30)))
            {
                ScanForOrphanedAssets();
            }
            GUI.enabled = true;

            if (orphanedAssetPaths.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Found {orphanedAssetPaths.Count} orphaned asset(s):", EditorStyles.boldLabel);

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                foreach (var path in orphanedAssetPaths)
                {
                    if (string.IsNullOrEmpty(path)) continue;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(path);
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space();
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Delete All Orphaned Assets", GUILayout.Height(40)))
                {
                    if (EditorUtility.DisplayDialog("Confirm Deletion",
                        $"Are you sure you want to delete {orphanedAssetPaths.Count} orphaned CoACD collider asset(s)?\n\nThis action cannot be undone.",
                        "Delete", "Cancel"))
                    {
                        DeleteOrphanedAssets();
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            else if (!isScanning)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("No orphaned assets found. Click 'Scan' to search.", MessageType.None);
            }
        }

        private void ScanForOrphanedAssets()
        {
            isScanning = true;
            orphanedAssetPaths.Clear();

            try
            {
                // Find all .asset files matching CoACD pattern
                var allAssetGuids = AssetDatabase.FindAssets("_Colliders t:ScriptableObject");
                var potentialCoACDAssets = allAssetGuids
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Where(path => !path.StartsWith("Packages/") && !path.StartsWith("Library/"))
                    .Where(path => path.EndsWith("_Colliders.asset") ||
                                   path.Contains("_Colliders ") && path.EndsWith(".asset"))
                    .ToList();

                var assetsWithMissingScript = new HashSet<string>();
                var validColliderData = new List<CoACDColliderData>();

                // Check for missing scripts and load valid assets
                foreach (var assetPath in potentialCoACDAssets)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

                    // Check if asset has missing script
                    if (asset == null || asset.GetType() == typeof(Object))
                    {
                        assetsWithMissingScript.Add(assetPath);
                        continue;
                    }

                    // Try to load as CoACDColliderData
                    var colliderData = AssetDatabase.LoadAssetAtPath<CoACDColliderData>(assetPath);
                    if (colliderData != null)
                    {
                        validColliderData.Add(colliderData);
                    }
                }

                // Find referenced CoACD components
                var referencedColliderData = new HashSet<CoACDColliderData>();

                // Search in scenes if enabled
                if (searchInScenes)
                {
                    var sceneGuids = AssetDatabase.FindAssets("t:Scene");
                    foreach (var guid in sceneGuids)
                    {
                        var scenePath = AssetDatabase.GUIDToAssetPath(guid);

                        // Skip scenes in packages or read-only locations
                        if (scenePath.StartsWith("Packages/") || scenePath.StartsWith("Library/"))
                            continue;

                        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                        var components = Object.FindObjectsOfType<CoACD>(true);
                        foreach (var component in components)
                        {
                            var data = GetColliderDataFromComponent(component);
                            if (data != null)
                                referencedColliderData.Add(data);
                        }

                        EditorSceneManager.CloseScene(scene, true);
                    }
                }

                // Search in all prefabs
                var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
                foreach (var guid in prefabGuids)
                {
                    var prefabPath = AssetDatabase.GUIDToAssetPath(guid);

                    // Skip prefabs in packages or read-only locations
                    if (prefabPath.StartsWith("Packages/") || prefabPath.StartsWith("Library/"))
                        continue;

                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab == null) continue;

                    var components = prefab.GetComponentsInChildren<CoACD>(true);
                    foreach (var component in components)
                    {
                        var data = GetColliderDataFromComponent(component);
                        if (data != null)
                            referencedColliderData.Add(data);
                    }
                }

                // Collect orphaned assets
                orphanedAssetPaths.AddRange(assetsWithMissingScript);

                var unreferencedAssets = validColliderData
                    .Where(asset => !referencedColliderData.Contains(asset))
                    .Select(asset => AssetDatabase.GetAssetPath(asset))
                    .ToList();

                orphanedAssetPaths.AddRange(unreferencedAssets);

                Debug.Log($"Scan complete: Found {assetsWithMissingScript.Count} assets with missing scripts and {unreferencedAssets.Count} unreferenced assets.");
            }
            finally
            {
                isScanning = false;
                Repaint();
            }
        }

        private CoACDColliderData GetColliderDataFromComponent(CoACD component)
        {
            if (component == null) return null;

            var serializedObject = new SerializedObject(component);
            var colliderDataProp = serializedObject.FindProperty("_colliderData");
            return colliderDataProp?.objectReferenceValue as CoACDColliderData;
        }

        private void DeleteOrphanedAssets()
        {
            int deletedCount = 0;

            foreach (var path in orphanedAssetPaths.ToList())
            {
                if (string.IsNullOrEmpty(path)) continue;

                if (AssetDatabase.DeleteAsset(path))
                {
                    deletedCount++;
                }
                else
                {
                    Debug.LogWarning($"Failed to delete asset at {path}");
                }
            }

            orphanedAssetPaths.Clear();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Deleted {deletedCount} orphaned CoACD collider asset(s).");
            EditorUtility.DisplayDialog("Deletion Complete", $"Successfully deleted {deletedCount} orphaned asset(s).", "OK");
        }
    }
}
