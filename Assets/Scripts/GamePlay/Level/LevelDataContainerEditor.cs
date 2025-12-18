using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NamPhuThuy.GamePlay;
using UnityEngine;
using System.Text;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;

namespace NamPhuThuy.Data
{
    [CustomEditor(typeof(LevelDataContainer))]
    public class LevelDataContainerEditor : Editor
    {
        #region Private Fields

        private string _levelPrefabsFolderPath = "Assets/Prefabs/Levels";
        private LevelMode _defaultLevelMode = LevelMode.Normal;
        private DefaultAsset _folderAsset;

        #endregion

        #region Flags

        private bool _onlyNumberedLevels = true;

        #endregion


        #region Callbacks

        private void OnEnable()
        {
            // Try to load the folder asset for the current path
            if (Directory.Exists(_levelPrefabsFolderPath))
            {
                _folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(_levelPrefabsFolderPath);
            }

            randomizeRangeStartProp = serializedObject.FindProperty("randomizeRangeStart");
            randomizeRangeEndProp = serializedObject.FindProperty("randomizeRangeEnd");
            randomizeOptionsProp = serializedObject.FindProperty("randomizeOptions");
            levelsDataProp = serializedObject.FindProperty("levelsData");
        }

        public override void OnInspectorGUI()
        {
            LevelDataContainer container = (LevelDataContainer)target;

            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Level Auto-Population", EditorStyles.boldLabel);

            // Drag and drop folder field
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Level Prefabs Folder", GUILayout.Width(150));

            DefaultAsset newFolderAsset = EditorGUILayout.ObjectField(_folderAsset, typeof(DefaultAsset), false) as DefaultAsset;

            if (newFolderAsset != _folderAsset)
            {
                _folderAsset = newFolderAsset;
                if (_folderAsset != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(_folderAsset);
                    if (Directory.Exists(assetPath))
                    {
                        _levelPrefabsFolderPath = assetPath;
                    }
                    else
                    {
                        DebugLogger.LogWarning("Selected asset is not a folder!");
                        _folderAsset = null;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // Text field for manual path entry (optional)
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Or enter path manually:", GUILayout.Width(150));
            string newPath = EditorGUILayout.TextField(_levelPrefabsFolderPath);
            if (newPath != _levelPrefabsFolderPath)
            {
                _levelPrefabsFolderPath = newPath;
                // Try to load the folder asset for the new path
                if (Directory.Exists(_levelPrefabsFolderPath))
                {
                    _folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(_levelPrefabsFolderPath);
                }
                else
                {
                    _folderAsset = null;
                }
            }
            EditorGUILayout.EndHorizontal();

            _defaultLevelMode = (LevelMode)EditorGUILayout.EnumPopup("Default Level Mode", _defaultLevelMode);
            _onlyNumberedLevels = EditorGUILayout.Toggle("Only Add Numbered Levels", _onlyNumberedLevels);

            EditorGUILayout.Space(5);

            // Buttons
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add All Level Prefabs"))
            {
                AddAllLevelPrefabs(container);
            }

            if (GUILayout.Button("Replace All Level Prefabs"))
            {
                ReplaceAllLevelPrefabs(container);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear All Levels"))
            {
                if (EditorUtility.DisplayDialog("Clear Levels",
                        "Are you sure you want to clear all level data?", "Yes", "No"))
                {
                    ClearAllLevels(container);
                }
            }

            if (GUILayout.Button("Sort by Name"))
            {
                SortLevelsByName(container);
            }

            if (GUILayout.Button("Export"))
            {
                ExportLevelsToTxt();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Validate folder path
            if (!Directory.Exists(_levelPrefabsFolderPath))
            {
                EditorGUILayout.HelpBox($"Folder path does not exist: {_levelPrefabsFolderPath}", MessageType.Warning);
            }
            else
            {
                var prefabs = GetLevelPrefabsInFolder();
                EditorGUILayout.LabelField($"Prefabs found in folder: {prefabs.Length}");

                if (_onlyNumberedLevels)
                {
                    var numberedPrefabs = FilterNumberedPrefabs(prefabs);
                    EditorGUILayout.LabelField($"Numbered prefabs: {numberedPrefabs.Length}");
                }
            }

            DrawLevelDistributionGUI(container);
        }

        #endregion

        #region Level Distribution

        private SerializedProperty randomizeRangeStartProp;
        private SerializedProperty randomizeRangeEndProp;
        private SerializedProperty randomizeOptionsProp;
        private SerializedProperty levelsDataProp;

        private void DrawLevelDistributionGUI(LevelDataContainer container)
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(randomizeOptionsProp.arraySize == 0 || levelsDataProp.arraySize == 0))
            {
                if (GUILayout.Button("Randomize LevelModes In Range"))
                {
                    ApplyRandomizeFromEditor();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ApplyRandomizeFromEditor()
        {
            var container = target as LevelDataContainer;
            if (container == null) return;

            // Record undo
            Undo.RecordObject(container, "Randomize LevelModes");

            // perform randomization
            container.RandomizeLevelModesInRange(container.RandomizeRangeStart, container.RandomizeRangeEnd, container.RandomizeOptions);

            // mark dirty so edits are saved
            EditorUtility.SetDirty(container);
        }

        #endregion

        #region Level Management

        private void AddAllLevelPrefabs(LevelDataContainer container)
        {
            GameObject[] levelPrefabs = GetLevelPrefabsInFolder();

            if (_onlyNumberedLevels)
            {
                levelPrefabs = FilterNumberedPrefabs(levelPrefabs);
            }

            levelPrefabs = SortPrefabsByNumber(levelPrefabs);

            if (levelPrefabs.Length == 0)
            {
                EditorUtility.DisplayDialog("No Prefabs Found",
                    $"No GameObjects found in folder: {_levelPrefabsFolderPath}", "OK");
                return;
            }

            SerializedProperty levelsDataProperty = serializedObject.FindProperty("levelsData");
            SerializedProperty maxLevelProperty = serializedObject.FindProperty("maxLevel");

            // Get current array or create new one
            int currentLength = levelsDataProperty.arraySize;
            int newLength = currentLength + levelPrefabs.Length;

            levelsDataProperty.arraySize = newLength;

            // Add new level data entries
            for (int i = 0; i < levelPrefabs.Length; i++)
            {
                SerializedProperty levelDataProperty = levelsDataProperty.GetArrayElementAtIndex(currentLength + i);

                SerializedProperty levelIdProperty = levelDataProperty.FindPropertyRelative("levelId");
                SerializedProperty levelPrefabProperty = levelDataProperty.FindPropertyRelative("levelPrefab");
                SerializedProperty levelModeProperty = levelDataProperty.FindPropertyRelative("levelMode");

                levelIdProperty.intValue = currentLength + i;
                levelPrefabProperty.objectReferenceValue = levelPrefabs[i];
                levelModeProperty.enumValueIndex = (int)_defaultLevelMode;
            }

            // Update max level
            maxLevelProperty.intValue = newLength;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(container);

            DebugLogger.Log($"Added {levelPrefabs.Length} level prefabs to LevelDataContainer. Total levels: {newLength}");
        }

        private void ReplaceAllLevelPrefabs(LevelDataContainer container)
        {
            GameObject[] levelPrefabs = GetLevelPrefabsInFolder();

            if (levelPrefabs.Length == 0)
            {
                EditorUtility.DisplayDialog("No Prefabs Found",
                    $"No GameObjects found in folder: {_levelPrefabsFolderPath}", "OK");
                return;
            }

            // Sort by level number for proper ordering
            levelPrefabs = SortPrefabsByNumber(levelPrefabs);

            if (levelPrefabs.Length == 0)
            {
                string message = _onlyNumberedLevels
                    ? $"No numbered level prefabs found in folder: {_levelPrefabsFolderPath}"
                    : $"No GameObjects found in folder: {_levelPrefabsFolderPath}";
                EditorUtility.DisplayDialog("No Prefabs Found", message, "OK");
                return;
            }

            SerializedProperty levelsDataProperty = serializedObject.FindProperty("levelsData");
            SerializedProperty maxLevelProperty = serializedObject.FindProperty("maxLevel");

            // Clear and resize array
            levelsDataProperty.ClearArray();
            levelsDataProperty.arraySize = levelPrefabs.Length;

            // Populate with new level data
            for (int i = 0; i < levelPrefabs.Length; i++)
            {
                SerializedProperty levelDataProperty = levelsDataProperty.GetArrayElementAtIndex(i);

                SerializedProperty levelIdProperty = levelDataProperty.FindPropertyRelative("levelId");
                SerializedProperty levelPrefabProperty = levelDataProperty.FindPropertyRelative("levelPrefab");
                SerializedProperty levelModeProperty = levelDataProperty.FindPropertyRelative("levelMode");

                levelIdProperty.intValue = i;
                levelPrefabProperty.objectReferenceValue = levelPrefabs[i];
                levelModeProperty.enumValueIndex = (int)_defaultLevelMode;
            }

            // Update max level
            maxLevelProperty.intValue = levelPrefabs.Length;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(container);

            DebugLogger.Log($"Replaced all levels with {levelPrefabs.Length} level prefabs from folder.");
        }

        private void ClearAllLevels(LevelDataContainer container)
        {
            SerializedProperty levelsDataProperty = serializedObject.FindProperty("levelsData");
            SerializedProperty maxLevelProperty = serializedObject.FindProperty("maxLevel");

            levelsDataProperty.ClearArray();
            maxLevelProperty.intValue = 0;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(container);

            DebugLogger.LogSimple(message: "Cleared all level data.");
        }

        #endregion

        #region Filtering and Sorting

        /// <summary>
        /// Filter prefabs that contain numbers in their names
        /// </summary>
        private GameObject[] FilterNumberedPrefabs(GameObject[] prefabs)
        {
            List<GameObject> numberedPrefabs = new List<GameObject>();

            foreach (var prefab in prefabs)
            {
                if (prefab != null && ContainsNumber(prefab.name))
                {
                    numberedPrefabs.Add(prefab);
                }
            }

            return numberedPrefabs.ToArray();
        }

        /// <summary>
        /// Check if a string contains any numbers
        /// </summary>
        private bool ContainsNumber(string text)
        {
            return Regex.IsMatch(text, @"\d");
        }

        /// <summary>
        /// Extract number from prefab name for sorting
        /// </summary>
        private int ExtractNumberFromName(string name)
        {
            Match match = Regex.Match(name, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int number))
            {
                return number;
            }
            return 0; // Default to 0 if no number found
        }

        /// <summary>
        /// Sort prefabs by the number in their names
        /// </summary>
        private GameObject[] SortPrefabsByNumber(GameObject[] prefabs)
        {
            return prefabs.OrderBy(prefab =>
            {
                if (prefab == null) return int.MaxValue;
                return ExtractNumberFromName(prefab.name);
            }).ToArray();
        }



        private void SortLevelsByName(LevelDataContainer container)
        {
            SerializedProperty levelsDataProperty = serializedObject.FindProperty("levelsData");

            // Get all level data
            var levelDataList = new List<(GameObject prefab, LevelMode mode, int originalIndex)>();

            for (int i = 0; i < levelsDataProperty.arraySize; i++)
            {
                SerializedProperty levelDataProperty = levelsDataProperty.GetArrayElementAtIndex(i);
                SerializedProperty levelPrefabProperty = levelDataProperty.FindPropertyRelative("levelPrefab");
                SerializedProperty levelModeProperty = levelDataProperty.FindPropertyRelative("levelMode");

                GameObject prefab = levelPrefabProperty.objectReferenceValue as GameObject;
                LevelMode mode = (LevelMode)levelModeProperty.enumValueIndex;

                levelDataList.Add((prefab, mode, i));
            }

            // Sort by prefab name (handle null cases)
            levelDataList.Sort((a, b) =>
            {
                string nameA = a.prefab != null ? a.prefab.name : "";
                string nameB = b.prefab != null ? b.prefab.name : "";
                return string.Compare(nameA, nameB, System.StringComparison.OrdinalIgnoreCase);
            });

            // Apply sorted data back
            for (int i = 0; i < levelDataList.Count; i++)
            {
                SerializedProperty levelDataProperty = levelsDataProperty.GetArrayElementAtIndex(i);
                SerializedProperty levelIdProperty = levelDataProperty.FindPropertyRelative("levelId");
                SerializedProperty levelPrefabProperty = levelDataProperty.FindPropertyRelative("levelPrefab");
                SerializedProperty levelModeProperty = levelDataProperty.FindPropertyRelative("levelMode");

                levelIdProperty.intValue = i;
                levelPrefabProperty.objectReferenceValue = levelDataList[i].prefab;
                levelModeProperty.enumValueIndex = (int)levelDataList[i].mode;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(container);

            DebugLogger.Log("Sorted levels by prefab name.");
        }

        private GameObject[] GetLevelPrefabsInFolder()
        {
            if (!Directory.Exists(_levelPrefabsFolderPath))
            {
                return new GameObject[0];
            }

            // Get all prefab files in the folder
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { _levelPrefabsFolderPath });

            GameObject[] levelPrefabs = new GameObject[prefabGuids.Length];

            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                levelPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            }

            // Sort by name for consistent ordering
            System.Array.Sort(levelPrefabs, (a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;
                return string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase);
            });

            return levelPrefabs;
        }

        #endregion

        #region Export
        public void ExportLevelsToTxt()
        {
            LevelDataContainer levelDataContainer = target as LevelDataContainer;

            string path = EditorUtility.SaveFilePanel(
                "Export Level Data",
                Application.dataPath,
                "LevelData",
                "txt"
            );

            if (string.IsNullOrEmpty(path))
                return;

            StringBuilder sb = new StringBuilder();

            foreach (var level in levelDataContainer.LevelsData)
            {
                sb.AppendLine($"level {level.levelId + 1}");
                sb.AppendLine($"mode: {level.levelMode}");
                sb.AppendLine(); // empty line between levels
            }

            File.WriteAllText(path, sb.ToString());

            Debug.Log($"Level data exported to: {path}");
            AssetDatabase.Refresh();
        }
        #endregion
    }
}
#endif