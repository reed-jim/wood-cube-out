#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using AntsFlow;

namespace AntsFlow.EditorTools
{
    public class LevelFolderConverterWindow : EditorWindow
    {
        private DefaultAsset folderAsset;
        private List<LevelPrefabInfo> prefabList = new List<LevelPrefabInfo>();
        private Vector2 scrollPosition;

        [Serializable]
        private class LevelPrefabInfo
        {
            public string assetPath;
            public GameObject prefab;
        }

        [MenuItem("Tools/Level Folder Converter")]
        public static void ShowWindow()
        {
            GetWindow<LevelFolderConverterWindow>("Level Converter");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Level Prefab Json Converter", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            folderAsset = (DefaultAsset)EditorGUILayout.ObjectField("Folder containing prefabs", folderAsset, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                ScanFolder();
            }

            if (folderAsset != null)
            {
                string path = AssetDatabase.GetAssetPath(folderAsset);
                if (!AssetDatabase.IsValidFolder(path))
                {
                    EditorGUILayout.HelpBox("Selected asset is not a valid folder.", MessageType.Error);
                    prefabList.Clear();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Scan/Refresh Prefabs", GUILayout.Height(30)))
                    {
                        ScanFolder();
                    }
                    if (prefabList.Count > 0 && GUILayout.Button("Convert All to JSON", GUILayout.Height(30)))
                    {
                        ConvertAll();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField($"Found {prefabList.Count} LevelItem Prefabs:", EditorStyles.miniBoldLabel);

                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                    for (int i = 0; i < prefabList.Count; i++)
                    {
                        var info = prefabList[i];
                        if (info == null || info.prefab == null) continue;

                        EditorGUILayout.BeginHorizontal(GUI.skin.box);
                        EditorGUILayout.LabelField(info.prefab.name, GUILayout.Width(250));
                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            EditorGUILayout.ObjectField(info.prefab, typeof(GameObject), false);
                        }

                        if (GUILayout.Button("Convert To Json", GUILayout.Width(120)))
                        {
                            ConvertSingle(info);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please drag and drop a folder containing LevelItem prefabs.", MessageType.Info);
                prefabList.Clear();
            }
        }

        private void ScanFolder()
        {
            prefabList.Clear();
            if (folderAsset == null) return;

            string folderPath = AssetDatabase.GetAssetPath(folderAsset);
            if (!AssetDatabase.IsValidFolder(folderPath)) return;

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
                try
                {
                    LevelItem levelItem = prefabRoot.GetComponentInChildren<LevelItem>(true);
                    if (levelItem != null)
                    {
                        prefabList.Add(new LevelPrefabInfo
                        {
                            assetPath = assetPath,
                            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath)
                        });
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
        }

        private void ConvertSingle(LevelPrefabInfo info)
        {
            if (info == null || string.IsNullOrEmpty(info.assetPath)) return;

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(info.assetPath);
            try
            {
                LevelItem levelItem = prefabRoot.GetComponentInChildren<LevelItem>(true);
                if (levelItem != null)
                {
                    levelItem.ConvertToJson();
                    EditorUtility.SetDirty(prefabRoot);
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, info.assetPath);
                    Debug.Log($"Successfully converted {info.prefab.name} to JSON.");
                }
                else
                {
                    Debug.LogError($"Could not find LevelItem component in prefab: {info.prefab.name}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error converting prefab {info.prefab.name}: {ex.Message}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void ConvertAll()
        {
            int convertedCount = 0;
            for (int i = 0; i < prefabList.Count; i++)
            {
                var info = prefabList[i];
                if (info == null || string.IsNullOrEmpty(info.assetPath)) continue;

                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(info.assetPath);
                try
                {
                    LevelItem levelItem = prefabRoot.GetComponentInChildren<LevelItem>(true);
                    if (levelItem != null)
                    {
                        levelItem.ConvertToJson();
                        EditorUtility.SetDirty(prefabRoot);
                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, info.assetPath);
                        convertedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error converting prefab {info.prefab.name}: {ex.Message}");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Batch execution completed. Successfully converted {convertedCount} prefabs to JSON.");
        }
    }
}
#endif
