using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AnimationCurveJsonTool : EditorWindow
{
    [Serializable]
    private class CurveJson
    {
        public List<KeyframeJson> keys = new List<KeyframeJson>();
        public WrapMode preWrapMode;
        public WrapMode postWrapMode;
    }

    [Serializable]
    private class KeyframeJson
    {
        public float time;
        public float value;
        public float inTangent;
        public float outTangent;
        public int tangentMode;
        public float inWeight;
        public float outWeight;
        public int weightedMode;

        public KeyframeJson(Keyframe key)
        {
            time = key.time;
            value = key.value;
            inTangent = key.inTangent;
            outTangent = key.outTangent;
            tangentMode = key.tangentMode;
            inWeight = key.inWeight;
            outWeight = key.outWeight;
            weightedMode = (int)key.weightedMode;
        }

        public Keyframe ToKeyframe()
        {
            var key = new Keyframe(
                time,
                value,
                inTangent,
                outTangent,
                inWeight,
                outWeight
            );

            key.tangentMode = tangentMode;
            key.weightedMode = (WeightedMode)weightedMode;

            return key;
        }
    }

    private UnityEngine.Object target;
    private SerializedObject serializedObject;
    private SerializedProperty curveProperty;

    private string json = "";

    [MenuItem("Tools/Animation Curve JSON")]
    public static void ShowWindow()
    {
        var window = GetWindow<AnimationCurveJsonTool>(
            "Animation Curve JSON"
        );

        window.minSize = new Vector2(500, 400);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Animation Curve JSON", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        var newTarget = EditorGUILayout.ObjectField(
            "Target",
            target,
            typeof(MonoBehaviour),
            true
        );

        if (newTarget != target)
        {
            SetTarget(newTarget);
        }

        if (target == null)
        {
            EditorGUILayout.HelpBox(
                "Select a GameObject/component containing an AnimationCurve.",
                MessageType.Info
            );

            return;
        }

        if (curveProperty == null)
        {
            EditorGUILayout.HelpBox(
                "No AnimationCurve field was found.",
                MessageType.Error
            );

            return;
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Detected Curve",
            curveProperty.displayName
        );

        EditorGUILayout.PropertyField(curveProperty);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        if (GUILayout.Button("Read Curve → JSON", GUILayout.Height(30)))
        {
            ReadCurve();
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("JSON");

        json = EditorGUILayout.TextArea(
            json,
            GUILayout.MinHeight(180)
        );

        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Write JSON → Curve", GUILayout.Height(30)))
            {
                WriteCurve();
            }

            if (GUILayout.Button("Copy JSON"))
            {
                EditorGUIUtility.systemCopyBuffer = json;
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Save JSON File"))
        {
            SaveJsonFile();
        }

        if (GUILayout.Button("Load JSON File"))
        {
            LoadJsonFile();
        }
    }

    private void SetTarget(UnityEngine.Object newTarget)
    {
        target = newTarget;
        curveProperty = null;
        serializedObject = null;

        if (target == null)
            return;

        serializedObject = new SerializedObject(target);

        curveProperty = FindAnimationCurve(serializedObject);

        if (curveProperty == null)
        {
            Debug.LogWarning(
                $"No AnimationCurve found on {target.GetType().Name}"
            );
        }
    }

    private static SerializedProperty FindAnimationCurve(
        SerializedObject serializedObject)
    {
        var iterator = serializedObject.GetIterator();

        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (iterator.propertyType == SerializedPropertyType.AnimationCurve)
            {
                return serializedObject.FindProperty(iterator.propertyPath);
            }
        }

        return null;
    }

    private void ReadCurve()
    {
        serializedObject.Update();

        AnimationCurve curve = curveProperty.animationCurveValue;

        if (curve == null)
        {
            Debug.LogError("Curve is null.");
            return;
        }

        var data = new CurveJson
        {
            preWrapMode = curve.preWrapMode,
            postWrapMode = curve.postWrapMode
        };

        foreach (var key in curve.keys)
        {
            data.keys.Add(new KeyframeJson(key));
        }

        json = JsonUtility.ToJson(data, true);

        Repaint();
    }

    private void WriteCurve()
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogError("JSON is empty.");
            return;
        }

        CurveJson data;

        try
        {
            data = JsonUtility.FromJson<CurveJson>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Invalid JSON:\n{e}");
            return;
        }

        if (data == null || data.keys == null)
        {
            Debug.LogError("JSON does not contain valid curve data.");
            return;
        }

        var keys = new Keyframe[data.keys.Count];

        for (int i = 0; i < data.keys.Count; i++)
        {
            keys[i] = data.keys[i].ToKeyframe();
        }

        var curve = new AnimationCurve(keys)
        {
            preWrapMode = data.preWrapMode,
            postWrapMode = data.postWrapMode
        };

        Undo.RecordObject(
            target,
            "Write Animation Curve From JSON"
        );

        serializedObject.Update();

        curveProperty.animationCurveValue = curve;

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(target);

        Debug.Log("AnimationCurve updated from JSON.");
    }

    private void SaveJsonFile()
    {
        string path = EditorUtility.SaveFilePanel(
            "Save AnimationCurve JSON",
            Application.dataPath,
            "curve.json",
            "json"
        );

        if (string.IsNullOrEmpty(path))
            return;

        File.WriteAllText(path, json);

        Debug.Log($"Curve JSON saved to:\n{path}");
    }

    private void LoadJsonFile()
    {
        string path = EditorUtility.OpenFilePanel(
            "Load AnimationCurve JSON",
            Application.dataPath,
            "json"
        );

        if (string.IsNullOrEmpty(path))
            return;

        json = File.ReadAllText(path);

        Repaint();
    }
}
