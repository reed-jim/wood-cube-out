#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class IdleGameTool : EditorWindow
{
    private float scaleFactor = 1.0f;
    private bool scalePosition = true;
    private bool scaleSizeDelta = true;

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

        GUILayout.Label("Scale RectTransform Hierarchy", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        scaleFactor = EditorGUILayout.FloatField("Scale Factor", scaleFactor);
        scalePosition = EditorGUILayout.Toggle("Scale Anchored Position", scalePosition);
        scaleSizeDelta = EditorGUILayout.Toggle("Scale Size Delta", scaleSizeDelta);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Scale Selected & Children"))
        {
            ScaleSelectedHierarchy();
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

    private void ScaleSelectedHierarchy()
    {
        if (Selection.activeTransform == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select a GameObject in the Hierarchy.", "OK");
            return;
        }

        if (scaleFactor <= 0)
        {
            EditorUtility.DisplayDialog("Invalid Scale", "Scale factor should be greater than 0.", "OK");
            return;
        }

        // Retrieve all RectTransform components including those on child GameObjects
        RectTransform[] rectTransforms = Selection.activeTransform.GetComponentsInChildren<RectTransform>(true);

        if (rectTransforms.Length == 0)
        {
            EditorUtility.DisplayDialog("No RectTransforms Found", "The selected object and its children do not contain any RectTransform components.", "OK");
            return;
        }

        // Register action in Undo history so changes can be reverted with Ctrl+Z / Cmd+Z
        Undo.RegisterCompleteObjectUndo(rectTransforms, "Scale RectTransforms");

        int modifiedCount = 0;
        foreach (RectTransform rect in rectTransforms)
        {
            if (scalePosition)
            {
                rect.anchoredPosition *= scaleFactor;
            }

            if (scaleSizeDelta)
            {
                rect.sizeDelta *= scaleFactor;
            }

            // Mark object dirty so Unity detects the change for saving
            EditorUtility.SetDirty(rect);
            modifiedCount++;
        }

        Debug.Log($"Successfully scaled {modifiedCount} RectTransform(s) by factor of {scaleFactor}.");
    }
}
#endif
