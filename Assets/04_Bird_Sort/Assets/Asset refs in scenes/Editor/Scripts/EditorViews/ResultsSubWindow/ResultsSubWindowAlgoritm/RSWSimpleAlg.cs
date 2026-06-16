#if UNITY_EDITOR

using System;
using System.Linq;
using SearchEngine.Additions;
using SearchEngine.EditorViews.Data;
using SearchEngine.EditorViews.Tables;
using SearchEngine.Memento;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.ResultsSubWindow
{
    public class RSWSimpleAlg : IResultsSubWindowAlgoritm, IOriginator<RSWSimpleAlg.Memento>
    {
        public event Action CloseButtonClicked;

        [SerializeField] private SceneGOData[] data;
        [SerializeField] private UIActionsRSW uiActions;

        private Vector2 scrollPos;
        private float tableHeight;
        private const int tableRowH = 22;
        
        public Vector2 WindowSize
        {
            set
            {
                tableHeight = value.y - 53;
            }
        }

        public RSWSimpleAlg(SceneGOData[] data, UIActionsRSW uiActions)
        {
            this.data = data;
            this.uiActions = uiActions;
            this.CheckSerializeFields();
        }

        public void ShowHeaderContent()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("X", GUILayout.Width(18)))
            {
                EventSender.SendEvent(CloseButtonClicked);
            }
            GUILayout.Label(string.Format("({0})", data.Length));
            if (data != null && data.Any())
            {
                HelperGUI.AssetField(data.First().ObjPath, UIActionsRSW.TitleObjWidth);
            }
            GUILayout.EndHorizontal();
        }

        public void ShowMiddleContent()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
                ShowTable();
            GUILayout.EndScrollView();
        }

        private void ShowTable()
        {
            if(!this.data.Any())
                return;

            var data0 = this.data[0];

            int begin;
            int end;
            DataTableEV<object>.GetDisplayRows(tableRowH, (int)tableHeight, data0.GameObjOnScene.Length, scrollPos.y, out begin, out end);

            GUILayout.Space(begin * tableRowH);

            GUILayout.BeginVertical();
                for (int j = 0; j < data0.GameObjOnScene.Length; j++)
                {
                    uiActions.ShowGOButton(data0.GameObjOnScene[j], data0.ObjPath, tableRowH);
                }
            GUILayout.EndVertical();
            
            GUILayout.Space((data0.GameObjOnScene.Length - end) * tableRowH);
        }

        
        #region  memento part
        public Memento GetMemento()
        {
            return new Memento
            {
                data = CopyAbleHelper.ToArray(this.data),
            };
        }

        public void SetMemento(Memento mem)
        {
            if (mem == null && mem.Validate())
            {
                MementoHelper.ShowDataLoadingWarningMsg();
                return;
            }

            this.data = CopyAbleHelper.ToArray(mem.data);
        }

        [Serializable]
        public class Memento : IValidatable
        {
            public SceneGOData[] data;

            public bool Validate()
            {
                return
                    data != null
                    && data.All(v => v != null && v.Validate());
            }
        }
        #endregion
    }
}

#endif