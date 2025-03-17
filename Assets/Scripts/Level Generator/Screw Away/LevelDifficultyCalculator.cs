
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static GameEnum;
#if UNITY_EDITOR
using UnityEditor;

public class LevelDifficultyCalculator : EditorWindow
{
    private int level;
    private int startLevel;
    private int endLevel;
    private Transform levelContainer;

    private LevelDataContainer levelDataContainer;

    private LevelDifficultyConfiguration _currentLevelDifficultyConfiguration;
    private float expectedDifficulty;

    private string prefabPath = "Assets/Prefabs/MyPrefab.prefab";

    private LevelDifficultyConfiguration levelDifficultyConfiguration;


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

        // level = EditorGUILayout.IntField("Level", level);
        startLevel = EditorGUILayout.IntField("Start Level", startLevel);
        endLevel = EditorGUILayout.IntField("End Level", endLevel);
        levelContainer = (Transform)EditorGUILayout.ObjectField("Level Container", levelContainer, typeof(Transform), true);

        levelDataContainer = (LevelDataContainer)EditorGUILayout.ObjectField("Level Data Container", levelDataContainer, typeof(LevelDataContainer), true);
        expectedDifficulty = EditorGUILayout.FloatField("Expected Difficulty", expectedDifficulty);

        if (GUILayout.Button("Calculate"))
        {
            Calculate();
        }

        if (GUILayout.Button("Calculate Fast Range"))
        {
            CalculateFastAll();
        }

        if (GUILayout.Button("Generate Screw Box Difficulty"))
        {
            GenerateScrewBoxDifficulty();
        }

        if (GUILayout.Button("Generate Screw Box Difficulty Template"))
        {
            GenerateScrewBoxDifficultyTemplate(level);
        }

        levelDifficultyConfiguration = (LevelDifficultyConfiguration)EditorGUILayout.ObjectField("Result", levelDifficultyConfiguration, typeof(LevelDifficultyConfiguration), true);

