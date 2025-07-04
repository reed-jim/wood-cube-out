#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Saferio.Util.SaferioTween;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static GameEnum;

public class HingeJointGenerator : EditorWindow
{
    private GameObject target;
    private GameObject screwPrefab;
    private Material targetMaterial;



    private Vector3 dimensionSize;
    private Vector3 distance;
    private float expectedScale = 1.2f;
    private int maxScrew = 124;
    private float expectedOffsetRatioAutoSnapSurface = 0.55f;
    private Vector3 expectedLocalPositionOffset;
    private Vector3 expectedLocalRotationOffset;



    private float ratioSnapOffset = 0.06f;



    private string prefabPath = "Assets/Prefabs/MyPrefab.prefab";


    private Vector2 scrollPosition;

    #region GENERATE LEVEL
    private GameObject piecePrefab;
    #endregion

    #region RANDOMIZE MODEL COLOR
    private Dictionary<Renderer, Material> _cachedMaterialDictionary;
    #endregion

    #region EXTRACT FROM OTHER GAMES' LEVEL
    private string matchName;
    #endregion


    [MenuItem("Saferio/Tools/Level Tool")]
    public static void ShowWindow()
    {
        GetWindow<HingeJointGenerator>("Level Tool");
    }

    private CountryDataContainer countryDataContainer;

    private void OnGUI()
    {
        float padding = 20f;

        Rect areaRect = new Rect(padding, padding, position.width - 2 * padding, position.height - 2 * padding);

        GUILayout.BeginArea(areaRect);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);

        // prefabPath = EditorGUILayout.TextField("Prefab Path", prefabPath);

        // target = (GameObject)EditorGUILayout.ObjectField("Target", target, typeof(GameObject), true);
        screwPrefab = (GameObject)EditorGUILayout.ObjectField("Screw Prefab", screwPrefab, typeof(GameObject), true);
        // targetMaterial = (Material)EditorGUILayout.ObjectField("Target Material", targetMaterial, typeof(Material), true);
        piecePrefab = (GameObject)EditorGUILayout.ObjectField("Piece Prefab", piecePrefab, typeof(GameObject), true);
        countryDataContainer = (CountryDataContainer)EditorGUILayout.ObjectField("countryDataContainer", countryDataContainer, typeof(CountryDataContainer), true);

        // dimensionSize = EditorGUILayout.Vector3Field("Dimension Size", dimensionSize);
        // distance = EditorGUILayout.Vector3Field("Distance", distance);

        expectedScale = EditorGUILayout.FloatField("Expected Scale", expectedScale);

        maxScrew = EditorGUILayout.IntField("Max Screw", maxScrew);
        matchName = EditorGUILayout.TextField("Match Name", matchName);

        // expectedLocalPositionOffset = EditorGUILayout.Vector3Field("Expected Local Position Offset", expectedLocalPositionOffset);
        // expectedLocalRotationOffset = EditorGUILayout.Vector3Field("Expected Local Rotation Offset", expectedLocalRotationOffset);

        // ratioSnapOffset = EditorGUILayout.FloatField("Ratio Snap Offset", ratioSnapOffset);

        // expectedOffsetRatioAutoSnapSurface = EditorGUILayout.FloatField("Expected Offset Ratio Auto Snap Surface", expectedOffsetRatioAutoSnapSurface);

        // if (GUILayout.Button("Generate"))
        // {
        //     Generate(prefabPath);
        // }

