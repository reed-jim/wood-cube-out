#if UNITY_EDITOR
using System.Diagnostics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SaferioSceneOpener : EditorWindow
{
    [MenuItem("Tools/Open Scene/Loading")]
    public static void OpenLoadingScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Loading.unity");
    }

    [MenuItem("Tools/Open Scene/Gameplay")]
    public static void OpenGameplayScene()
    {
        EditorSceneManager.OpenScene("Assets/Tower Shooter/Scenes/Gameplay.unity");
    }

    [MenuItem("Tools/Open Scene/Test")]
    public static void OpenTestScene()
    {
        EditorSceneManager.OpenScene("Assets/Tower Shooter/Scenes/Test.unity");
    }

    [MenuItem("Tools/Open Persistent Save Path")]
    public static void OpenPersistentSavePath()
    {
        string path = Application.persistentDataPath;

        Process.Start($"\"{path}\"");
    }
}
#endif