        GUILayout.EndArea();
    }

    private void CalculateFastAll()
    {
        for (int i = startLevel; i <= endLevel; i++)
        {
            CalulateFast(i);
        }
    }

    private void CalulateFast(int level)
    {
        // Transform level = Selection.activeTransform;

        // Auto load level into current scene for simulating Physics
        string prefabPath = $"Assets/Prefabs/Screw Away/Levels/Level {level}.prefab";

        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(prefabPath);

        if (levelContainer.childCount > 0)
        {
            DestroyImmediate(levelContainer.GetChild(0).gameObject);
        }

        Transform sceneLevel = Instantiate(levelPrefab, levelContainer).transform;

        BasicScrew[] screws = GetComponentsFromAllChildren<BasicScrew>(sceneLevel).ToArray();

        int numScrew = screws.Length;

        Dictionary<string, List<string>> obstaclesOfPieces = new Dictionary<string, List<string>>();

        bool isLevelRubik = false;

        int totalBlockedScrew = 0;

        for (int i = 0; i < numScrew; i++)
        {
            string pieceName = screws[i].transform.parent.name;

            if (pieceName.Contains("L-Shaped"))
            {
                isLevelRubik = true;

                break;
            }

            List<string> obstacleName = GetScrewObstacles(screws[i].transform, isIncludeHiddenScrew: false);

            if (obstacleName.Count > 0)
            {
                totalBlockedScrew++;
            }

            if (!obstaclesOfPieces.ContainsKey(pieceName))
            {
                obstaclesOfPieces.Add(pieceName, obstacleName);
            }
            else
            {
                foreach (var item in obstacleName)
                {
                    if (!obstaclesOfPieces[pieceName].Contains(item))
                    {
                        obstaclesOfPieces[pieceName].Add(item);
                    }
                }
            }
        }

        Debug.Log(
            $"SAFERIO <color=#73FF83>Level {level}: <color=#FF7A75>{totalBlockedScrew}/{numScrew} ({(float)totalBlockedScrew / numScrew * 100}%<color=#fff>)");

        // // DEBUG
        // foreach (var piece in obstaclesOfPieces)
        // {
        //     foreach (var obstacle in piece.Value)
        //     {
        //         Debug.Log("SAFERIO " + piece.Key + " / " + obstacle);
        //     }

        //     Debug.Log("SAFERIO ----------------");
        // }

        if (isLevelRubik)
        {
            Debug.Log($"SAFERIO <color=#73FF83>Level {level}: Rubik<color=#fff>");

            return;
        }

        bool isValid = true;

        // CHECK LEVEL DESIGN ISSUES
        foreach (var obstaclesOfPiece in obstaclesOfPieces)
        {
            foreach (var obstacle in obstaclesOfPiece.Value)
            {
                if (obstaclesOfPieces[obstacle].Contains(obstaclesOfPiece.Key))
                {
                    Debug.Log($"SAFERIO <color=#73FF83>Level {level}:<color=#fff> {obstaclesOfPiece.Key} {obstacle} <color=#FF7A75>INVALID<color=#fff>");

                    isValid = false;

                    break;
                }

            }
        }

        if (isValid)
        {
            Debug.Log($"SAFERIO <color=#73FF83>Level {level}: VALID<color=#fff>");
        }
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

            if (numBlockingObjects > 1)
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
        difficultyText += $" <color=#fff>- Total Screws Blocked: <color=#FF7A75>({numScrewBlocked})";
        difficultyText += $" <color=#fff>- Total Obstacles: <color=#FF7A75>({totalObstacles})";
        difficultyText += $" <color=#fff>- Final Difficulty Multiplier: <color=#FF7A75>({finalDifficultyMultiplier})";
        difficultyText += $" <color=#fff>- Final Difficulty: <color=#FF7A75>{finalDifficulty}</color>";

        Debug.Log(difficultyText);

        EditorGUIUtility.systemCopyBuffer = $"{finalDifficulty}";


        List<LevelPhase> generatedLevelPhases = new List<LevelPhase>();

        // GENERATE BASED ON EXPECTED DIFFICULTY
        float remainingDifficulty = expectedDifficulty -
            WEIGHT_BLOCKED_RATIO_DIFFICULTY * blockedRatioDifficultyNormalized +
            WEIGHT_SCREW_PORT_FACTION_CHANCE_MODIFIER * screwPortFactionChanceModifierDifficultyNormalized +
            WEIGHT_START_GAME_TRIPLE_SAME_FACTION_GENERATION * startGameTripleSameFactionGenerationDifficulty;

        float remainingDifficultyNormalized = remainingDifficulty / WEIGHT_PHASES_DIFFICULTY;

        Debug.Log(remainingDifficulty + "/" + remainingDifficultyNormalized);

        for (int i = 7; i >= 0; i--)
        {
            int phaseIndex = i;

            float progress = remainingDifficultyNormalized;

            progress = Mathf.Clamp01(progress);

            LevelPhase levelPhase = new LevelPhase(phaseIndex: 0, endProgress: 1 - progress);

            generatedLevelPhases.Add(levelPhase);

            levelPhase = new LevelPhase(phaseIndex, 1);

            generatedLevelPhases.Add(levelPhase);

            break;
        }

        levelDifficultyConfiguration = new LevelDifficultyConfiguration();

        levelDifficultyConfiguration.LevelPhases = generatedLevelPhases.ToArray();
        levelDifficultyConfiguration.AdsScrewBoxDifficultyLevelModifier = -99;
        levelDifficultyConfiguration.RatioGeneratingSameFactionForTripleNoBlockedScrewStartGame = 1;
    }

    public static LevelDifficultyConfiguration GenerateScrewBoxDifficultyTemplate(int level)
    {
        float WEIGHT_REPEAT_TEMPLATE = 0.8f;
        float WEIGHT_LEVEL_INDEX = 1 - WEIGHT_REPEAT_TEMPLATE;
        float DIFFICULTY_MULTIPLIER = 1.6f;

        float MAX_SCREW_BOXES_GENERATION_DIFFICULTY = Mathf.Pow(2, GameConstants.SCREW_FACTION.Length - 1);

        List<LevelPhase> generatedLevelPhases = new List<LevelPhase>();

        // // RANDOM
        // List<int> validMaxPhaseIndexToRandom = new List<int>();

        // for (int i = 0; i < GameConstants.SCREW_FACTION.Length; i++)
        // {
        //     if (0.9f * (Mathf.Pow(2, i) / MAX_SCREW_BOXES_GENERATION_DIFFICULTY) > expectedDifficultyNormalized)
        //     {
        //         validMaxPhaseIndexToRandom.Add(i);
        //     }
        // }

        // int randomIndex = UnityEngine.Random.Range(0, validMaxPhaseIndexToRandom.Count);
        // int maxPhase = validMaxPhaseIndexToRandom[randomIndex];

        int PREDICTED_MAX_LEVEL = 100;

        float[] weightForModules = new float[5] { 0.08f, 0.12f, 0.18f, 0.4f, 0.22f };

        float expectedDifficultyNormalized =
            DIFFICULTY_MULTIPLIER * WEIGHT_REPEAT_TEMPLATE * weightForModules[(level - 1) % 5] + WEIGHT_LEVEL_INDEX * ((float)level / PREDICTED_MAX_LEVEL);

        int maxPhaseIndex = GameConstants.SCREW_FACTION.Length - 1;

        for (int i = maxPhaseIndex; i >= 0; i--)
        {
            int phaseIndex = i;

            float progress = expectedDifficultyNormalized / Mathf.Pow(2, phaseIndex) * MAX_SCREW_BOXES_GENERATION_DIFFICULTY;

            if (progress > 0.1f && progress < 0.9f)
            {
                LevelPhase levelPhase = new LevelPhase(0, 1 - progress);

                generatedLevelPhases.Add(levelPhase);

                levelPhase = new LevelPhase(phaseIndex, 1);

                generatedLevelPhases.Add(levelPhase);

                break;
            }
            else
            {
                continue;
            }
        }

        // levelDataContainer.SetLevelDifficultyConfiguration(30, generatedLevelPhases.ToArray());

        LevelDifficultyConfiguration levelDifficultyConfiguration = new LevelDifficultyConfiguration();

        levelDifficultyConfiguration.LevelPhases = generatedLevelPhases.ToArray();
        levelDifficultyConfiguration.AdsScrewBoxDifficultyLevelModifier = -99;
        levelDifficultyConfiguration.RatioGeneratingSameFactionForTripleNoBlockedScrewStartGame = 1;

        return levelDifficultyConfiguration;
    }

    private void GenerateScrewBoxDifficulty()
    {
        float MAX_SCREW_BOXES_GENERATION_DIFFICULTY = Mathf.Pow(2, GameConstants.SCREW_FACTION.Length - 1);

        List<LevelPhase> generatedLevelPhases = new List<LevelPhase>();

        // RANDOM
        List<int> validMaxPhaseIndexToRandom = new List<int>();

        for (int i = 0; i < GameConstants.SCREW_FACTION.Length; i++)
        {
            if (0.9f * (Mathf.Pow(2, i) / MAX_SCREW_BOXES_GENERATION_DIFFICULTY) > expectedDifficulty)
            {
                validMaxPhaseIndexToRandom.Add(i);
            }
        }

        int randomIndex = UnityEngine.Random.Range(0, validMaxPhaseIndexToRandom.Count);
        int maxPhase = validMaxPhaseIndexToRandom[randomIndex];

        for (int i = maxPhase; i >= 0; i--)
        {
            int phaseIndex = i;

            float progress = expectedDifficulty / Mathf.Pow(2, phaseIndex) * MAX_SCREW_BOXES_GENERATION_DIFFICULTY;

            if (progress > 0.1f && progress < 0.9f)
            {
                LevelPhase levelPhase = new LevelPhase(0, 1 - progress);

                generatedLevelPhases.Add(levelPhase);

                levelPhase = new LevelPhase(phaseIndex, 1);

                generatedLevelPhases.Add(levelPhase);

                break;
            }
            else
            {
                continue;
            }
        }

        levelDataContainer.SetLevelDifficultyConfiguration(level, generatedLevelPhases.ToArray());
    }

    #region SCREW
    public List<string> GetScrewObstacles(Transform screw, bool isIncludeHiddenScrew)
    {
        MeshFilter meshFilter = screw.GetComponent<MeshFilter>();

        float screwRadius = screw.lossyScale.x * 0.45f * 0.779f * meshFilter.sharedMesh.bounds.size.x;

        Vector3 pointOne = screw.position + screw.lossyScale.z * 1f * 0.779f * meshFilter.sharedMesh.bounds.size.z * screw.forward;
        Vector3 pointTwo = pointOne + 10f * screw.forward;

        Collider[] hits = new Collider[16];

        Physics.OverlapCapsuleNonAlloc(pointOne, pointTwo, screwRadius, hits, LayerMask.GetMask("Default"));

        List<string> obstacleInstanceIds = new List<string>();

        if (hits != null)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] == null)
                {
                    continue;
                }

                if (hits[i].transform == screw.parent)
                {
                    continue;
                }

                IObjectPart objectPart = hits[i].transform.GetComponent<IObjectPart>();

                if (objectPart != null)
                {
                    if (isIncludeHiddenScrew)
                    {
                        continue;
                    }

                    Vector3 hitPos;

                    // Concave
                    if (objectPart.IsConcave)
                    {
                        RaycastHit hit;

                        float radius = screw.lossyScale.x * 0.45f * 0.779f * meshFilter.sharedMesh.bounds.size.x;

                        Physics.CapsuleCast(
                            pointOne - radius * screw.forward, pointOne - radius * screw.forward + 0.01f * screw.forward,
                            screw.lossyScale.x * 0.45f * 0.779f * meshFilter.sharedMesh.bounds.size.x, screw.forward, out hit, 10, LayerMask.GetMask("Default"));

                        if (hit.collider != null)
                        {
                            hitPos = hit.point;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        hitPos = hits[i].ClosestPoint(pointOne);
                    }

                    // Perpendicular distance
                    Vector3 positionInScrewSpace = screw.InverseTransformPoint(hitPos);

                    float distance = positionInScrewSpace.z - screw.lossyScale.z * 1f * 0.779f * meshFilter.sharedMesh.bounds.size.z;

                    distance = Mathf.Max(distance, 0);

                    if (distance < screw.lossyScale.z * 0.5f * 0.779f * meshFilter.sharedMesh.bounds.size.z)
                    {
                        if (Mathf.Abs(positionInScrewSpace.x) > screwRadius || Mathf.Abs(positionInScrewSpace.y) > screwRadius)
                        {
                            continue;
                        }

                        obstacleInstanceIds.Add(hits[i].transform.name);
                    }
                }
            }
        }

        return obstacleInstanceIds;
    }
    #endregion

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


