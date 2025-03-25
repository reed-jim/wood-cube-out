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
    public float cubeSize = 0.1f;
    public float clusterDistance = 0.1f;

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

        MeshFilter meshFilter = originalMeshObject.GetComponent<MeshFilter>();

        Vector3[] vertices = meshFilter.sharedMesh.vertices;
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

        List<GameObject> spawnedPieces = new List<GameObject>();

        for (int i = 0; i < vertices.Length; i++)
        {
            bool isSkip = false;

            for (int j = 0; j < spawnedPieces.Count; j++)
            {
                if (Vector3.Distance(spawnedPieces[j].transform.position, vertices[i]) < 0.01f)
                {
                    isSkip = true;

                    break;
                }
            }

            if (isSkip)
            {
                continue;
            }

            GameObject piece = Instantiate(cubePrefab, container);

            piece.transform.position = vertices[i];

            spawnedPieces.Add(piece);
        }

        return;
        
        // COMBINE

        List<int> checkedList = new List<int>();

        Queue<Transform[]> toCombineMeshesQueue = new Queue<Transform[]>();

        Debug.Log("SAFERIO " + spawnedPieces.Count);

        for (int i = 0; i < spawnedPieces.Count; i++)
        {
            if (checkedList.Contains(spawnedPieces[i].transform.GetInstanceID()))
            {
                continue;
            }

            List<Transform> combinedList = new List<Transform>();

            combinedList.Add(spawnedPieces[i].transform);

            for (int j = i + 1; j < spawnedPieces.Count; j++)
            {
                if (!checkedList.Contains(spawnedPieces[j].transform.GetInstanceID()))
                {
                    combinedList.Add(spawnedPieces[j].transform);
                }
            }

            // Debug.Log("SAFERIO " + combinedList.OrderBy(item => Vector3.Distance(item.position, spawnedPieces[i].transform.position)).Take(10).ToList());

            List<Transform> chosenList = combinedList.OrderBy(item => Vector3.Distance(item.position, spawnedPieces[i].transform.position)).Take(10).ToList();

            // Debug.Log("SAFERIO " + chosenList.Count);

            toCombineMeshesQueue.Enqueue(chosenList.ToArray());

            // CombineMeshes(chosenList.ToArray());

            for (int j = 0; j < chosenList.Count; j++)
            {
                checkedList.Add(chosenList[j].GetInstanceID());

                // Debug.Log("SAFERIO CHECKED" + chosenList[j].GetInstanceID());
            }
        }

        Debug.Log("SAFERIO " + toCombineMeshesQueue.Count);

        while (toCombineMeshesQueue.Count > 0)
        {
            Transform[] toCombineMeshes = toCombineMeshesQueue.Dequeue();

            CombineMeshes(toCombineMeshes);
        }
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

        combinedMesh.AddComponent<MeshFilter>().sharedMesh = mesh;
        combinedMesh.AddComponent<MeshRenderer>();

        combinedMesh.transform.position = averagePosition;

        Debug.Log("SAFERIO " + combinedMesh);

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
}
