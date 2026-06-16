#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SearchEngine.Additions;
using SearchEngine.EditorViews.AssetTypes;
using SearchEngine.EditorViews.Data;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.Tables
{
    [Serializable]
    public class AssetInfoAlg : ITableDisplayAlg
    {
        [SerializeField] private IList<AssetInfoData> data;
        private IEditorView filter;
        private IEditorView sorting;
        [SerializeField] private string assetsFormat;
        [SerializeField] private string titleDataColumn;
        private string titleTemplate = "{0} ({1})";
        private const int buttonXWidth = 18;
        private int buttonXClicked = -1;
        private string buttonPathClicked = string.Empty;
        private bool activeToggle = true;
        public bool AddCurrSceneButton { get; set; }

        public AssetInfoAlg(IList<AssetInfoData> data, IEditorView filter, IEditorView sorting, string titleDataColumn, string assetsFormat = "")
        {
            this.data = data;
            this.filter = filter;
            this.sorting = sorting;
            this.assetsFormat = assetsFormat;
            this.titleDataColumn = titleDataColumn;
            this.CheckSerializeFields();            
        }

        public void ShowPreTitle()
        {
            if(filter == null && sorting == null && !AddCurrSceneButton)
                return;

            EditorGUILayout.BeginVertical(HelperGUI.GUIStyleTableHeader2());
            
            GUILayout.BeginHorizontal();
                if(filter!=null)
                {
                    GUILayout.BeginHorizontal(HelperGUI.GUIStyleFrame2());
                    filter.ShowGUI();
                    GUILayout.EndHorizontal();
                }
                if (sorting != null)
                {
                    GUILayout.BeginHorizontal(HelperGUI.GUIStyleFrame2());
                    sorting.ShowGUI();
                    GUILayout.EndHorizontal();
                }
            GUILayout.EndHorizontal();
            
            if (AddCurrSceneButton)
            {
                GUILayout.Space(5);
                if (GUILayout.Button("Add current scene", GUILayout.ExpandWidth(false)))
                {
                    AddCurrScene();
                }
            }
            
            GUILayout.EndVertical();    
        }

        public void ShowTitle()
        {
            var temp = activeToggle;
            activeToggle = EditorGUILayout.Toggle(activeToggle, GUILayout.MaxWidth(HelperGUI.ToggleWidth));
            if (temp != activeToggle)
                SetAllAssetsActive(activeToggle);

            if (GUILayout.Button("X", GUILayout.MaxWidth(buttonXWidth)))
            {
                data.Clear();
                activeToggle = true;
            }

            GUILayout.Label(string.Format(titleTemplate, titleDataColumn, data.Count), GUILayout.ExpandWidth(true));
        }

        public void ShowRow(int rowNum)
        {
            AssetInfoData asset = data[rowNum];

            var temp = EditorGUILayout.Toggle(asset.Active, GUILayout.Width(HelperGUI.ToggleWidth));
            if (temp != asset.Active)
            {
                asset.Active = temp;
                UpdateActiveToggle();
            }

            if (GUILayout.Button("X", GUILayout.Width(buttonXWidth)))
            {
                buttonXClicked = rowNum;
            }

            GUI.enabled = asset.Active;
                ShowAssetPath(asset.AssetPath);
            GUI.enabled = true;
        }

        public void AfterTable()
        {
            if (buttonXClicked >= 0)
            {
                data.RemoveAt(buttonXClicked);
                UpdateActiveToggle();
                buttonXClicked = -1;
            }

            if (!string.IsNullOrEmpty(buttonPathClicked))
            {
                var paths = HelperGeneral.GetAssets(buttonPathClicked, assetsFormat);
                data.Clear();
                AssetInfoData.AddNewAssets(data, paths);
                UpdateActiveToggle();
                buttonPathClicked = string.Empty;
            }
        }

        public void OnTableDragAndDrop()
        {
            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }
            else if (evt.type == EventType.DragPerform)
            {
                var newAssets = new HashSet<string>();
                foreach (var path in DragAndDrop.paths)
                {
                    if (!path.StartsWith("Assets")
                        || new Asset(path).isMeta)
                        continue;

                    if (new Asset(path).isFolder)
                    {
                        foreach (var v in HelperGeneral.GetAssets(path, assetsFormat))
                        {
                            newAssets.Add(v);
                        }
                    }
                    else if (string.IsNullOrEmpty(assetsFormat) || path.EndsWith(string.Format(".{0}", assetsFormat)))
                    {
                        newAssets.Add(path);
                    }
                }
                AssetInfoData.AddNewAssets(data, newAssets);
            }

            UpdateActiveToggle();
        }

        private void ShowAssetPath(string path)
        {

            string[] words = path.Split(new char[] { '\\', '/' });
            int len = words.Length - 1;
            for (int i = 0; i < len; i++)
            {
                if (GUILayout.Button(words[i], GUILayout.ExpandWidth(false)))
                {
                    buttonPathClicked = string.Join("/", words.Take(i + 1).ToArray());
                }
                GUILayout.Label("/", GUILayout.ExpandWidth(false));
            }

            HelperGUI.AssetField(path, words.Last());
        }

        public void UpdateActiveToggle()
        {
            activeToggle = data.All(x => x.Active);
        }

        private void SetAllAssetsActive(bool active)
        {
            foreach (var s in data)
                s.Active = active;
            UpdateActiveToggle();
        }
        
        private void AddCurrScene()
        {
            string path = SceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;
            AssetInfoData.AddNewAssets(data, new []{ path });
        }
    }
}

#endif