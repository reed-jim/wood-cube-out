#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using System.Collections.Generic;
using static GameEnum;
using Dreamteck.Utilities;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

public class GoodsSortLevelTool : EditorWindow
{
    private GameObject tilePrefab;
    private int numRow = 8;
    private int numColumn = 5;
    private float distanceRatio = 0f;
    private bool isIncludeSpecialBlocks;

    #region GOODS SORT
    private GameObject shelfPrefab;
    private GameObject shelfContainer;
    private GameObject goodsContainer;
    private GameObject[] _allGoods;
    #endregion

    private string folderPath = "Assets/Prefabs/Color Block Jam/Blocks";
    private string barricadeFolderPath = "Assets/Prefabs/Color Block Jam/Barricades";

    [MenuItem("Saferio/GoodsSortLevelTool")]
    public static void ShowWindow()
    {
        GetWindow<GoodsSortLevelTool>("GoodsSortLevelTool");
    }


    private void OnGUI()
    {
        float padding = 20f;

        Rect areaRect = new Rect(padding, padding, position.width - 2 * padding, position.height - 2 * padding);

        GUILayout.BeginArea(areaRect);

        GUILayout.Label("Board Generator Settings", EditorStyles.boldLabel);

        numRow = EditorGUILayout.IntField("Number of Rows", numRow);
        numColumn = EditorGUILayout.IntField("Number of Columns", numColumn);
        isIncludeSpecialBlocks = EditorGUILayout.Toggle("Is Include Special Blocks", isIncludeSpecialBlocks);

        shelfPrefab = (GameObject)EditorGUILayout.ObjectField("Shelf Prefab", shelfPrefab, typeof(GameObject));
        goodsContainer = (GameObject)EditorGUILayout.ObjectField("Goods Container", goodsContainer, typeof(GameObject));

        GUILayout.Space(16);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 16;
        buttonStyle.padding = new RectOffset(20, 20, 20, 20);
        buttonStyle.fixedHeight = 60;

        if (GUILayout.Button("Generate Level", buttonStyle))
        {
            GenerateLevel();
        }

        GUILayout.EndArea();
    }

    private async Task GenerateFullLevel()
    {
        Transform level = Selection.activeTransform;

        tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Color Block Jam/Board/Tile.prefab");

        GameObject TileContainerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Color Block Jam/Containers/Tile Container.prefab");
        GameObject BlockContainerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Color Block Jam/Containers/Block Container.prefab");
        GameObject BarricadeContainerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Color Block Jam/Containers/Barricade Container.prefab");

        // Remove all existing blocks
        List<GameObject> toRemoveList = new List<GameObject>();

        for (int i = 0; i < level.childCount; i++)
        {
            toRemoveList.Add(level.GetChild(i).gameObject);
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item);
        }

        Transform tileContainer = ((GameObject)PrefabUtility.InstantiatePrefab(TileContainerPrefab, level)).transform;
        Transform blockContainer = ((GameObject)PrefabUtility.InstantiatePrefab(BlockContainerPrefab, level)).transform;
        Transform barricadeContainer = ((GameObject)PrefabUtility.InstantiatePrefab(BarricadeContainerPrefab, level)).transform;
    }

    private void GenerateLevel()
    {
        // // Load
        // _allGoods = LoadAllPrefabsInFolder("");

        int numGoods = 3 * numRow * numColumn;
        int numShelf = numRow * numColumn;

        Vector3 tileSize = shelfPrefab.GetComponent<SpriteRenderer>().bounds.size;

        // POOL GOODS
        List<int> remaingFactions = new List<int>();

        for (int i = 0; i < numShelf; i++)
        {
            int faction = Random.Range(0, 99);

            for (int j = 0; j < 3; j++)
            {
                remaingFactions.Add(faction);
            }
        }

        Vector3 position = new Vector3();

        for (int i = 0; i < numRow; i++)
        {
            for (int j = 0; j < numColumn; j++)
            {
                int tileIndex = j + i * numColumn;

                position.x = (-(numColumn - 1) / 2f + j) * (1 + distanceRatio) * tileSize.x;
                position.y = ((numRow - 1) / 2f - i) * (1 + distanceRatio) * tileSize.y;

                // generate goods shelf
                GameObject goodShelf = Instantiate(shelfPrefab, goodsContainer.transform);

                goodShelf.transform.position = position;

                // generate product
                int randomFaction = remaingFactions[Random.Range(0, remaingFactions.Count)];

                remaingFactions.Remove(randomFaction);
            }
        }

        EditorUtility.SetDirty(Selection.activeTransform);
    }

    #region UTIL
    bool IsValidArea(bool[] isTileFull, int startX, int startZ, int sizeX, int sizeZ)
    {
        for (int i = startZ; i <= startZ + sizeZ - 1; i++)
        {
            if (i >= numRow)
            {
                return false;
            }

            for (int j = startX; j <= startX + sizeX - 1; j++)
            {
                if (j >= numColumn)
                {
                    return false;
                }

                if (isTileFull[j + numColumn * i])
                {
                    return false;
                }
            }
        }

        return true;
    }

    bool IsValidArea(bool[] isTileFull, int startX, int startZ, int sizeX, int sizeZ, int numRow, int numColumn)
    {
        for (int i = startZ; i <= startZ + sizeZ - 1; i++)
        {
            if (i >= numRow)
            {
                return false;
            }

            for (int j = startX; j <= startX + sizeX - 1; j++)
            {
                if (j >= numColumn)
                {
                    return false;
                }

                if (isTileFull[j + numColumn * i])
                {
                    return false;
                }
            }
        }

        return true;
    }

    private float GetTileDistance()
    {
        tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Color Block Jam/Board/Tile.prefab");

        return (1 + distanceRatio) * tilePrefab.GetComponent<MeshRenderer>().bounds.size.x;
    }

    private GameObject[] LoadAllPrefabsInFolder(string folder)
    {
        List<GameObject> prefabs = new List<GameObject>();

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { folder });

        // if (prefabGUIDs.Length == 0)
        // {
        //     Debug.LogWarning("No prefabs found in folder: " + folder);
        //     return nu;
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

    private GameObject[] LoadPrefabsInExactFolder(string folder)
    {
        List<GameObject> prefabs = new List<GameObject>();

        // Make sure the folder path ends with a slash
        if (!folder.EndsWith("/"))
            folder += "/";

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { folder });

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Check if the asset is in the exact folder (not a subfolder)
            if (Path.GetDirectoryName(path).Replace("\\", "/") + "/" == folder)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    prefabs.Add(prefab);
                }
            }
        }

        return prefabs.ToArray();
    }

    private int RandomIntExceptValue(int from, int to, int except)
    {
        List<int> pool = new List<int>();

        for (int i = from; i <= to; i++)
        {
            int faction = i;

            if (faction != except)
            {
                pool.Add(faction);
            }
        }

        int random = pool[Random.Range(0, pool.Count)];

        return random;
    }
    #endregion
}
#endif