        GUIStyle centeredStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 20
        };

        GUILayout.Label("Generate", centeredStyle);

        if (GUILayout.Button("AutoAssignSprites"))
        {
            AutoAssignSprites();
        }

        if (GUILayout.Button("Generate Level"))
        {
            GenerateLevel();
        }

        if (GUILayout.Button("Generate Level Simple"))
        {
            GenerateLevelSimple();
        }

        if (GUILayout.Button("Generate Level Pattern"))
        {
            GenerateLevelPattern();
        }

        if (GUILayout.Button("Generate Level Rubik"))
        {
            GenerateLevelRubik();
        }

        if (GUILayout.Button("Generate Level Rubik Symmetrically"))
        {
            GenerateLevelRubikQuarter();
        }

        if (GUILayout.Button("Randomize Level Pieces' Color"))
        {
            RandomizeLevelModelColor();
        }

        if (GUILayout.Button("Revert Original Model Color"))
        {
            RevertOriginalModelColor();
        }

        // if (GUILayout.Button("Fine Tune Level"))
        // {
        //     FineTuneLevel();
        // }

        GUILayout.Space(16);
        GUILayout.Label("Screws", centeredStyle);

        if (GUILayout.Button("Generate And Snap Screws"))
        {
            GenerateScrews();
            SnapSurfaceScrewForAllPieceSequentially();
        }

        if (GUILayout.Button("Generate Screws"))
        {
            GenerateScrews();
        }

        if (GUILayout.Button("Count number of screws"))
        {
            CountNumberOfScrews();
        }

        GUILayout.Space(16);
        GUILayout.Label("Hinge Joint", centeredStyle);

        if (GUILayout.Button("Generate Hinge Joint All"))
        {
            GenerateHingeJointAll(Selection.activeTransform);
        }

        if (GUILayout.Button("Remove Mesh Colliders"))
        {
            RemoveAllMeshColliders();
        }

        // if (GUILayout.Button("Generate Hinge Joint All Levels"))
        // {
        //     GenerateHingeJointAllLevels();
        // }

        // if (GUILayout.Button("Check HingeJoint Number"))
        // {
        //     CheckHingeJointNumber(prefabPath);
        // }

        // if (GUILayout.Button("Reset Local Rotation"))
        // {
        //     ResetLocalRotation(target.transform);
        // }

        // if (GUILayout.Button("Flip Horizontally"))
        // {
        //     FlipHorizontally(Selection.activeTransform, prefabPath);
        // }

        // if (GUILayout.Button("Flip Vertically"))
        // {
        //     FlipVertically(Selection.activeTransform, prefabPath);
        // }

        // if (GUILayout.Button("Move"))
        // {
        //     Move(Selection.activeTransform, prefabPath);
        // }

        // if (GUILayout.Button("Auto Assign Screw Faction"))
        // {
        //     // AutoAssignScrewFaction(prefabPath);
        // }

        // if (GUILayout.Button("Spawn Screw"))
        // {
        //     SpawnScrew(prefabPath);
        // }

        // if (GUILayout.Button("Assign Material"))
        // {
        //     AssignMaterial(prefabPath);
        // }

        // if (GUILayout.Button("Center Level Pivot"))
        // {
        //     CenterLevelPivot(prefabPath);
        // }

        // if (GUILayout.Button("Remove All Grandchildren Of"))
        // {
        //     RemoveAllGrandchildrenOf();
        // }

        // if (GUILayout.Button("Get Screw Number By Phase"))
        // {
        //     GetScrewNumberByPhase(prefabPath);
        // }

        // if (GUILayout.Button("Move All Screws"))
        // {
        //     MoveAllScrews(prefabPath);
        // }

        // if (GUILayout.Button("Scale All Screws"))
        // {
        //     ScaleAllScrews(prefabPath);
        // }

        // if (GUILayout.Button("Snap Surface"))
        // {
        //     SnapSurface();
        // }

        GUILayout.Space(16);
        GUILayout.Label("Delete", centeredStyle);

        if (GUILayout.Button("Delete Screws"))
        {
            ClearScrews();
        }

        GUILayout.Space(16);
        GUILayout.Label("Snap", centeredStyle);

        if (GUILayout.Button("Snap Surface All"))
        {
            SnapSurfaceAll(Selection.activeTransform);
        }

        if (GUILayout.Button("Snap Surface All Pieces Sequentially"))
        {
            SnapSurfaceScrewForAllPieceSequentially();
        }

        // if (GUILayout.Button("Snap Rotation Surface"))
        // {
        //     SnapRotationSurface();
        // }

        // if (GUILayout.Button("Magic Snap"))
        // {
        //     MagicSnap();
        // }

        GUILayout.Space(16);
        GUILayout.Label("Extract other games' level", centeredStyle);

        // if (GUILayout.Button("ChangeMesh"))
        // {
        //     ChangeMesh();
        // }

        if (GUILayout.Button("Extract Pieces"))
        {
            ExtractPieces();
        }

        GUILayout.Space(16);
        GUILayout.Label("Util", centeredStyle);

        if (GUILayout.Button("Add Required Components To Pieces"))
        {
            AddRequiredComponentsToPieces();
        }

        if (GUILayout.Button("Set Lossy Scale All Screws"))
        {
            SetLossyScaleAllScrews();
        }

        if (GUILayout.Button("Check Level Issues"))
        {
            CheckLevelIssues();
        }

        if (GUILayout.Button("Check Level Concave Colliders"))
        {
            CheckForConcaveColliderAllLevels();
        }

        if (GUILayout.Button("Check Missing Components"))
        {
            CheckMissingComponents();
        }

        if (GUILayout.Button("Check HingeJoint Number All Levels"))
        {
            CheckHingeJointNumberAllLevels();
        }

        if (GUILayout.Button("Check Null HingeJoint's Rigidbody"))
        {
            CheckForNullHingeJointRigidbody(Selection.activeTransform);
        }

        if (GUILayout.Button("CheckAddressablesLevels"))
        {
            CheckAddressablesLevels();
        }

        if (GUILayout.Button("Fold All"))
        {
            FoldAllInPrefabMode();
        }

        // if (GUILayout.Button("Overlap"))
        // {
        //     IsOverlapping(Selection.activeTransform.position, Selection.activeGameObject);
        // }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void AutoAssignSprites()
    {
        string[] files = Directory.GetFiles("Assets/Sprites/Country Flags", "flag_*.png");

        var sortedFiles = files.OrderBy(file =>
        {
            var match = Regex.Match(Path.GetFileNameWithoutExtension(file), @"flag_(\d+)");
            return match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
        });

        int index = 0;

        foreach (string file in sortedFiles)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(file);

            countryDataContainer.Countries[index].Flag = sprite;

            index++;
        }
    }

    bool IsOverlapping(Vector3 spawnPosition, GameObject target)
    {
        Collider[] colliders = Physics.OverlapBox(spawnPosition, 0.5f * target.transform.localScale, target.transform.rotation);

        int totalCollider = 0;

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject == target)
            {

            }
            else
            {
                totalCollider++;

                Debug.Log(spawnPosition + "/" + target.name + "/" + colliders[i].name);
            }
        }

        Debug.Log(target.name + "/" + totalCollider);

        return totalCollider > 0;
    }

    private void CenterLevelPivot(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        BasicObjectPart[] objectParts = GetComponentsFromAllChildren<BasicObjectPart>(levelPrefab.transform).ToArray();

        Vector3 averagePostition = new Vector3();

        for (int i = 0; i < objectParts.Length; i++)
        {
            averagePostition += objectParts[i].transform.position;
        }

        averagePostition /= objectParts.Length;

        Selection.activeTransform.position = averagePostition;
    }

    private void ResetLocalRotation(Transform target)
    {
        target.localPosition = Vector3.zero;
        target.localRotation = Quaternion.Euler(Vector3.zero);
        target.localScale = new Vector3(1 / target.transform.parent.localScale.x, 1 / target.transform.parent.localScale.y, 1 / target.transform.parent.localScale.z);
    }

    private void FlipHorizontally(Transform target, string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        Transform flippedObject = Instantiate(target, levelPrefab.transform);

        flippedObject.name = $"{target.name.Replace("- Generated", "")} - Generated";

        Vector3 position = target.transform.position;

        position.y *= -1;

        Vector3 eulerAngle = target.transform.rotation.eulerAngles;

        eulerAngle.y *= -1;

        flippedObject.transform.position = position;
        flippedObject.transform.rotation = Quaternion.Euler(eulerAngle);

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void FlipVertically(Transform target, string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        Transform flippedObject = Instantiate(target, levelPrefab.transform);

        flippedObject.name = $"{target.name.Replace("- Generated", "")} - Generated";

        Vector3 position = target.transform.position;

        position.x *= -1;

        Vector3 eulerAngle = target.transform.rotation.eulerAngles;

        eulerAngle.x *= -1;

        flippedObject.transform.position = position;
        flippedObject.transform.rotation = Quaternion.Euler(eulerAngle);

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void Move(Transform target, string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        Transform clonedObject = Instantiate(target, levelPrefab.transform);

        clonedObject.name = $"{target.name.Replace("- Generated", "")} - Generated";

        clonedObject.transform.position += distance.x * clonedObject.transform.right;
        clonedObject.transform.position += distance.y * clonedObject.transform.up;
        clonedObject.transform.position += distance.z * clonedObject.transform.forward;

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void Generate(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        RemoveOldHingeJoints(target);

        for (int i = 0; i < target.transform.childCount; i++)
        {
            BasicScrew screw = target.transform.GetChild(i).GetComponent<BasicScrew>();

            if (screw != null)
            {
                HingeJoint joint = target.AddComponent<HingeJoint>();

                // WARNING: HingeJoint axis gizmos may not update correctly, so be sure to double-check it.

                joint.connectedBody = screw.GetComponent<Rigidbody>();
                joint.anchor = screw.transform.localPosition;

                // CRUCIAL
                joint.axis = screw.transform.localRotation * Vector3.forward;

                screw.Joint = joint;
            }
        }

        EditorUtility.SetDirty(target);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void RemoveOldHingeJoints(GameObject objectPart)
    {
        // REMOVE OLD HINGE JOINTS
        HingeJoint[] hingeJoints = objectPart.GetComponents<HingeJoint>();

        for (int I = 0; I < hingeJoints.Length; I++)
        {
            DestroyImmediate(hingeJoints[I], true);
        }
    }

    private void GenerateHingeJointAllLevels()
    {
        GameObject[] levels = LoadAllPrefabsInFolder("Assets/Prefabs/Screw Away/Levels");

        int level = 1;

        for (int i = 0; i < levels.Length; i++)
        {
            RotateObject rotateObject = levels[i].GetComponent<RotateObject>();

            if (rotateObject == null)
            {
                continue;
            }

            GenerateHingeJointAll(levels[i].transform, $"Assets/Prefabs/Screw Away/Levels/Level {level}.prefab");

            level++;
        }
    }

    private void GenerateHingeJointAll(Transform level, string path = null)
    {
        BasicObjectPart[] objectParts = GetComponentsFromAllChildren<BasicObjectPart>(level).ToArray();

        for (int i = 0; i < objectParts.Length; i++)
        {
            RemoveOldHingeJoints(objectParts[i].gameObject);

            GenerateHingeJoint(objectParts[i].gameObject);
        }

        void GenerateHingeJoint(GameObject target)
        {
            for (int i = 0; i < target.transform.childCount; i++)
            {
                BasicScrew screw = target.transform.GetChild(i).GetComponent<BasicScrew>();

                if (screw != null)
                {
                    HingeJoint joint = target.AddComponent<HingeJoint>();

                    // WARNING: HingeJoint axis gizmos may not update correctly, so be sure to double-check it.
                    joint.connectedBody = screw.GetComponent<Rigidbody>();
                    joint.anchor = screw.transform.localPosition;

                    // CRUCIAL
                    joint.axis = screw.transform.localRotation * Vector3.forward;

                    screw.Joint = joint;
                }
            }
        }

        EditorUtility.SetDirty(level);
    }

    #region CHECKING LEVEL
    private void CheckLevelIssues()
    {
        GameObject level = Selection.activeGameObject;

        BasicObjectPart[] objectParts = GetComponentsFromAllChildren<BasicObjectPart>(level.transform).ToArray();
        BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(level.transform).ToArray();

        int numScrew = screws.Length;
        Vector3 screwSize = screws[0].GetComponent<MeshRenderer>().bounds.size;



        Dictionary<string, List<string>> obstaclesOfPieces = new Dictionary<string, List<string>>();

        int totalBlockedScrew = 0;

        for (int i = 0; i < numScrew; i++)
        {
            string pieceName = screws[i].transform.parent.name;

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


            // Check screws that are too near each other
            for (int j = 0; j < screws[i].transform.parent.childCount; j++)
            {
                Transform checkScrew = screws[i].transform.parent.GetChild(j);

                if (checkScrew.GetInstanceID() == screws[i].transform.GetInstanceID())
                {
                    continue;
                }

                if (Vector3.Distance(checkScrew.position, screws[i].transform.position) < 0.3f * screwSize.x)
                {
                    Debug.Log($"<color=#Ff8E9E> invalid: {screws[i].transform.parent.name} / {screws[i].name} {checkScrew.name}</color>");

                    break;
                }
            }
        }

        foreach (var obstaclesOfPiece in obstaclesOfPieces)
        {
            foreach (var obstacle in obstaclesOfPiece.Value)
            {
                if (obstaclesOfPieces[obstacle].Contains(obstaclesOfPiece.Key))
                {
                    Debug.Log($"SAFERIO <color=#73FF83>Level {level}:<color=#fff> {obstaclesOfPiece.Key} {obstacle} <color=#FF7A75>INVALID<color=#fff>");

                    // isValid = false;

                    // break;
                }
            }
        }
    }
    private void CheckHingeJointNumber()
    {
        GameObject levelPrefab = Selection.activeGameObject;

        BasicObjectPart[] objectParts = GetComponentsFromAllChildren<BasicObjectPart>(levelPrefab.transform).ToArray();
        BaseScrew[] screw = GetComponentsFromAllChildren<BaseScrew>(levelPrefab.transform).ToArray();

        int numScrew = screw.Length;
        int numHingeJoint = 0;

        for (int i = 0; i < objectParts.Length; i++)
        {
            HingeJoint[] hingeJoints = objectParts[i].GetComponents<HingeJoint>();

            if (hingeJoints.Length == 0)
            {
                break;
            }

            if (hingeJoints[0].connectedBody == null)
            {
                break;
            }

            // Check DUPLICATE components
            BasicObjectPart[] basicObjectParts = objectParts[i].GetComponents<BasicObjectPart>();

            if (basicObjectParts.Length > 1)
            {
                break;
            }

            int numHingeJointInObjectPart = hingeJoints.Length;

            numHingeJoint += numHingeJointInObjectPart;
        }

        bool isPassed = numHingeJoint == numScrew;

        if (numScrew % 3 != 0)
        {
            isPassed = false;
        }

        string resultText = $"<color=#89FF5B>Passed</color>";

        if (!isPassed)
        {
            resultText = $"<color=#FF7878>Failed</color>";
        }

        Debug.Log($"{levelPrefab.name}: {numHingeJoint}/{numScrew} - {resultText}<color=#fff></color>");
    }

    private void CheckHingeJointNumber(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        BasicObjectPart[] objectParts = GetComponentsFromAllChildren<BasicObjectPart>(levelPrefab.transform).ToArray();
        BaseScrew[] screw = GetComponentsFromAllChildren<BaseScrew>(levelPrefab.transform).ToArray();

        int numScrew = screw.Length;
        int numHingeJoint = 0;

        for (int i = 0; i < objectParts.Length; i++)
        {
            HingeJoint[] hingeJoints = objectParts[i].GetComponents<HingeJoint>();

            if (hingeJoints.Length == 0)
            {
                break;
            }

            if (hingeJoints[0].connectedBody == null)
            {
                break;
            }

            // Check DUPLICATE components
            BasicObjectPart[] basicObjectParts = objectParts[i].GetComponents<BasicObjectPart>();

            if (basicObjectParts.Length > 1)
            {
                break;
            }

            int numHingeJointInObjectPart = hingeJoints.Length;

            numHingeJoint += numHingeJointInObjectPart;
        }

        bool isPassed = numHingeJoint == numScrew;

        if (numScrew % 3 != 0)
        {
            isPassed = false;
        }

        string resultText = $"<color=#89FF5B>Passed</color>";

        if (!isPassed)
        {
            resultText = $"<color=#FF7878>Failed</color>";
        }

        Debug.Log($"{levelPrefab.name}: {numHingeJoint}/{numScrew} - {resultText}<color=#fff></color>");
    }

    private void CheckHingeJointNumberAllLevels()
    {
        for (int i = 1; i <= 225; i++)
        {
            CheckHingeJointNumber($"Assets/Prefabs/Screw Away/Levels/Level {i}.prefab");
        }
    }

    private void CheckInvalidScrews()
    {
        Transform level = Selection.activeTransform;

        BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(level).ToArray();

        Vector3 screwSize = screws[0].GetComponent<MeshRenderer>().bounds.size;

        for (int i = 0; i < screws.Length - 1; i++)
        {
            for (int j = 0; j < screws[i].transform.parent.childCount; j++)
            {
                Transform checkScrew = screws[i].transform.parent.GetChild(j);

                if (checkScrew.GetInstanceID() == screws[i].transform.GetInstanceID())
                {
                    continue;
                }

                if (Vector3.Distance(checkScrew.position, screws[i].transform.position) < 0.5f * screwSize.x)
                {
                    Debug.Log($"<color=#Ff8E9E> invalid: {screws[i].transform.parent.name} / {screws[i].name}</color>");

                    break;
                }
            }
        }
    }

    private void CheckAddressablesLevels()
    {
        int total = 0;

        for (int i = 1; i <= 175; i++)
        {
            int level = i;

            string levelPrefab = $"Level {level}";

            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(levelPrefab);

            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    total++;

                    Debug.Log($"<color=#32CD32>SAFERIO Level {level} Passed</color> - Total: {total}");
                }
            };
        }
    }

    private void CheckForConcaveColliderAllLevels()
    {
        GameObject[] levels = LoadAllPrefabsInFolder("Assets/Prefabs/Screw Away/Levels");

        int totalConcavePieces = 0;

        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i].name.Contains("Level"))
            {
                totalConcavePieces += CheckForConcaveCollider(levels[i].transform);
            }
        }

        Debug.Log("SAFERIO " + totalConcavePieces);
    }

    private int CheckForNullHingeJointRigidbody(Transform level)
    {
        HingeJoint[] hingeJoints = GetComponentsFromAllChildren<HingeJoint>(level).ToArray();

        int totalConcavePieces = 0;

        for (int i = 0; i < hingeJoints.Length; i++)
        {
            if (hingeJoints[i].connectedBody == null)
            {
                Debug.Log("<color=#f99>SAFERIO LEVEL " + level.name + "/" + hingeJoints[i].transform.parent.name + "</color>");

                totalConcavePieces++;
            }
        }

        return totalConcavePieces;
    }

    private int CheckForConcaveCollider(Transform level)
    {
        BasicObjectPart[] pieces = GetComponentsFromAllChildren<BasicObjectPart>(level).ToArray();

        int totalConcavePieces = 0;

        for (int i = 0; i < pieces.Length; i++)
        {
            if (!pieces[i].GetComponent<MeshCollider>().convex)
            {
                Debug.Log("<color=#f99>SAFERIO LEVEL " + level.name + "/" + pieces[i].name + "</color>");

                totalConcavePieces++;
            }
        }

        return totalConcavePieces;
    }

    private void CheckMissingComponents()
    {
        Transform selected = Selection.activeTransform;

        GameObject go = selected.gameObject;
        Component[] components = go.GetComponents<Component>();

        Debug.Log("SAFERIO " + components.Length);

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                Debug.Log($"SAFERIO Missing Component found on GameObject: {go.name}", go);
            }
        }
    }
    #endregion

    private void AutoAssignScrewFaction(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(levelPrefab.transform).ToArray();

        int currentFaction = 0;

        List<GameFaction> remainingFactionForScrews = new List<GameFaction>();

        for (int i = 0; i < screws.Length; i++)
        {
            remainingFactionForScrews.Add(GameConstants.SCREW_FACTION[currentFaction]);

            if (i > 0 && (i + 1) % 3 == 0)
            {
                currentFaction++;

                if (currentFaction >= GameConstants.SCREW_FACTION.Length)
                {
                    currentFaction = 0;
                }
            }
        }

        for (int i = 0; i < screws.Length; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, remainingFactionForScrews.Count);

            screws[i].ScrewId = i;
            screws[i].Faction = remainingFactionForScrews[randomIndex];

            remainingFactionForScrews.RemoveAt(randomIndex);
        }

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void SpawnScrew(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        Vector3 position = new Vector3();

        int currentX = 0;
        int currentY = 0;
        int currentZ = 0;

        dimensionSize.x = Mathf.Max(dimensionSize.x, 1);
        dimensionSize.y = Mathf.Max(dimensionSize.y, 1);
        dimensionSize.z = Mathf.Max(dimensionSize.z, 1);

        Transform target = Selection.activeTransform;

        for (int i = 0; i < dimensionSize.x * dimensionSize.y * dimensionSize.z; i++)
        {
            GameObject screw = (GameObject)PrefabUtility.InstantiatePrefab(screwPrefab, target.transform);

            position.x = (-(dimensionSize.x - 1) / 2f + currentX) * distance.x;
            position.y = (-(dimensionSize.y - 1) / 2f + currentY) * distance.y;
            position.z = (-(dimensionSize.z - 1) / 2f + currentZ) * distance.z;

            currentX++;

            if (currentX >= dimensionSize.x)
            {
                currentY++;

                if (currentY >= dimensionSize.y)
                {
                    currentZ++;

                    currentY = 0;
                }

                currentX = 0;
            }

            // for (int j = 0; j < dimensionSize.x; j++)
            // {
            //     position.x = (-(dimensionSize.x - 1) / 2f + currentX) * distance.x + target.transform.position.x;
            // }

            // for (int j = 0; j < dimensionSize.y; j++)
            // {
            //     position.y = (-(dimensionSize.y - 1) / 2f + currentY) * distance.y + target.transform.position.y;
            // }

            // for (int j = 0; j < dimensionSize.z; j++)
            // {
            //     position.z = (-(dimensionSize.z - 1) / 2f + currentZ) * distance.z + target.transform.position.z;
            // }

            screw.transform.localPosition = position + expectedLocalPositionOffset;
            screw.transform.localEulerAngles += expectedLocalRotationOffset;
            screw.transform.localScale = TransformUtil.ComponentWiseDivine(expectedScale * Vector3.one, target.transform.localScale);
        }




        // for (int i = 0; i < dimensionSize.x; i++)
        // {
        //     for (int j = 0; j < dimensionSize.y; j++)
        //     {
        //         GameObject screw = (GameObject)PrefabUtility.InstantiatePrefab(screwPrefab, target.transform);

        //         Vector3 position;

        //         position.x = (-(dimensionSize.x - 1) / 2f + j) * distance.x;
        //         position.y = (-(dimensionSize.y - 1) / 2f + i) * distance.y;
        //         position.z = (-(dimensionSize.y - 1) / 2f + i) * distance.z;

        //         position += target.transform.position;

        //         screw.transform.position = position;
        //         screw.transform.localScale = TransformUtil.ComponentWiseDivine(expectedScale * Vector3.one, target.transform.localScale);
        //     }
        // }

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void AssignMaterial(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        BasicObjectPart[] objectParts = GetComponentsFromAllChildren<BasicObjectPart>(levelPrefab.transform).ToArray();

        for (int i = 0; i < objectParts.Length; i++)
        {
            MeshRenderer meshRenderer = objectParts[i].GetComponent<MeshRenderer>();

            Debug.Log(meshRenderer);

            List<Material> materials = meshRenderer.materials.ToList();

            materials[0] = targetMaterial;

            meshRenderer.SetMaterials(materials);
        }

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    #region MOVE
    private void MoveAllScrews(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(levelPrefab.transform).ToArray();

        for (int i = 0; i < screws.Length; i++)
        {
            if (!screws[i].gameObject.activeSelf)
            {
                continue;
            }

            screws[i].transform.position -= ratioSnapOffset * screws[i].transform.lossyScale.x * screws[i].transform.forward;
        }

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void ScaleAllScrews(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(levelPrefab.transform).ToArray();

        if (expectedScale <= 0)
        {
            expectedScale = 1;
        }

        for (int i = 0; i < screws.Length; i++)
        {
            screws[i].transform.localScale *= expectedScale;
        }

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void SetLossyScaleAllScrews()
    {
        GameObject level = Selection.activeGameObject;

        BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(level.transform).ToArray();

        if (expectedScale <= 0)
        {
            expectedScale = 1;
        }

        for (int i = 0; i < screws.Length; i++)
        {
            screws[i].transform.localScale = TransformUtil.ComponentWiseDivine(expectedScale * screws[i].transform.localScale, screws[i].transform.lossyScale);
        }

        EditorUtility.SetDirty(level);

        // PrefabUtility.SaveAsPrefabAsset(level, path);
    }
    #endregion

    #region REMOVE
    private void RemoveAllGrandchildrenOf()
    {
        List<GameObject> toRemoveList = new List<GameObject>();

        for (int i = 0; i < Selection.activeTransform.childCount; i++)
        {
            for (int j = 0; j < Selection.activeTransform.GetChild(i).childCount; j++)
            {
                toRemoveList.Add(Selection.activeTransform.GetChild(i).GetChild(j).gameObject);
            }
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item);
        }

        EditorUtility.SetDirty(Selection.activeTransform);
    }

    private void RemoveAllMeshColliders()
    {
        Transform[] selections = Selection.transforms;

        for (int i = 0; i < selections.Length; i++)
        {
            RemoveAllMeshColliders(selections[i]);
        }
    }

    private void RemoveAllMeshColliders(Transform selection)
    {
        MeshCollider[] meshColliders = selection.GetComponents<MeshCollider>();

        // Remove all existing blocks
        List<MeshCollider> toRemoveList = new List<MeshCollider>();

        for (int i = 0; i < meshColliders.Length; i++)
        {
            toRemoveList.Add(meshColliders[i]);
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item);
        }
    }
    #endregion

    #region INFO
    private void GetScrewNumberByPhase(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        MultiPhaseScrew[] screws = GetComponentsFromAllChildren<MultiPhaseScrew>(levelPrefab.transform).ToArray();

        Dictionary<int, int> numberScrewByPhase = new Dictionary<int, int>();

        for (int i = 0; i < screws.Length; i++)
        {
            if (numberScrewByPhase.ContainsKey(screws[i].Phase))
            {
                numberScrewByPhase[screws[i].Phase]++;
            }
            else
            {
                numberScrewByPhase.Add(screws[i].Phase, 1);
            }
        }

        foreach (var phase in numberScrewByPhase.Keys)
        {
            Debug.Log($"Phase {phase}: {numberScrewByPhase[phase]}");
        }
    }

    private void CountNumberOfScrews()
    {
        BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(Selection.activeTransform).ToArray();

        int screwNumber = screws.Length;

        string output = "";

        output += $"Number of screws: {screws.Length}";

        int screwNumberToAdd = 3 - (screwNumber % 3);
        int screwNumberToRemove = screwNumber % 3;

        string[] units = new string[2];

        units[0] = screwNumberToAdd > 1 ? "screws" : "screw";
        units[1] = screwNumberToRemove > 1 ? "screws" : "screw";

        if (screwNumber % 3 == 0)
        {
            output += $" -<color=#89FF5B> Valid";
        }
        else
        {
            output += $" -<color=#FF7878> Invalid - ";
            output += $"<color=#fff>Need to add <color=#89FF5B>{screwNumberToAdd} {units[0]} <color=#fff>or remove <color=#89FF5B>{screwNumberToRemove} {units[1]}";
        }

        Debug.Log(output);
    }
    #endregion

    #region SNAP
    private void SnapSurface(Transform target)
    {
        Transform selectedObject = target;

        Vector3 screwSize = selectedObject.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        screwSize.z *= 0.779f;

        Vector3 origin = selectedObject.position + 0.8f * selectedObject.lossyScale.z * screwSize.z * selectedObject.forward;

        int maxRaycastDistance = 100;

        RaycastHit hit;

        if (selectedObject.gameObject.scene.GetPhysicsScene().Raycast(origin, -selectedObject.forward, out hit, maxRaycastDistance))
        {
            IObjectPart objectPart = hit.collider.GetComponent<IObjectPart>();

            if (objectPart != null)
            {
                Snap();
            }
            else
            {
                SnapByDirection(selectedObject.forward);
                // RaycastAgain();
            }
        }
        else
        {
            SnapByDirection(selectedObject.forward);
            // RaycastAgain();
        }

        void SnapByDirection(Vector3 raycastDirection)
        {
            if (selectedObject.gameObject.scene.GetPhysicsScene().Raycast(origin, raycastDirection, out hit, maxRaycastDistance))
            {
                IObjectPart objectPart = hit.collider.GetComponent<IObjectPart>();

                if (objectPart != null)
                {
                    Snap();
                }
            }
        }

        void RaycastAgain()
        {
            Vector3 direction = -target.transform.position + target.parent.position;

            target.rotation = Quaternion.LookRotation(-direction);

            if (selectedObject.gameObject.scene.GetPhysicsScene().Raycast(origin, -selectedObject.forward, out hit, maxRaycastDistance))
            {
                IObjectPart recastObjectPart = hit.collider.GetComponent<IObjectPart>();

                if (recastObjectPart != null)
                {
                    Snap();
                }
            }
        }

        void Snap()
        {
            selectedObject.position = hit.point - expectedOffsetRatioAutoSnapSurface * selectedObject.lossyScale.z * screwSize.z * selectedObject.forward;

            Quaternion surfaceRotation = Quaternion.FromToRotation(selectedObject.forward, hit.normal) * selectedObject.rotation;

            selectedObject.rotation = surfaceRotation;
            selectedObject.localEulerAngles = selectedObject.localEulerAngles.ChangeZ(0);
        }
    }

    private void SnapSurface()
    {
        SnapSurface(Selection.activeTransform);
    }

    private void SnapSurfaceAll(Transform target)
    {
        BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(target).ToArray();

        foreach (var screw in screws)
        {
            SnapSurface(screw.transform);
        }

        EditorUtility.SetDirty(Selection.activeTransform);
    }

    private void SnapSurfaceScrewForAllPieceSequentially()
    {
        BasicObjectPart[] pieces = GetComponentsFromAllChildren<BasicObjectPart>(Selection.activeTransform).ToArray();

        foreach (var piece in pieces)
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                pieces[i].gameObject.SetActive(false);
            }

            piece.gameObject.SetActive(true);

            SnapSurfaceAll(piece.transform);
        }

        foreach (var piece in pieces)
        {
            piece.gameObject.SetActive(true);
        }

        EditorUtility.SetDirty(Selection.activeTransform);
    }

    private void SnapParentSurface()
    {
        Transform selectedObject = Selection.activeTransform;

        Vector3 screwSize = selectedObject.GetComponent<MeshFilter>().sharedMesh.bounds.size;

        Vector3 origin = selectedObject.position + 1f * selectedObject.lossyScale.z * screwSize.z * selectedObject.forward;
        Vector3 direction = selectedObject.parent.position - selectedObject.position;

        RaycastHit hit;

        if (selectedObject.gameObject.scene.GetPhysicsScene().Raycast(origin, direction, out hit, 10))
        {
            IObjectPart objectPart = hit.collider.GetComponent<IObjectPart>();

            if (objectPart != null)
            {
                selectedObject.position = hit.point - 0.3f * selectedObject.lossyScale.z * screwSize.z * selectedObject.forward;

                Quaternion surfaceRotation = Quaternion.FromToRotation(selectedObject.forward, hit.normal) * selectedObject.rotation;

                selectedObject.rotation = surfaceRotation;
            }
        }
    }

    private void SnapRotationSurface()
    {
        Transform selectedObject = Selection.activeTransform;

        RaycastHit hit;

        Vector3 direction = selectedObject.parent.position - selectedObject.position;

        Vector3 screwSize = selectedObject.GetComponent<MeshFilter>().sharedMesh.bounds.size;

        Vector3 origin = selectedObject.position + 0.8f * selectedObject.lossyScale.z * screwSize.z * selectedObject.forward;

        if (selectedObject.gameObject.scene.GetPhysicsScene().Raycast(origin, direction, out hit, 10))
        {
            IObjectPart objectPart = hit.collider.GetComponent<IObjectPart>();

            if (objectPart != null)
            {
                Quaternion surfaceRotation = Quaternion.FromToRotation(selectedObject.forward, hit.normal) * selectedObject.rotation;

                selectedObject.rotation = surfaceRotation;
            }
        }
    }

    private void MagicSnap()
    {
        SnapRotationSurface();
        SnapSurface();
    }
    #endregion

    #region UTIL
    private void AddRequiredComponentsToPieces()
    {
        Transform level = Selection.activeTransform;

        Transform[] pieces = GetComponentsFromAllChildren<Transform>(level).ToArray();

        for (int i = 0; i < pieces.Length; i++)
        {
            // Level Container
            if (pieces[i] == level)
            {
                if (pieces[i].GetComponent<RotateObject>() == null)
                {
                    pieces[i].AddComponent<RotateObject>();
                }

                continue;
            }

            // Pieces
            pieces[i].AddComponent<BasicObjectPart>();
            pieces[i].AddComponent<ObjectPartMaterialPropertyBlock>();
            pieces[i].AddComponent<Rigidbody>();

            if (pieces[i].GetComponent<MeshCollider>() == null)
            {
                MeshCollider meshCollider = pieces[i].AddComponent<MeshCollider>();

                meshCollider.convex = true;
            }
        }

        EditorUtility.SetDirty(level);
    }

    private void FoldAllInPrefabMode()
    {
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            FoldRecursively(prefabStage.prefabContentsRoot.transform);
        }
        else
        {
            Debug.LogWarning("Not in Prefab Mode!");
        }
    }

    private static void FoldRecursively(Transform parentTransform)
    {
        var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        var window = GetWindow(type);
        var exprec = type.GetMethod("SetExpandedRecursive");
        exprec.Invoke(window, new object[] { Selection.activeGameObject.GetInstanceID(), false });

        foreach (Transform child in parentTransform)
        {
            FoldRecursively(child);
        }
    }

    private static EditorWindow GetHierarchyWindow()
    {
        var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
        foreach (var window in windows)
        {
            if (window.titleContent.text == "Hierarchy")
            {
                return window;
            }
        }
        return null;
    }

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

    private int RandomWithoutZero(int maxValue)
    {
        return UnityEngine.Random.Range(1, maxValue) * (UnityEngine.Random.Range(0, 2) * 2 - 1);
    }

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



    #region GENERATE LEVEL
    private void FineTuneLevel()
    {
        BasicObjectPart[] pieces = GetComponentsFromAllChildren<BasicObjectPart>(Selection.activeTransform).ToArray();

        int maxTryTime = 36;

        for (int i = 0; i < pieces.Length; i++)
        {
            for (int j = 0; j < maxTryTime; j++)
            {
                Vector3 position = Flee(pieces[i].transform, j + 1);

                pieces[i].transform.position = position;

                if (!IsNear(pieces[i].transform))
                {
                    break;
                }
            }
        }

        Vector3 Flee(Transform target, float fleeStrength)
        {
            Vector3 fleeDirection = Vector3.zero;

            Vector3 halfSize = 0.5f * target.transform.localScale.x * target.GetComponent<Collider>().bounds.size;
            float safeDistance = halfSize.x;

            // Iterate through all the objects in the group
            foreach (BasicObjectPart obj in pieces)
            {
                // Calculate the direction vector from the current object to the fleeing object
                Vector3 directionToObject = target.position - obj.transform.position;

                fleeDirection += directionToObject.normalized;

                // // If the object is within the safe distance, flee from it
                // if (directionToObject.magnitude < safeDistance)
                // {
                //     // Accumulate the flee direction, adding more force the closer we are
                //     fleeDirection += directionToObject.normalized;
                // }
            }

            Debug.Log(fleeDirection * fleeStrength);

            // If there is any flee direction, apply the force to the object
            if (fleeDirection != Vector3.zero)
            {
                // Normalize the flee direction to get a consistent speed, and scale by flee strength
                fleeDirection.Normalize();
                return target.position + fleeDirection * fleeStrength;
            }



            return target.position + fleeDirection * fleeStrength;
        }

        bool IsNear(Transform target)
        {
            Vector3 halfSize = 0.5f * target.transform.localScale.x * target.GetComponent<Collider>().bounds.size;

            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i].transform != target)
                {
                    if (Mathf.Abs(pieces[i].transform.position.x - target.position.x) < halfSize.x &&
                        Mathf.Abs(pieces[i].transform.position.y - target.position.y) < halfSize.y &&
                        Mathf.Abs(pieces[i].transform.position.z - target.position.z) < halfSize.z
                    )
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    bool IsNear(Transform target, Transform[] pieces)
    {
        Vector3 halfSize = 0.6f * target.transform.localScale.x * target.GetComponent<Collider>().bounds.size;

        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null)
            {
                continue;
            }

            if (pieces[i].transform != target)
            {
                if (Mathf.Abs(pieces[i].transform.position.x - target.position.x) < halfSize.x &&
                    Mathf.Abs(pieces[i].transform.position.y - target.position.y) < halfSize.y &&
                    Mathf.Abs(pieces[i].transform.position.z - target.position.z) < halfSize.z
                )
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void GenerateLevelSimple()
    {
        int totalPiece = UnityEngine.Random.Range(20, 28);

        totalPiece += totalPiece % 2;

        int halfTotalPiece = totalPiece / 2;

        Transform[] generatedPieces = new Transform[totalPiece];

        // Remove all existing blocks
        List<GameObject> toRemoveList = new List<GameObject>();

        for (int i = 0; i < Selection.activeTransform.childCount; i++)
        {
            toRemoveList.Add(Selection.activeTransform.GetChild(i).gameObject);
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item);
        }

        // MATERIALS
        Material[] materialPool = LoadAllMaterialsInFolder("Assets/Materials/Wood/Single Color");

        int tryTime = 0;
        int maxTryTime = 36;
        bool isFound;

        for (int i = 0; i < halfTotalPiece; i++)
        {
            tryTime = 0;
            isFound = false;

            Transform generatedPiece = ((GameObject)PrefabUtility.InstantiatePrefab(piecePrefab, Selection.activeTransform)).transform;

            generatedPiece.name = $"Piece {i + 1}";

            // float positionX = (i / 4f) * UnityEngine.Random.Range(1.5f, 4);

            // Vector3 position = new Vector3(positionX, UnityEngine.Random.Range(-8, 8), UnityEngine.Random.Range(-8, 8));

            // generatedPiece.position = position;

            // if (IsNear(generatedPiece, generatedPieces))
            // {
            //     positionX = 1.8f * i;
            // }

            // for (int j = 0; j < maxTryTime; j++)
            // {
            //     positionX = (j / 12 + 1) * UnityEngine.Random.Range(1.5f, 4);

            //     float multiplier = j / 12 + 1;

            //     position = new Vector3(positionX, multiplier * UnityEngine.Random.Range(-4, 4), multiplier * UnityEngine.Random.Range(-4, 4));

            //     generatedPiece.position = position;

            //     if (!IsNear(generatedPiece, generatedPieces))
            //     {
            //         isFound = true;

            //         break;
            //     }
            // }

            // if (!isFound)
            // {
            //     position = position.ChangeX(1.8f * i);
            // }

            Vector3 position = new Vector3(1.8f * UnityEngine.Random.Range(1, 8), UnityEngine.Random.Range(-12, 12), UnityEngine.Random.Range(-12, 12));
            // Vector3 rotation = 30 * new Vector3(0, 3, UnityEngine.Random.Range(-3, 3));
            Vector3 rotation = new Vector3(0, 180, 90);

            generatedPiece.position = position;

            // MATERIAL
            generatedPiece.GetComponent<MeshRenderer>().sharedMaterial = materialPool[UnityEngine.Random.Range(0, materialPool.Length)];

            generatedPiece.rotation = Quaternion.Euler(rotation);
            generatedPiece.position = position;

            generatedPieces[i] = generatedPiece;
        }

        for (int i = halfTotalPiece; i < totalPiece; i++)
        {
            Transform generatedPiece = ((GameObject)PrefabUtility.InstantiatePrefab(piecePrefab, Selection.activeTransform)).transform;

            generatedPiece.name = $"Piece {i + 1}";

            // MATERIAL
            generatedPiece.GetComponent<MeshRenderer>().sharedMaterial = generatedPieces[i - halfTotalPiece].GetComponent<MeshRenderer>().sharedMaterial;

            // POSITION
            Vector3 position = generatedPieces[i - halfTotalPiece].position;

            position.x *= -1;

            generatedPiece.position = position;

            // ROTATION
            Vector3 rotation = generatedPieces[i - halfTotalPiece].rotation.eulerAngles;

            rotation.x += 180;
            rotation.z += 180;

            generatedPiece.rotation = Quaternion.Euler(rotation);
        }

        EditorUtility.SetDirty(Selection.activeTransform);
    }

    private void GenerateLevelPattern()
    {
        int totalPiece = UnityEngine.Random.Range(24, 32);

        totalPiece += totalPiece % 2;

        int halfTotalPiece = totalPiece / 2;

        Transform[] generatedPieces = new Transform[totalPiece];

        // Remove all existing blocks
        List<GameObject> toRemoveList = new List<GameObject>();

        for (int i = 0; i < Selection.activeTransform.childCount; i++)
        {
            toRemoveList.Add(Selection.activeTransform.GetChild(i).gameObject);
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item);
        }

        // MATERIALS
        Material[] materialPool = LoadAllMaterialsInFolder("Assets/Materials/Wood/Single Color");

        Vector3 pieceSize = piecePrefab.GetComponent<MeshFilter>().sharedMesh.bounds.size * piecePrefab.transform.localScale.x;
        Vector3 currentPieceSize;

        int halfTotalSpawn = 0;
        int maxPiecePerXLayer = 4;
        int pieceInCurrentXLayer;

        List<Vector3> firstMatrix = new List<Vector3>();

        List<Transform> possibleSinglePieces = new List<Transform>();
        List<Vector3> coordinators = new List<Vector3>();

        // X
        for (int i = 2; i < 6; i++)
        {
            pieceInCurrentXLayer = 0;

            Transform possibleSinglePiece = null;

            Vector3 coordinator = new Vector3();

            // Y
            for (int j = -1; j < 1; j++)
            {
                // Z
                for (int k = -2; k < 2; k++)
                {
                    if (halfTotalSpawn >= halfTotalPiece || pieceInCurrentXLayer > maxPiecePerXLayer)
                    {
                        break;
                    }

                    int random = UnityEngine.Random.Range(0, 160 / (8 * (i + 1)));

                    if (i == 2)
                    {
                        random = UnityEngine.Random.Range(0, 1);
                    }

                    if (random == 0)
                    {
                        Transform generatedPiece = ((GameObject)PrefabUtility.InstantiatePrefab(piecePrefab, Selection.activeTransform)).transform;

                        generatedPiece.name = $"Piece {halfTotalSpawn + 1}";

                        Vector3 rotation = new Vector3(0, 180, 90);
                        generatedPiece.rotation = Quaternion.Euler(rotation);

                        currentPieceSize = generatedPiece.GetComponent<MeshRenderer>().bounds.size;

                        Vector3 position = new Vector3(1.6f * i * currentPieceSize.x, 1 * j * currentPieceSize.y, 1 * k * currentPieceSize.z);

                        // position += 0.1f * new Vector3(0, currentPieceSize.y * UnityEngine.Random.Range(-3, 3), currentPieceSize.z * UnityEngine.Random.Range(-3, 3));
                        // Vector3 rotation = 30 * new Vector3(0, 3, UnityEngine.Random.Range(-3, 3));

                        generatedPiece.position = position;

                        // MATERIAL
                        generatedPiece.GetComponent<MeshRenderer>().sharedMaterial = materialPool[UnityEngine.Random.Range(0, materialPool.Length)];


                        generatedPiece.position = position;

                        generatedPieces[halfTotalSpawn] = generatedPiece;

                        halfTotalSpawn++;
                        pieceInCurrentXLayer++;

                        coordinator = new Vector3(i, j, k);

                        firstMatrix.Add(coordinator);

                        possibleSinglePiece = generatedPiece;

                        possibleSinglePieces.Add(generatedPiece);
                        coordinators.Add(coordinator);
                    }
                }
            }
        }

        // RANDOMIZE
        for (int k = 0; k < possibleSinglePieces.Count; k++)
        {
            Debug.Log(possibleSinglePieces[k].name + "/" + firstMatrix.Count(item => item.x == coordinators[k].x && item.z == coordinators[k].z));

            if (firstMatrix.Count(item => item.x == coordinators[k].x && item.z == coordinators[k].z) == 1)
            {
                currentPieceSize = possibleSinglePieces[k].GetComponent<MeshRenderer>().bounds.size;

                possibleSinglePieces[k].transform.position += new Vector3(0, 0.5f * currentPieceSize.y * RandomWithoutZero(2), 0);
            }

            if (firstMatrix.Count(item => item.x == coordinators[k].x && item.y == coordinators[k].y) == 1)
            {
                currentPieceSize = possibleSinglePieces[k].GetComponent<MeshRenderer>().bounds.size;

                possibleSinglePieces[k].transform.position += new Vector3(0, 0, 0.5f * currentPieceSize.z * RandomWithoutZero(2));
            }
        }

        // Flip
        for (int i = halfTotalSpawn; i < 2 * halfTotalSpawn; i++)
        {
            Transform generatedPiece = ((GameObject)PrefabUtility.InstantiatePrefab(piecePrefab, Selection.activeTransform)).transform;

            generatedPiece.name = $"Piece {i + 1}";

            // MATERIAL
            generatedPiece.GetComponent<MeshRenderer>().sharedMaterial = generatedPieces[i - halfTotalSpawn].GetComponent<MeshRenderer>().sharedMaterial;

            // POSITION
            Vector3 position = generatedPieces[i - halfTotalSpawn].position;

            position.x *= -1;

            generatedPiece.position = position;

            // ROTATION
            Vector3 rotation = generatedPieces[i - halfTotalSpawn].rotation.eulerAngles;

            rotation.x += 180;
            rotation.z += 180;

            generatedPiece.rotation = Quaternion.Euler(rotation);
        }

        maxPiecePerXLayer = 2;

        List<Vector2> matrix = new List<Vector2>();

        // Center
        for (int i = -6; i < 4; i++)
        {
            pieceInCurrentXLayer = 0;

            Transform possibleSinglePiece = null;

            for (int j = -1; j < 2; j++)
            {
                if (pieceInCurrentXLayer >= maxPiecePerXLayer)
                {
                    break;
                }

                int random = UnityEngine.Random.Range(0, 3);

                if (random == 0)
                {
                    Transform generatedPiece = ((GameObject)PrefabUtility.InstantiatePrefab(piecePrefab, Selection.activeTransform)).transform;

                    generatedPiece.name = $"Piece {halfTotalSpawn + 1}";

                    Vector3 rotation = new Vector3(0, -90, 90);
                    generatedPiece.rotation = Quaternion.Euler(rotation);

                    currentPieceSize = generatedPiece.GetComponent<MeshRenderer>().bounds.size;

                    Vector3 position = new Vector3(0, 1f * j * currentPieceSize.y, 1.6f * i * currentPieceSize.z);

                    generatedPiece.position = position;

                    // MATERIAL
                    generatedPiece.GetComponent<MeshRenderer>().sharedMaterial = materialPool[UnityEngine.Random.Range(0, materialPool.Length)];

                    generatedPiece.position = position;

                    pieceInCurrentXLayer++;

                    possibleSinglePiece = generatedPiece;

                    matrix.Add(new Vector2(i, j));
                }
            }

            if (possibleSinglePiece == null)
            {
                continue;
            }

            if (matrix.Count(item => item.x == i) == 1)
            {
                currentPieceSize = possibleSinglePiece.GetComponent<MeshRenderer>().bounds.size;

                possibleSinglePiece.transform.position += new Vector3(0, 0.5f * currentPieceSize.y * UnityEngine.Random.Range(-2, 2), 0);
            }
        }

        EditorUtility.SetDirty(Selection.activeTransform);
    }

    private void GenerateLevelRubik()
    {
        piecePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Screw Away/Levels/Common/Box - Rubik-like.prefab");

        // Remove all existing blocks
        List<GameObject> toRemoveList = new List<GameObject>();

        for (int i = 0; i < Selection.activeTransform.childCount; i++)
        {
            toRemoveList.Add(Selection.activeTransform.GetChild(i).gameObject);
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item);
        }

        // MATERIALS
        Material[] materialPool = LoadAllMaterialsInFolder("Assets/Materials/Wood/Single Color");

        Vector3 pieceSize = piecePrefab.GetComponent<MeshFilter>().sharedMesh.bounds.size * piecePrefab.transform.localScale.x;
        Vector3 currentPieceSize;

        int totalSpawn = 0;

        float distanceMultiplier = 1.05f;

        // HINGE
        GameObject[] piecePrefabs = LoadAllPrefabsInFolder("Assets/Prefabs/Screw Away/Levels/Common");

        List<GameObject> hinges = new List<GameObject>();

        Dictionary<Vector3Int, Transform> tilesWithPiece = new Dictionary<Vector3Int, Transform>();
        Dictionary<Vector3Int, bool> isTilesForHingeFull = new Dictionary<Vector3Int, bool>();

        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (piecePrefabs[i].name.Contains("Hinge"))
            {
                hinges.Add(piecePrefabs[i]);
            }
        }

        int numRow = 2;

        int numScrewSpawned = 0;

        // X
        for (int i = -numRow; i <= numRow; i++)
        {
            Material randomMaterial = materialPool[UnityEngine.Random.Range(0, materialPool.Length)];
            // Y
            for (int j = -numRow; j <= numRow; j++)
            {
                // Z
                for (int k = -numRow; k <= numRow; k++)
                {
                    if (numScrewSpawned >= maxScrew)
                    {
                        tilesWithPiece.Add(new Vector3Int(i, j, k), null);

                        continue;
                    }

                    int random = UnityEngine.Random.Range(0, 2);

                    if (random == 0)
                    {
                        Transform generatedPiece = ((GameObject)PrefabUtility.InstantiatePrefab(piecePrefab, Selection.activeTransform)).transform;

                        generatedPiece.name = $"Piece {totalSpawn + 1}";

                        numScrewSpawned += generatedPiece.transform.childCount;

                        Vector3 rotation = new Vector3(0, 0, 0);
                        generatedPiece.rotation = Quaternion.Euler(rotation);

                        currentPieceSize = generatedPiece.GetComponent<MeshRenderer>().bounds.size;

                        Vector3 position = new Vector3(
                            distanceMultiplier * i * currentPieceSize.x, distanceMultiplier * j * currentPieceSize.y, distanceMultiplier * k * currentPieceSize.z);

                        generatedPiece.position = position;

                        // MATERIAL
                        // generatedPiece.GetComponent<MeshRenderer>().sharedMaterial = materialPool[UnityEngine.Random.Range(0, materialPool.Length)];
                        generatedPiece.GetComponent<MeshRenderer>().sharedMaterial = randomMaterial;

                        totalSpawn++;

                        tilesWithPiece.Add(new Vector3Int(i, j, k), generatedPiece);
                    }
                    else
                    {
                        tilesWithPiece.Add(new Vector3Int(i, j, k), null);
                    }

                    isTilesForHingeFull.Add(new Vector3Int(i, j, k), false);
                }
            }
        }

        // for (int i = -numRow; i <= numRow; i++)
        // {
        //     // Y
        //     for (int j = -numRow; j <= numRow; j++)
        //     {
        //         // Z
        //         for (int k = -numRow; k <= numRow; k++)
        //         {
        //             Debug.Log(new Vector3(i, j, k) + "/" + tilesWithPiece[new Vector3Int(i, j, k)]);
        //         }
        //     }
        // }

        // GENERATE HINGE
        for (int i = -numRow; i <= numRow; i++)
        {
            for (int j = -numRow; j <= numRow; j++)
            {
                for (int k = -numRow; k <= numRow; k++)
                {
                    if (tilesWithPiece[new Vector3Int(i, j, k)] != null)
                    {
                        int random = UnityEngine.Random.Range(0, 1);

                        BlockHinge randomHinge = hinges[UnityEngine.Random.Range(0, hinges.Count)].GetComponent<BlockHinge>();

                        if (randomHinge.name == "Hinge L-Shaped 2")
                        {
                            continue;
                        }

                        if (random == 0)
                        {
                            Transform piece = tilesWithPiece[new Vector3Int(i, j, k)];

                            int x = i;
                            int y = j;
                            int z = k;

                            if (IsValidForHinge(randomHinge, x, y, z))
                            {
                                Transform hinge =
                                ((GameObject)PrefabUtility.InstantiatePrefab(randomHinge.gameObject, Selection.activeTransform)).transform;

                                Vector3 size = piece.GetComponent<MeshRenderer>().bounds.size;

                                if (hinge.name == "Hinge L-Shaped")
                                {
                                    hinge.transform.position = piece.transform.position + 0.57f * new Vector3(-size.x, size.y, 0);
                                }
                                else
                                {
                                    hinge.transform.position = piece.transform.position + 0.57f * new Vector3(-size.x, 0, 0);
                                }
                            }
                        }
                    }
                }
            }
        }

        bool IsValidForHinge(BlockHinge hinge, int x, int y, int z)
        {
            if (tilesWithPiece.ContainsKey(new Vector3Int(x - 1, y, z)))
            {
                if (tilesWithPiece[new Vector3Int(x - 1, y, z)] != null)
                {
                    return false;
                }
            }

            for (int i = z - (hinge.size - 1) / 2; i <= z + (hinge.size - 1) / 2; i++)
            {
                if (i < 0 || i > numRow)
                {
                    return false;
                }

                if (tilesWithPiece[new Vector3Int(x, y, i)] == null || isTilesForHingeFull[new Vector3Int(x, y, i)])
                {
                    return false;
                }
            }

            // set tile full
            for (int i = z - (hinge.size - 1) / 2; i <= z + (hinge.size - 1) / 2; i++)
            {
                if (i < 0 || i > numRow)
                {
                    continue;
                }

                isTilesForHingeFull[new Vector3Int(x, y, i)] = true;
            }

            return true;
        }

        EditorUtility.SetDirty(Selection.activeTransform);
    }

    private void GenerateLevelRubikSymmetrically()
    {
        piecePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Screw Away/Levels/Common/Box - Rubik-like.prefab");

        // Remove all existing blocks
        List<GameObject> toRemoveList = new List<GameObject>();

        for (int i = 0; i < Selection.activeTransform.childCount; i++)
        {
            toRemoveList.Add(Selection.activeTransform.GetChild(i).gameObject);
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item);
        }

        // MATERIALS
        Material[] materialPool = LoadAllMaterialsInFolder("Assets/Materials/Wood/Single Color");

        Vector3 currentPieceSize;

        int totalSpawn = 0;
        float distanceMultiplier = 1.05f;
        int numRow = 2;
        int numScrewSpawned = 0;

        Dictionary<Vector3Int, bool> isTilesContainPiece = new Dictionary<Vector3Int, bool>();
        Dictionary<int, Material> cachedMaterials = new Dictionary<int, Material>();

        Vector3 symmetricalFactor = new Vector3(0, 1, 1);

        // // XPlane
        // Matrix4x4 swapMatrix = new Matrix4x4(
        //     new Vector4(0, 1, 0, 0),
        //     new Vector4(1, 0, 0, 0),
        //     new Vector4(0, 0, 1, 0),
        //     new Vector4(0, 0, 0, 1)
        // );

        // // ZPlane
        // Matrix4x4 swapMatrix = new Matrix4x4(
        //     new Vector4(0, 0, 1, 0),
        //     new Vector4(0, 1, 0, 0),
        //     new Vector4(1, 0, 0, 0),
        //     new Vector4(0, 0, 0, 1)
        // );

        // // YPlane
        // Matrix4x4 swapMatrix = new Matrix4x4(
        //     new Vector4(0, 1, 0, 0),
        //     new Vector4(1, 0, 0, 0),
        //     new Vector4(0, 0, 1, 0),
        //     new Vector4(0, 0, 0, 1)
        // );

        Matrix4x4 swapMatrix = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, 0, 1)
        );

        // GENERATE FIRST HALF
        // X
        for (int i = 0; i <= numRow + 0; i++)
        {
            Material randomMaterial = materialPool[UnityEngine.Random.Range(0, materialPool.Length)];

            cachedMaterials.Add(i, randomMaterial);

            // Y
            for (int j = -0; j <= numRow; j++)
            {
                // Z
                for (int k = -numRow - 0; k <= numRow + 0; k++)
                {
                    if (numScrewSpawned >= maxScrew)
                    {
                        continue;
                    }

                    int random = UnityEngine.Random.Range(0, 3);

                    Vector3Int coordinator = new Vector3Int(i, j, k);
                    Vector3 transformedCoordinator = swapMatrix.MultiplyPoint3x4(coordinator);
                    Vector3Int transformedCoordinatorInt = new Vector3Int(
                        Mathf.RoundToInt(transformedCoordinator.x),
                        Mathf.RoundToInt(transformedCoordinator.y),
                        Mathf.RoundToInt(transformedCoordinator.z)
                    );

                    if (random == 0)
                    {
                        SpawnBlock(transformedCoordinatorInt, randomMaterial);

                        totalSpawn++;

                        isTilesContainPiece.Add(transformedCoordinatorInt, true);
                    }
                    else
                    {
                        isTilesContainPiece.Add(transformedCoordinatorInt, false);
                    }
                }
            }
        }

        // GENERATE SECOND HALF
        // X
        for (int i = -1; i >= -numRow - 0; i--)
        {
            Material cachedMaterial = cachedMaterials[-i];

            // Y
            for (int j = -numRow; j <= numRow; j++)
            {
                // Z
                for (int k = -numRow - 0; k <= numRow + 0; k++)
                {
                    Vector3Int coordinator = new Vector3Int(i, j, k);
                    Vector3 transformedCoordinator = swapMatrix.MultiplyPoint3x4(coordinator);
                    Vector3Int transformedCoordinatoraInt = new Vector3Int(
                        Mathf.RoundToInt(transformedCoordinator.x),
                        Mathf.RoundToInt(transformedCoordinator.y),
                        Mathf.RoundToInt(transformedCoordinator.z)
                    );

                    Vector3Int cachedCoordinator = new Vector3Int(-i, j, k);
                    Vector3 transformedCachedCoordinator = swapMatrix.MultiplyPoint3x4(cachedCoordinator);
                    Vector3Int transformedCachedCoordinatorInt = new Vector3Int(
                        Mathf.RoundToInt(transformedCachedCoordinator.x),
                        Mathf.RoundToInt(transformedCachedCoordinator.y),
                        Mathf.RoundToInt(transformedCachedCoordinator.z)
                    );

                    if (isTilesContainPiece[transformedCachedCoordinatorInt])
                    {
                        SpawnBlock(transformedCoordinatoraInt, cachedMaterial);
                    }
                }
            }
        }

        void SpawnBlock(Vector3Int coordinator, Material randomMaterial)
        {
            Transform generatedPiece = ((GameObject)PrefabUtility.InstantiatePrefab(piecePrefab, Selection.activeTransform)).transform;

            generatedPiece.name = $"Piece {totalSpawn + 1}";

            numScrewSpawned += generatedPiece.transform.childCount;

            Vector3 rotation = new Vector3(0, 0, 0);
            generatedPiece.rotation = Quaternion.Euler(rotation);

            currentPieceSize = generatedPiece.GetComponent<MeshRenderer>().bounds.size;

            Vector3 position = new Vector3
            (
                distanceMultiplier * coordinator.x * currentPieceSize.x,
                distanceMultiplier * coordinator.y * currentPieceSize.y,
                distanceMultiplier * coordinator.z * currentPieceSize.z
            );

            generatedPiece.position = position;

            // MATERIAL
            generatedPiece.GetComponent<MeshRenderer>().sharedMaterial = randomMaterial;

            totalSpawn++;
        }

        EditorUtility.SetDirty(Selection.activeTransform);
    }

    private void GenerateLevelRubikQuarter()
    {
        if (piecePrefab == null)
        {
            piecePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Screw Away/Levels/Common/Box - Rubik-like (3 Screws) Variant 2.prefab");
        }

        // Remove all existing blocks
        List<GameObject> toRemoveList = new List<GameObject>();

        for (int i = 0; i < Selection.activeTransform.childCount; i++)
        {
            toRemoveList.Add(Selection.activeTransform.GetChild(i).gameObject);
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item);
        }

        // MATERIALS
        Material[] materialPool = LoadAllMaterialsInFolder("Assets/Materials/Wood/Single Color");

        Vector3 currentPieceSize;

        int totalSpawn = 0;
        float distanceMultiplier = 1.05f;
        int numRow = 2;
        int numScrewSpawned = 0;

        Vector3Int additional = new Vector3Int(0, 0, 0);

        Dictionary<Vector3Int, bool> isTilesContainPiece = new Dictionary<Vector3Int, bool>();
        Dictionary<Vector3Int, Material> cachedMaterials = new Dictionary<Vector3Int, Material>();

        // GENERATE FIRST HALF
        // X
        for (int i = 0; i <= numRow + additional.x; i++)
        {
            Material randomMaterial = materialPool[UnityEngine.Random.Range(0, materialPool.Length)];

            // Y
            for (int j = 0; j <= numRow + additional.y; j++)
            {
                // Z
                for (int k = 0; k <= numRow + additional.z; k++)
                {
                    if (numScrewSpawned >= maxScrew)
                    {
                        continue;
                    }

                    int random = UnityEngine.Random.Range(0, 3);

                    Vector3Int coordinator = new Vector3Int(i, j, k);

                    if (random == 0)
                    {
                        SpawnBlock(coordinator, randomMaterial);

                        totalSpawn++;

                        isTilesContainPiece.Add(coordinator, true);
                        cachedMaterials.Add(coordinator, randomMaterial);
                    }
                    else
                    {
                        isTilesContainPiece.Add(coordinator, false);
                    }
                }
            }
        }

        // GENERATE SECOND QUARTER
        // X
        for (int i = -numRow - additional.x; i <= numRow + additional.x; i++)
        {
            // Y
            for (int j = -numRow - additional.y; j <= numRow + additional.y; j++)
            {
                // Z
                for (int k = -numRow - additional.z; k <= numRow + additional.z; k++)
                {
                    if (i >= 0 && j >= 0 && k >= 0)
                    {
                        continue;
                    }

                    Vector3Int coordinator = new Vector3Int(i, j, k);
                    Vector3Int absoluteCoordinator = GetAbsoluteVector3(coordinator);

                    if (isTilesContainPiece[absoluteCoordinator])
                    {
                        SpawnBlock(coordinator, cachedMaterials[absoluteCoordinator]);
                    }
                }
            }
        }

        return;

        // GENERATE THIRD QUARTER
        // X
        for (int i = -numRow - additional.x; i <= -1; i++)
        {
            // Y
            for (int j = 0; j <= numRow + additional.y; j++)
            {
                // Z
                for (int k = -numRow - additional.z; k <= numRow + additional.z; k++)
                {
                    Vector3Int coordinator = new Vector3Int(i, j, k);
                    Vector3Int cachedCoordinator = new Vector3Int(-i, j, k);

                    if (isTilesContainPiece[cachedCoordinator])
                    {
                        SpawnBlock(coordinator, cachedMaterials[cachedCoordinator]);
                    }
                }
            }
        }

        // GENERATE FOURTH QUARTER
        // X
        for (int i = -numRow - additional.x; i <= -1; i++)
        {
            // Y
            for (int j = -numRow - additional.y; j <= -1; j++)
            {
                // Z
                for (int k = -numRow - additional.z; k <= numRow + additional.z; k++)
                {
                    Vector3Int coordinator = new Vector3Int(i, j, k);
                    Vector3Int cachedCoordinator = new Vector3Int(-i, -j, k);

                    if (isTilesContainPiece[cachedCoordinator])
                    {
                        SpawnBlock(coordinator, cachedMaterials[cachedCoordinator]);
                    }
                }
            }
        }

        void SpawnBlock(Vector3Int coordinator, Material randomMaterial)
        {
            Transform generatedPiece = ((GameObject)PrefabUtility.InstantiatePrefab(piecePrefab, Selection.activeTransform)).transform;

            generatedPiece.name = $"Piece {totalSpawn + 1}";

            numScrewSpawned += generatedPiece.transform.childCount;

            Vector3 rotation = new Vector3(0, 0, 0);
            generatedPiece.rotation = Quaternion.Euler(rotation);

            currentPieceSize = generatedPiece.GetComponent<MeshRenderer>().bounds.size;

            Vector3 position = new Vector3
            (
                distanceMultiplier * coordinator.x * currentPieceSize.x,
                distanceMultiplier * coordinator.y * currentPieceSize.y,
                distanceMultiplier * coordinator.z * currentPieceSize.z
            );

            generatedPiece.position = position;

            // MATERIAL
            generatedPiece.GetComponent<MeshRenderer>().sharedMaterial = randomMaterial;

            totalSpawn++;
        }

        EditorUtility.SetDirty(Selection.activeTransform);
    }

    private void GenerateLevel()
    {
        int totalPiece = UnityEngine.Random.Range(24, 32);

        totalPiece += totalPiece % 2;

        int halfTotalPiece = totalPiece / 2;

        Transform[] generatedPieces = new Transform[totalPiece];

        // Remove all existing blocks
        List<GameObject> toRemoveList = new List<GameObject>();

        for (int i = 0; i < Selection.activeTransform.childCount; i++)
        {
            toRemoveList.Add(Selection.activeTransform.GetChild(i).gameObject);
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item);
        }

        // MATERIALS
        Material[] materialPool = LoadAllMaterialsInFolder("Assets/Materials/Wood/Single Color");

        int tryTime = 0;
        int maxTryTime = 36;
        bool isFound;

        for (int i = 0; i < halfTotalPiece; i++)
        {
            tryTime = 0;
            isFound = false;

            Transform generatedPiece = ((GameObject)PrefabUtility.InstantiatePrefab(piecePrefab, Selection.activeTransform)).transform;

            generatedPiece.name = $"Piece {i + 1}";

            float positionX;
            Vector3 position = Vector3.zero;

            // float positionX = (i / 4f) * UnityEngine.Random.Range(1.5f, 4);

            // Vector3 position = new Vector3(positionX, UnityEngine.Random.Range(-8, 8), UnityEngine.Random.Range(-8, 8));

            // generatedPiece.position = position;

            // if (IsNear(generatedPiece, generatedPieces))
            // {
            //     positionX = 1.8f * i;
            // }

            for (int j = 0; j < maxTryTime; j++)
            {
                positionX = (j / 12 + 1) * 1.8f * UnityEngine.Random.Range(1, 4);

                float multiplier = j / 12 + 1;

                position = new Vector3(positionX, multiplier * UnityEngine.Random.Range(-4, 4), multiplier * UnityEngine.Random.Range(-4, 4));

                generatedPiece.position = position;

                if (!IsNear(generatedPiece, generatedPieces))
                {
                    isFound = true;

                    break;
                }
            }

            if (!isFound)
            {
                position = position.ChangeX(1.8f * i);
            }

            // for (int j = 0; j < maxTryTime; j++)
            // {
            //     position = new Vector3(UnityEngine.Random.Range(1, 12), j * UnityEngine.Random.Range(-1, 1), j * UnityEngine.Random.Range(-1, 1));

            //     generatedPiece.position = position;

            //     Debug.Log("TRY " + generatedPiece.name + "/" + j);

            //     if (!IsOverlapping(position, generatedPiece.gameObject))
            //     {
            //         break;
            //     }
            // }

            // while (IsOverlapping(position, generatedPiece.gameObject) && tryTime <= maxTryTime)
            // {
            //     tryTime++;

            //     position = new Vector3(UnityEngine.Random.Range(1, 12), tryTime * UnityEngine.Random.Range(-1, 1), tryTime * UnityEngine.Random.Range(-1, 1));

            //     generatedPiece.position = position;

            //     Debug.Log("TRY " + generatedPiece.name + "/" + tryTime);
            // }

            Vector3 rotation = 45 * new Vector3(UnityEngine.Random.Range(0, 4), 0, 2 * UnityEngine.Random.Range(-1, 1));

            // MATERIAL
            generatedPiece.GetComponent<MeshRenderer>().sharedMaterial = materialPool[UnityEngine.Random.Range(0, materialPool.Length)];

            // generatedPiece.rotation = Quaternion.Euler(new Vector3(30 * UnityEngine.Random.Range(-3, 3), 0, -90));
            generatedPiece.rotation = Quaternion.Euler(new Vector3(0, 180, 90));
            generatedPiece.position = position;

            generatedPieces[i] = generatedPiece;
        }

        for (int i = halfTotalPiece; i < totalPiece; i++)
        {
            Transform generatedPiece = ((GameObject)PrefabUtility.InstantiatePrefab(piecePrefab, Selection.activeTransform)).transform;

            generatedPiece.name = $"Piece {i + 1}";

            // MATERIAL
            generatedPiece.GetComponent<MeshRenderer>().sharedMaterial = generatedPieces[i - halfTotalPiece].GetComponent<MeshRenderer>().sharedMaterial;

            // POSITION
            Vector3 position = generatedPieces[i - halfTotalPiece].position;

            position.x *= -1;

            generatedPiece.position = position;

            // ROTATION
            Vector3 rotation = generatedPieces[i - halfTotalPiece].rotation.eulerAngles;

            rotation.z += 180;

            generatedPiece.rotation = Quaternion.Euler(rotation);
        }

        EditorUtility.SetDirty(Selection.activeTransform);

        bool IsOverlapping(Vector3 spawnPosition, GameObject target)
        {
            Collider[] colliders = Physics.OverlapBox(spawnPosition, 0.5f * target.transform.localScale, target.transform.rotation);

            int totalCollider = 0;

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject == target)
                {

                }
                else
                {
                    totalCollider++;

                    // Debug.Log(spawnPosition + "/" + target.name + "/" + colliders[i].name);
                }
            }

            Debug.Log(target.name + "/" + totalCollider);

            return totalCollider > 0;
        }
    }

    private void RandomizeLevelModelColor()
    {
        GameObject levelPrefab = Selection.activeGameObject;

        BasicObjectPart[] objectParts = TransformUtil.GetComponentsFromAllChildren<BasicObjectPart>(levelPrefab.transform).ToArray();
        Renderer[] objectPartRenderers = new Renderer[objectParts.Length];

        // MATERIALS
        ObjectPartMaterialContainer objectPartMaterialContainer = AssetDatabase.LoadAssetAtPath<ObjectPartMaterialContainer>(
            "Assets/Scriptable Objects/Material Container/Object Part Material Container.asset");

        List<Material> materialPool = objectPartMaterialContainer.Materials.ToList();

        List<Material> piecesMaterials = new List<Material>();

        for (int i = 0; i < objectParts.Length; i++)
        {
            objectPartRenderers[i] = objectParts[i].GetComponent<MeshRenderer>();
        }

        Material material = null;

        for (int i = 0; i < objectParts.Length; i++)
        {
            if (!materialPool.Contains(objectPartRenderers[i].sharedMaterial))
            {
                return;
            }

            if (!piecesMaterials.Contains(objectPartRenderers[i].sharedMaterial))
            {
                piecesMaterials.Add(objectPartRenderers[i].sharedMaterial);
            }
        }

        Dictionary<Material, int> newMaterialIndexes = new Dictionary<Material, int>();

        const int NUM_GRADIENT_MATERIAL = 9;

        // Cache old materials
        _cachedMaterialDictionary = new Dictionary<Renderer, Material>();

        for (int i = 0; i < objectPartRenderers.Length; i++)
        {
            _cachedMaterialDictionary.Add(objectPartRenderers[i], objectPartRenderers[i].sharedMaterial);
        }

        // Random new material for each material index
        foreach (var pieceMaterial in piecesMaterials)
        {
            newMaterialIndexes.Add(
                pieceMaterial, UnityEngine.Random.Range(0, materialPool.Count - NUM_GRADIENT_MATERIAL));
        }

        for (int i = 0; i < objectParts.Length; i++)
        {
            int newMaterialIndex = newMaterialIndexes[objectPartRenderers[i].sharedMaterial];

            objectPartRenderers[i].sharedMaterial = materialPool[newMaterialIndex];
        }

        EditorUtility.SetDirty(levelPrefab);
    }

    private void RevertOriginalModelColor()
    {
        if (_cachedMaterialDictionary == null)
        {
            return;
        }

        for (int i = 0; i < _cachedMaterialDictionary.Count; i++)
        {
            Renderer renderer = _cachedMaterialDictionary.Keys.ToArray()[i];
            Material material = _cachedMaterialDictionary.Values.ToArray()[i];

            renderer.sharedMaterial = material;
        }
    }

    private GameObject[] LoadAllPrefabsInFolder(string folder)
    {
        List<GameObject> prefabs = new List<GameObject>();

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:GameObject", new[] { folder });

        // if (prefabGUIDs.Length == 0)
        // {
        //     Debug.LogWarning("No prefabs found in folder: " + folder);
        //     return null;
        // }

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                prefabs.Add(prefab);
            }
        }

        return prefabs.ToArray();
    }

    private Material[] LoadAllMaterialsInFolder(string folder)
    {
        List<Material> prefabs = new List<Material>();

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Material", new[] { folder });

        // if (prefabGUIDs.Length == 0)
        // {
        //     Debug.LogWarning("No prefabs found in folder: " + folder);
        //     return null;
        // }

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material prefab = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (prefab != null)
            {
                prefabs.Add(prefab);
            }
        }

        return prefabs.ToArray();
    }

    private Vector3Int GetAbsoluteVector3(Vector3Int input)
    {
        Vector3Int absoluteOne = input;

        absoluteOne.x = Mathf.Abs(input.x);
        absoluteOne.y = Mathf.Abs(input.y);
        absoluteOne.z = Mathf.Abs(input.z);

        return absoluteOne;
    }
    #endregion

    #region GENERATE SCREW
    private void ClearScrews()
    {
        Transform level = Selection.activeTransform;

        BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(level).ToArray();

        List<BaseScrew> toRemoveList = new List<BaseScrew>();

        toRemoveList.AddRange(screws);

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item.gameObject, true);
        }

        EditorUtility.SetDirty(level);
    }

    private void GenerateScrews()
    {
        Transform level = Selection.activeTransform;

        screwPrefab = LoadAllPrefabsInFolder("Assets/Prefabs/Screw Away/Screw")[2];

        // Remove old screws
        BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(level).ToArray();

        List<BaseScrew> toRemoveList = new List<BaseScrew>();

        toRemoveList.AddRange(screws);

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item.gameObject, true);
        }

        BasicObjectPart[] pieces = GetComponentsFromAllChildren<BasicObjectPart>(level).ToArray();

        for (int i = 0; i < pieces.Length; i++)
        {
            GenerateScrewForPiece(pieces[i]);
        }

        EditorUtility.SetDirty(level);

        void GenerateScrewForPiece(BasicObjectPart piece)
        {
            // reset rotation
            Quaternion cachedPieceRotation = piece.transform.rotation;

            piece.transform.rotation = Quaternion.identity;
            //

            Vector3 pieceSize = piece.GetComponent<MeshCollider>().bounds.size;
            Vector3 halfSize = 0.5f * pieceSize;

            Vector3 screwSize = screwPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.size * expectedScale;
            Vector3 pieceBoundCenter = piece.GetComponent<MeshFilter>().sharedMesh.bounds.center;

            int maxScrewX = (int)(0.4f * piece.GetComponent<Collider>().bounds.size.x / screwSize.x);
            int maxScrewY = (int)(0.4f * piece.GetComponent<Collider>().bounds.size.y / screwSize.y);
            int maxScrewZ = (int)(0.4f * piece.GetComponent<Collider>().bounds.size.z / screwSize.z);

            maxScrewX = Mathf.Max(maxScrewX, 1);
            maxScrewY = Mathf.Max(maxScrewY, 1);
            maxScrewZ = Mathf.Max(maxScrewZ, 1);

            // Debug.Log(piece.name + "/" + pieceSize + "/" + screwSize);
            // Debug.Log(maxScrewX + "/" + maxScrewY + "/" + maxScrewZ);

            // avoid spawn too many
            if (maxScrewX * maxScrewY * maxScrewZ > 16)
            {
                maxScrewX = Mathf.Min(maxScrewX, 2);
                maxScrewY = Mathf.Min(maxScrewY, 2);
                maxScrewZ = Mathf.Min(maxScrewZ, 2);
            }

            Dictionary<Vector3, bool> isTilesEmpty = new Dictionary<Vector3, bool>();

            for (int i = 0; i < maxScrewX; i++)
            {
                for (int j = 0; j < maxScrewY; j++)
                {
                    for (int k = 0; k < maxScrewZ; k++)
                    {
                        isTilesEmpty.Add(new Vector3(i, j, k), true);
                    }
                }
            }

            Debug.Log("SAFERIO " + piece.name + "/" + screwSize + "/" + maxScrewX + "/" + maxScrewY + "/" + maxScrewZ);

            int numScrewSpawned = 0;

            for (int i = 0; i < maxScrewX; i++)
            {
                for (int j = 0; j < maxScrewY; j++)
                {
                    for (int k = 0; k < maxScrewZ; k++)
                    {
                        if (numScrewSpawned >= maxScrew)
                        {
                            break;
                        }

                        if (!IsValidTile(i, j, k, isTilesEmpty))
                        {
                            continue;
                        }

                        // bool isRandomSpawn = UnityEngine.Random.Range(0, 1) == 0;

                        // isRandomSpawn = true;

                        // if (!isRandomSpawn)
                        // {
                        //     continue;
                        // }

                        BaseScrew screw = PrefabUtility.InstantiatePrefab(screwPrefab, piece.transform).GetComponent<BaseScrew>();

                        screw.name = $"Screw 3D {numScrewSpawned}";

                        numScrewSpawned++;

                        List<FaceDirection> poolDirection = new List<FaceDirection>();

                        // poolDirection.Add(FaceDirection.Front);
                        // poolDirection.Add(FaceDirection.Top);
                        // poolDirection.Add(FaceDirection.Side);

                        // if (1.6f * screwSize.x > pieceSize.x || 1.6f * screwSize.y > pieceSize.y)
                        // {
                        //     poolDirection.Remove(FaceDirection.Front);
                        // }
                        // if (1.6f * screwSize.y > pieceSize.y || 1.6f * screwSize.z > pieceSize.z)
                        // {
                        //     poolDirection.Remove(FaceDirection.Side);
                        // }
                        // if (1.6f * screwSize.x > pieceSize.x || 1.6f * screwSize.z > pieceSize.z)
                        // {
                        //     poolDirection.Remove(FaceDirection.Top);
                        // }

                        // if (poolDirection.Count == 0)
                        // {
                        //     poolDirection.Add(FaceDirection.Top);
                        // }

                        // FaceDirection faceDirection = poolDirection[UnityEngine.Random.Range(0, poolDirection.Count)];

                        float areaFront = pieceSize.x * pieceSize.y;
                        float areaTop = pieceSize.x * pieceSize.z;
                        float areaSide = pieceSize.y * pieceSize.z;

                        if (areaFront >= areaTop && areaFront >= areaSide)
                        {
                            poolDirection.Add(FaceDirection.Front);
                            // poolDirection.Add(FaceDirection.Top);
                        }

                        if (areaTop >= areaFront && areaTop >= areaSide)
                        {
                            poolDirection.Add(FaceDirection.Top);
                            // poolDirection.Add(FaceDirection.Side);
                        }

                        if (areaSide >= areaFront && areaSide >= areaTop)
                        {
                            poolDirection.Add(FaceDirection.Side);
                            // poolDirection.Add(FaceDirection.Top);
                        }

                        FaceDirection faceDirection = poolDirection[UnityEngine.Random.Range(0, poolDirection.Count)];

                        screw.transform.position = piece.transform.position +
                            new Vector3
                            (
                                (-(maxScrewX - 1) / 2f + i) * 0.7f * pieceSize.x / maxScrewX,
                                (-(maxScrewY - 1) / 2f + j) * 0.7f * pieceSize.y / maxScrewY,
                                (-(maxScrewZ - 1) / 2f + k) * 0.7f * pieceSize.z / maxScrewZ
                            );

                        int direction = UnityEngine.Random.Range(0, 10) < 3 ? -1 : 1;

                        // prevent invalid screw generation
                        direction = 1;

                        Debug.Log("SAFERIO " + piece.name + "/" + faceDirection);

                        if (faceDirection == FaceDirection.Front)
                        {
                            screw.transform.position = screw.transform.position.ChangeZ(piece.transform.position.z - direction * pieceSize.z);
                            // screw.transform.rotation = Quaternion.Euler(new Vector3(180, 0, 0));

                            // screw.transform.position = piece.transform.position + piece.transform.forward * pieceSize.z;
                            screw.transform.rotation = Quaternion.LookRotation(-piece.transform.forward);

                            for (int m = 0; m < maxScrewZ; m++)
                            {
                                isTilesEmpty[new Vector3(i, j, m)] = false;
                            }
                        }
                        else if (faceDirection == FaceDirection.Top)
                        {
                            screw.transform.position = screw.transform.position.ChangeY(piece.transform.position.y + direction * pieceSize.y);
                            // screw.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));

                            // screw.transform.position = piece.transform.position + piece.transform.up * pieceSize.y;
                            screw.transform.rotation = Quaternion.LookRotation(piece.transform.up);

                            for (int m = 0; m < maxScrewY; m++)
                            {
                                isTilesEmpty[new Vector3(i, m, k)] = false;
                            }
                        }
                        else
                        {
                            direction = UnityEngine.Random.Range(0, 10) < 5 ? -1 : 1;

                            if (direction == -1)
                            {
                                RaycastHit hitFromPiece;

                                level.gameObject.scene.GetPhysicsScene().Raycast(screw.transform.position, Vector3.left, out hitFromPiece);

                                if (hitFromPiece.collider != null)
                                {
                                    direction = 1;
                                }
                            }

                            // screw.transform.position = screw.transform.position.ChangeX(piece.transform.position.x + direction * pieceSize.x);
                            // screw.transform.rotation = Quaternion.Euler(new Vector3(0, direction * 90, 0));

                            screw.transform.position = piece.transform.position - piece.transform.right * pieceSize.x;
                            screw.transform.rotation = Quaternion.LookRotation(-piece.transform.right, piece.transform.up);

                            for (int m = 0; m < maxScrewX; m++)
                            {
                                isTilesEmpty[new Vector3(m, j, k)] = false;
                            }

                            if (direction == -1)
                            {
                                Debug.Log($"<color=#Ff8E9E> {piece.name} / {screw.name}</color>");
                            }
                        }

                        // COMPENSATE MODEL BOUND CENTER NOT AT (0, 0, 0)
                        screw.transform.position += level.transform.localScale.x * pieceBoundCenter;
                        screw.transform.localScale = TransformUtil.ComponentWiseDivine(Mathf.Max(1, expectedScale) * screw.transform.localScale, screw.transform.lossyScale);

                        // Remove invalid screw
                        RaycastHit hit;

                        level.gameObject.scene.GetPhysicsScene().Raycast(screw.transform.position, -screw.transform.forward, out hit);

                        if (hit.collider == null)
                        {
                            DestroyImmediate(screw.gameObject);
                        }
                        else
                        {
                            // if (hit.collider.GetComponent<BaseScrew>() != null)
                            // {
                            //     DestroyImmediate(screw.gameObject);
                            // }
                        }
                    }
                }
            }

            piece.transform.rotation = cachedPieceRotation;
        }

        bool IsValidTile(int x, int y, int z, Dictionary<Vector3, bool> isTilesEmpty)
        {
            if (isTilesEmpty[new Vector3(x, y, z)])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    #endregion

    #region EXTRACT OTHER GAMES' LEVEL
    private void ChangeMesh()
    {
        Transform selection = Selection.activeTransform;

        MeshFilter meshFilter = selection.GetComponent<MeshFilter>();

        Mesh originalMesh = meshFilter.sharedMesh;

        Mesh newMesh = new Mesh();

        newMesh.vertices = originalMesh.vertices;
        newMesh.triangles = originalMesh.triangles;

        Vector3[] newVertices = originalMesh.vertices;

        for (int i = 0; i < newMesh.vertexCount; i++)
        {
            int random = UnityEngine.Random.Range(0, 2);

            if (random == 0)
            {
                newVertices[i].x *= 1.2f;
            }
        }

        newMesh.vertices = newVertices;

        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();

        newMesh.name = "Generated";

        meshFilter.mesh = newMesh;
    }
    private void ExtractPieces()
    {
        Transform level = Selection.activeTransform;

        Transform[] allChildren = TransformUtil.GetComponentsFromAllChildren<Transform>(level.transform).ToArray();

        Material[] materialPool = LoadAllMaterialsInFolder("Assets/Materials/Wood/Single Color");

        Material randomMaterial = materialPool[UnityEngine.Random.Range(0, materialPool.Length)];

        for (int i = 0; i < allChildren.Length; i++)
        {
            Transform child = allChildren[i];

            if (child.name.Contains(matchName))
            {
                Transform piece = Instantiate(child, level);

                piece.transform.position = child.position;

                Component[] components = piece.GetComponents<Component>();

                foreach (Component component in components)
                {
                    if (!(component is Transform) && !(component is MeshFilter) && !(component is MeshRenderer) && !(component is MeshCollider))
                    {
                        DestroyImmediate(component, allowDestroyingAssets: true);
                    }

                    if (component == null)
                    {
                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(piece.gameObject);
                    }
                }

                MeshRenderer meshRenderer = piece.GetComponent<MeshRenderer>();

                if (meshRenderer != null)
                {
                    meshRenderer.sharedMaterial = randomMaterial;
                }
                else
                {
                    DestroyImmediate(piece.gameObject, allowDestroyingAssets: true);

                    continue;
                }

                // Remove all existing blocks
                List<GameObject> toRemoveList = new List<GameObject>();

                for (int j = 0; j < piece.childCount; j++)
                {
                    toRemoveList.Add(piece.GetChild(j).gameObject);
                }

                foreach (var item in toRemoveList)
                {
                    DestroyImmediate(item, allowDestroyingAssets: true);
                }
            }
        }
    }
    #endregion
}
#endif

enum FaceDirection
{
    Front,
    Top,
    Side
}
