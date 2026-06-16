#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using SearchEngine.Additions;
using SearchEngine.EditorViews.Data;
using SearchEngine.EditorViews.ResultsSubWindow;
using SearchEngine.EditorViews.Tables;
using SearchEngine.EditorWindows;
using SearchEngine.Memento;
using SearchEngine.Memento.KeyValueMementoClasses;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.Results
{
    public class ResultsAssetsFinderEV : IEditorView, IOriginator<ResultsAssetsFinderEV.Memento>
    {
        [SerializeField] private DataTableEV<string> foundAssetsView;
        [SerializeField] private DataTableEV<string> foundScenesView;
        [SerializeField] private DataTableEV<string> notFoundAssetsView;
        [SerializeField] private DataTableEV<string> notFoundScenesView;

        [SerializeField] private List<string> foundAssets=new List<string>();
        [SerializeField] private List<bool> dataAssetsToggles=new List<bool>();
        [SerializeField] private List<string> foundScenes=new List<string>();
        [SerializeField] private List<string> notFoundAssets=new List<string>();
        [SerializeField] private List<string> notFoundScenes=new List<string>();

        [SerializeField] private IList<AssetInfoData> scenesForSearch;

        private Dictionary<string, List<SceneGOData>> dataAssets = new Dictionary<string, List<SceneGOData>>();
        
        public ResultsAssetsFinderEV(IList<AssetInfoData> scenesForSearch)
        {
            this.scenesForSearch = scenesForSearch;
            
            AssetAlg tableAlg3 = new AssetAlg(notFoundAssets, "Asset({0})");
            notFoundAssetsView = new DataTableEV<string>(notFoundAssets, tableAlg3);

            AssetAlg tableAlg4 = new AssetAlg(notFoundScenes, "Scene({0})");
            notFoundScenesView = new DataTableEV<string>(notFoundScenes, tableAlg4);

            AssetObjectAlg tableAlg = new AssetObjectAlg(foundAssets, dataAssetsToggles, "Asset({0})");
            foundAssetsView = new DataTableEV<string>(foundAssets, tableAlg);
            foundAssetsView.TableHeight = 250;
            foundAssetsView.ExpandTable = false;
            tableAlg.DetailsButtonClicked += ShowAssetDetails;
            tableAlg.ToggleChanged += UpdateFoundScenes;
            
            AssetAlg3 tableAlg2 = new AssetAlg3(foundScenes, "Scene({0})");
            foundScenesView = new DataTableEV<string>(foundScenes, tableAlg2);
            foundScenesView.TableHeight = 250;
            foundScenesView.ExpandTable = false;
            tableAlg2.DetailsButtonClicked += ShowSceneDetails;
            tableAlg2.UpdateButtonClicked += UpdateScenes;
            
            this.CheckSerializeFields();
        }

        public void UpdateData(Dictionary<string, List<SceneGOData>> data, List<string> scenes)
        {
            if (data == null || scenes == null)
            {
                return;
            }

            dataAssets = CopyAbleHelper.Copy01(data).ToDictionary(v=>v.Key, v=>v.Value);
            foundAssets.Clear();
            notFoundAssets.Clear();
            SortTableData(dataAssets, foundAssets, notFoundAssets);

            dataAssetsToggles.Clear();
            foreach (var v in foundAssets)
            {
                dataAssetsToggles.Add(true);
            }

            foundScenes.Clear();
            notFoundScenes.Clear();
            foreach (var scene in scenes)
            {
                bool found = false;
                foreach (var sgd in dataAssets.Keys)
                {
                    if (dataAssets[sgd].Any(x => x.ObjPath == scene))
                    {
                        found = true;
                        break;
                    }
                }
                if(found)
                    foundScenes.Add(scene);
                else
                    notFoundScenes.Add(scene);
            }
        }
        
        private void SortTableData(Dictionary<string, List<SceneGOData>> data, List<string> found, List<string> notFound)
        {
            foreach (var v in data)
            {
                if (v.Value.Any())
                {
                    found.Add(v.Key);
                }
                else
                {
                    notFound.Add(v.Key);                    
                }
            }
        }

        public void ShowGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("Found", HelperGUI.GetGuiStyleTextBold(18,TextAnchor.MiddleCenter));

            var rect = EditorGUILayout.BeginHorizontal();
                if (Event.current.type == EventType.Repaint)
                {
                    int tableWidth = (int)rect.width / 2-4;
                    foundAssetsView.TableWidth = tableWidth;
                    foundScenesView.TableWidth = tableWidth;
                    notFoundAssetsView.TableWidth = tableWidth;
                    notFoundScenesView.TableWidth = tableWidth;
                }
                    
                foundAssetsView.ShowGUI();
                GUILayout.FlexibleSpace();
                foundScenesView.ShowGUI();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.Label("Not found", HelperGUI.GetGuiStyleTextBold(18, TextAnchor.MiddleCenter));

            GUILayout.BeginHorizontal();
                notFoundAssetsView.ShowGUI();
                GUILayout.FlexibleSpace();
                notFoundScenesView.ShowGUI();
            GUILayout.EndHorizontal();
        }

        private void UpdateFoundScenes()
        {
            HashSet<string> currSubTableData = new HashSet<string>();
            for (int i = 0; i < dataAssetsToggles.Count; i++)
            {
                if (dataAssetsToggles[i])
                {
                    foreach (var scene in dataAssets[foundAssets[i]])
                        currSubTableData.Add(scene.ObjPath);
                }
            }

            foundScenes.Clear();
            foundScenes.AddRange(currSubTableData);
        }

        private void ShowAssetDetails(string asset)
        {
            if (dataAssets.Keys.Contains(asset))
            {
                var title = "Found used asset in scenes";
                var resultsSW = new RSWComplex0Facade();
                resultsSW.ReCreate(
                    title,
                    asset, 
                    dataAssets[asset].Select(v=>v.ObjPath).ToList(), 
                    dataAssets[asset].Select(v=>v.GameObjOnScene).ToList());

                ResultsEW.Instance.AddData(resultsSW);
                ResultsEW.Instance.Show();
            }
        }

        private void ShowSceneDetails(string scene)
        {
            List<DataGOPath[]> data = new List<DataGOPath[]>();
            List<string> assets = new List<string>();
            foreach (var fgd in dataAssets)
            {
                foreach (var sgd in fgd.Value)
                {
                    if (sgd.ObjPath == scene)
                    {
                        assets.Add(fgd.Key);
                        data.Add(sgd.GameObjOnScene);
                        break;
                    }
                }
            }

            var title = "Found used assets in scene";
            var resultsSW = new RSWComplex1Facade();
            resultsSW.ReCreate(
                title,
                scene,
                assets,
                data);

            ResultsEW.Instance.AddData(resultsSW);
            ResultsEW.Instance.Show();
        }

        private void UpdateScenes()
        {
            scenesForSearch.Clear();
            AssetInfoData.AddNewAssets(scenesForSearch, foundScenes);
        }


        #region  memento part
        public Memento GetMemento()
        {
            return new Memento
            {
                foundData = CopyAbleHelper.Copy01(this.dataAssets, new StringSceneGODataL()),
                allScenes = this.notFoundScenes.Union(this.foundScenes).ToList(),
            };
        }

        public void SetMemento(Memento mem)
        {
            if (mem == null || !mem.Validate())
            {
                MementoHelper.ShowDataLoadingWarningMsg();
                return;
            }

            UpdateData(
                CopyAbleHelper.Copy01(mem.foundData).ToDictionary(v=>v.Key, v=>v.Value),
                mem.allScenes.ToList());
        }

        [Serializable]
        public class Memento : IValidatable
        {
            public StringSceneGODataL[] foundData;
            public List<string> allScenes;

            public bool Validate()
            {
                return
                    foundData != null
                    && allScenes != null
                    && foundData.All(v => v != null && v.Validate());
            }
        }
        #endregion
    }
}

#endif