public static class LevelDifficultyCalculatorUltility
{
    public static LevelDifficultyConfiguration GenerateScrewBoxDifficultyTemplate(int level)
    {
        float WEIGHT_REPEAT_TEMPLATE = 0.8f;
        float WEIGHT_LEVEL_INDEX = 1 - WEIGHT_REPEAT_TEMPLATE;
        float DIFFICULTY_MULTIPLIER = RemoteConfigController.GetFloatConfig(FirebaseConfig.DIFFICULTY_MULTIPLIER, 0.4f);

        if (RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_JOINT, false))
        {
            DIFFICULTY_MULTIPLIER *= 1.5f;
        }

        float MAX_SCREW_BOXES_GENERATION_DIFFICULTY = Mathf.Pow(2, GameConstants.SCREW_FACTION.Length - 1);

        List<LevelPhase> generatedLevelPhases = new List<LevelPhase>();

        int PREDICTED_MAX_LEVEL = 125;

        // float[] weightForModules = new float[5] { 0.08f, 0.12f, 0.18f, 0.4f, 0.22f };
        float[] weightForModules = new float[5] { 0.2f, 0.3f, 0.45f, 1f, 0.55f };

        int validLevel = Mathf.Max(level, 1) % PREDICTED_MAX_LEVEL;

