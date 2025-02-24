#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using SearchEngine.Additions;
using UnityEditor;
using UnityEngine;

namespace SearchEngine.EditorViews.Tables
{
    public class DataTableEV<T> : IEditorView
    {
        public event Action TableDragAndDrop;

        public int TableWidth { get; set; }
        [SerializeField] private IList<T> data;
        [SerializeField] private ITableDisplayAlg tableAlg;
        private IEditorView warnMsg;

        private Vector2 scrollPos;
        private int rowHeight;
        private const int minRowHeight = 5;
        private const int maxRowHeight = 1000;
        private int tableHeight;
        private const int minTableHeight = 60;
        private const int maxTableHeight = 1000;
        private float currTableHeight;

        public const int IndexWidth = 40;  
        
        public bool DragAndDropEnabled { get; set; }
        public bool ExpandTable { get; set; }
        public bool IndexEnabled { get; set; }
        public int TableHeight
        {
            get { return tableHeight; }
            set
            {
                tableHeight = Mathf.Max(minTableHeight, value);
                tableHeight = Mathf.Min(maxTableHeight, tableHeight);
                currTableHeight = tableHeight;
            }
        }
        public int RowHeight
        {
            get { return rowHeight; }
            set
            {
                rowHeight = Mathf.Max(minRowHeight, value);
                rowHeight = Mathf.Min(maxRowHeight, rowHeight);
            }
        }
        public string Items { get; set; }

        public DataTableEV(IList<T> data, ITableDisplayAlg tableAlg, IEditorView warnMsg=null)
        {
            this.data = data;
            this.tableAlg = tableAlg;
            this.warnMsg = warnMsg;
            this.CheckSerializeFields();

            RowHeight = 24;
            currTableHeight = tableHeight = 30;
            ExpandTable = true;
            IndexEnabled = true;
            Items = "items";
        }

        public virtual void ShowGUI()
        {
            Rect dropArea;
            if (TableWidth > 0)
            {
                dropArea = EditorGUILayout.BeginVertical(new GUIStyle(), GUILayout.MaxWidth(TableWidth));
            }
            else
            {
                   dropArea = EditorGUILayout.BeginVertical(new GUIStyle());
            }
                GUILayout.Space(8);
                    tableAlg.ShowPreTitle();

                    EditorGUILayout.BeginHorizontal(HelperGUI.GUIStyleTableHeader());
                        if (IndexEnabled)
                        {
                            GUILayout.Label("№", GUILayout.Width(IndexWidth));
                        }
                        tableAlg.ShowTitle();
                    GUILayout.EndHorizontal();


                Rect tableArea=new Rect();
                if (!ExpandTable)
                {
                    GUILayout.BeginVertical(HelperGUI.GUIStyleTableContent(), GUILayout.Height(tableHeight));
                }
                else
                {
                    tableArea = EditorGUILayout.BeginVertical(HelperGUI.GUIStyleTableContent());                    
                }
                    ShowRowList();
                GUILayout.EndVertical();
            
                if (ExpandTable
                    && Event.current.type == EventType.Repaint)
                {
                    currTableHeight = tableArea.height;
                }
                tableAlg.AfterTable();
            GUILayout.EndVertical();
            if (DragAndDropEnabled)
                CatchTableDragAndDrop(dropArea);
        }
        
        private void ShowRowList()
        {
            int rowsCount = data.Count;
            if (rowsCount == 0)
            {
                string msg = DragAndDropEnabled
                    ? string.Format("Drag and drop new {0} here...", Items)
                    : "Empty list!";
                
                GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(msg, HelperGUI.GetGuiStyleTextBold(16, TextAnchor.MiddleCenter));
                    if (warnMsg != null)
                        warnMsg.ShowGUI();
                GUILayout.FlexibleSpace();
                
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);           
                int begin;
                int end;
                GetDisplayRows(rowHeight, (int)currTableHeight, rowsCount, scrollPos.y, out begin, out end);
                GUILayout.Space(begin * rowHeight);
                    for (int i = begin; i < end; i++)
                    {
                        GUILayout.BeginHorizontal(HelperGUI.GUIStyleTableRow(), GUILayout.Height(rowHeight));
                        if (IndexEnabled)
                        {
                            GUILayout.Label((i+1).ToString(), GUILayout.Width(IndexWidth));
                        }
                            tableAlg.ShowRow(i);
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.Space((rowsCount - end) * rowHeight);

            GUILayout.EndScrollView();
        }

        private void CatchTableDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated)
            {
                if (dropArea.Contains(evt.mousePosition))
                    EventSender.SendEvent(TableDragAndDrop);
            }
            else if (evt.type == EventType.DragPerform)
            {
                if (dropArea.Contains(evt.mousePosition))
                    EventSender.SendEvent(TableDragAndDrop);
            }
        }

        public static void GetDisplayRows(int rowHeight, int tableHeight, int tableRowCount, float scrollPosY, out int begin, out int end)
        {
            begin = Mathf.Max(0, (int)(scrollPosY / rowHeight));
            if (begin > tableRowCount - tableHeight / rowHeight) //because scrollPosY gives a wrong data at the top and end of table
                begin = Mathf.Max(0, tableRowCount - tableHeight / rowHeight);
            end = begin + tableHeight / rowHeight;
            end = Mathf.Min(end+1, tableRowCount);
        }
    }
}

#endif