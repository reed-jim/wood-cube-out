using System.Collections.Generic;
using System.Threading.Tasks;
using EzySlice;
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
            AnalyseMesh();
        }
    }

    private async Task AnalyseMesh()
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

        for (int i = 0; i < vertices.Length; i++)
        {
            GameObject piece = Instantiate(cubePrefab, container);

            piece.transform.position = vertices[i];
        }
    }
}
