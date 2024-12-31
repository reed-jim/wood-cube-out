#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class BusOutLevelGeneratorEditor : EditorWindow
{
    [SerializeField] private GameObject busContainerPrefab;
    [SerializeField] private GameObject busPrefab;
    private string prefabPath = "Assets/Prefabs/MyPrefab.prefab";

    [Header("CUSTOMIZE")]
    [SerializeField] private float tileSize = 3;
    [SerializeField] private int areaRow;
    [SerializeField] private int areaColumn;
    [SerializeField] private float maxAngleVariationMagnitude;

    #region PRIVATE FIELD
    private bool[] isTilesChecked;
    #endregion

    [MenuItem("Tools/Saferio/Bus Out/Level Generator")]
    public static void ShowWindow()
    {
        GetWindow<BusOutLevelGeneratorEditor>("Bus Out Level Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Bus Level Generator", EditorStyles.boldLabel);

        prefabPath = EditorGUILayout.TextField("Prefab Path", prefabPath);
        busContainerPrefab = (GameObject)EditorGUILayout.ObjectField("Bus Container Prefab", busContainerPrefab, typeof(GameObject), false);
        busPrefab = (GameObject)EditorGUILayout.ObjectField("Bus Prefab", busPrefab, typeof(GameObject), false);

        tileSize = EditorGUILayout.FloatField("Tile Size", tileSize);
        areaRow = EditorGUILayout.IntField("Area Rows", areaRow);
        areaColumn = EditorGUILayout.IntField("Area Columns", areaColumn);
        maxAngleVariationMagnitude = EditorGUILayout.FloatField("Max Angle Variation", maxAngleVariationMagnitude);

        if (GUILayout.Button("Generate"))
        {
            ModifyPrefab(prefabPath);
        }
    }

    private void ModifyPrefab(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        if (levelPrefab != null)
        {
            Transform busContainer = Instantiate(busContainerPrefab, levelPrefab.transform).transform;

            busContainer.name = "Bus Container";

            Generate(busContainer);

            EditorUtility.SetDirty(levelPrefab);

            PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
        }
        else
        {
            Debug.LogError($"Prefab not found at path: {path}");
        }
    }

    private void Generate(Transform busContainer)
    {
        int busIndex = 0;

        isTilesChecked = new bool[areaRow * areaColumn];

        for (int i = 0; i < isTilesChecked.Length; i++)
        {
            int xIndex = i % areaColumn;
            int yIndex = (i - xIndex) / areaColumn;

            if (!isTilesChecked[i])
            {
                Direction direction = (Direction)Random.Range(0, 4);

                if (direction == Direction.Left || direction == Direction.Right)
                {
                    bool isValid = true;

                    for (int j = xIndex; j <= xIndex + 2; j++)
                    {
                        if (j >= areaColumn)
                        {
                            isValid = false;

                            break;
                        }
                        else
                        {
                            if (isTilesChecked[j + yIndex * areaColumn])
                            {
                                isValid = false;

                                break;
                            }
                        }
                    }

                    if (isValid)
                    {
                        for (int j = xIndex + 1; j <= xIndex + 2; j++)
                        {
                            isTilesChecked[j + yIndex * areaColumn] = true;
                        }

                        Vector3 position = new Vector3();

                        position.x = (xIndex + 1) * tileSize - 0.5f * areaColumn * tileSize;
                        position.z = yIndex * tileSize - 0.5f * areaRow * tileSize;

                        GameObject bus = (GameObject)PrefabUtility.InstantiatePrefab(busPrefab, busContainer);
                        // GameObject bus = Instantiate(busPrefab, busContainer);

                        bus.name = $"Bus {busIndex}";

                        bus.transform.position = position;
                        bus.transform.eulerAngles = Vector3.zero + new Vector3(0, 90 + Random.Range(-maxAngleVariationMagnitude, maxAngleVariationMagnitude), 0);

                        if (direction == Direction.Left)
                        {
                            bus.transform.eulerAngles += new Vector3(0, 180, 0);
                        }

                        bus.GetComponent<VehicleFaction>().SetRandomFaction();

                        busIndex++;
                    }
                }
                else
                {
                    bool isValid = IsValidVerticleAreaForBus(yIndex, yIndex + 2, xIndex);

                    if (isValid)
                    {
                        for (int j = yIndex + 1; j <= yIndex + 2; j++)
                        {
                            isTilesChecked[xIndex + j * areaColumn] = true;
                        }

                        Vector3 position = new Vector3();

                        position.x = xIndex * tileSize - 0.5f * areaColumn * tileSize;
                        position.z = (yIndex + 1) * tileSize - 0.5f * areaRow * tileSize;

                        GameObject bus = (GameObject)PrefabUtility.InstantiatePrefab(busPrefab, busContainer);

                        bus.name = $"Bus {busIndex}";

                        bus.transform.position = position;
                        bus.transform.eulerAngles = Vector3.zero + new Vector3(0, Random.Range(-maxAngleVariationMagnitude, maxAngleVariationMagnitude), 0);

                        if (direction == Direction.Down)
                        {
                            bus.transform.eulerAngles += new Vector3(0, 180, 0);
                        }

                        bus.GetComponent<VehicleFaction>().SetRandomFaction();

                        busIndex++;
                    }
                }

                isTilesChecked[i] = true;
            }
        }
    }

    private bool IsValidVerticleAreaForBus(int startYIndex, int endYIndex, int xIndex)
    {
        bool isValid = true;

        for (int j = startYIndex; j <= endYIndex; j++)
        {
            if (j >= areaRow)
            {
                isValid = false;

                break;
            }
            else
            {
                if (isTilesChecked[xIndex + j * areaColumn])
                {
                    isValid = false;

                    break;
                }
            }
        }

        return isValid;
    }
}
#endif
