using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace NamPhuThuy.EditorTools
{
    public class ColliderAssetCleaner : EditorWindow
    {
        private List<string> colliderAssetPaths = new List<string>();
        private Vector2 scrollPos;
        private bool isScanning = false;

        [MenuItem("Tools/TrinhNam/Collider Asset Cleaner")]
        public static void ShowWindow()
        {
            GetWindow<ColliderAssetCleaner>("Collider Cleaner");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Collider Asset Cleaner", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This tool finds and deletes all .asset files with 'Colliders' in their name.",
                MessageType.Info);

            EditorGUILayout.Space();

            GUI.enabled = !isScanning;
            if (GUILayout.Button("Scan for Collider Assets", GUILayout.Height(30)))
            {
                ScanForColliderAssets();
            }
            GUI.enabled = true;

            if (colliderAssetPaths.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Found {colliderAssetPaths.Count} collider asset(s):", EditorStyles.boldLabel);

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                foreach (var path in colliderAssetPaths)
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
                if (GUILayout.Button("Delete All Collider Assets", GUILayout.Height(40)))
                {
                    if (EditorUtility.DisplayDialog("Confirm Deletion",
                        $"Are you sure you want to delete {colliderAssetPaths.Count} collider asset(s)?\n\nThis action cannot be undone.",
                        "Delete", "Cancel"))
                    {
                        DeleteColliderAssets();
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            else if (!isScanning)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("No collider assets found. Click 'Scan' to search.", MessageType.None);
            }
        }

        private void ScanForColliderAssets()
        {
            isScanning = true;
            colliderAssetPaths.Clear();

            try
            {
                // Find all .asset files in the Assets folder
                var allAssetGuids = AssetDatabase.FindAssets("t:Object", new[] { "Assets" });
                
                colliderAssetPaths = allAssetGuids
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Where(path => path.EndsWith(".asset"))
                    .Where(path => path.Contains("Colliders") || path.Contains("_Colliders"))
                    .ToList();

                Debug.Log($"Scan complete: Found {colliderAssetPaths.Count} collider asset(s).");
            }
            finally
            {
                isScanning = false;
                Repaint();
            }
        }

        private void DeleteColliderAssets()
        {
            int deletedCount = 0;

            foreach (var path in colliderAssetPaths.ToList())
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

            colliderAssetPaths.Clear();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Deleted {deletedCount} collider asset(s).");
            EditorUtility.DisplayDialog("Deletion Complete", $"Successfully deleted {deletedCount} asset(s).", "OK");
        }
    }
}
