#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using UnityEditor;
using UnityEngine;

namespace NamPhuThuy.EditorTools
{
    
    
    public class LevelModifyTool: EditorWindow
    {
        #region Fields
        private Transform selectedLevel;
        private Vector2 scrollPosition;
        private string customPrefabPath = "";
        private string customScrewTypeName = "custom";
        private bool showStatistics = true;
        private bool showScrewSwitching = true;
        private bool showUtilities = true;
        private int maxScrewCount = 100;
        private Material[] materialPool;
        #endregion
        
        #region Callbacks

        [MenuItem("Tools/TrinhNam/Level Modify Tool")]
        public static void ShowWindow()
        {
            GetWindow<LevelModifyTool>("Level Modify Tool");
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Level Modify Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawLevelSelection();
            EditorGUILayout.Space(10);

            if (selectedLevel != null)
            {
                DrawStatisticsSection();
                EditorGUILayout.Space(10);
                
                DrawScrewSwitchingSection();
                EditorGUILayout.Space(10);
                
                DrawUtilitiesSection();
            }
            else
            {
                EditorGUILayout.HelpBox("Please select a level GameObject in the hierarchy", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

         private void DrawLevelSelection()
        {
            EditorGUILayout.LabelField("Level Selection", EditorStyles.boldLabel);
            selectedLevel = EditorGUILayout.ObjectField("Selected Level", selectedLevel, typeof(Transform), true) as Transform;
            
            if (GUILayout.Button("Use Selected GameObject"))
            {
                if (Selection.activeTransform != null)
                {
                    selectedLevel = Selection.activeTransform;
                }
                else
                {
                    Debug.LogWarning("No GameObject selected in hierarchy");
                }
            }
        }

        private void DrawStatisticsSection()
        {
            showStatistics = EditorGUILayout.Foldout(showStatistics, "Statistics", true);
            if (showStatistics)
            {
                EditorGUI.indentLevel++;
                
                if (GUILayout.Button("Show Screw Statistics"))
                {
                    LogScrewStatistics(selectedLevel);
                }

                var screwCount = GetScrewTypeCount(selectedLevel);
                int totalScrews = screwCount.Values.Sum();
                
                EditorGUILayout.LabelField($"Total Screws: {totalScrews}");
                
                foreach (var kvp in screwCount)
                {
                    EditorGUILayout.LabelField($"- {kvp.Key}: {kvp.Value}");
                }
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawScrewSwitchingSection()
        {
            showScrewSwitching = EditorGUILayout.Foldout(showScrewSwitching, "Screw Switching", true);
            if (showScrewSwitching)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Switch Screw Types", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Switch to Normal Screws"))
                {
                    SwitchToNormalScrew(selectedLevel);
                }
                if (GUILayout.Button("Switch to Locked Color Screws"))
                {
                    SwitchToLockedColorScrew(selectedLevel);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Custom Screw Type", EditorStyles.boldLabel);
                customPrefabPath = EditorGUILayout.TextField("Prefab Path", customPrefabPath);
                customScrewTypeName = EditorGUILayout.TextField("Type Name", customScrewTypeName);
                
                if (GUILayout.Button("Switch to Custom Screw Type"))
                {
                    if (!string.IsNullOrEmpty(customPrefabPath))
                    {
                        SwitchScrewType(selectedLevel, customPrefabPath, customScrewTypeName);
                    }
                    else
                    {
                        Debug.LogWarning("Please specify a prefab path");
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawUtilitiesSection()
        {
            showUtilities = EditorGUILayout.Foldout(showUtilities, "Utilities", true);
            if (showUtilities)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Screw Management", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear All Screws"))
                {
                    if (EditorUtility.DisplayDialog("Clear Screws", "Are you sure you want to remove all screws from this level?", "Yes", "No"))
                    {
                        ClearAllScrews(selectedLevel);
                    }
                }
                if (GUILayout.Button("Count Screws"))
                {
                    CountScrews(selectedLevel);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
                maxScrewCount = EditorGUILayout.IntField("Max Screw Count", maxScrewCount);
                if (GUILayout.Button("Limit Screw Count"))
                {
                    LimitScrewCount(selectedLevel, maxScrewCount);
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Level Validation", EditorStyles.boldLabel);
                if (GUILayout.Button("Validate Level"))
                {
                    ValidateLevel(selectedLevel);
                }

                if (GUILayout.Button("Fix Missing Components"))
                {
                    FixMissingComponents(selectedLevel);
                }
                
                EditorGUI.indentLevel--;
            }
        }
        #endregion
        
        
        #region Screw Switching

        /// <summary>
        /// Switch all screws in the level to normal screws
        /// </summary>
        public static void SwitchToNormalScrew(Transform level)
        {
            if (level == null)
            {
                DebugLogger.LogError("Level transform is null");
                return;
            }

            BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(level).ToArray();

            if (screws.Length == 0)
            {
                DebugLogger.LogWarning($"No screws found in level: {level.name}");
                return;
            }

            GameObject normalScrewPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GamePlayConst.NORMAL_SCREW_PREFAB_PATH);

            if (normalScrewPrefab == null)
            {
                DebugLogger.LogError($"Normal screw prefab not found at path: {GamePlayConst.NORMAL_SCREW_PREFAB_PATH}");
                return;
            }

            List<BaseScrew> toRemoveList = new List<BaseScrew>(screws);

            DebugLogger.Log($"Switching {screws.Length} screws to normal screws in level: {level.name}", Color.cyan);

            for (int i = 0; i < screws.Length; i++)
            {
                Transform originalScrew = screws[i].transform;
                Transform parent = originalScrew.parent;

                // Store original transform data
                Vector3 position = originalScrew.position;
                Quaternion rotation = originalScrew.rotation;
                Vector3 localScale = originalScrew.localScale;
                string screwName = originalScrew.name;

                // Instantiate new normal screw
                Transform newScrew = ((GameObject)PrefabUtility.InstantiatePrefab(normalScrewPrefab, parent)).transform;

                // Apply original transform data
                newScrew.position = position;
                newScrew.rotation = rotation;
                newScrew.localScale = localScale;
                // newScrew.name = screwName;

                DebugLogger.LogIf(i < 5, $"Switched screw {i + 1}: {screwName}", Color.green);
            }

            // Remove original screws
            foreach (var screw in toRemoveList)
            {
                Object.DestroyImmediate(screw.gameObject);
            }

            EditorUtility.SetDirty(level);
            DebugLogger.Log($"Successfully switched {screws.Length} screws to normal screws", Color.green, true);
        }

        /// <summary>
        /// Switch all screws in the level to locked color screws
        /// </summary>
        public static void SwitchToLockedColorScrew(Transform level)
        {
            if (level == null)
            {
                DebugLogger.LogError("Level transform is null");
                return;
            }

            BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(level).ToArray();

            if (screws.Length == 0)
            {
                DebugLogger.LogWarning($"No screws found in level: {level.name}");
                return;
            }

            GameObject lockedColorScrewPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(GamePlayConst.LOCKED_COLOR_SCREW_PREFAB_PATH);

            if (lockedColorScrewPrefab == null)
            {
                DebugLogger.LogError(
                    $"Locked color screw prefab not found at path: {GamePlayConst.LOCKED_COLOR_SCREW_PREFAB_PATH}");
                return;
            }

            List<BaseScrew> toRemoveList = new List<BaseScrew>(screws);

            DebugLogger.Log($"Switching {screws.Length} screws to locked color screws in level: {level.name}",
                Color.cyan);

            for (int i = 0; i < screws.Length; i++)
            {
                Transform originalScrew = screws[i].transform;
                Transform parent = originalScrew.parent;

                // Store original transform data
                Vector3 position = originalScrew.position;
                Quaternion rotation = originalScrew.rotation;
                Vector3 localScale = originalScrew.localScale;
                string screwName = originalScrew.name;

                // Instantiate new locked color screw
                Transform newScrew = ((GameObject)PrefabUtility.InstantiatePrefab(lockedColorScrewPrefab, parent))
                    .transform;

                // Apply original transform data
                newScrew.position = position;
                newScrew.rotation = rotation;
                newScrew.localScale = localScale;
                newScrew.name = screwName;

                DebugLogger.LogIf(i < 5, $"Switched screw {i + 1}: {screwName}", Color.yellow);
            }

            // Remove original screws
            foreach (var screw in toRemoveList)
            {
                Object.DestroyImmediate(screw.gameObject);
            }

            EditorUtility.SetDirty(level);
            DebugLogger.Log($"Successfully switched {screws.Length} screws to locked color screws", Color.yellow, true);
        }

        /// <summary>
        /// Switch screws to a specific type by prefab path
        /// </summary>
        public static void SwitchScrewType(Transform level, string prefabPath, string screwTypeName = "custom")
        {
            if (level == null)
            {
                DebugLogger.LogError("Level transform is null");
                return;
            }

            BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(level).ToArray();

            if (screws.Length == 0)
            {
                DebugLogger.LogWarning($"No screws found in level: {level.name}");
                return;
            }

            GameObject targetScrewPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (targetScrewPrefab == null)
            {
                DebugLogger.LogError($"{screwTypeName} screw prefab not found at path: {prefabPath}");
                return;
            }

            List<BaseScrew> toRemoveList = new List<BaseScrew>(screws);

            DebugLogger.Log($"Switching {screws.Length} screws to {screwTypeName} screws in level: {level.name}",
                Color.magenta);

            for (int i = 0; i < screws.Length; i++)
            {
                Transform originalScrew = screws[i].transform;
                Transform parent = originalScrew.parent;

                // Store original transform data
                Vector3 position = originalScrew.position;
                Quaternion rotation = originalScrew.rotation;
                Vector3 localScale = originalScrew.localScale;
                string screwName = originalScrew.name;

                // Instantiate new screw
                Transform newScrew = ((GameObject)PrefabUtility.InstantiatePrefab(targetScrewPrefab, parent)).transform;

                // Apply original transform data
                newScrew.position = position;
                newScrew.rotation = rotation;
                newScrew.localScale = localScale;
                newScrew.name = screwName;
            }

            // Remove original screws
            foreach (var screw in toRemoveList)
            {
                Object.DestroyImmediate(screw.gameObject);
            }

            EditorUtility.SetDirty(level);
            DebugLogger.Log($"Successfully switched {screws.Length} screws to {screwTypeName} screws", Color.magenta,
                true);
        }

        #endregion


        #region Utility Methods

        /// <summary>
        /// Get all components of type T from all children recursively
        /// </summary>
        public static List<T> GetComponentsFromAllChildren<T>(Transform parent) where T : Component
        {
            List<T> components = new List<T>();
            GetComponentsFromAllChildrenRecursive<T>(parent, components);
            return components;
        }

        /// <summary>
        /// Recursive helper for GetComponentsFromAllChildren
        /// </summary>
        private static void GetComponentsFromAllChildrenRecursive<T>(Transform parent, List<T> components)
            where T : Component
        {
            T component = parent.GetComponent<T>();
            if (component != null)
            {
                components.Add(component);
            }

            foreach (Transform child in parent)
            {
                GetComponentsFromAllChildrenRecursive<T>(child, components);
            }
        }

        /// <summary>
        /// Count screws by type in the level
        /// </summary>
        public static Dictionary<string, int> GetScrewTypeCount(Transform level)
        {
            var screwCount = new Dictionary<string, int>();

            if (level == null) return screwCount;

            BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(level).ToArray();

            foreach (var screw in screws)
            {
                string screwType = screw.GetType().Name;

                if (screwCount.ContainsKey(screwType))
                    screwCount[screwType]++;
                else
                    screwCount[screwType] = 1;
            }

            return screwCount;
        }

        /// <summary>
        /// Log screw statistics for the level
        /// </summary>
        public static void LogScrewStatistics(Transform level)
        {
            if (level == null)
            {
                DebugLogger.LogError("Level transform is null");
                return;
            }

            var screwCount = GetScrewTypeCount(level);
            int totalScrews = screwCount.Values.Sum();

            DebugLogger.Log($"=== Screw Statistics for {level.name} ===", Color.cyan, true);
            DebugLogger.Log($"Total Screws: {totalScrews}", Color.white, true);

            foreach (var kvp in screwCount)
            {
                DebugLogger.Log($"- {kvp.Key}: {kvp.Value}", Color.white);
            }

            DebugLogger.Log("================================", Color.cyan);
        }

        
        
        /// <summary>
        /// Clear all screws from the level
        /// </summary>
        public static void ClearAllScrews(Transform level)
        {
            if (level == null)
            {
                DebugLogger.LogError("Level transform is null");
                return;
            }

            BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(level).ToArray();
            int screwCount = screws.Length;

            foreach (var screw in screws)
            {
                Object.DestroyImmediate(screw.gameObject);
            }

            EditorUtility.SetDirty(level);
            DebugLogger.Log($"Cleared {screwCount} screws from level: {level.name}", Color.red, true);
        }

        /// <summary>
        /// Count total screws in the level
        /// </summary>
        public static void CountScrews(Transform level)
        {
            if (level == null)
            {
                DebugLogger.LogError("Level transform is null");
                return;
            }

            BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(level).ToArray();
            DebugLogger.Log($"Total screws in {level.name}: {screws.Length}", Color.cyan, true);
        }

        /// <summary>
        /// Limit the number of screws in the level
        /// </summary>
        public static void LimitScrewCount(Transform level, int maxCount)
        {
            if (level == null)
            {
                DebugLogger.LogError("Level transform is null");
                return;
            }

            BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(level).ToArray();
            
            if (screws.Length <= maxCount)
            {
                DebugLogger.Log($"Level already has {screws.Length} screws (within limit of {maxCount})", Color.green);
                return;
            }

            int toRemoveCount = screws.Length - maxCount;
            var toRemove = screws.Skip(maxCount).ToArray();

            foreach (var screw in toRemove)
            {
                Object.DestroyImmediate(screw.gameObject);
            }

            EditorUtility.SetDirty(level);
            DebugLogger.Log($"Removed {toRemoveCount} screws. Level now has {maxCount} screws", Color.yellow, true);
        }

        /// <summary>
        /// Validate level structure and components
        /// </summary>
        public static void ValidateLevel(Transform level)
        {
            if (level == null)
            {
                DebugLogger.LogError("Level transform is null");
                return;
            }

            DebugLogger.Log($"=== Validating Level: {level.name} ===", Color.cyan, true);

            // Check for screws
            BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(level).ToArray();
            DebugLogger.Log($"Found {screws.Length} screws", Color.white);

            // Check for basic object parts
            var objectParts = GetComponentsFromAllChildren<BasicObjectPart>(level);
            DebugLogger.Log($"Found {objectParts.Count} object parts", Color.white);

            // Check for missing components
            var allTransforms = GetComponentsFromAllChildren<Transform>(level);
            int missingComponents = 0;

            foreach (var transform in allTransforms)
            {
                var components = transform.GetComponents<Component>();
                foreach (var component in components)
                {
                    if (component == null)
                    {
                        missingComponents++;
                    }
                }
            }

            if (missingComponents > 0)
            {
                DebugLogger.LogWarning($"Found {missingComponents} missing components");
            }
            else
            {
                DebugLogger.Log("No missing components found", Color.green);
            }

            DebugLogger.Log("=== Validation Complete ===", Color.cyan);
        }
        
        
        /// <summary>
        /// Fix missing components in the level
        /// </summary>
        public static void FixMissingComponents(Transform level)
        {
            if (level == null)
            {
                DebugLogger.LogError("Level transform is null");
                return;
            }

            var allTransforms = GetComponentsFromAllChildren<Transform>(level);
            int fixedCount = 0;

            foreach (var transform in allTransforms)
            {
                if (GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject) > 0)
                {
                    fixedCount++;
                }
            }

            EditorUtility.SetDirty(level);
            DebugLogger.Log($"Fixed missing components on {fixedCount} GameObjects", Color.green, true);
        }
        #endregion
    }
}
#endif