        float expectedDifficultyNormalized =
            DIFFICULTY_MULTIPLIER *
                (WEIGHT_REPEAT_TEMPLATE * weightForModules[(Mathf.Max(level, 1) - 1) % 5] + WEIGHT_LEVEL_INDEX * ((float)validLevel / PREDICTED_MAX_LEVEL));

        int maxPhaseIndex = GameConstants.SCREW_FACTION.Length - 1;

        // // DEBUG
        // for (int i = 1; i <= 125; i++)
        // {
        //     float test =
        //     DIFFICULTY_MULTIPLIER *
        //         (WEIGHT_REPEAT_TEMPLATE * weightForModules[(Mathf.Max(i, 1) - 1) % 5] + WEIGHT_LEVEL_INDEX * ((float)i / PREDICTED_MAX_LEVEL));

        //     test *= (0.4f / 1f);

        //     Debug.Log($"SAFERIO LEVEL {i} - Hard Percent: {test * 100}%");
        // }

        // // RANDOMIZE
        // maxPhaseIndex = UnityEngine.Random.Range(GameConstants.SCREW_FACTION.Length / 2, GameConstants.SCREW_FACTION.Length - 1);

        for (int i = maxPhaseIndex; i >= 0; i--)
        {
            int phaseIndex = i;

            float progress = expectedDifficultyNormalized / Mathf.Pow(2, phaseIndex) * MAX_SCREW_BOXES_GENERATION_DIFFICULTY;

            progress = Mathf.Clamp01(progress);

            LevelPhase levelPhase = new LevelPhase(phaseIndex: 0, endProgress: 1 - progress);

            generatedLevelPhases.Add(levelPhase);

            levelPhase = new LevelPhase(phaseIndex, 1);

            generatedLevelPhases.Add(levelPhase);

            break;
        }

