#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public class ThemeConfigJsonEditor : EditorWindow
{
    private ThemeConfig targetTheme;
    private string json = "";

    private Vector2 scrollPosition;

    [MenuItem("Tools/Theme Config JSON")]
    public static void ShowWindow()
    {
        GetWindow<ThemeConfigJsonEditor>("Theme Config JSON");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "ThemeConfig JSON Importer",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space(5);

        targetTheme = (ThemeConfig)EditorGUILayout.ObjectField(
            "Theme Config",
            targetTheme,
            typeof(ThemeConfig),
            false
        );

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "JSON",
            EditorStyles.boldLabel
        );

        scrollPosition = EditorGUILayout.BeginScrollView(
            scrollPosition,
            GUILayout.ExpandHeight(true)
        );

        json = EditorGUILayout.TextArea(
            json,
            GUILayout.ExpandHeight(true)
        );

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Load From File"))
        {
            LoadFromFile();
        }

        if (GUILayout.Button("Export Current"))
        {
            ExportCurrent();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        GUI.enabled = targetTheme != null && !string.IsNullOrWhiteSpace(json);

        if (GUILayout.Button("APPLY JSON", GUILayout.Height(35)))
        {
            ApplyJson();
        }

        GUI.enabled = true;
    }

    private void ApplyJson()
    {
        if (targetTheme == null)
        {
            EditorUtility.DisplayDialog(
                "No ThemeConfig",
                "Please select a ThemeConfig asset.",
                "OK"
            );

            return;
        }

        try
        {
            Undo.RecordObject(
                targetTheme,
                "Apply ThemeConfig JSON"
            );

            JsonUtility.FromJsonOverwrite(
                json,
                targetTheme
            );

            EditorUtility.SetDirty(targetTheme);
            AssetDatabase.SaveAssets();

            Debug.Log(
                $"ThemeConfig JSON applied: {targetTheme.name}"
            );

            EditorUtility.DisplayDialog(
                "Success",
                "ThemeConfig has been updated.",
                "OK"
            );
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);

            EditorUtility.DisplayDialog(
                "Invalid JSON",
                exception.Message,
                "OK"
            );
        }
    }

    private void LoadFromFile()
    {
        string path = EditorUtility.OpenFilePanel(
            "Load Theme JSON",
            Application.dataPath,
            "json"
        );

        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            json = System.IO.File.ReadAllText(path);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);

            EditorUtility.DisplayDialog(
                "Error",
                "Could not read JSON file.",
                "OK"
            );
        }
    }

    private void ExportCurrent()
    {
        if (targetTheme == null)
        {
            EditorUtility.DisplayDialog(
                "No ThemeConfig",
                "Please select a ThemeConfig asset.",
                "OK"
            );

            return;
        }

        string path = EditorUtility.SaveFilePanel(
            "Export Theme JSON",
            Application.dataPath,
            targetTheme.name,
            "json"
        );

        if (string.IsNullOrEmpty(path))
            return;

        string exportedJson = JsonUtility.ToJson(
            targetTheme,
            true
        );

        System.IO.File.WriteAllText(
            path,
            exportedJson
        );

        Debug.Log(
            $"ThemeConfig exported to: {path}"
        );
    }
}
#endif
