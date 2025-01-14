#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static GameEnum;

public class LevelDifficultyCalculator : EditorWindow
{
    private int level;
    private LevelDataContainer levelDataContainer;

    private LevelDifficultyConfiguration _currentLevelDifficultyConfiguration;

    private string prefabPath = "Assets/Prefabs/MyPrefab.prefab";


    private string resultText;


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

        // prefabPath = EditorGUILayout.TextField("Prefab Path", prefabPath);

        level =
            EditorGUILayout.IntField("Level", level);
        levelDataContainer =
            (LevelDataContainer)EditorGUILayout.ObjectField("Level Data Container", levelDataContainer, typeof(LevelDataContainer), true);

        if (GUILayout.Button("Calculate"))
        {
            Calculate();
        }

        GUILayout.EndArea();
    }

    private void Calculate()
    {
        const int MAX_SCREW = 300;

        // const float WEIGHT_NUM_SCREW = 0.2f;
        const float WEIGHT_PHASES_DIFFICULTY = 580f;
        // const float WEIGHT_MODEL_DIFFICULTY = 1.8f;
        const float WEIGHT_BLOCKED_RATIO_DIFFICULTY = 20f;
        const float WEIGHT_SCREW_PORT_FACTION_CHANCE_MODIFIER = 200f;
        const float WEIGHT_START_GAME_TRIPLE_SAME_FACTION_GENERATION = 200f;

        prefabPath = $"Assets/Prefabs/Screw Away/Levels/Level {level}.prefab";

        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(prefabPath);

        _currentLevelDifficultyConfiguration = levelDataContainer.GetLevelDifficultyConfiguration(level);



        // Number Screw difficulty
        BasicScrew[] screws = GetComponentsFromAllChildren<BasicScrew>(levelPrefab.transform).ToArray();

        int numScrew = screws.Length;

        float numScrewDifficultyNormalized = (float)numScrew / MAX_SCREW;



        // Initial difficulty
        float MAX_SCREW_BLOCKED = (numScrew - 1);

        int totalObstacles = 0;
        int numScrewBlocked = 0;

        for (int i = 0; i < numScrew; i++)
        {
            int numBlockingObjects = screws[i].CountBlockingObjects(isIncludeHiddenScrew: false);

            totalObstacles += numBlockingObjects;

            if (numBlockingObjects > 0)
            {
                numScrewBlocked++;
            }

            // if (numBlockingObjects > 2)
            // {
            //     totalObstacles++;
            // }
        }

        float blockedRatioDifficultyNormalized = (float)numScrewBlocked / MAX_SCREW_BLOCKED;
        float finalDifficultyMultiplier = (1 + 0.1f * ((float)totalObstacles / numScrew) * totalObstacles / 100f);



        // Screw boxes generation difficulty
        float MAX_SCREW_BOXES_GENERATION_DIFFICULTY = Mathf.Pow(2, GameConstants.SCREW_FACTION.Length - 1);

        float screwBoxesGenerationDifficulty = 0;

        float currentProgress = 0;

        for (int i = 0; i < _currentLevelDifficultyConfiguration.LevelPhases.Length; i++)
        {
            LevelPhase phase = _currentLevelDifficultyConfiguration.LevelPhases[i];

            screwBoxesGenerationDifficulty += (phase.EndProgress - currentProgress) * Mathf.Pow(2, phase.PhaseIndex);

            currentProgress = phase.EndProgress;
        }

        float screwBoxesGenerationDifficultyNormalized = screwBoxesGenerationDifficulty / MAX_SCREW_BOXES_GENERATION_DIFFICULTY;



        // Chance to get screw ports' faction difficulty
        float MAX_SCREW_PORT_FACTION_DIFFICULTY_MODIFIER = 99;

        float screwPortFactionChanceModifierDifficultyNormalized =
            _currentLevelDifficultyConfiguration.ScrewPortFactionDifficultyModifier / MAX_SCREW_PORT_FACTION_DIFFICULTY_MODIFIER;



        // Number of triple matched color no blocked at game start
        float startGameTripleSameFactionGenerationDifficulty =
            1 - _currentLevelDifficultyConfiguration.RatioGeneratingSameFactionForTripleNoBlockedScrewStartGame;


        // Final
        float finalDifficulty =
            // WEIGHT_NUM_SCREW * numScrewDifficultyNormalized +
            WEIGHT_PHASES_DIFFICULTY * screwBoxesGenerationDifficultyNormalized +
            // WEIGHT_MODEL_DIFFICULTY * modelDifficultyNormalized +
            WEIGHT_BLOCKED_RATIO_DIFFICULTY * blockedRatioDifficultyNormalized +
            WEIGHT_SCREW_PORT_FACTION_CHANCE_MODIFIER * screwPortFactionChanceModifierDifficultyNormalized +
            WEIGHT_START_GAME_TRIPLE_SAME_FACTION_GENERATION * startGameTripleSameFactionGenerationDifficulty;

        finalDifficulty *= finalDifficultyMultiplier;

        // string difficultyText = $"Num Screw Difficulty: <color=#FF7A75>{WEIGHT_NUM_SCREW * numScrewDifficultyNormalized}";
        // difficultyText += $" <color=#9DFF8D>({numScrewDifficultyNormalized * 100}%)";
        // difficultyText += $" <color=#fff>- Model Difficulty: <color=#FF7A75>{WEIGHT_MODEL_DIFFICULTY * modelDifficultyNormalized}";
        // difficultyText += $" <color=#9DFF8D>({modelDifficultyNormalized * 100}%)";
        string difficultyText = $"Screw Boxes Generation Difficulty: <color=#FF7A75>{WEIGHT_PHASES_DIFFICULTY * screwBoxesGenerationDifficultyNormalized}";
        difficultyText += $" <color=#9DFF8D>({screwBoxesGenerationDifficultyNormalized * 100}%)";
        difficultyText += $" <color=#fff>- Blocked Ratio Difficulty: <color=#FF7A75>{WEIGHT_BLOCKED_RATIO_DIFFICULTY * blockedRatioDifficultyNormalized}";
        difficultyText += $" <color=#9DFF8D>({blockedRatioDifficultyNormalized * 100}%)";
        difficultyText += $" <color=#fff>- Screw Port Faction Chance Modifier Difficulty: <color=#FF7A75>{WEIGHT_SCREW_PORT_FACTION_CHANCE_MODIFIER * screwPortFactionChanceModifierDifficultyNormalized}";
        difficultyText += $" <color=#9DFF8D>({screwPortFactionChanceModifierDifficultyNormalized * 100}%)";
        difficultyText += $" <color=#fff>- Start Game Triple Match Difficulty: <color=#FF7A75>{WEIGHT_START_GAME_TRIPLE_SAME_FACTION_GENERATION * startGameTripleSameFactionGenerationDifficulty}";
        difficultyText += $" <color=#9DFF8D>({startGameTripleSameFactionGenerationDifficulty * 100}%)";
        difficultyText += $" <color=#fff>- Total Screws: <color=#FF7A75>({numScrew})";
        difficultyText += $" <color=#fff>- Total Obstacles: <color=#FF7A75>({totalObstacles})";
        difficultyText += $" <color=#fff>- Final Difficulty Multiplier: <color=#FF7A75>({finalDifficultyMultiplier})";
        difficultyText += $" <color=#fff>- Final Difficulty: <color=#FF7A75>{finalDifficulty}</color>";

        Debug.Log(difficultyText);

        EditorGUIUtility.systemCopyBuffer = $"{finalDifficulty}";
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
