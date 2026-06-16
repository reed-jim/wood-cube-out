#if UNITY_EDITOR

using System;
using System.Linq;
using SearchEngine.Additions;
using SearchEngine.EditorViews.GCModeFacade;
using SearchEngine.EditorWindows;
using SearchEngine.Memento;
using UnityEngine;
using UnityEditor;

namespace SearchEngine.EditorViews
{
    public class SearchEnginesEV : IEditorViewDisposable, IOriginator<SearchEnginesEV.Memento>
    {
        private SearchEngineTypes currToolbar;
        private string[] toolbarTitles = { "Search engine for asset refs", "Search engine for missing refs" };
        private SearchEngineFacadeEV[] engines = new SearchEngineFacadeEV[2];
        private GUIStyle toolbarStyle;        
        private Vector2 scrollPos;

        public bool IsSearching
        {
            get { return engines[(int)currToolbar].IsSearching; }
        }

        public void ShowGUI()
        {
            var lastToolbar = currToolbar;
            if (engines[(int) currToolbar] != null)
                GUI.enabled = !engines[(int) currToolbar].IsSearching;
                currToolbar = (SearchEngineTypes)GUILayout.Toolbar((int)currToolbar, toolbarTitles, HelperGUI.ToolbarStyle, GUILayout.Height(30));
            GUI.enabled = true;
            
            if (currToolbar != lastToolbar)
            {
                if (engines[(int)lastToolbar] != null)
                    engines[(int)lastToolbar].Dispose();
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                if(engines[(int)currToolbar]!=null)
                    engines[(int)currToolbar].ShowGUI();
            GUILayout.EndScrollView();
        }

        public void ReloadEngines()
        {
            if (engines.Any(v => v == null))
            {
                engines[0] = new AFSearchEngineFacadeEV();
                engines[1] = new MRFSearchEngineFacadeEV();

                foreach (var v in engines)
                {
                    v.ClearAllTables += ClearAllTables;
                }            
            }

            engines[0].ReCreate();
            engines[1].ReCreate();

            LoadSearchEnginesData();
        }

        private void LoadSearchEnginesData()
        {
            string resDir = HelperGeneral.GetGCResDir();
            if(resDir==null)
                return;  
            
            var state = AssetDatabase.LoadAssetAtPath(GetSaveFile(resDir), typeof(SearchEngineState)) as SearchEngineState;
            if (state == null)
                return; 
             
            if (!state.Validate())
            {
                MementoHelper.ShowDataLoadingWarningMsg();
                return;
            }
             
            try
            {
                this.SetMemento(state.engines);
                ((AFSearchEngineFacadeEV)engines[0]).SetMemento(state.modeAF);
                ((MRFSearchEngineFacadeEV)engines[1]).SetMemento(state.modeMRF);         

                if (state.resultsEWVisible)
                {
                    ResultsEW.Instance.SetMemento(state.resultsEW);
                    ResultsEW.Instance.Show();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        } 
        
        private void SaveSearchEnginesData()
        {
            string resDir = HelperGeneral.GetGCResDir();
            if(resDir==null)
                return;

            SearchEngineState state = ScriptableObject.CreateInstance<SearchEngineState>();
            state.engines = this.GetMemento();

            if (engines[0] is AFSearchEngineFacadeEV)
            {
                state.modeAF = ((AFSearchEngineFacadeEV)engines[0]).GetMemento();
            }
            if (engines[1] is MRFSearchEngineFacadeEV)
            {
                state.modeMRF = ((MRFSearchEngineFacadeEV)engines[1]).GetMemento();
            }

            var windows = Resources.FindObjectsOfTypeAll<ResultsEW>();
            if (windows.Any())
            {
                state.resultsEWVisible = true;
                state.resultsEW = windows[0].GetMemento();
            }
            else
            {
                state.resultsEWVisible = false;
            } 
            
            AssetDatabase.CreateAsset(state, GetSaveFile(resDir));
            AssetDatabase.SaveAssets();      
            
        }
         
        private string GetSaveFile(string resDir)
        {
            return string.Format("{0}{1}", resDir, "/SearchEnginesData.asset")
                .Replace("/","\\");
        }

        public void Dispose()
        {
            if(engines[(int)currToolbar] != null)
                engines[(int)currToolbar].Dispose();
            SaveSearchEnginesData();
            ResultsEW.CloseCurrWindow();
        } 

        public void ClearAllTables()
        {
            if (currToolbar == SearchEngineTypes.AssetsFinder)
            {
                engines[0].ReCreate();
            }
            if (currToolbar == SearchEngineTypes.MissingRefsFinder)
            {
                engines[1].ReCreate();
            }
        }

        #region  memento part
        public Memento GetMemento()
        { 
            return new Memento
            {
                currToolbar = this.currToolbar,
            };
        }

        public void SetMemento(Memento mem)
        {
            if (mem == null)
            {
                MementoHelper.ShowDataLoadingWarningMsg();
                return;
            }
            
            currToolbar = mem.currToolbar;
        }

        [Serializable]
        public class Memento
        {
            public SearchEngineTypes currToolbar;
        }
        #endregion
    }
}

#endif