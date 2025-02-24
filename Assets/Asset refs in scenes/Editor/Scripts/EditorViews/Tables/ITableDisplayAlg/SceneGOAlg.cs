#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using SearchEngine.Additions;
using SearchEngine.EditorViews.Data;
using SearchEngine.EditorViews.ResultsSubWindow;
using SearchEngine.EditorWindows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.Tables
{
    [Serializable]
    public class SceneGOAlg : ITableDisplayAlg
    {
        [SerializeField] private List<SceneGOData> data;
        private string titleScenesTemplate = "Scene ({0})";
        private string titleMissingsTemplate = "Missings ({0})";
        private int titleMissingsWidth = 90;

        public string SubWindowTitle { private get; set; }

        public SceneGOAlg(List<SceneGOData> data)
        {
            this.data = data;
            this.CheckSerializeFields();
        }

        public void ShowPreTitle(){}

        public void ShowTitle()
        {
            int foundResults = 0;
            foreach (var g in data)
                foundResults += g.GameObjOnScene.Count();
            GUILayout.Label(string.Format(titleMissingsTemplate, foundResults), GUILayout.Width(titleMissingsWidth));

            GUILayout.Label(string.Format(titleScenesTemplate, data.Count), GUILayout.ExpandWidth(true));
        }

        public void ShowRow(int rowNum)
        {
            SceneGOData asset = data[rowNum];

            GUILayout.Label(asset.GameObjOnScene.Count().ToString(), GUILayout.Width(titleMissingsWidth));
            HelperGUI.AssetField(asset.ObjPath);

            if (GUILayout.Button("details...", GUILayout.ExpandWidth(false)))
            {
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(asset.ObjPath);
                ShowResultsDataWindow(asset);
            }
        }

        public void AfterTable(){}

        private void ShowResultsDataWindow(SceneGOData data)
        {
            var resultsSW = new RswSimpleFacade();
            resultsSW.ReCreate(SubWindowTitle, new[] { data });

            ResultsEW.Instance.AddData(resultsSW);
            ResultsEW.Instance.Show();
        }
    }
}

#endif