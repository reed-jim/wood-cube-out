#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NamPhuThuy.Common;
using NamPhuThuy.EditorTools;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static GameEnum;

public class YarnGameTool : EditorWindow
{
    private Material targetMaterial;
    private PieceMaterialContainer _pieceMaterialContainer;
    private FaceDirection expectedDirection;

    private GUIStyle _centeredButtonStyle;

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

    #region CALLBACKS

    private void OnGUI()
    {
        float padding = 20f;

        Rect areaRect = new Rect(padding, padding, position.width - 2 * padding, position.height - 2 * padding);

        GUILayout.BeginArea(areaRect);
        
        if (_centeredButtonStyle == null)
        {
            _centeredButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
        }
        
        countryDataContainer = (CountryDataContainer)EditorGUILayout.ObjectField("countryDataContainer", countryDataContainer, typeof(CountryDataContainer), true);
        _pieceMaterialContainer = (PieceMaterialContainer)EditorGUILayout.ObjectField("pieceMaterialContainer", _pieceMaterialContainer, typeof(PieceMaterialContainer), true);
        
        GUILayout.Button("Load Assets");
        {
            _pieceMaterialContainer = Resources.Load<PieceMaterialContainer>("PieceMaterialContainer");
        }
        
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
        GUILayout.Label("Yarn Level Setup", centeredStyle);
       
        SetupLevelButtons();

        GUILayout.Space(16);
        GUILayout.Label("Util", centeredStyle);

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void SetupLevelButtons()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Step 1:  Assign Materials", GUILayout.Width(250)))
            AssignMaterials();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Step 2: Add MeshColliders For Children", GUILayout.Width(250)))
            AddMeshCollidersForChilren();
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Step 3: AssignComponentsForPieces", GUILayout.Width(250)))
            AssignComponentsForPieces();
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Step 4: AssignFactionBasedOnMaterial", GUILayout.Width(250)))
            AssignFactionBasedOnMaterial();
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.Space(8);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Run All Level Setup Steps", GUILayout.Width(300)))
            SetupSelectedLevels();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        
        if (GUILayout.Button("CheckFactionsYarnLevel"))
        {
            CheckFactionsYarnLevel();
        }
    }

    #endregion

    #region YARN LEVEL SETUP

    private void AssignMaterials()
    {
        Transform level = Selection.activeTransform;
        Transform[] allChildren = TransformUtil.GetComponentsFromAllChildren<Transform>(level.transform).ToArray();
        
        if (_pieceMaterialContainer == null)
            DebugLogger.LogError("PieceMaterialContainer not found in Resources!");

        foreach (var item in allChildren)
        {
            Renderer renderer = item.GetComponent<Renderer>();
            if (renderer == null)
            {
                DebugLogger.LogError(message: $"Renderere is null");
                continue;
            }

            Material currentMaterial = renderer.sharedMaterial;

            if (_pieceMaterialContainer.materials.Any(material => ShareCommonWord(material.name, currentMaterial.name)))
            {
                Material match = _pieceMaterialContainer.materials.First(material => ShareCommonWord(material.name, currentMaterial.name));
                renderer.material = match;
            }
        }

        RemoveMissingScripts();

        EditorUtility.SetDirty(level);

        
    }

    private void AddMeshCollidersForChilren()
    {
        Transform root = Selection.activeTransform;
        if (root == null)
        {
            DebugLogger.LogError(message: "No Transform selected to add MeshColliders for children.");
            return;
        }

        int addedCount = 0;
        AddMeshCollidersRecursive(root, ref addedCount);

        DebugLogger.Log(message: $"Added MeshCollider to {addedCount} child object(s).");

        if (addedCount > 0)
        {
            EditorUtility.SetDirty(root);
            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);
        }

