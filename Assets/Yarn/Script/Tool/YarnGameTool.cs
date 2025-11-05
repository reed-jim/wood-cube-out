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

public class YarnGameTool : EditorWindow
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


    private FaceDirection expectedDirection;



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

    #region Color Convert
    public Vector4 rgba;
    #endregion


    [MenuItem("Saferio/Yarn Level Tool")]
    public static void ShowWindow()
    {
        GetWindow<YarnGameTool>("Yarn Game Tool");
    }

    private CountryDataContainer countryDataContainer;

    private void OnGUI()
    {
        float padding = 20f;

        Rect areaRect = new Rect(padding, padding, position.width - 2 * padding, position.height - 2 * padding);

        GUILayout.BeginArea(areaRect);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);

        screwPrefab = (GameObject)EditorGUILayout.ObjectField("Screw Prefab", screwPrefab, typeof(GameObject), true);
        piecePrefab = (GameObject)EditorGUILayout.ObjectField("Piece Prefab", piecePrefab, typeof(GameObject), true);
        countryDataContainer = (CountryDataContainer)EditorGUILayout.ObjectField("countryDataContainer", countryDataContainer, typeof(CountryDataContainer), true);

        expectedScale = EditorGUILayout.FloatField("Expected Scale", expectedScale);

        maxScrew = EditorGUILayout.IntField("Max Screw", maxScrew);
        matchName = EditorGUILayout.TextField("Match Name", matchName);

        GUIStyle centeredStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 20
        };

        rgba = EditorGUILayout.Vector4Field("RGBA Color", rgba);
        Color color = new Color(rgba.x, rgba.y, rgba.z, 1);
        EditorGUILayout.ColorField("RGBA Color", color);

        GUILayout.Space(16);
        GUILayout.Label("Yarn", centeredStyle);

        if (GUILayout.Button("AssignComponentsForPieces"))
        {
            AssignComponentsForPieces();
        }

        if (GUILayout.Button("CheckFactionsYarnLevel"))
        {
            CheckFactionsYarnLevel();
        }

        if (GUILayout.Button("ExtractFullLevel"))
        {
            ExtractFullLevel();
        }

        if (GUILayout.Button("AssignFactionBasedOnMaterial"))
        {
            AssignFactionBasedOnMaterial();
        }

        GUILayout.Space(16);
        GUILayout.Label("Util", centeredStyle);

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

        // Transform[] pieces = GetComponentsFromAllChildren<Transform>(level).ToArray();

        if (level.GetComponent<RotateObject>() == null)
        {
            level.AddComponent<RotateObject>();
        }

        for (int i = 0; i < level.childCount; i++)
        {
            Transform piece = level.GetChild(i);

            // // Level Container
            // if (pieces[i] == level)
            // {
            //     if (pieces[i].GetComponent<RotateObject>() == null)
            //     {
            //         pieces[i].AddComponent<RotateObject>();
            //     }

            //     continue;
            // }

            // Pieces
            piece.AddComponent<BasicObjectPart>();
            piece.AddComponent<ObjectPartMaterialPropertyBlock>();
            piece.AddComponent<Rigidbody>();

            if (piece.GetComponent<MeshCollider>() == null)
            {
                MeshCollider meshCollider = piece.AddComponent<MeshCollider>();

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

    private Material LoadMaterialAtPath(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(path);
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

    private void ExtractFullLevel()
    {
        Transform level = Selection.activeTransform;

        Transform[] allChildren = TransformUtil.GetComponentsFromAllChildren<Transform>(level.transform).ToArray();

        PieceMaterialContainer pieceMaterialContainer = AssetDatabase.LoadAssetAtPath<PieceMaterialContainer>("Assets/Yarn/SO/PieceMaterialContainer.asset");

        foreach (var item in allChildren)
        {
            Renderer renderer = item.GetComponent<Renderer>();

            if (renderer != null)
            {
                Material currentMaterial = renderer.sharedMaterial;

                if (pieceMaterialContainer.materials.Any(item => ShareCommonWord(item.name, currentMaterial.name)))
                {
                    Material match = pieceMaterialContainer.materials.First(item => ShareCommonWord(item.name, currentMaterial.name));

                    renderer.material = match;

                    // Debug.Log("SAFERIO material " + item.name + "/" + match);
                }

                // Assign material for decor models
                for (int i = 0; i < item.childCount; i++)
                {
                    Renderer decorRenderer = item.GetChild(i).GetComponent<Renderer>();

                    Material[] materialArray = new Material[decorRenderer.sharedMaterials.Length];

                    for (int j = 0; j < materialArray.Length; j++)
                    {
                        materialArray[j] = pieceMaterialContainer.materials[0];
                    }

                    for (int j = 0; j < decorRenderer.sharedMaterials.Length; j++)
                    {
                        decorRenderer.sharedMaterials = materialArray;
                    }
                }
            }
        }

        RemoveMissingScripts();

        EditorUtility.SetDirty(level);

        bool ShareCommonWord(string str1, string str2)
        {
            // Split both strings into words, considering non-alphabetical characters as delimiters
            var words1 = str1.Split(new char[] { ' ', '_', '-' }, System.StringSplitOptions.RemoveEmptyEntries);
            var words2 = str2.Split(new char[] { ' ', '_', '-' }, System.StringSplitOptions.RemoveEmptyEntries);

            // Check if there's any overlap between the words in the two strings
            return words1.Intersect(words2).Any();
        }
    }

    private void AssignFactionBasedOnMaterial()
    {
        Transform level = Selection.activeTransform;

        BasePiece[] pieces = TransformUtil.GetComponentsFromAllChildren<BasePiece>(level.transform).ToArray();

        for (int i = 0; i < pieces.Length; i++)
        {
            PieceServiceLocator pieceServiceLocator = pieces[i].GetComponent<PieceServiceLocator>();

            pieceServiceLocator.pieceProperty.Faction = CommonUtil.FindEnumFromString<GameFaction>(
                pieceServiceLocator.pieceMeshRenderer.sharedMaterial.name);
        }

        EditorUtility.SetDirty(level);
    }

    private static void RemoveMissingScripts()
    {
        GameObject selection = Selection.activeGameObject;

        Transform[] selectedGameObjects = TransformUtil.GetComponentsFromAllChildren<Transform>(selection.transform).ToArray();

        int totalCount = 0;
        foreach (Transform go in selectedGameObjects)
        {
            int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go.gameObject);
            if (count > 0)
            {
                Debug.Log($"Removed {count} missing scripts from GameObject: {go.name}", go);
            }
            totalCount += count;
        }

        if (totalCount == 0)
        {
            Debug.Log("No missing scripts found in the selected GameObjects.");
        }
        else
        {
            Debug.Log($"Total missing scripts removed: {totalCount}");
        }
    }
    #endregion

    #region YARN
    private void AssignComponentsForPieces()
    {
        Transform level = Selection.activeTransform;

        for (int i = 0; i < level.childCount; i++)
        {
            Transform piece = level.GetChild(i);

            piece.AddComponent<BasePiece>();
            piece.AddComponent<PieceProperty>();
            piece.AddComponent<PieceMaterialPropertyBlock>();
            piece.AddComponent<PieceServiceLocator>().AssignComponents();
        }

        // BasePiece[] pieces = TransformUtil.GetComponentsArrayFromAllChildren<BasePiece>(level);

        // for (int i = 0; i < pieces.Length; i++)
        // {
        //     pieces[i].GetComponent<PieceServiceLocator>().AssignComponents();
        // }

        EditorUtility.SetDirty(level);
    }

    private void CheckFactionsYarnLevel()
    {
        Transform level = Selection.activeTransform;

        BasePiece[] pieces = TransformUtil.GetComponentsArrayFromAllChildren<BasePiece>(level);

        Dictionary<GameFaction, int> numPiecesByFaction = new Dictionary<GameFaction, int>();

        for (int i = 0; i < pieces.Length; i++)
        {
            GameFaction faction = pieces[i].GetComponent<PieceProperty>().Faction;

            if (numPiecesByFaction.ContainsKey(faction))
            {
                numPiecesByFaction[faction]++;
            }
            else
            {
                numPiecesByFaction.Add(faction, 1);
            }
        }

        foreach (var item in numPiecesByFaction)
        {
            Debug.Log($"SAFERIO {item.Key}: {item.Value}");
        }

        EditorUtility.SetDirty(level);
    }
    #endregion
}
#endif
