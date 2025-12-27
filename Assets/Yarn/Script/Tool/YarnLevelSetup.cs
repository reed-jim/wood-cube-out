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

public class YarnLevelSetup : EditorWindow
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
    private Vector2 _scrollPos;
    private Vector2 _scrollPosInteractables;
    private Vector2 _scrollPosDecors;

    #endregion

    #region Color Convert

    public Vector4 rgba;

    #endregion

    [MenuItem("Saferio/Yarn Level Setup")]
    public static void ShowWindow()
    {
        GetWindow<YarnLevelSetup>("Yarn Level Setup");
    }

    private CountryDataContainer countryDataContainer;

    #region CALLBACKS

    private void OnGUI()
    {
        float padding = 20f;

        Rect areaRect = new Rect(padding, padding, position.width - 2 * padding, position.height - 2 * padding);

        GUILayout.BeginArea(areaRect);
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(800));

        if (_centeredButtonStyle == null)
        {
            _centeredButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
        }

        countryDataContainer = (CountryDataContainer)EditorGUILayout.ObjectField("countryDataContainer",
            countryDataContainer, typeof(CountryDataContainer), true);
        _pieceMaterialContainer = (PieceMaterialContainer)EditorGUILayout.ObjectField("pieceMaterialContainer",
            _pieceMaterialContainer, typeof(PieceMaterialContainer), true);

        if (GUILayout.Button("Load Assets"))
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
        GUILayout.Label("PIECES", centeredStyle);

        GUILayout.Label("Interactables", centeredStyle);
        _scrollPosInteractables = EditorGUILayout.BeginScrollView(_scrollPosInteractables, GUILayout.Height(250));
        for (int i = 0; i < interactablePieces.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"[{i}]", GUILayout.Width(30));
            interactablePieces[i] =
                (Transform)EditorGUILayout.ObjectField(interactablePieces[i], typeof(GameObject), false);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                interactablePieces.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Label("Decors", centeredStyle);
        _scrollPosDecors = EditorGUILayout.BeginScrollView(_scrollPosDecors, GUILayout.Height(250));
        for (int i = 0; i < decorPieces.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"[{i}]", GUILayout.Width(30));
            decorPieces[i] = (Transform)EditorGUILayout.ObjectField(decorPieces[i], typeof(GameObject), false);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                decorPieces.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        RemapFactionInfo();
        GUILayout.Space(16);
        EditorGUILayout.EndScrollView();


        GUILayout.EndArea();
    }

    private void SetupLevelButtons()
    {
        if (GUILayout.Button(text: "Filter Interactable/Decor Pieces", GUILayout.Width(250)))
        {
            foreach (Transform transform in Selection.transforms)
            {
                FilterInteractablePieces(transform);
            }
        }

        if (GUILayout.Button("Step 1:  Assign Materials", GUILayout.Width(250)))
            AssignMaterials();

        if (GUILayout.Button("Step 2: Add MeshColliders For Children", GUILayout.Width(250)))
            AddMeshCollidersForChilren(Selection.activeTransform);


        if (GUILayout.Button("Step 3: AssignComponentsForPieces", GUILayout.Width(250)))
            AssignComponentsForPieces();


        if (GUILayout.Button("Step 4: AssignFactionBasedOnMaterial", GUILayout.Width(250)))
            AssignFactionBasedOnMaterial(Selection.activeTransform);


        GUILayout.Space(8);

        if (GUILayout.Button("Setup for Selected Levels", GUILayout.Width(300)))
            SetupSelectedLevels();


        if (GUILayout.Button("CheckFactionsYarnLevel"))
        {
            CheckFactionsYarnLevel();
        }
    }

    private void RemapFactionInfo()
    {
// Faction Remap Tool UI
        GUILayout.Space(16);
        GUILayout.Label("Faction Remap Tool", EditorStyles.boldLabel);

        // Draw editable list
        if (factionRemapConfig != null)
        {
            for (int i = 0; i < factionRemapConfig.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                factionRemapConfig[i].fromFaction =
                    (GameFaction)EditorGUILayout.EnumPopup("From", factionRemapConfig[i].fromFaction);
                factionRemapConfig[i].toFaction =
                    (GameFaction)EditorGUILayout.EnumPopup("To", factionRemapConfig[i].toFaction);

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    factionRemapConfig.RemoveAt(i);
                    i--;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        if (GUILayout.Button("Add Remap Entry", GUILayout.Width(150)))
        {
            factionRemapConfig.Add(new FactionRemapEntry());
        }

        GUILayout.Space(4);
        if (GUILayout.Button("Apply Faction Remap To Selected Levels", GUILayout.Width(300)))
        {
            ApplyFactionRemapToSelectedLevels();
        }
    }

    #endregion

    #region Piece's faction remap

    [System.Serializable]
    private class FactionRemapEntry
    {
        public GameFaction fromFaction;
        public GameFaction toFaction;
    }

    [SerializeField] private List<FactionRemapEntry> factionRemapConfig = new List<FactionRemapEntry>
    {
        // Example default "set\-first\-config"
        /*new FactionRemapEntry { fromFaction = GameFaction.YarnCream, toFaction = GameFaction.YarnLightBrown },
        new FactionRemapEntry { fromFaction = GameFaction.YarnCyan, toFaction = GameFaction.YarnLightCyan },
        new FactionRemapEntry { fromFaction = GameFaction.YarnDarkBrown, toFaction = GameFaction.YarnBrown },
        new FactionRemapEntry { fromFaction = GameFaction.YarnLightGreen, toFaction = GameFaction.YarnLightBrown },
        new FactionRemapEntry { fromFaction = GameFaction.YarnMagenta, toFaction = GameFaction.YarnPink },
        new FactionRemapEntry { fromFaction = GameFaction.YarnGray, toFaction = GameFaction.YarnSnow },
        new FactionRemapEntry { fromFaction = GameFaction.YarnGreen, toFaction = GameFaction.YarnForest },
        new FactionRemapEntry { fromFaction = GameFaction.YarnRed, toFaction = GameFaction.YarnCrimson },
        new FactionRemapEntry { fromFaction = GameFaction.YarnTeal, toFaction = GameFaction.YarnLightCyan },
        new FactionRemapEntry { fromFaction = GameFaction.YarnWhite, toFaction = GameFaction.YarnSnow },
        new FactionRemapEntry { fromFaction = GameFaction.YarnYellow, toFaction = GameFaction.YarnLime },*/
        new FactionRemapEntry { fromFaction = GameFaction.YarnSteel, toFaction = GameFaction.YarnDeep },
        new FactionRemapEntry { fromFaction = GameFaction.YarnSky, toFaction = GameFaction.YarnLightCyan },
        
        // Current black: Cream, Purple, Teal, White, Yellow
    };

    private void ApplyFactionRemapToSelectedLevels()
    {
        foreach (Transform level in Selection.transforms)
        {
            ApplyFactionRemapToLevel(level);
        }
    }

    private void ApplyFactionRemapToLevel(Transform level)
    {
        if (level == null)
        {
            DebugLogger.LogError(message: "No level selected. Please select a level root Transform.");
            return;
        }

        if (factionRemapConfig == null || factionRemapConfig.Count == 0)
        {
            DebugLogger.LogError(message: "Faction remap config is empty.");
            return;
        }

        // Get all PieceProperty under level
        PieceProperty[] pieceProps =
            TransformUtil.GetComponentsFromAllChildren<PieceProperty>(level.transform).ToArray();

        if (pieceProps.Length == 0)
        {
            DebugLogger.Log(message: $"No PieceProperty found under level: {level.name}");
            return;
        }

        int changedCount = 0;

        foreach (var pieceProp in pieceProps)
        {
            GameFaction oldFaction = pieceProp.Faction;
            GameFaction newFaction = GetRemappedFaction(oldFaction);

            if (!Equals(oldFaction, newFaction))
            {
                pieceProp.Faction = newFaction;
                pieceProp.SetColorByFaction();
                changedCount++;

                EditorUtility.SetDirty(pieceProp.gameObject);
            }
        }

        EditorUtility.SetDirty(level);
        DebugLogger.Log(
            message: $"Applied faction remap to {changedCount} PieceProperty component(s) under level {level.name}.");
    }

    private GameFaction GetRemappedFaction(GameFaction original)
    {
        if (factionRemapConfig == null) return original;

        foreach (var entry in factionRemapConfig)
        {
            if (entry != null && entry.fromFaction.Equals(original))
            {
                return entry.toFaction;
            }
        }

        return original; // no mapping -> keep
    }

    #endregion

    #region YARN LEVEL SETUP

    List<Transform> interactablePieces = new List<Transform>();
    List<Transform> decorPieces = new List<Transform>();

    private void FilterInteractablePieces(Transform level)
    {
        DebugLogger.Log();
        Transform[] allChildren = TransformUtil.GetComponentsFromAllChildren<Transform>(level.transform).ToArray();

        interactablePieces.Clear();
        decorPieces.Clear();

        foreach (var item in allChildren)
        {
            Renderer renderer = item.GetComponent<Renderer>();
            if (renderer == null)
            {
                DebugLogger.LogError(message: $"Renderer is null");
                continue;
            }

            Material currentMaterial = renderer.sharedMaterial;

            if (_pieceMaterialContainer.materials.Any(material => ShareCommonWord(material.name, currentMaterial.name)))
            {
                interactablePieces.Add(item);
            }
            else // Decor
            {
                decorPieces.Add(item);
                if (item.GetComponent<Rigidbody>() == null)
                    item.AddComponent<Rigidbody>().isKinematic = true;
                else
                {
                    item.GetComponent<Rigidbody>().isKinematic = true;
                }

                item.gameObject.layer = LayerMask.NameToLayer(LayerConst.DECOR);
            }
        }
    }

    private void AssignMaterials()
    {
        if (_pieceMaterialContainer == null)
            DebugLogger.LogError("PieceMaterialContainer not found in Resources!");

        foreach (var item in interactablePieces)
        {
            Renderer renderer = item.GetComponent<Renderer>();
            if (renderer == null)
            {
                DebugLogger.LogError(message: $"Renderere is null");
                continue;
            }

            Material currentMaterial = renderer.sharedMaterial;

            Material match =
                _pieceMaterialContainer.materials.First(material =>
                    ShareCommonWord(material.name, currentMaterial.name));
            renderer.material = match;
        }

        RemoveMissingScripts();
    }

    private void AddMeshCollidersForChilren(Transform root)
    {
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

        foreach (var item in interactablePieces)
        {
            Renderer renderer = item.GetComponent<Renderer>();
            if (renderer == null)
            {
                DebugLogger.LogError(message: $"Renderere is null");
                continue;
            }

            item.gameObject.layer = LayerMask.NameToLayer("Default");

            item.AddComponent<BasePiece>();
            item.AddComponent<PieceProperty>();
            PieceMaterialPropertyBlock pieceMaterialPropertyBlock = item.AddComponent<PieceMaterialPropertyBlock>();
            item.AddComponent<PieceServiceLocator>().AssignComponents();

            pieceMaterialPropertyBlock.noDepthWriteMaterial = noDepthWriteMaterial;
            pieceMaterialPropertyBlock.pieceMaterialContainer = _pieceMaterialContainer;

            item.GetComponent<MeshCollider>().convex = true;
        }

        EditorUtility.SetDirty(level);
    }

    private void AssignFactionBasedOnMaterial(Transform level)
    {
        BasePiece[] pieces = TransformUtil.GetComponentsFromAllChildren<BasePiece>(level.transform).ToArray();

        for (int i = 0; i < pieces.Length; i++)
        {
            PieceServiceLocator pieceServiceLocator = pieces[i].GetComponent<PieceServiceLocator>();

            DebugLogger.Log(message: $"Material's name: {pieceServiceLocator.pieceMeshRenderer.sharedMaterial.name}");
            DebugLogger.Log(
                message:
                $"Faction: {CommonUtil.FindEnumFromString<GameFaction>(pieceServiceLocator.pieceMeshRenderer.sharedMaterial.name)}");

            pieceServiceLocator.pieceProperty.Faction =
                CommonUtil.FindEnumFromString<GameFaction>(pieceServiceLocator.pieceMeshRenderer.sharedMaterial.name);
        }

        EditorUtility.SetDirty(level);
    }

    private void ChangeDecorLayer(Transform level)
    {
        Transform[] allChildren = TransformUtil.GetComponentsFromAllChildren<Transform>(level.transform).ToArray();

        decorPieces.Clear();

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
            }
            else // Decor
            {
                decorPieces.Add(item);
                item.AddComponent<Rigidbody>().isKinematic = true;
                item.gameObject.layer = LayerMask.NameToLayer(LayerConst.DECOR);
            }
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

        str1 = Regex.Replace(str1, @"\d", "");
        str2 = Regex.Replace(str2, @"\d", "");

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
            EditorUtility.SetDirty(level);
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
            FilterInteractablePieces(levelRoot);
            AssignMaterials();
            AddMeshCollidersForChilren(levelRoot);
            AssignComponentsForPieces();
            AssignFactionBasedOnMaterial(levelRoot);

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