        void AddMeshCollidersRecursive(Transform parent, ref int count)
        {
            foreach (Transform child in parent)
            {
                // Only add if it has a MeshFilter and no MeshCollider yet
                MeshFilter meshFilter = child.GetComponent<MeshFilter>();
                MeshCollider meshCollider = child.GetComponent<MeshCollider>();

                if (meshFilter != null && meshCollider == null)
                {
                    meshCollider = child.gameObject.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = meshFilter.sharedMesh;
                    meshCollider.convex = true;
                    count++;
                }

                AddMeshCollidersRecursive(child, ref count);
            }
        }
    }
    
    private void AssignComponentsForPieces()
    {
        Transform level = Selection.activeTransform;

        level.AddComponent<RotateObject>();

        Material noDepthWriteMaterial =
            AssetDatabase.LoadAssetAtPath<Material>("Assets/Yarn/Shader/Yarn No Depth Write.shadergraph");
        

        /*
        for (int i = 0; i < level.childCount; i++)
        {
            Transform piece = level.GetChild(i);

            piece.gameObject.layer = LayerMask.NameToLayer("Default");

            piece.AddComponent<BasePiece>();
            piece.AddComponent<PieceProperty>();
            PieceMaterialPropertyBlock pieceMaterialPropertyBlock = piece.AddComponent<PieceMaterialPropertyBlock>();
            piece.AddComponent<PieceServiceLocator>().AssignComponents();

            pieceMaterialPropertyBlock.noDepthWriteMaterial = noDepthWriteMaterial;
            pieceMaterialPropertyBlock.pieceMaterialContainer = _pieceMaterialContainer;

            piece.GetComponent<MeshCollider>().convex = true;
        }*/
        
        Transform[] allChildren = TransformUtil.GetComponentsFromAllChildren<Transform>(level.transform).ToArray();
        foreach (var item in allChildren)
        {
            Renderer renderer = item.GetComponent<Renderer>();
            if (renderer == null)
            {
                DebugLogger.LogError(message: $"Renderere is null");
                continue;
            }

            Material currentMaterial = renderer.sharedMaterial;

            if (_pieceMaterialContainer.materials.Any(material => ShareCommonWord(material.name, currentMaterial.name)))
            {
                Material match = _pieceMaterialContainer.materials.First(material => ShareCommonWord(material.name, currentMaterial.name));
                
                item.gameObject.layer = LayerMask.NameToLayer("Default");

                item.AddComponent<BasePiece>();
                item.AddComponent<PieceProperty>();
                PieceMaterialPropertyBlock pieceMaterialPropertyBlock = item.AddComponent<PieceMaterialPropertyBlock>();
                item.AddComponent<PieceServiceLocator>().AssignComponents();

                pieceMaterialPropertyBlock.noDepthWriteMaterial = noDepthWriteMaterial;
                pieceMaterialPropertyBlock.pieceMaterialContainer = _pieceMaterialContainer;

                item.GetComponent<MeshCollider>().convex = true;
            }
        }

        EditorUtility.SetDirty(level);
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
    
    private bool ShareCommonWord(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return false;

        var separators = new[] { ' ', '_', '-' };

        // Local helper to check if a word is excluded
        bool IsExcluded(string w) =>
            YarnGameToolConst.ExceptWordsCompare
                .Any(e => string.Equals(e, w, System.StringComparison.OrdinalIgnoreCase));

        var words1 = str1
            .Split(separators, System.StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !IsExcluded(w))
            .Select(w => w.ToLowerInvariant())
            .ToArray();

        var words2 = str2
            .Split(separators, System.StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !IsExcluded(w))
            .Select(w => w.ToLowerInvariant())
            .ToArray();

        DebugLogger.Log(message: $"words1: => {string.Join(",", words1)}");
        DebugLogger.Log(message: $"words2: => {string.Join(",", words2)}");

        var set1 = new HashSet<string>(words1);
        return words2.Any(set1.Contains);
    }
   
    
    private void SetupSelectedLevels()
    {
        Transform[] levels = Selection.transforms;
        if (levels == null || levels.Length == 0)
        {
            DebugLogger.LogError(message: "No levels selected. Please select one or more level root Transforms.");
            return;
        }

        DebugLogger.Log(message: $"Running level setup for {levels.Length} level(s).");

        foreach (Transform level in levels)
        {
            if (level == null) continue;
            SetupLevel(level);
        }
    }
    
    private void SetupLevel(Transform levelRoot)
    {
        if (levelRoot == null)
        {
            DebugLogger.LogError(message: "Level root is null when running setup.");
            return;
        }

        // Temporarily set the active transform so existing methods work as-is
        Transform previousSelection = Selection.activeTransform;
        Selection.activeTransform = levelRoot;

        try
        {
            AssignMaterials();
            AddMeshCollidersForChilren();
            AssignComponentsForPieces();
            AssignFactionBasedOnMaterial();

            DebugLogger.Log(message: $"Level setup completed for: {levelRoot.name}");
        }
        finally
        {
            // Restore previous selection
            Selection.activeTransform = previousSelection;
        }
    }
    
    #endregion


    #region CHECKING LEVEL

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

    #region UTIL

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

    private static void RemoveMissingScripts()
    {
        GameObject selection = Selection.activeGameObject;

        Transform[] selectedGameObjects =
            TransformUtil.GetComponentsFromAllChildren<Transform>(selection.transform).ToArray();

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

    

   

    #endregion
}
#endif