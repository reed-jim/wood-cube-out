using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LA.PostBuild
{
    public class PostBuildEditor : EditorWindow
    {
        //field
        private static string _projectName;
        private static string _featureName;
        private static bool _isOverrideName;

        public static string PathToConfig => Application.persistentDataPath + "/postBuild.json";

        [MenuItem("LA/Post Process Build")] 
        private static void Init()
        {
            var box = GetWindow<PostBuildEditor>(false,"Post Process Build");
            InitData();
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 100;
            EditorGUILayout.LabelField("Post Process Build");
            ShowGUI();
        }

        private void ShowGUI()
        {
            _projectName = EditorGUILayout.TextField("Project Name:", _projectName);
            _featureName = EditorGUILayout.TextField("Feature Name:", _featureName);
        EditorGUIUtility.labelWidth = position.width-50;
        EditorGUIUtility.fieldWidth = 30;
            _isOverrideName = EditorGUILayout.Toggle("Is Override Name After Build: ",_isOverrideName,new GUIStyle(GUI.skin.toggle));
            if (GUILayout.Button("Save"))
            {
                SaveToJson();
            }
            if (GUILayout.Button("Test"))
            {
                Test();
            }
        }

        private static void SaveToJson()
        {
            var data = new PostBuildData()
            {
                ProjectName = _projectName,
                FeatureName = _featureName,
                IsOverrideName = _isOverrideName,
            };
            string jsonData = JsonUtility.ToJson(data);
            File.WriteAllText(PathToConfig,jsonData);
        }

        private static void InitData()
        {
            if (!File.Exists(PathToConfig))
            {
                return;
            }

            string jsonData = File.ReadAllText(PathToConfig);
            var data = JsonUtility.FromJson<PostBuildData>(jsonData);
            _projectName = data.ProjectName;
            _featureName = data.FeatureName;
            _isOverrideName = data.IsOverrideName;
        }

        private static void Test()
        {
            string pathToBuiltProject = @"D:\Build\Unity\pvpIl2Cpp.apk";
            Debug.Log(pathToBuiltProject);
            if (!File.Exists(PostBuildEditor.PathToConfig))
                return;
            string jsonText = File.ReadAllText(PostBuildEditor.PathToConfig);
            var data = JsonUtility.FromJson<PostBuildData>(jsonText);
            if(!data.IsOverrideName)
                return;
            var time = DateTime.Now;
            string afterName = data.ProjectName + "_" + data.FeatureName + "_" + time.Hour + ":" + time.Minute + "_" +
                               time.ToString("ddMMyyyy")+".apk";
            string afterPath = Directory.GetParent(pathToBuiltProject)!.ToString();
            afterPath = Path.Combine(afterPath, afterName);
            Debug.Log(afterPath);
            //File.Move(pathToBuiltProject,afterPath);
        }

    }

    public struct PostBuildData
    {
        public string ProjectName;
        public string FeatureName;
        public bool IsOverrideName;
    }
}