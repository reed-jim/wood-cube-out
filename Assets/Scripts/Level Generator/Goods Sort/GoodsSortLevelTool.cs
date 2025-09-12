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
    private GameObject dishPrefab;
    private GameObject shelfContainer;
    private GameObject goodsContainer;
    private GameObject[] _allGoods;
    #endregion


    private Dictionary<int, GameObject> _generatedShelves;

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
        dishPrefab = (GameObject)EditorGUILayout.ObjectField("Dish Prefab", dishPrefab, typeof(GameObject));
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

    private void GenerateLevel()
    {
        // Load
        _allGoods = LoadAllPrefabsInFolder("Assets/Scripts/Level Generator/Goods Sort/Assets/Prefabs/Goods");
        shelfPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/Level Generator/Goods Sort/Assets/Prefabs/Shelf.prefab");
        shelfContainer = Selection.activeTransform.GetChild(0).gameObject;

        // Remove all existing blocks
        List<GameObject> toRemoveList = new List<GameObject>();

        for (int i = 0; i < shelfContainer.transform.childCount; i++)
        {
            toRemoveList.Add(shelfContainer.transform.GetChild(i).gameObject);
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item);
        }

        // Properties
        int numShelf = numRow * numColumn;

        _generatedShelves = GenerateShelves();

        GenerateItemsAllLayers(2, _generatedShelves.Count);
        // GenerateLayer(0, numShelf);
        // GenerateLayer(1, numShelf);

        EditorUtility.SetDirty(Selection.activeTransform);
    }

    private Dictionary<int, GameObject> GenerateShelves()
    {
        Vector3 tileSize = shelfPrefab.GetComponent<BoxCollider2D>().size;

        Dictionary<int, GameObject> generatedShelves = new Dictionary<int, GameObject>();

        // Remove random shelves symetrically
        List<Vector2Int> emptyCoordinators = new List<Vector2Int>();

        for (int i = 0; i < numColumn / 2; i++)
        {
            for (int j = 0; j < numRow; j++)
            {
                int random = Random.Range(0, 3);

                if (random == 0)
                {
                    emptyCoordinators.Add(new Vector2Int(i, j));
                    emptyCoordinators.Add(new Vector2Int(numColumn - 1 - i, j));

                    // Debug.Log("SAFERIO" + new Vector2Int(j, i) + "/" + new Vector2Int(j, numColumn - 1 - i));
                }
            }
        }

        // Generate
        Vector3 position = new Vector3();

        for (int i = 0; i < numRow; i++)
        {
            for (int j = 0; j < numColumn; j++)
            {
                if (emptyCoordinators.Contains(new Vector2Int(j, i)))
                {
                    continue;
                }

                position.x = (-(numColumn - 1) / 2f + j) * (1 + distanceRatio) * tileSize.x;
                position.y = 1.5f * ((numRow - 1) / 2f - i) * (1 + distanceRatio) * tileSize.y;

                GameObject shelf = (GameObject)PrefabUtility.InstantiatePrefab(shelfPrefab, goodsContainer.transform);

                shelf.name = $"Shelf {j + i * numColumn}";

                shelf.transform.position = position;

                generatedShelves.Add(j + numColumn * i, shelf);
            }
        }

        return generatedShelves;
    }

    private void GenerateItemsAllLayers(int maxLayer, int numShelf)
    {
        Vector3 tileSize = shelfPrefab.GetComponent<BoxCollider2D>().size;

        // Pools
        List<int> poolFactions = new List<int>();
        List<int> poolLayers = new List<int>();

        for (int i = 0; i < maxLayer * numShelf; i++)
        {
            int faction = Random.Range(0, _allGoods.Length);

            for (int j = 0; j < 3; j++)
            {
                poolFactions.Add(faction);
            }
        }

        for (int i = 0; i < 3 * numShelf; i++)
        {
            for (int j = 0; j < maxLayer; j++)
            {
                poolLayers.Add(j);
            }
        }

        Debug.Log("SAFERIO " + poolFactions.Count + "/" + poolLayers.Count);

        // Generate
        List<BaseItem> _spawnedItems = new List<BaseItem>();

        Vector3 position = new Vector3();

        for (int i = 0; i < numRow; i++)
        {
            for (int j = 0; j < numColumn; j++)
            {
                int shelfIndex = j + i * numColumn;

                if (!_generatedShelves.ContainsKey(shelfIndex))
                {
                    continue;
                }

                position.x = (-(numColumn - 1) / 2f + j) * (1 + distanceRatio) * tileSize.x;
                position.y = ((numRow - 1) / 2f - i) * (1 + distanceRatio) * tileSize.y;

                // generate product
                for (int m = 0; m < maxLayer; m++)
                {
                    // avoid three same type items
                    int currentFaction = 0;
                    int numSameFaction = 0;

                    BaseDish dish = null;

                    if (m > 0)
                    {
                        dish = PrefabUtility.InstantiatePrefab(dishPrefab, _generatedShelves[shelfIndex].transform).GetComponent<BaseDish>();

                        dish.transform.localPosition = new Vector3(0, -2, m);
                    }

                    for (int k = 0; k < 3; k++)
                    {
                        int randomFaction = poolFactions[Random.Range(0, poolFactions.Count)];
                        int randomLayer = poolLayers[Random.Range(0, poolLayers.Count)];

                        if (numSameFaction == 0)
                        {
                            currentFaction = randomFaction;
                            numSameFaction++;
                        }
                        else
                        {
                            if (randomFaction == currentFaction)
                            {
                                numSameFaction++;
                            }
                            else
                            {
                                currentFaction = randomFaction;
                            }

                            if (numSameFaction == 3)
                            {
                                while (randomFaction == currentFaction)
                                {
                                    randomFaction = poolFactions[Random.Range(0, poolFactions.Count)];
                                }
                            }
                        }

                        randomLayer = m;

                        GameObject itemObject = (GameObject)PrefabUtility.InstantiatePrefab(_allGoods[randomFaction], _generatedShelves[shelfIndex].transform);

                        BaseItem item = itemObject.GetComponent<BaseItem>();

                        position.x = -0.9f * (1 - k);
                        position.y = 0.2f;
                        position.z = -1 + 0.1f * randomLayer;

                        item.transform.localPosition = position;
                        item.transform.localScale = Vector3.one;

                        if (m > 0)
                        {
                            PutFoodOnDish(item.transform, dish.transform, k);
                        }

                        poolFactions.Remove(randomFaction);
                        poolLayers.Remove(randomLayer);

                        item.TYPE = _allGoods[randomFaction].name;
                        item.LAYER = randomLayer;

                        // if (randomLayer > 0)
                        // {
                        //     item.GetComponent<SpriteRenderer>().color = new Color(0.1f, 0.1f, 0.1f, 1);
                        // }

                        _spawnedItems.Add(item);
                    }
                }
            }
        }

        #region  Randomly remove items
        List<GameObject> toRemoveItems = new List<GameObject>();

        int randomRemoveFactionNumber = (int)Random.Range(1, numShelf / 2f);

        _spawnedItems.Shuffle();

        for (int i = 0; i < randomRemoveFactionNumber; i++)
        {
            string toRemoveFaction = _allGoods[Random.Range(0, _allGoods.Length)].name;

            int numItemsFound = 0;

            for (int j = 0; j < _spawnedItems.Count; j++)
            {
                if (_spawnedItems[j].TYPE == toRemoveFaction)
                {
                    toRemoveItems.Add(_spawnedItems[j].gameObject);

                    numItemsFound++;

                    if (numItemsFound == 3)
                    {
                        break;
                    }
                }
            }
        }

        foreach (var item in toRemoveItems)
        {
            DestroyImmediate(item);
        }
        #endregion

        #region DIFFICULTY
        float ratioMatchItems = GetRatioMatchItemsFirstLayer(_spawnedItems.ToArray());
        float ratioEmptySlot = GetRatioEmptySlotFirstLayer(numShelf);

        Debug.Log("SAFERIO " + ratioMatchItems);
        Debug.Log("SAFERIO " + ratioEmptySlot);
        #endregion
    }

    private void PutFoodOnDish(Transform item, Transform dish, int foodIndexInDish)
    {
        item.SetParent(dish);

        Vector3 position;

        position.x = -0.9f * (1 - foodIndexInDish);
        position.y = 0.2f;
        position.z = -0.1f;

        item.localPosition = position;
        item.localScale = Vector3.one;
        
        dish.localScale = 0.5f * Vector3.one;
    }

    private void GenerateLayer(int layer, int numShelf)
    {
        Vector3 tileSize = shelfPrefab.GetComponent<SpriteRenderer>().bounds.size;

        // Pool Items
        List<int> remaingFactions = new List<int>();

        for (int i = 0; i < numShelf; i++)
        {
            int faction = Random.Range(0, _allGoods.Length);

            for (int j = 0; j < 3; j++)
            {
                remaingFactions.Add(faction);
            }
        }

        // Difficulty

        // Generate
        List<BaseItem> _spawnedItems = new List<BaseItem>();

        Vector3 position = new Vector3();

        for (int i = 0; i < numRow; i++)
        {
            for (int j = 0; j < numColumn; j++)
            {
                int shelfIndex = j + i * numColumn;

                if (!_generatedShelves.ContainsKey(shelfIndex))
                {
                    continue;
                }

                position.x = (-(numColumn - 1) / 2f + j) * (1 + distanceRatio) * tileSize.x;
                position.y = ((numRow - 1) / 2f - i) * (1 + distanceRatio) * tileSize.y;

                // generate product
                for (int k = 0; k < 3; k++)
                {
                    int randomFaction = remaingFactions[Random.Range(0, remaingFactions.Count)];

                    GameObject itemObject = (GameObject)PrefabUtility.InstantiatePrefab(_allGoods[randomFaction], _generatedShelves[shelfIndex].transform);

                    BaseItem item = itemObject.GetComponent<BaseItem>();

                    position.x = -0.35f * (1 - k);
                    position.y = -0.55f;
                    position.z = -1 + 0.1f * layer;

                    item.transform.localPosition = position;

                    remaingFactions.Remove(randomFaction);

                    item.TYPE = _allGoods[randomFaction].name;
                    item.LAYER = layer;

                    if (layer > 0)
                    {
                        item.GetComponent<SpriteRenderer>().color = new Color(0.1f, 0.1f, 0.1f, 1);
                    }

                    _spawnedItems.Add(item);
                }
            }
        }

        // Randomly remove items
        List<GameObject> toRemoveItems = new List<GameObject>();

        int randomRemoveFactionNumber = (int)Random.Range(1, numShelf / 2f);

        _spawnedItems.Shuffle();

        for (int i = 0; i < randomRemoveFactionNumber; i++)
        {
            string toRemoveFaction = _allGoods[Random.Range(0, _allGoods.Length)].name;

            int numItemsFound = 0;

            for (int j = 0; j < _spawnedItems.Count; j++)
            {
                if (_spawnedItems[j].TYPE == toRemoveFaction)
                {
                    toRemoveItems.Add(_spawnedItems[j].gameObject);

                    numItemsFound++;

                    if (numItemsFound == 3)
                    {
                        break;
                    }
                }
            }
        }

        foreach (var item in toRemoveItems)
        {
            DestroyImmediate(item);
        }
    }

    #region DIFFICULTY
    private void CalculateDifficulty(BaseItem[] spawnedItems)
    {
        for (int i = 0; i < spawnedItems.Length; i++)
        {

        }
    }

    private float GetRatioMatchItemsFirstLayer(BaseItem[] spawnedItems)
    {
        int matchRatio = 0;
        int totalItems = 0;
        int totalMatchItems = 0;

        for (int i = 0; i < spawnedItems.Length; i++)
        {
            if (spawnedItems[i].LAYER == 0)
            {
                totalItems++;

                int totalItemWithSameType = 1;

                for (int j = i + 1; j < spawnedItems.Length; j++)
                {
                    if (spawnedItems[j].TYPE == spawnedItems[i].TYPE)
                    {
                        totalItemWithSameType++;
                    }
                }

                if (totalItemWithSameType >= 3)
                {
                    totalMatchItems++;
                }
            }
        }

        return (float)totalMatchItems / totalItems;
    }

    private float GetRatioEmptySlotFirstLayer(int numShelf)
    {
        int totalSlots = 3 * numShelf;
        int totalTakenSlots = 0;

        foreach (var shelf in _generatedShelves.Values)
        {
            BaseItem[] items = TransformUtil.GetComponentsFromAllChildren<BaseItem>(shelf.transform).ToArray();

            totalTakenSlots += items.Length;
        }

        // for (int i = 0; i < _generatedShelves.Count; i++)
        // {
        //     if (!_generatedShelves.ContainsKey(i))
        //         {
        //             continue;
        //         }

        //     BaseItem[] items = TransformUtil.GetComponentsFromAllChildren<BaseItem>(_generatedShelves[i].transform).ToArray();

        //     totalTakenSlots += items.Length;
        // }

        return (float)totalTakenSlots / totalSlots;
    }
    #endregion

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
