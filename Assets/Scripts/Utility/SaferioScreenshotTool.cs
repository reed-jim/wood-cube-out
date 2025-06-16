#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class SaferioScreenshotTool : EditorWindow
{
    private string directoryPath = "C:/Unity Projects/iOS IAP Review/Wood Jam Color Block";
    private string fileName = "Screenshot";

    // Add menu item to open the window
    [MenuItem("Tools/SaferioScreenshotTool")]
    public static void ShowWindow()
    {
        // Show the window
        EditorWindow.GetWindow(typeof(SaferioScreenshotTool));
    }

    private void OnGUI()
    {
        GUILayout.Label("Screenshot Tool", EditorStyles.boldLabel);

        directoryPath = EditorGUILayout.TextField("Directory Path", directoryPath);
        fileName = EditorGUILayout.TextField("File Name", fileName);

        if (GUILayout.Button("Take Screenshot"))
        {
            TakeScreenshot();
        }
    }

    private void TakeScreenshot()
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, fileName + ".png");

        ScreenCapture.CaptureScreenshot(filePath);

        EditorUtility.DisplayDialog("Screenshot Taken", "Screenshot saved to: " + filePath, "OK");
    }
}
#endif
