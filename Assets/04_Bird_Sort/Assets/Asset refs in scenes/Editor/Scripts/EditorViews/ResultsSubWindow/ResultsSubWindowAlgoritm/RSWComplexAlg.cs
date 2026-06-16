#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using SearchEngine.Additions;
using SearchEngine.EditorViews.Data;
using SearchEngine.EditorViews.Tables;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.ResultsSubWindow
{
    public abstract class RSWComplexAlg<T1,T2> : IResultsSubWindowAlgoritm
    {
        public event Action CloseButtonClicked;
        
        protected T1 headerData;
        [SerializeField] protected List<KeyValuePair<T2, DataGOPath[]>> contentData;
        [SerializeField] protected bool[] dataFoldout;
        [SerializeField] private UIActionsRSW uiActions;

        private Vector2 scrollPos;
        private float tableHeight;
        private const int tableRowH = 20;

        
        public RSWComplexAlg(T1 headerData, List<KeyValuePair<T2, DataGOPath[]>> contentData, bool[] dataFoldout, UIActionsRSW uiActions)
        {
            this.headerData = headerData;
            this.contentData = contentData;
            this.uiActions = uiActions;
            this.dataFoldout = dataFoldout;
            this.CheckSerializeFields();
        }

        public void ShowHeaderContent()
        {
            GUILayout.BeginHorizontal();
                if (GUILayout.Button("X", GUILayout.Width(18)))
                {
                    EventSender.SendEvent(CloseButtonClicked);
                }
                GUILayout.Label(string.Format("({0})", contentData.Count));
                DisplayHeaderAsset();
            GUILayout.EndHorizontal();            
        }

        public void ShowMiddleContent()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
                ShowTable();
            GUILayout.EndScrollView();
        }
        
        public Vector2 WindowSize
        {
            set
            {
                tableHeight = value.y - 53;
            }
        }

        private void ShowTable()
        {
            int tableRowCount = contentData.Count;
            for (int i = 0; i < contentData.Count; i++)
            {
                if (dataFoldout[i])
                    tableRowCount += contentData.ElementAt(i).Value.Length;
            }

            int begin;
            int end;
            DataTableEV<object>.GetDisplayRows(tableRowH, (int)tableHeight, tableRowCount, scrollPos.y, out begin, out end);

            GUILayout.Space(begin * tableRowH);

            int posI=0;
            int row=0;
            for (int i = 0; i < contentData.Count; i++)
            {
                if (dataFoldout[i])
                    row += contentData.ElementAt(i).Value.Length+1;
                else
                    row++;
                if (row >= begin)
                {
                    posI = i;
                    if (dataFoldout[i])
                        row -= contentData.ElementAt(i).Value.Length + 1;
                    else
                        row--;
                    break;
                }
            }

            ShowTable2(begin, end, posI, row);

            GUILayout.Space((tableRowCount-end) * tableRowH);
        }
        private void ShowTable2(int begin, int end, int posI, int row)
        {
            for (int i = posI; i < contentData.Count; i++)
            {
                var data = this.contentData.ElementAt(i).Value;
                bool foldout = dataFoldout[i];

                row++;
                if (row >= begin && row <= end)
                {
                    ShowTableRowTitle(foldout, i);
                }

                if (foldout)
                {
                    if (row+ data.Length < begin && row+1 > end)
                        return;

                    GUILayout.BeginVertical(HelperGUI.GUIStyleWindowContent());
                    for (int j = 0; j < data.Length; j++)
                    {
                        row++;
                        if (row >= begin && row <= end)
                        {
                            uiActions.ShowGOButton(data[j], GetScenePath(i), tableRowH);
                        }
                        else if (row > end)
                        {
                            GUILayout.EndVertical();
                            return;
                        }
                    }
                    GUILayout.EndVertical();
                }
            }
        }
        private void ShowTableRowTitle(bool foldout, int rowNum)
        {
            GUIStyle gs = GUI.skin.GetStyle("foldout");
                gs.fixedWidth = 18;
                gs.stretchWidth = false;
            var style = foldout ?
                HelperGUI.GUIStyleWindowHeader() :
                HelperGUI.GUIStyleWindow1();
            GUILayout.BeginHorizontal(style, GUILayout.Height(tableRowH));
                string title = string.Format("({0})", contentData.ElementAt(rowNum).Value.Length);
                dataFoldout[rowNum] = EditorGUILayout.Foldout(foldout, title, gs);
                DisplayRowHeaderAsset(rowNum);
            GUILayout.EndHorizontal();
        }

        protected virtual void DisplayHeaderAsset() {}
        protected virtual void DisplayRowHeaderAsset(int rowNum) {}
        protected virtual string GetScenePath(int rowNum)
        {
            return string.Empty;
        }
    }
}

#endif