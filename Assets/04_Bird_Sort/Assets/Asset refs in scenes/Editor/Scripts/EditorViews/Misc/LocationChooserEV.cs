#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using SearchEngine.Additions;
using SearchEngine.EditorViews.AssetTypes;
using SearchEngine.EditorViews.Data;
using UnityEditor;
using UnityEngine;

namespace SearchEngine.EditorViews
{
    public class LocationChooserEV : IEditorView
    {
        [SerializeField] private IList<AssetInfoData> data;
        [SerializeField] private string assetsFormat;
        
        public string Items { get; set; }
        private AssetsFilterEV filter;
        private AssetPathsEV assetPaths = new AssetPathsEV();

        public LocationChooserEV(IList<AssetInfoData> data, AssetsFilterEV filter, string assetsFormat = "")
        {
            this.data = data;
            this.filter = filter;
            this.assetsFormat = assetsFormat;
            this.CheckSerializeFields();
            Items = "items";
        }

        public void ShowGUI()
        {
            Rect dropArea = EditorGUILayout.BeginVertical();
                GUILayout.BeginVertical(HelperGUI.GUIStyleWindow1());
                GUILayout.BeginVertical();
                    assetPaths.ShowGUI();
                    if(filter!=null)
                        filter.ShowGUI();
                
                    if (GUILayout.Button(string.Format("Add {0}", Items), GUILayout.ExpandWidth(false)))
                    {
                        IEnumerable<string> newAssets = assetPaths.FindAssets(assetsFormat);
                        if (filter != null && filter.Enabled)
                        {
                            AssetTypes.AssetTypes filt = filter.Filter;
                            newAssets = newAssets.Where(v => AssetTypesHelper.CheckAsset(v, filt));
                        }
                        AssetInfoData.AddNewAssets(data, newAssets);  
                    }
                GUILayout.EndVertical();
                GUILayout.EndVertical();
            GUILayout.EndVertical();
            
            CatchTableDragAndDrop(dropArea);
        }

        private void CatchTableDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated)
            {
                if (dropArea.Contains(evt.mousePosition))
                {
                    foreach (var path in DragAndDrop.paths)
                    {
                        if (CheckPath(path))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                            return;
                        }
                    }
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
            }
            else if (evt.type == EventType.DragPerform)
            {
                if (dropArea.Contains(evt.mousePosition))
                {
                    foreach (var path in DragAndDrop.paths)
                    {
                        if (CheckPath(path))
                        {
                            if (path.Length == 6)
                                assetPaths.SetPathfolder(string.Empty);
                            else
                                assetPaths.SetPathfolder(path.Substring(7));
                            return;
                        }
                    }
                }
            }
        }

        private bool CheckPath(string path)
        {
            return Directory.Exists(path) && path.StartsWith("Assets");
        }
    }
}

#endif