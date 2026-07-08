using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AutoFillLevelItemsWindow : EditorWindow
{
    private List<GameObject> levelPrefabs = new List<GameObject>();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Auto Fill Level Items")]
    public static void ShowWindow()
    {
        GetWindow<AutoFillLevelItemsWindow>("Auto Fill Level Items");
    }

    private void OnGUI()
    {
        GUILayout.Label("Drag and drop Level prefabs here:", EditorStyles.boldLabel);

        // Drag and drop area
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag prefabs here");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject go)
                        {
                            if (go.GetComponent<LevelItem>() != null)
                            {
                                if (!levelPrefabs.Contains(go))
                                {
                                    levelPrefabs.Add(go);
                                }
                            }
                        }
                    }
                }
                Event.current.Use();
                break;
        }

        // List display
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < levelPrefabs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            levelPrefabs[i] = (GameObject)EditorGUILayout.ObjectField($"Level {i + 1}", levelPrefabs[i], typeof(GameObject), false);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                levelPrefabs.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear All", GUILayout.Height(30)))
        {
            levelPrefabs.Clear();
        }

        if (GUILayout.Button("Add Selected from Project", GUILayout.Height(30)))
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null && go.GetComponent<LevelItem>() != null)
                {
                    if (!levelPrefabs.Contains(go))
                    {
                        levelPrefabs.Add(go);
                    }
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (GUILayout.Button("Auto Fill All Themes for All Prefabs", GUILayout.Height(40)))
        {
            AutoFillAll();
        }
    }

    private void AutoFillAll()
    {
        int updatedCount = 0;
        int totalCount = levelPrefabs.Count;

        for (int i = 0; i < totalCount; i++)
        {
            GameObject prefab = levelPrefabs[i];
            if (prefab == null) continue;

            string assetPath = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning($"Skipped {prefab.name} because it is not an asset.");
                continue;
            }

            LevelItem levelItem = prefab.GetComponent<LevelItem>();
            if (levelItem != null)
            {
                bool modified = levelItem.AutoFillAllThemes();
                if (modified)
                {
                    EditorUtility.SetDirty(levelItem);
                    PrefabUtility.SavePrefabAsset(prefab);
                    updatedCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Auto Fill Complete", $"Successfully auto filled {updatedCount} of {totalCount} prefabs.", "OK");
    }
}
