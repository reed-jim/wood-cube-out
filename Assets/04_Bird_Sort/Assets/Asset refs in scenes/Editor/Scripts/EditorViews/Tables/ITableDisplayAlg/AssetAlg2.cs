#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using SearchEngine.Additions;
using UnityEditor;
using UnityEngine;

namespace SearchEngine.EditorViews.Tables
{
    [Serializable]
    public class AssetAlg2<T> : ITableDisplayAlg
    {
        public event Action<T> DetailsButtonClicked;
        public event Action ToggleChanged;
        [SerializeField] private string titleTemplate;
        [SerializeField] private IList<T> data;
        [SerializeField] private IList<bool> dataToggles;
        
        private bool activeToggle = true;
        
        public AssetAlg2(IList<T> data, List<bool> dataToggles, string titleTemplate)
        {
            this.data = data;
            this.dataToggles = dataToggles;
            if (string.IsNullOrEmpty(titleTemplate))
                titleTemplate = "Asset ({0})";
            this.titleTemplate = titleTemplate;
            this.CheckSerializeFields();
        }

        public void ShowPreTitle() { }

        public void ShowTitle()
        {
            var temp = activeToggle;
            activeToggle = EditorGUILayout.Toggle(activeToggle, GUILayout.MaxWidth(HelperGUI.ToggleWidth));
            if (temp != activeToggle)
                SetAllAssetsActive(activeToggle);

            GUILayout.Label(string.Format(titleTemplate, data.Count), GUILayout.ExpandWidth(true));
        }

        public void ShowRow(int rowNum)
        {
            var temp = EditorGUILayout.Toggle(dataToggles[rowNum], GUILayout.Width(HelperGUI.ToggleWidth));
            if (temp != dataToggles[rowNum])
            {
                dataToggles[rowNum] = temp;
                UpdateActiveToggle();
            }

            ShowAsset(data[rowNum]);

            if (GUILayout.Button("details..", GUILayout.Width(60)))
                {
                    EventSender.SendEvent(DetailsButtonClicked, data[rowNum]);
                }
        }

        public void AfterTable(){}
        
        private void SetAllAssetsActive(bool active)
        {
            for (int i = 0; i < dataToggles.Count; i++)
            {
                dataToggles[i] = active;                
            }

            UpdateActiveToggle();
        }

        private void UpdateActiveToggle()
        {
            activeToggle = dataToggles.All(x => x);
            EventSender.SendEvent(ToggleChanged);
        }

        protected virtual void ShowAsset(T asset)
        {        
        }
    }
}

#endif