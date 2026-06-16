#if UNITY_EDITOR

using System;
using UnityEngine;
using SearchEngine.Additions;
using UnityEditor;
using SearchEngine.Memento;

namespace SearchEngine.EditorViews.Results
{
    public class ResultsMissingRefsEV : IEditorView, IOriginator<ResultsMissingRefsEV.Memento>
    {
        private int currToolbar;
        private string[] toolbarTitles = { "prefabs and models", "mono scripts", "component`s inspector properties" };

        [SerializeField] private IEditorView goTable;
        [SerializeField] private IEditorView compsTable;
        [SerializeField] private IEditorView inspPropsTable;
        private string noAssetsMsg = string.Empty;
        
        public ResultsMissingRefsEV(IEditorView goTable, IEditorView compsTable, IEditorView inspPropsTable, string noAssetsMsg = "")
        {
            this.goTable = goTable;
            this.compsTable = compsTable;
            this.noAssetsMsg = noAssetsMsg;
            this.inspPropsTable = inspPropsTable;
            this.CheckSerializeFields();
        }

        public void ShowGUI()
        {
            GUILayout.Space(10);  

            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Missing refs to:", GUILayout.Width(90));
                currToolbar = GUILayout.Toolbar(currToolbar, toolbarTitles,  GUILayout.ExpandWidth(false), GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();


            if (currToolbar == 0 || currToolbar == 1) 
            {
                GUILayout.Space(5);
                GUILayout.Label("Found", HelperGUI.GetGuiStyleTextBold(18, TextAnchor.MiddleCenter));
            }
            if (currToolbar == 0)
                ShowTable(goTable); 
            else if (currToolbar == 1)
                ShowTable(compsTable);
            else if (currToolbar == 2)
                ShowTable(inspPropsTable);
        }

        private void ShowTable(IEditorView view)
        {
            if (view == null)
            {
                EditorGUILayout.LabelField(noAssetsMsg);
            }
            else
            {
                view.ShowGUI();                    
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