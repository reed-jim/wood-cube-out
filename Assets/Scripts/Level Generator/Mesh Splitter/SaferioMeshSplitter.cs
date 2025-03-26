using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// using EzySlice;
using UnityEditor;
using UnityEngine;

public class SaferioMeshSplitter : EditorWindow
{
    public GameObject originalMeshObject;
    public GameObject cubePrefab;
    public Material material;
    public float cubeSize = 0.1f;
    public float clusterDistance = 0.1f;

    private List<Vector3> spawnedPositions;
    List<GameObject> spawnedPieces;

    [MenuItem("Tools/Mesh Splitter")]
    static void Init()
    {
        SaferioMeshSplitter window = (SaferioMeshSplitter)GetWindow(typeof(SaferioMeshSplitter));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Mesh Splitter Tool", EditorStyles.boldLabel);
        originalMeshObject = (GameObject)EditorGUILayout.ObjectField("Mesh Object", originalMeshObject, typeof(GameObject), true);
        cubePrefab = (GameObject)EditorGUILayout.ObjectField("Cube Prefab", cubePrefab, typeof(GameObject), true);
        material = (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), true);
        cubeSize = EditorGUILayout.FloatField("Cube Size", cubeSize);

        if (GUILayout.Button("Split Mesh"))
        {
            SplitMesh();
        }

