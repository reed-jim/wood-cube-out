using UnityEngine;
using UnityEditor;

public class ChainGenerator : EditorWindow
{
    GameObject linkPrefab;
    int chainLength = 5;
    Vector2 startPosition = Vector2.zero;
    float linkSpacing = 0.5f;

    [MenuItem("Tools/2D Chain Generator")]
    public static void ShowWindow()
    {
        GetWindow<ChainGenerator>("2D Chain Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Chain Settings", EditorStyles.boldLabel);

        linkPrefab = (GameObject)EditorGUILayout.ObjectField("Link Prefab", linkPrefab, typeof(GameObject), false);
        chainLength = EditorGUILayout.IntField("Chain Length", chainLength);
        linkSpacing = EditorGUILayout.FloatField("Link Spacing", linkSpacing);
        startPosition = EditorGUILayout.Vector2Field("Start Position", startPosition);

        if (GUILayout.Button("Generate Chain"))
        {
            if (linkPrefab == null)
            {
                Debug.LogError("Please assign a link prefab.");
                return;
            }

            GenerateChain();
        }
    }

    void GenerateChain()
    {
        GameObject previousLink = null;

        for (int i = 0; i < chainLength; i++)
        {
            Vector2 position = startPosition + Vector2.down * linkSpacing * i;
            GameObject newLink = (GameObject)PrefabUtility.InstantiatePrefab(linkPrefab);
            newLink.transform.position = position;
            newLink.name = $"ChainLink_{i}";

            HingeJoint2D joint = newLink.GetComponent<HingeJoint2D>();

            if (joint != null && previousLink != null)
            {
                joint.connectedBody = previousLink.GetComponent<Rigidbody2D>();
                joint.connectedAnchor = new Vector2(0, linkSpacing);
            }

            previousLink = newLink;
        }

        Debug.Log($"Generated chain with {chainLength} links.");
    }
}
