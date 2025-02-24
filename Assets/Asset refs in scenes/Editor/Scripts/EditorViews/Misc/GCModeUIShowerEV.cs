#if UNITY_EDITOR

using System;
using SearchEngine.Additions;
using SearchEngine.Memento;
using UnityEngine;

namespace SearchEngine.EditorViews.GCModeFacade
{
    public class GCModeUIShowerEV : IEditorView, IOriginator<GCModeUIShowerEV.Memento>
    {
        public event Action ClearAllTables;
        [SerializeField] protected ScenesRunner scenesRunner;
        [SerializeField] private IEditorView assetsView;
        [SerializeField] private IEditorView scenesView;
        [SerializeField] private IEditorView searchPanelView;
        [SerializeField] private IEditorView resultsView;
        [SerializeField] private string[] toolbarTitles;
        private int currToolbar = 0;
        

        public int CurrToolbar
        {
            get { return currToolbar; }
        }

        public GCModeUIShowerEV(ScenesRunner scenesRunner, IEditorView assets, IEditorView scenes, IEditorView results, IEditorView searchPanel,
            string[] toolbarTitles)
        {
            this.scenesRunner = scenesRunner;
            this.assetsView = assets;
            this.scenesView = scenes;
            this.resultsView = results;
            this.searchPanelView = searchPanel;
            this.toolbarTitles = toolbarTitles;
            this.CheckSerializeFields();
        }

        public void ShowGUI()
        {
            if(scenesRunner.InProgress)
            {
                scenesRunner.ContinueExecuting();
            }
            {
                ShowToolbar();
            }

            ShowSearchPanel();
        }

        private void ShowSearchPanel()
        {
            ShowSearchPanelContent();
        }

        private void ShowSearchPanelContent()
        {
            GUILayout.BeginVertical(HelperGUI.GUIStyleWindow2());
            searchPanelView.ShowGUI();
            GUILayout.EndVertical();
        }

        private void ShowToolbar()
        {
            GUILayout.BeginVertical(HelperGUI.GUIStyleWindow2());

                GUILayout.BeginHorizontal();
                    currToolbar = GUILayout.Toolbar(currToolbar, toolbarTitles, 
                        HelperGUI.ToolbarStyle, GUILayout.Height(25), GUILayout.ExpandHeight(false));

                    if (GUILayout.Button(new GUIContent("X", "Clear all tables"), GUILayout.Width(18)))
                    {
                        EventSender.SendEvent(ClearAllTables);
                    }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
            
                if (currToolbar == 0)
                {
                    assetsView.ShowGUI();
                }
                else if (currToolbar == 1)
                {
                    scenesView.ShowGUI();
                }
                else if (currToolbar == 2)
                {
                    resultsView.ShowGUI();
                }
            GUILayout.EndVertical();
        }

        public void SetPrevToolbarItem()
        {
            if (currToolbar > 0)
                currToolbar--;
        }
    
        public void SetNextToolbarItem()
        {
            if (currToolbar < 2)
                currToolbar++;
        }

        
        #region  memento part
        public Memento GetMemento()
        {
            return new Memento
            {
                currToolbar = this.currToolbar
            };
        }
        
        public void SetMemento(Memento mem)
        {
            if (mem == null)
            {
                MementoHelper.ShowDataLoadingWarningMsg();
                return;
            }

            this.currToolbar = mem.currToolbar;
        }

        [Serializable]
        public class Memento
        {
            public int currToolbar;
        }
        #endregion
    }
}

#endif