#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using SearchEngine.Additions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SearchEngine.EditorViews.Tables
{
    [Serializable]
    public class AssetAlg3 : ITableDisplayAlg
    {
        public event Action<string> DetailsButtonClicked;
        public event Action UpdateButtonClicked;
        [SerializeField] private List<string> data;
        [SerializeField] private string titleTemplate;

        public AssetAlg3(List<string> data, string titleTemplate)
        {
            this.data = data;
            if (string.IsNullOrEmpty(titleTemplate))
                titleTemplate = "Asset ({0})";
            this.titleTemplate = titleTemplate;
            this.CheckSerializeFields();
        }

        public void ShowPreTitle(){}

        public void ShowTitle()
        {
            GUILayout.Label(string.Format(titleTemplate, data.Count));

            if (GUILayout.Button(new GUIContent("Copy", "Copy to \"Scenes for search\""), GUILayout.Width(50), GUILayout.Height(16)))
            {
                EventSender.SendEvent(UpdateButtonClicked);
            }
        }

        public void ShowRow(int rowNum)
        {
            HelperGUI.AssetField(data[rowNum]);

            if (GUILayout.Button("details..", GUILayout.Width(60)))
            {
                EventSender.SendEvent(DetailsButtonClicked, data[rowNum]);
            }
        }

        public void AfterTable(){}
    }
}

#endif