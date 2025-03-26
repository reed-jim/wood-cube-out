#if UNITY_EDITOR

using System;
using SearchEngine.Additions;
using SearchEngine.Memento;
using UnityEditor;
using UnityEngine;

namespace SearchEngine.EditorViews.AssetTypes
{
    public class AssetsFilterEV : IEditorView, IOriginator<AssetsFilterEV.Memento>
    {
        public event Action ChangeEnable;
        public event Action ChangeFilter;
        public AssetTypes Filter { get; set; }
        public bool Enabled { get; set; }

        public AssetsFilterEV()
        {
            Filter = (AssetTypes)AssetTypesHelper.EnumSum;
        }

        public void ShowGUI()
        {
            bool entryEnabled = Enabled;
            AssetTypes entryFilter = Filter;

            GUILayout.BeginHorizontal(GUILayout.Height(20));
                Enabled = GUILayout.Toggle(Enabled, "Filter", GUILayout.Width(65));
                GUI.enabled = Enabled;
                    Filter = (AssetTypes)EditorGUILayout.EnumMaskPopup(new GUIContent(""), Filter, GUILayout.Width(100), GUILayout.ExpandWidth(false));
                GUI.enabled = true;
            GUILayout.EndHorizontal();

            if ((int) Filter == -1)
            {
                Filter = (AssetTypes)AssetTypesHelper.EnumSum;
            }
            if (entryEnabled != Enabled)
            {
                EventSender.SendEvent(ChangeEnable);
            }
            if (entryFilter != Filter)
            {
                EventSender.SendEvent(ChangeFilter);
            }
        }


        #region  memento part
        public Memento GetMemento()
        {
            return new Memento
            {
                Filter = this.Filter,
                Enabled = this.Enabled,
            };
        }
        public void SetMemento(Memento mem)
        {
            if (mem == null)
            {
                MementoHelper.ShowDataLoadingWarningMsg();
                return;
            }

            this.Filter = mem.Filter;
            this.Enabled = mem.Enabled;
        }

        [Serializable]
        public class Memento
        {
            public AssetTypes Filter;
            public bool Enabled;
        }
        #endregion
    }
}

#endif