using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugManager : Singleton<DebugManager>
{
    public UnlockAugmentUI unlockAugmentUI;
}

[CustomEditor(typeof(DebugManager))]
public class DebugManagerEditor : Editor
{
    private bool showTestDataSection = true;
    private string customLogMessage = "";

    public override void OnInspectorGUI()
    {
        // Draw the default inspector properties (if any exist in DebugManager)
        DrawDefaultInspector();

        DebugManager debugManager = (DebugManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);

        // --- Section 1: Quick Action Buttons ---
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("General Actions", EditorStyles.miniBoldLabel);

            if (GUILayout.Button("Unlock All Features", GUILayout.Height(25)))
            {
                UnlockAllFeatures(debugManager);
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // --- Section 2: Collapsible Test Data Section ---
        showTestDataSection = EditorGUILayout.Foldout(showTestDataSection, "Test Data Controls", true);
        if (showTestDataSection)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                // Input field example
                customLogMessage = EditorGUILayout.TextField("Custom Message", customLogMessage);

                // Horizontal row of buttons
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Spawn Item"))
                    {
                        // TODO: Add logic
                    }

                    if (GUILayout.Button("Kill All Enemies"))
                    {
                        // TODO: Add logic
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);

        // --- Section 3: Dangerous Actions (Red Styled Button) ---
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Danger Zone", EditorStyles.miniBoldLabel);

            // Temporarily change GUI color for a warning button
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);

            if (GUILayout.Button("Reset Entire Game State", GUILayout.Height(30)))
            {
                // TODO: Add destructive action logic
            }

            GUI.backgroundColor = originalColor; // Restore original color
        }
        EditorGUILayout.EndVertical();
    }

    private void UnlockAllFeatures(DebugManager debugManager)
    {
        FeatureUnlockManager.Ins.UnlockAllFeature();

        debugManager.unlockAugmentUI.ChangeDataToUnlockState();
    }
}