        // if (GUILayout.Button("Combine Mesh"))
        // {
        //     CombineMeshes();
        // }
    }

    private async Task SplitMesh()
    {
        Transform container = Selection.activeTransform;

        // DELETE
        List<GameObject> toRemoveList = new List<GameObject>();

        for (int i = 0; i < container.childCount; i++)
        {
            toRemoveList.Add(container.GetChild(i).gameObject);
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item.gameObject, true);
        }
        //

        MeshFilter meshFilter = originalMeshObject.GetComponent<MeshFilter>();

        Vector3[] vertices = meshFilter.sharedMesh.vertices;

        // REDUCE
        // vertices = ReduceVertices(vertices);

        // FIND POLYGONS (sliced by up plane)
        Dictionary<float, List<Vector3>> polygons = new Dictionary<float, List<Vector3>>();

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];

            if (polygons.Count == 0)
            {
                polygons.Add(vertex.y, new List<Vector3>() { vertex });
            }
            else
            {
                bool isFound = false;

                foreach (var polygon in polygons)
                {
                    if (Math.Abs(polygon.Key - vertex.y) < 0.01f)
                    {
                        polygon.Value.Add(vertex);

                        isFound = true;

                        break;
                    }
                }

                if (!isFound)
                {
                    polygons.Add(vertex.y, new List<Vector3>() { vertex });
                }
            }
        }
        //

        // FIND POLYGONS (sliced by Z plane)
        Dictionary<float, List<Vector3>> polygonsVericallySliced = new Dictionary<float, List<Vector3>>();

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];

            if (polygonsVericallySliced.Count == 0)
            {
                polygonsVericallySliced.Add(vertex.z, new List<Vector3>() { vertex });
            }
            else
            {
                bool isFound = false;

                foreach (var polygon in polygonsVericallySliced)
                {
                    if (Math.Abs(polygon.Key - vertex.z) < 0.01f)
                    {
                        polygon.Value.Add(vertex);

                        isFound = true;

                        break;
                    }
                }

                if (!isFound)
                {
                    polygonsVericallySliced.Add(vertex.z, new List<Vector3>() { vertex });
                }
            }
        }

        // FIND POLYGONS (sliced by X plane)
        Dictionary<float, List<Vector3>> polygonsXPlaneSliced = new Dictionary<float, List<Vector3>>();

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];

            if (polygonsXPlaneSliced.Count == 0)
            {
                polygonsXPlaneSliced.Add(vertex.x, new List<Vector3>() { vertex });
            }
            else
            {
                bool isFound = false;

                foreach (var polygon in polygonsXPlaneSliced)
                {
                    if (Math.Abs(polygon.Key - vertex.x) < 0.01f)
                    {
                        polygon.Value.Add(vertex);

                        isFound = true;

                        break;
                    }
                }

                if (!isFound)
                {
                    polygonsXPlaneSliced.Add(vertex.x, new List<Vector3>() { vertex });
                }
            }
        }

        // FILL POLYGONS
        Vector3 pieceSize = cubePrefab.GetComponent<Renderer>().bounds.size;

        int totalPiece = 0;

        spawnedPositions = new List<Vector3>();
        spawnedPieces = new List<GameObject>();

        FillPolygon(polygons, container, excludeAxis: "Y");
        FillPolygon(polygonsVericallySliced, container, excludeAxis: "Z");
        // FillPolygon(polygonsXPlaneSliced, container, excludeAxis: "X");
        //





        // Dictionary<Vector3, int> vertexMap = new Dictionary<Vector3, int>();
        // List<Vector3> simplifiedVertices = new List<Vector3>();

        // float mergeDistance = 0.1f;

        // // SIMPLIFY
        // for (int i = 0; i < vertices.Length; i++)
        // {
        //     Vector3 vertex = vertices[i];
        //     bool found = false;

        //     // Check if the vertex is close enough to an existing one
        //     foreach (var kvp in vertexMap)
        //     {
        //         if (Vector3.Distance(kvp.Key, vertex) < mergeDistance)
        //         {
        //             found = true;
        //             break;
        //         }
        //     }

        //     if (!found)
        //     {
        //         // Add the vertex if it doesn't match any existing ones within the merge distance
        //         simplifiedVertices.Add(vertex);
        //         vertexMap[vertex] = simplifiedVertices.Count - 1;
        //     }
        // }

        // for (int i = 0; i < vertices.Length; i++)
        // {
        //     bool isSkip = false;

        //     for (int j = 0; j < spawnedPieces.Count; j++)
        //     {
        //         if (Vector3.Distance(spawnedPieces[j].transform.position, vertices[i]) < 0.01f)
        //         {
        //             isSkip = true;

        //             break;
        //         }
        //     }

        //     if (isSkip)
        //     {
        //         continue;
        //     }

        //     GameObject piece = Instantiate(cubePrefab, container);

        //     piece.transform.position = vertices[i];

        //     spawnedPieces.Add(piece);
        // }

        // return;

        // COMBINE

        List<int> checkedList = new List<int>();

        Queue<Transform[]> toCombineMeshesQueue = new Queue<Transform[]>();

        Debug.Log("SAFERIO " + spawnedPieces.Count);

        spawnedPieces = spawnedPieces.OrderBy(item => Vector3.Distance(item.transform.position, spawnedPieces[0].transform.position)).ToList();

        for (int i = 0; i < spawnedPieces.Count; i++)
        {
            if (checkedList.Contains(spawnedPieces[i].transform.GetInstanceID()))
            {
                continue;
            }

            List<Transform> combinedList = new List<Transform>();

            combinedList.Add(spawnedPieces[i].transform);

            // find adjacent cubes
            for (int j = i + 1; j < spawnedPieces.Count; j++)
            {
                if (!checkedList.Contains(spawnedPieces[j].transform.GetInstanceID()))
                {
                    for (int k = 0; k < combinedList.Count; k++)
                    {
                        if (Vector3.Distance(combinedList[k].transform.position, spawnedPieces[j].transform.position) < 1.5f * pieceSize.x)
                        {
                            combinedList.Add(spawnedPieces[j].transform);

                            break;
                        }
                    }
                }
            }

            // Debug.Log("SAFERIO " + combinedList.OrderBy(item => Vector3.Distance(item.position, spawnedPieces[i].transform.position)).Take(10).ToList());

            List<Transform> chosenList = combinedList.OrderBy(item => Vector3.Distance(item.position, spawnedPieces[i].transform.position)).Take(16).ToList();

            // Debug.Log("SAFERIO " + chosenList.Count);

            toCombineMeshesQueue.Enqueue(chosenList.ToArray());

            // CombineMeshes(chosenList.ToArray());

            for (int j = 0; j < chosenList.Count; j++)
            {
                checkedList.Add(chosenList[j].GetInstanceID());

                // Debug.Log("SAFERIO CHECKED" + chosenList[j].GetInstanceID());
            }
        }

        while (toCombineMeshesQueue.Count > 0)
        {
            Transform[] toCombineMeshes = toCombineMeshesQueue.Dequeue();

            CombineMeshes(toCombineMeshes);
        }
    }

    private void FillPolygon(Dictionary<float, List<Vector3>> polygons, Transform pieceContainer, string excludeAxis)
    {
        Vector3 pieceSize = cubePrefab.GetComponent<Renderer>().bounds.size;

        int totalPiece = 0;

        foreach (var polygon in polygons)
        {
            Vector3[] polygonArray = polygon.Value.ToArray();

            FindPolygonBound(polygonArray, out Vector3 leftBottomCorner, out Vector3 rightTopCorner);

            float minX = leftBottomCorner.x;
            float maxX = rightTopCorner.x;
            float minY = leftBottomCorner.y;
            float maxY = rightTopCorner.y;
            float minZ = leftBottomCorner.z;
            float maxZ = rightTopCorner.z;

            float step = pieceSize.x;

            // if (polygonArray.Length > 0 && polygonArray.Length < 6)
            // {
            //     for (int i = 0; i < polygonArray.Length; i++)
            //     {
            //         GameObject piece = Instantiate(cubePrefab, pieceContainer);

            //         piece.name = $"Piece {totalPiece}";

            //         piece.transform.position = polygonArray[i];

            //         totalPiece++;
            //     }

            //     continue;
            // }

            if (excludeAxis == "Y")
            {
                for (float i = minZ; i < maxZ; i += step)
                {
                    for (float j = minX; j < maxX; j += step)
                    {
                        bool isInPolygon = IsPointInPolygon(new Vector3(j, 0, i), polygonArray, excludeAxis);

                        if (isInPolygon)
                        {
                            Vector3 spawnPosition = new Vector3(j, polygon.Key, i);

                            if (IsSpawnPositionExist(spawnPosition))
                            {
                                continue;
                            }

                            GameObject piece = Instantiate(cubePrefab, pieceContainer);

                            piece.name = $"Piece {totalPiece}";

                            piece.transform.position = spawnPosition;

                            spawnedPositions.Add(spawnPosition);
                            spawnedPieces.Add(piece);

                            totalPiece++;
                        }
                    }
                }
            }
            else if (excludeAxis == "Z")
            {
                for (float i = minY; i < maxY; i += step)
                {
                    for (float j = minX; j < maxX; j += step)
                    {
                        bool isInPolygon = IsPointInPolygon(new Vector3(j, i, 0), polygonArray, excludeAxis);

                        if (isInPolygon)
                        {
                            Vector3 spawnPosition = new Vector3(j, i, polygon.Key);

                            if (IsSpawnPositionExist(spawnPosition))
                            {
                                continue;
                            }

                            GameObject piece = Instantiate(cubePrefab, pieceContainer);

                            piece.name = $"Piece {totalPiece}";

                            piece.transform.position = spawnPosition;

                            spawnedPositions.Add(spawnPosition);
                            spawnedPieces.Add(piece);

                            totalPiece++;
                        }
                    }
                }
            }
            else if (excludeAxis == "X")
            {
                for (float i = minZ; i < maxZ; i += step)
                {
                    for (float j = minY; j < maxY; j += step)
                    {
                        bool isInPolygon = IsPointInPolygon(new Vector3(0, j, i), polygonArray, excludeAxis);

                        if (isInPolygon)
                        {
                            Vector3 spawnPosition = new Vector3(polygon.Key, j, i);

                            if (IsSpawnPositionExist(spawnPosition))
                            {
                                continue;
                            }

                            GameObject piece = Instantiate(cubePrefab, pieceContainer);

                            piece.name = $"Piece {totalPiece}";

                            piece.transform.position = spawnPosition;

                            spawnedPositions.Add(spawnPosition);
                            spawnedPieces.Add(piece);

                            totalPiece++;
                        }
                    }
                }
            }
        }
    }

    private Vector3[] ReduceVertices(Vector3[] vertices)
    {
        List<Vector3> reducedVertices = new List<Vector3>();

        for (int i = 0; i < vertices.Length; i++)
        {
            bool isSkip = false;

            for (int j = 0; j < reducedVertices.Count; j++)
            {
                if (Vector3.Distance(reducedVertices[j], vertices[i]) < 0.01f)
                {
                    isSkip = true;

                    break;
                }
            }

            if (isSkip)
            {
                continue;
            }

            reducedVertices.Add(vertices[i]);
        }

        return reducedVertices.ToArray();
    }

    private void CombineMeshes(Transform[] selected)
    {
        MeshFilter[] meshFilters = new MeshFilter[selected.Length];
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int j = 0; j < selected.Length; j++)
        {
            meshFilters[j] = selected[j].GetComponent<MeshFilter>();
        }

        int i = 0;

        // FIND AVERAGE POSITION (avoid weird position after combined)
        Vector3 averagePosition = new Vector3();

        for (int j = 0; j < selected.Length; j++)
        {
            averagePosition += selected[j].transform.position;
        }

        averagePosition /= selected.Length;

        for (int j = 0; j < selected.Length; j++)
        {
            selected[j].transform.position -= averagePosition;
        }
        //

        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix * meshFilters[i].transform.parent.localToWorldMatrix; ;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);

        Bounds combineMeshBound = new Bounds();

        combineMeshBound.center = Vector3.zero;
        combineMeshBound.size = mesh.bounds.size;

        mesh.bounds = combineMeshBound;

        GameObject combinedMesh = new GameObject();

        combinedMesh.transform.SetParent(selected[0].parent);

        // MATERIALS
        // MATERIALS
        ObjectPartMaterialContainer objectPartMaterialContainer = AssetDatabase.LoadAssetAtPath<ObjectPartMaterialContainer>(
            "Assets/Scriptable Objects/Material Container/Object Part Material Container.asset");

        List<Material> materialPool = objectPartMaterialContainer.Materials.ToList();

        combinedMesh.AddComponent<MeshFilter>().sharedMesh = mesh;
        combinedMesh.AddComponent<MeshRenderer>().material = materialPool[UnityEngine.Random.Range(0, materialPool.Count)];

        combinedMesh.transform.position = averagePosition;

        // DELETE
        List<GameObject> toRemoveList = new List<GameObject>();

        for (int j = 0; j < selected.Length; j++)
        {
            toRemoveList.Add(selected[j].gameObject);
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item, true);
        }
        //
    }

    #region UTIL
    private Vector3[] GetSquareCorners(Vector3 center, float size)
    {
        float halfSize = size / 2;
        return new Vector3[]
        {
            new Vector3(center.x - halfSize, center.y - halfSize),
            new Vector3(center.x + halfSize, center.y - halfSize),
            new Vector3(center.x + halfSize, center.y + halfSize),
            new Vector3(center.x - halfSize, center.y + halfSize)
        };
    }

    private void FindPolygonBound(Vector3[] polygon, out Vector3 leftBottomCorner, out Vector3 rightTopCorner)
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        for (int i = 0; i < polygon.Length; i++)
        {
            if (polygon[i].x < minX)
            {
                minX = polygon[i].x;
            }

            if (polygon[i].x > maxX)
            {
                maxX = polygon[i].x;
            }

            if (polygon[i].y < minY)
            {
                minY = polygon[i].y;
            }

            if (polygon[i].y > maxY)
            {
                maxY = polygon[i].y;
            }

            if (polygon[i].z < minZ)
            {
                minZ = polygon[i].z;
            }

            if (polygon[i].z > maxZ)
            {
                maxZ = polygon[i].z;
            }
        }

        leftBottomCorner = new Vector3(minX, minY, minZ);
        rightTopCorner = new Vector3(maxX, maxY, maxZ);
    }

    private bool IsPointInPolygon(Vector3 point, Vector3[] polygon, string excludeAxis)
    {
        int n = polygon.Length;
        bool inside = false;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if (excludeAxis == "Y")
            {
                if ((polygon[i].z > point.z) != (polygon[j].z > point.z) &&
                point.x < (polygon[j].x - polygon[i].x) * (point.z - polygon[i].z) / (polygon[j].z - polygon[i].z) + polygon[i].x)
                {
                    inside = !inside;
                }
            }
            else if (excludeAxis == "Z")
            {
                if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
                {
                    inside = !inside;
                }
            }
            else if (excludeAxis == "X")
            {
                if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                point.z < (polygon[j].z - polygon[i].z) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].z)
                {
                    inside = !inside;
                }
            }
        }

        return inside;
    }

    private bool IsSpawnPositionExist(Vector3 point)
    {
        for (int i = 0; i < spawnedPositions.Count; i++)
        {
            if (Vector3.Distance(spawnedPositions[i], point) < 0.08f)
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}