        LevelDifficultyConfiguration levelDifficultyConfiguration = new LevelDifficultyConfiguration();
        // LevelDifficultyConfiguration levelDifficultyConfiguration = ScriptableObject.CreateInstance<LevelDifficultyConfiguration>();

        levelDifficultyConfiguration.LevelPhases = generatedLevelPhases.ToArray();
        levelDifficultyConfiguration.AdsScrewBoxDifficultyLevelModifier = -99;
        levelDifficultyConfiguration.RatioGeneratingSameFactionForTripleNoBlockedScrewStartGame = 1;

        return levelDifficultyConfiguration;
    }

    public static Dictionary<int, List<Transform>> FindPieceDependency(BasicObjectPart[] pieces)
    {
        Dictionary<int, List<Transform>> obstaclesOfPieces = new Dictionary<int, List<Transform>>();

        for (int i = 0; i < pieces.Length; i++)
        {
            List<Transform> dependency = GetPieceDependency(pieces[i].transform);

            obstaclesOfPieces.Add(pieces[i].transform.GetInstanceID(), dependency);
        }

        GamePersistentVariable.pieceDependencies = obstaclesOfPieces;

        // int totalBlockedScrew = 0;

        // for (int i = 0; i < numScrew; i++)
        // {
        //     int pieceName = screws[i].transform.parent.GetInstanceID();

        //     List<Transform> obstacleName = GetScrewObstacles(screws[i].transform, isIncludeHiddenScrew: true);

        //     if (obstacleName.Count > 0)
        //     {
        //         totalBlockedScrew++;
        //     }

        //     if (!obstaclesOfPieces.ContainsKey(pieceName))
        //     {
        //         obstaclesOfPieces.Add(pieceName, obstacleName);
        //     }
        //     else
        //     {
        //         foreach (var item in obstacleName)
        //         {
        //             if (!obstaclesOfPieces[pieceName].Contains(item))
        //             {
        //                 obstaclesOfPieces[pieceName].Add(item);
        //             }
        //         }
        //     }
        // }

        return obstaclesOfPieces;
    }

    public static List<Transform> GetPieceDependency(Transform piece)
    {
        List<Transform> dependency = new List<Transform>();

        Queue<Transform> toProcessPieces = new Queue<Transform>();

        List<int> checkedList = new List<int>();

        toProcessPieces.Enqueue(piece);

        checkedList.Add(piece.GetInstanceID());

        while (toProcessPieces.Count > 0)
        {
            Transform nextPiece = toProcessPieces.Dequeue();

            for (int i = 0; i < nextPiece.childCount; i++)
            {
                BaseScrew screw = nextPiece.GetChild(i).GetComponent<BaseScrew>();

                List<Transform> newFoundPieces = GetScrewObstacles(screw.transform, true);

                foreach (var item in newFoundPieces)
                {
                    if (!checkedList.Contains(item.GetInstanceID()))
                    {
                        toProcessPieces.Enqueue(item);

                        if (!dependency.Contains(item))
                        {
                            dependency.Add(item);
                        }

                        checkedList.Add(item.GetInstanceID());
                    }
                }
            }
        }

        return dependency;
    }

    public static List<Transform> GetScrewObstacles(Transform screw, bool isIncludeHiddenScrew)
    {
        MeshFilter meshFilter = screw.GetComponent<MeshFilter>();

        float screwRadius = screw.lossyScale.x * 0.45f * 0.779f * meshFilter.sharedMesh.bounds.size.x;

        Vector3 pointOne = screw.position + screw.lossyScale.z * 1f * 0.779f * meshFilter.sharedMesh.bounds.size.z * screw.forward;
        Vector3 pointTwo = pointOne + 10f * screw.forward;

        Collider[] hits = Physics.OverlapCapsule(pointOne, pointTwo, screwRadius, LayerMask.GetMask("Default"));

        List<Transform> obstacleInstanceIds = new List<Transform>();

        if (hits != null)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform == screw.parent)
                {
                    continue;
                }

                IObjectPart objectPart = hits[i].transform.GetComponent<IObjectPart>();

                if (objectPart != null)
                {
                    // obstacleInstanceIds.Add(hits[i].transform);

                    // if (isIncludeHiddenScrew)
                    // {
                    //     continue;
                    // }

                    Vector3 hitPos;

                    // Concave
                    if (objectPart.IsConcave)
                    {
                        RaycastHit hit;

                        float radius = screw.lossyScale.x * 0.45f * 0.779f * meshFilter.sharedMesh.bounds.size.x;

                        Physics.CapsuleCast(
                            pointOne - radius * screw.forward, pointOne - radius * screw.forward + 0.01f * screw.forward,
                            screw.lossyScale.x * 0.45f * 0.779f * meshFilter.sharedMesh.bounds.size.x, screw.forward, out hit, 10, LayerMask.GetMask("Default"));

                        if (hit.collider != null)
                        {
                            hitPos = hit.point;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        hitPos = hits[i].ClosestPoint(pointOne);
                    }

                    // Perpendicular distance
                    Vector3 positionInScrewSpace = screw.InverseTransformPoint(hitPos);

                    float distance = positionInScrewSpace.z - screw.lossyScale.z * 1f * 0.779f * meshFilter.sharedMesh.bounds.size.z;

                    distance = Mathf.Max(distance, 0);

                    if (distance < screw.lossyScale.z * 0.5f * 0.779f * meshFilter.sharedMesh.bounds.size.z)
                    {
                        if (Mathf.Abs(positionInScrewSpace.x) > screwRadius || Mathf.Abs(positionInScrewSpace.y) > screwRadius)
                        {
                            continue;
                        }

                        obstacleInstanceIds.Add(hits[i].transform);
                    }
                }
            }
        }

        return obstacleInstanceIds;
    }
}
