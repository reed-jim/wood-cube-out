#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class IdleGameTool : EditorWindow
{
    [MenuItem("Saferio/Tools/IdleGameTool")]
    public static void ShowWindow()
    {
        GetWindow<IdleGameTool>("IdleGameTool");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("AutoAssignIdForAugment"))
        {
            AutoAssignIdForAugment();
        }
    }

    private void AutoAssignIdForAugment()
    {
        const string path = "Assets/Tower Shooter/Data/Augment/Augment Container.asset";

        AugmentContainer augmentContainer = AssetDatabase.LoadAssetAtPath<AugmentContainer>(path);

        for (int i = 0; i < augmentContainer.augments.Count; i++)
        {
            Augment augment = augmentContainer.augments[i];

            augment.id = i;

            EditorUtility.SetDirty(augment);
        }

        EditorUtility.SetDirty(augmentContainer);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif
