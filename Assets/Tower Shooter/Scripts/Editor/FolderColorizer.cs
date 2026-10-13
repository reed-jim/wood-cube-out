using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public class FolderColorizer
{
    static FolderColorizer()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
    }

    private static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);

        if (AssetDatabase.IsValidFolder(path))
        {
            if (path.EndsWith("Tower Shooter"))
            {
                if (Event.current.type == EventType.Repaint)
                {
                    Color customColor = new Color(1f, 0.6f, 0.2f, 1f); // Desired color
                    string folderName = Path.GetFileName(path);

                    // Save original GUI color so we can restore it later
                    Color originalColor = GUI.color;
                    GUI.color = customColor;

                    if (selectionRect.height > 20)
                    {
                        // --- GRID VIEW ---
                        Rect iconRect = new Rect(selectionRect.x, selectionRect.y, selectionRect.width, selectionRect.width);
                        Rect labelRect = new Rect(selectionRect.x, selectionRect.y + selectionRect.width, selectionRect.width, selectionRect.height - selectionRect.width);

                        // Reset GUI color for the text so it doesn't get double-tinted weirdly if desired, 
                        // or leave GUI.color active if you want the text colored by it too.
                        GUI.color = originalColor;

                        // Draw custom colored text
                        GUIStyle style = new GUIStyle(EditorStyles.whiteLabel);
                        style.normal.textColor = customColor;
                        style.alignment = TextAnchor.UpperCenter;
                        GUI.Label(labelRect, folderName, style);
                    }
                    else
                    {
                        // --- LIST VIEW ---
                        Rect labelRect = new Rect(selectionRect.x + 18, selectionRect.y, selectionRect.width - 18, selectionRect.height);

                        // Restore GUI color before drawing text label
                        GUI.color = originalColor;

                        // Draw custom colored text
                        GUIStyle style = new GUIStyle(EditorStyles.label);
                        style.normal.textColor = customColor;
                        GUI.Label(labelRect, folderName, style);
                    }

                    // Ensure GUI color is always restored
                    GUI.color = originalColor;
                }
            }
        }
    }
}