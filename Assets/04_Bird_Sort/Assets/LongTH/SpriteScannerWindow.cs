#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SpriteScannerWindow : EditorWindow
{
    [Serializable]
    public class SpriteInfo
    {
        // Tên sprite
        public string spriteName;

        // Scene hoặc Prefab đang dùng
        public string usedIn;

        // Path sprite
        public string assetPath;
    }

    private List<SpriteInfo> results =
        new List<SpriteInfo>();

    private Vector2 scroll;

    [MenuItem("Tools/Sprite Scanner")]
    public static void ShowWindow()
    {
        var wnd =
            GetWindow<SpriteScannerWindow>(
                "Sprite Scanner");

        wnd.minSize =
            new Vector2(800, 500);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        if (GUILayout.Button(
            "SCAN ALL SCENES + PREFABS",
            GUILayout.Height(40)))
        {
            ScanAll();
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            $"Results ({results.Count})",
            EditorStyles.boldLabel);

        EditorGUILayout.Space();

        scroll =
            EditorGUILayout.BeginScrollView(scroll);

        foreach (var info in results)
        {
            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.LabelField(
                info.spriteName,
                GUILayout.Width(200));

            EditorGUILayout.LabelField(
                info.usedIn,
                GUILayout.Width(250));

            EditorGUILayout.LabelField(
                info.assetPath);

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button(
            "EXPORT CSV",
            GUILayout.Height(30)))
        {
            ExportCSV();
        }

        if (GUILayout.Button(
            "EXPORT JSON",
            GUILayout.Height(30)))
        {
            ExportJSON();
        }
    }

    // =========================================================
    // SCAN ALL
    // =========================================================

    private void ScanAll()
    {
        results.Clear();

        // =====================================================
        // Scan Scenes
        // =====================================================

        foreach (var scene
            in EditorBuildSettings.scenes)
        {
            if (!scene.enabled)
                continue;

            ScanScene(scene.path);
        }

        // =====================================================
        // Scan Prefabs
        // =====================================================

        string[] prefabGuids =
            AssetDatabase.FindAssets(
                "t:Prefab",
                new[] { "Assets" });

        foreach (string guid in prefabGuids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            ScanPrefab(path);
        }

        // =====================================================
        // Remove Duplicate
        // =====================================================

        results = results
            .GroupBy(x =>
                x.spriteName +
                x.usedIn +
                x.assetPath)
            .Select(g => g.First())
            .ToList();

        Debug.Log(
            $"SCAN DONE => {results.Count} sprites");
    }

    // =========================================================
    // SCENE
    // =========================================================

    private void ScanScene(string scenePath)
    {
        Scene scene =
            EditorSceneManager.OpenScene(
                scenePath,
                OpenSceneMode.Single);

        string sceneName =
            Path.GetFileNameWithoutExtension(
                scenePath);

        foreach (GameObject root
            in scene.GetRootGameObjects())
        {
            ScanGameObject(root, sceneName);
        }
    }

    // =========================================================
    // PREFAB
    // =========================================================

    private void ScanPrefab(string prefabPath)
    {
        GameObject root =
            PrefabUtility.LoadPrefabContents(
                prefabPath);

        string prefabName =
            Path.GetFileNameWithoutExtension(
                prefabPath);

        ScanGameObject(root, prefabName);

        PrefabUtility.UnloadPrefabContents(root);
    }

    // =========================================================
    // GAMEOBJECT
    // =========================================================

    private void ScanGameObject(
        GameObject go,
        string usedIn)
    {
        Component[] components =
            go.GetComponents<Component>();

        foreach (var comp in components)
        {
            if (comp == null)
                continue;

            // =============================================
            // UI Image
            // =============================================

            Image img = comp as Image;

            if (img != null &&
                img.sprite != null)
            {
                AddResult(
                    img.sprite,
                    usedIn);
            }

            // =============================================
            // SpriteRenderer
            // =============================================

            SpriteRenderer sr =
                comp as SpriteRenderer;

            if (sr != null &&
                sr.sprite != null)
            {
                AddResult(
                    sr.sprite,
                    usedIn);
            }

            // =============================================
            // Serialized Fields
            // =============================================

            SerializedObject so =
                new SerializedObject(comp);

            SerializedProperty sp =
                so.GetIterator();

            while (sp.NextVisible(true))
            {
                if (sp.propertyType ==
                    SerializedPropertyType.ObjectReference)
                {
                    UnityEngine.Object obj =
                        sp.objectReferenceValue;

                    if (obj is Sprite spr)
                    {
                        AddResult(
                            spr,
                            usedIn);
                    }

                    else if (obj is Texture2D tex)
                    {
                        AddResultTex(
                            tex,
                            usedIn);
                    }
                }
            }
        }

        // =============================================
        // Children
        // =============================================

        foreach (Transform child
            in go.transform)
        {
            ScanGameObject(
                child.gameObject,
                usedIn);
        }
    }

    // =========================================================
    // ADD SPRITE
    // =========================================================

    private void AddResult(
        Sprite spr,
        string usedIn)
    {
        if (spr == null)
            return;

        string assetPath =
            AssetDatabase.GetAssetPath(spr);

        if (string.IsNullOrEmpty(assetPath))
            return;

        SpriteInfo info =
            new SpriteInfo();

        info.spriteName = spr.name;
        info.usedIn = usedIn;
        info.assetPath = assetPath;

        results.Add(info);
    }

    // =========================================================
    // ADD TEXTURE
    // =========================================================

    private void AddResultTex(
        Texture2D tex,
        string usedIn)
    {
        if (tex == null)
            return;

        string assetPath =
            AssetDatabase.GetAssetPath(tex);

        if (string.IsNullOrEmpty(assetPath))
            return;

        SpriteInfo info =
            new SpriteInfo();

        info.spriteName = tex.name;
        info.usedIn = usedIn;
        info.assetPath = assetPath;

        results.Add(info);
    }

    // =========================================================
    // EXPORT CSV
    // =========================================================

    private void ExportCSV()
    {
        string path =
            EditorUtility.SaveFilePanel(
                "Save CSV",
                "",
                "sprite_report.csv",
                "csv");

        if (string.IsNullOrEmpty(path))
            return;

        using (StreamWriter writer =
            new StreamWriter(
                path,
                false,
                Encoding.UTF8))
        {
            writer.WriteLine(
                "Sprite,Used In,Path");

            foreach (var info in results)
            {
                writer.WriteLine(
                    $"{info.spriteName}," +
                    $"{info.usedIn}," +
                    $"{info.assetPath}");
            }
        }

        Debug.Log(
            "EXPORT CSV DONE => " + path);
    }

    // =========================================================
    // EXPORT JSON
    // =========================================================

    private void ExportJSON()
    {
        string path =
            EditorUtility.SaveFilePanel(
                "Save JSON",
                "",
                "sprite_report.json",
                "json");

        if (string.IsNullOrEmpty(path))
            return;

        SpriteInfoList wrapper =
            new SpriteInfoList();

        wrapper.items = results;

        File.WriteAllText(
            path,
            JsonUtility.ToJson(
                wrapper,
                true),
            Encoding.UTF8);

        Debug.Log(
            "EXPORT JSON DONE => " + path);
    }

    // =========================================================
    // JSON WRAPPER
    // =========================================================

    [Serializable]
    public class SpriteInfoList
    {
        public List<SpriteInfo> items;
    }
}
#endif