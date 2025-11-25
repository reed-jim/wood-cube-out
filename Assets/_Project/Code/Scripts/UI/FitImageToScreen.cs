using System.Collections;
using System.Reflection;
using NamPhuThuy.UI;

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace NamPhuThuy.AnimateWithScripts
{
    [RequireComponent(typeof(Image))]

    public class FitImageToScreen : MonoBehaviour
    {
        [SerializeField] private RectTransform canvas;
        [SerializeField] private bool autoActiveOnEnable = true;
        public bool AutoActiveOnEnable { get => autoActiveOnEnable; set => autoActiveOnEnable = value; }

        // void Start()
        // {
        //     FitImage();
        // }

        void OnEnable()
        {
            if (!autoActiveOnEnable) return;
            StartCoroutine(DelayFitToScreen());
        }

        private IEnumerator DelayFitToScreen()
        {
            yield return new WaitForEndOfFrame();

            FitImage();
        }

        [ContextMenu("FitImage")]
        void FitImage()
        {
            DebugLogger.Log();
            Image image = GetComponent<Image>();
            if (image != null)
            {
                image.FitImageToRectTransform(canvas);
            }
        }
    }
    
    #if UNITY_EDITOR
    
    [CustomEditor(typeof(FitImageToScreen)), CanEditMultipleObjects]
    public class FitImageToScreenEditor : Editor
    {
        private SerializedProperty _canvasProp;
        private MethodInfo _fitMethod;

        private void OnEnable()
        {
            _canvasProp = serializedObject.FindProperty("canvas");
            _fitMethod = typeof(FitImageToScreen).GetMethod("FitImage", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_canvasProp);

            if (_canvasProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Assign a Canvas RectTransform.", MessageType.Warning);
            }

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(_fitMethod == null))
            {
                if (GUILayout.Button("Fit Image"))
                {
                    ApplyFitToTargets();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ApplyFitToTargets()
        {
            foreach (Object obj in targets)
            {
                var comp = obj as FitImageToScreen;
                if (comp == null) continue;

                var img = comp.GetComponent<Image>();
                if (img != null)
                {
                    Undo.RecordObject(img, "Fit Image");
                    if (img.transform is RectTransform rt)
                        Undo.RecordObject(rt, "Fit Image");
                }
                Undo.RecordObject(comp, "Fit Image");

                _fitMethod?.Invoke(comp, null);

                if (!Application.isPlaying)
                {
                    if (img != null)
                    {
                        EditorUtility.SetDirty(img);
                        if (img.transform is RectTransform rt)
                            EditorUtility.SetDirty(rt);
                    }
                    EditorUtility.SetDirty(comp);
                    EditorSceneManager.MarkSceneDirty(comp.gameObject.scene);
                }
            }
        }
    }
    #endif
}