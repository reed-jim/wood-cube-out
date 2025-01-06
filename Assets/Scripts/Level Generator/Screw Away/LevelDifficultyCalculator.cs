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
        const float WEIGHT_NUM_SCREW = 0.02f;
        const float WEIGHT_PHASES_DIFFICULTY = 0.7f;
        const float WEIGHT_INITIAL_RANDOM_SCREW_FACTION = 0.28f;


        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(levelPath);

        // BasicObjectPart[] objectParts = GetComponentsFromAllChildren<BasicObjectPart>(levelPrefab.transform).ToArray();

        // Number Screw difficulty
        float numScrewDifficulty = 0;

        BasicScrew[] screws = GetComponentsFromAllChildren<BasicScrew>(levelPrefab.transform).ToArray();

        int numScrew = screws.Length;

        numScrewDifficulty = numScrew;




        // Initial difficulty
        float modelDifficulty = 0;

        float percentBlockedScrews = 0;

        int numScrewBlocked = 0;

        for (int i = 0; i < numScrew; i++)
        {
            int numBlockingObjects = screws[i].CountBlockingObjectsPrefabMode();

            if (numBlockingObjects > 0)
            {
                numScrewBlocked++;
            }
        }

        percentBlockedScrews = (float)numScrewBlocked / numScrew;

        modelDifficulty = percentBlockedScrews * numScrew;



        // Screw boxes generation difficulty
        float screwBoxesGenerationDifficulty = 0;

        float currentProgress = 0;

        for (int i = 0; i < levelDifficultyConfiguration.LevelPhases.Length; i++)
        {
            LevelPhase phase = levelDifficultyConfiguration.LevelPhases[i];

            screwBoxesGenerationDifficulty += (phase.EndProgress - currentProgress) * Mathf.Pow(2, i);

            currentProgress = phase.EndProgress;
        }



        float finalDifficulty =
            WEIGHT_NUM_SCREW * numScrewDifficulty +
            WEIGHT_PHASES_DIFFICULTY * screwBoxesGenerationDifficulty +
            WEIGHT_INITIAL_RANDOM_SCREW_FACTION * modelDifficulty;


        Debug.Log(numScrewDifficulty + "/" + screwBoxesGenerationDifficulty + "/" + modelDifficulty + "/" + finalDifficulty);
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
