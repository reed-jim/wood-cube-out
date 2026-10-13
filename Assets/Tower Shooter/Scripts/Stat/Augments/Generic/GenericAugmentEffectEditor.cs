// using UnityEditor;
// using UnityEngine;
// using System.Reflection;

// [CustomEditor(typeof(GenericAugmentEffect))]
// public class GenericAugmentEffectEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         // Draw the default inspector properties
//         DrawDefaultInspector();

//         GenericAugmentEffect effect = (GenericAugmentEffect)target;

//         EditorGUILayout.Space(10);

//         if (GUILayout.Button("Test Generate Auto Description", GUILayout.Height(30)))
//         {
//             string result = effect.GenerateAutoDescription();
//             Debug.Log($"<b>[Auto Description Result]</b> {result}", effect);
//         }
//     }
// }