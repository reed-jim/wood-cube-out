#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static GameEnum;

public class LevelDifficultyCalculator : EditorWindow
{
    private LevelDifficultyConfiguration levelDifficultyConfiguration;



    private Vector3 dimensionSize;
    private Vector3 distance;
    private float expectedScale;
    private Vector3 expectedLocalPositionOffset;
    private Vector3 expectedLocalRotationOffset;



    private string prefabPath = "Assets/Prefabs/MyPrefab.prefab";

    [MenuItem("Saferio/Tools/Level Difficulty Calculator")]
    public static void ShowWindow()
    {
        GetWindow<LevelDifficultyCalculator>("Level Difficulty Calculator");
    }

    private void OnGUI()
    {
        float padding = 20f;

        Rect areaRect = new Rect(padding, padding, position.width - 2 * padding, position.height - 2 * padding);

        GUILayout.BeginArea(areaRect);

        prefabPath = EditorGUILayout.TextField("Prefab Path", prefabPath);

        levelDifficultyConfiguration =
            (LevelDifficultyConfiguration)EditorGUILayout.ObjectField("Level Difficulty Configuration", levelDifficultyConfiguration, typeof(LevelDifficultyConfiguration), true);

        if (GUILayout.Button("Calculate"))
        {
            Calculate(prefabPath);
        }

        GUILayout.EndArea();
    }

    private void Calculate(string levelPath)
    {
        // GameObject levelPrefab = PrefabUtility.LoadPrefabContents(levelPath);

        // BasicObjectPart[] objectParts = GetComponentsFromAllChildren<BasicObjectPart>(levelPrefab.transform).ToArray();

        // BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(levelPrefab.transform).ToArray();

        float difficulty = 0;

        float currentProgress = 0;

        for (int i = 0; i < levelDifficultyConfiguration.LevelPhases.Length; i++)
        {
            LevelPhase phase = levelDifficultyConfiguration.LevelPhases[i];

            difficulty += (phase.EndProgress - currentProgress) * Mathf.Pow(2, i);

            currentProgress = phase.EndProgress;
        }

        Debug.Log(difficulty);
    }

    #region UTIL
    public List<T> GetComponentsFromAllChildren<T>(Transform parent) where T : Component
    {
        List<T> components = new List<T>();
        GetComponentsFromAllChildrenRecursive<T>(parent, components);
        return components;
    }

    private void GetComponentsFromAllChildrenRecursive<T>(Transform parent, List<T> components) where T : Component
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
    #endregion
}
#endif
