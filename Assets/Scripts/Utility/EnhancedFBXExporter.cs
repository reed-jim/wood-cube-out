using UnityEditor;
using UnityEngine;
using UnityEditor.Formats.Fbx.Exporter;
using System.IO;

public class EnhancedFPXExporter : EditorWindow
{
    private string exportFolderPath = "";

    [MenuItem("Tools/EnhancedFPXExporter")]
    public static void ShowWindow()
    {
        EnhancedFPXExporter window = GetWindow<EnhancedFPXExporter>("Export FBX");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Export Selected Transforms' Children to FBX", EditorStyles.boldLabel);

        // Folder selection button
        if (GUILayout.Button("Select Export Folder"))
        {
            exportFolderPath = EditorUtility.OpenFolderPanel("Select Folder to Export FBX Files", Application.dataPath, "");
        }

        if (!string.IsNullOrEmpty(exportFolderPath))
        {
            GUILayout.Label("Export Folder: " + exportFolderPath);
        }

        // Export Button
        if (GUILayout.Button("Export FBX"))
        {
            ExportChildrenToFBX();
        }
    }

    private void ExportChildrenToFBX()
    {
        if (string.IsNullOrEmpty(exportFolderPath))
        {
            EditorUtility.DisplayDialog("Error", "Please select a folder to export the FBX files.", "OK");
            return;
        }

        Transform[] selectedTransforms = Selection.transforms;
        if (selectedTransforms.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No transforms selected!", "OK");
            return;
        }

        foreach (var root in selectedTransforms)
        {
            foreach (Transform child in root)
            {
                ExportChildToFBX(child);
            }
        }

        Debug.Log("Export Complete.");
    }

    private void ExportChildToFBX(Transform child)
    {
        string filePath = Path.Combine(exportFolderPath, $"{child.name}.fbx");

        // Export the child GameObject to an FBX file
        ModelExporter.ExportObject(filePath, child.gameObject);

        Debug.Log($"Exported {child.name} to {filePath}");
    }
}
