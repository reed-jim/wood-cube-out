#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class GamePersistentVariableWindow : EditorWindow
{
    [MenuItem("Tools/Game Persistent Variables")]
    public static void ShowWindow()
    {
        GetWindow<GamePersistentVariableWindow>("Persistent Variables");
    }

    private void OnEnable()
    {
        // Refresh the window continuously
        EditorApplication.update += Repaint;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Repaint;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Game Persistent Variables", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Chicken Flow", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.IntField("Bucket Order", GamePersistentVariable.bucketOrder);
            EditorGUILayout.IntField("Eating Chicken", GamePersistentVariable.eatingChickens.Count);
            EditorGUILayout.IntField("Moving Chicken", GamePersistentVariable.numMovingChicken);
            EditorGUILayout.IntField("numConsecutiveStuckBucket", GamePersistentVariable.numConsecutiveStuckBucket);
            EditorGUILayout.IntField("levelBoosterUseTime", GamePersistentVariable.levelBoosterUseTime);
            EditorGUILayout.IntField("levelRetryTime", GamePersistentVariable.levelRetryTime);
        }

        EditorGUILayout.Space();

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Values are most useful while the game is running.",
                MessageType.Info);
        }

        if (GUILayout.Button("Refresh"))
        {
            Repaint();
        }
    }
}
#endif
